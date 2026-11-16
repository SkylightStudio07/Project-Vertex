#if UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

// 녹화(GameViewRecorder)의 GIF 모드 인코더. 외부 프로그램 없이 GIF89a를 직접 쓴다.
//  - 메인 스레드는 화면 픽셀을 넘기기만 하고, 축소 · 색 줄이기 · 압축 · 파일 쓰기는 작업 스레드가 한다
//  - 바뀐 픽셀만 다시 칠한다: 지금 GIF에 보이는 색과 거의 같은 픽셀은 투명으로 두어, 가만히 있는 곳은 깜빡이지 않고 용량도 준다
//  - 색: 프레임마다 "바뀐 픽셀"만 모아 median cut으로 255색 팔레트를 새로 뽑고(지역 팔레트), 오차 분산 디더링으로 칠한다
//  - 완전히 같은 프레임은 앞 프레임을 더 오래 보여 준다
public sealed class GifWriter
{
    public readonly int Width, Height;
    public int FrameCount { get; private set; }
    public long Bytes => _bytes;

    private const int MaxBuffers = 4;            // 인코딩이 밀리면 이 이상은 쌓지 않고 그 프레임을 건너뛴다
    private const byte Transparent = 255;
    private const int ChangeThreshold = 10;      // |dR|+|dG|+|dB|가 이보다 작으면 안 바뀐 것으로 본다
    private const float Diffusion = 0.8f;        // 오차 분산 세기 (1 = Floyd–Steinberg 그대로)

    private readonly int _srcW, _srcH;
    private readonly int[] _x0, _x1, _y0, _y1;   // 출력 픽셀마다 원본 상자 범위 (y는 위에서부터)
    private readonly FileStream _file;
    private readonly BlockingCollection<(Color32[] px, float t)> _queue = new(MaxBuffers);
    private readonly ConcurrentBag<Color32[]> _pool = new();
    private readonly Thread _thread;
    private int _allocated;
    private long _bytes;

    private int[] _pending;                      // 다음에 쓸 프레임 (0xRRGGBB)
    private int[] _shown;                        // 지금 GIF에 보이는 색 (0xRRGGBB, 처음엔 -1)
    private float _pendingT, _firstT;
    private float _endT = -1f;
    private int _writtenCs;                      // 지금까지 쓴 표시 시간 (1/100초)

    // 색 줄이기 작업 공간
    private readonly int[] _histCount = new int[32768];
    private readonly long[] _histR = new long[32768], _histG = new long[32768], _histB = new long[32768];
    private readonly int[] _nearest = new int[32768];
    private readonly int[] _palR = new int[256], _palG = new int[256], _palB = new int[256];
    private int _palCount;

    public GifWriter(string path, int srcW, int srcH, int maxWidth)
    {
        _srcW = srcW; _srcH = srcH;
        Width = Mathf.Min(srcW, maxWidth);
        Height = Mathf.Max(1, Mathf.RoundToInt(srcH * (Width / (float)srcW)));
        _x0 = new int[Width]; _x1 = new int[Width];
        _y0 = new int[Height]; _y1 = new int[Height];
        Ranges(srcW, Width, _x0, _x1);
        Ranges(srcH, Height, _y0, _y1);
        _shown = new int[Width * Height];
        for (int i = 0; i < _shown.Length; i++) _shown[i] = -1;

        _file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 1 << 16);
        WriteHeader();
        _thread = new Thread(Work) { IsBackground = true, Name = "GifWriter" };
        _thread.Start();
    }

    private static void Ranges(int src, int dst, int[] a, int[] b)
    {
        float s = src / (float)dst;
        for (int i = 0; i < dst; i++)
        {
            a[i] = Mathf.Min(src - 1, (int)(i * s));
            b[i] = Mathf.Clamp((int)((i + 1) * s), a[i] + 1, src);
        }
    }

    // ── 메인 스레드 ──

    // 원본 크기(srcW × srcH) 버퍼를 빌린다. 인코딩이 밀려 있으면 false — 이번 프레임은 건너뛴다
    public bool TryRent(out Color32[] buffer)
    {
        if (_pool.TryTake(out buffer)) return true;
        if (_allocated >= MaxBuffers) { buffer = null; return false; }
        _allocated++;
        buffer = new Color32[_srcW * _srcH];
        return true;
    }

    // Texture2D.ReadPixels 그대로(아래 줄부터)인 픽셀과 녹화 시작 기준 시각(초)
    public void Submit(Color32[] buffer, float time)
    {
        if (!_queue.TryAdd((buffer, time))) _pool.Add(buffer);
    }

    // 남은 프레임을 다 쓰고 파일을 닫는다. endTime = 마지막 프레임이 끝나는 시각
    public void Finish(float endTime)
    {
        _endT = endTime;
        _queue.CompleteAdding();
        _thread.Join();
    }

    // ── 작업 스레드 ──

    private void Work()
    {
        try
        {
            foreach (var (px, t) in _queue.GetConsumingEnumerable())
            {
                var rgb = Downscale(px);
                _pool.Add(px);
                Accept(rgb, t);
            }
            if (_pending != null) WritePending(Mathf.Max(_endT, _pendingT + 1f / 15f));
            _file.WriteByte(0x3B);
            _bytes = _file.Length;
        }
        catch (Exception e)
        {
            Debug.LogError("[녹화] GIF 인코딩 실패: " + e);
        }
        finally
        {
            _file.Dispose();
        }
    }

    // 상자 평균으로 줄이고 위아래를 뒤집는다 → 0xRRGGBB
    private int[] Downscale(Color32[] px)
    {
        var rgb = new int[Width * Height];
        for (int y = 0; y < Height; y++)
        {
            int sy0 = _srcH - _y1[y], sy1 = _srcH - _y0[y];   // ReadPixels는 아래 줄부터라 뒤집는다
            for (int x = 0; x < Width; x++)
            {
                int sx0 = _x0[x], sx1 = _x1[x];
                int r = 0, g = 0, b = 0, n = 0;
                for (int sy = sy0; sy < sy1; sy++)
                {
                    int row = sy * _srcW;
                    for (int sx = sx0; sx < sx1; sx++)
                    {
                        var c = px[row + sx];
                        r += c.r; g += c.g; b += c.b; n++;
                    }
                }
                rgb[y * Width + x] = ((r / n) << 16) | ((g / n) << 8) | (b / n);
            }
        }
        return rgb;
    }

    private void Accept(int[] rgb, float t)
    {
        if (_pending == null) { _pending = rgb; _pendingT = _firstT = t; return; }
        if (Same(rgb, _pending)) return;      // 같은 화면이면 앞 프레임을 더 오래 보여 준다
        WritePending(t);
        _pending = rgb; _pendingT = t;
    }

    private static bool Same(int[] a, int[] b)
    {
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }

    private static bool Close(int a, int b)
    {
        if (b < 0) return false;
        int dr = ((a >> 16) & 255) - ((b >> 16) & 255);
        int dg = ((a >> 8) & 255) - ((b >> 8) & 255);
        int db = (a & 255) - (b & 255);
        return Math.Abs(dr) + Math.Abs(dg) + Math.Abs(db) < ChangeThreshold;
    }

    private void WritePending(float until)
    {
        // 표시 시간: 실제 시각에 맞춰 반올림 오차가 쌓이지 않게. 2/100초 미만은 브라우저가 느리게 틀어서 최소 2
        int endCs = Mathf.RoundToInt((until - _firstT) * 100f);
        int delay = Mathf.Clamp(endCs - _writtenCs, 2, 65535);
        _writtenCs += delay;

        // 지금 보이는 색과 달라진 사각형
        int left = Width, top = Height, right = -1, bottom = -1;
        for (int y = 0; y < Height; y++)
        {
            int row = y * Width;
            for (int x = 0; x < Width; x++)
            {
                if (Close(_pending[row + x], _shown[row + x])) continue;
                if (x < left) left = x;
                if (x > right) right = x;
                if (y < top) top = y;
                bottom = y;
            }
        }
        if (right < 0)
        {
            // 눈에 띄게 바뀐 곳이 없다: 1×1 투명 프레임으로 시간만 보탠다
            WriteFrameHeader(delay, 0, 0, 1, 1);
            _palCount = 1; _palR[0] = _palG[0] = _palB[0] = 0;
            WritePalette();
            Lzw(new[] { Transparent });
            FrameCount++;
            return;
        }
        int w = right - left + 1, h = bottom - top + 1;

        // 바뀐 픽셀만으로 팔레트
        Array.Clear(_histCount, 0, _histCount.Length);
        Array.Clear(_histR, 0, _histR.Length); Array.Clear(_histG, 0, _histG.Length); Array.Clear(_histB, 0, _histB.Length);
        for (int y = top; y <= bottom; y++)
            for (int x = left; x <= right; x++)
            {
                int i = y * Width + x;
                int c = _pending[i];
                if (Close(c, _shown[i])) continue;
                int r = (c >> 16) & 255, g = (c >> 8) & 255, b = c & 255;
                int k = ((r >> 3) << 10) | ((g >> 3) << 5) | (b >> 3);
                _histCount[k]++; _histR[k] += r; _histG[k] += g; _histB[k] += b;
            }
        BuildPalette();
        for (int i = 0; i < _nearest.Length; i++) _nearest[i] = -1;

        // 칠하기: 안 바뀐 픽셀은 투명, 바뀐 픽셀은 오차 분산 디더링
        var sub = new byte[w * h];
        var errCur = new float[(w + 2) * 3];
        var errNext = new float[(w + 2) * 3];
        for (int y = 0; y < h; y++)
        {
            Array.Clear(errNext, 0, errNext.Length);
            for (int x = 0; x < w; x++)
            {
                int i = (top + y) * Width + left + x;
                int c = _pending[i];
                if (Close(c, _shown[i])) { sub[y * w + x] = Transparent; continue; }
                int e = (x + 1) * 3;
                float fr = ((c >> 16) & 255) + errCur[e], fg = ((c >> 8) & 255) + errCur[e + 1], fb = (c & 255) + errCur[e + 2];
                int r = Clamp255(fr), g = Clamp255(fg), b = Clamp255(fb);
                int p = Nearest(r, g, b);
                sub[y * w + x] = (byte)p;
                _shown[i] = (_palR[p] << 16) | (_palG[p] << 8) | _palB[p];
                float er = (r - _palR[p]) * Diffusion, eg = (g - _palG[p]) * Diffusion, eb = (b - _palB[p]) * Diffusion;
                Spread(errCur, e + 3, er, eg, eb, 7f / 16f);
                Spread(errNext, e - 3, er, eg, eb, 3f / 16f);
                Spread(errNext, e, er, eg, eb, 5f / 16f);
                Spread(errNext, e + 3, er, eg, eb, 1f / 16f);
            }
            var tmp = errCur; errCur = errNext; errNext = tmp;
        }

        WriteFrameHeader(delay, left, top, w, h);
        WritePalette();
        Lzw(sub);
        FrameCount++;
    }

    private static void Spread(float[] err, int at, float r, float g, float b, float k)
    {
        err[at] += r * k; err[at + 1] += g * k; err[at + 2] += b * k;
    }

    private static int Clamp255(float v) => v < 0f ? 0 : v > 255f ? 255 : (int)(v + 0.5f);

    private int Nearest(int r, int g, int b)
    {
        int k = ((r >> 3) << 10) | ((g >> 3) << 5) | (b >> 3);
        int hit = _nearest[k];
        if (hit >= 0) return hit;
        // 칸 가운데 색으로 가장 가까운 팔레트 색을 찾아 둔다
        int cr = ((r >> 3) << 3) | 4, cg = ((g >> 3) << 3) | 4, cb = ((b >> 3) << 3) | 4;
        int best = 0, bestD = int.MaxValue;
        for (int i = 0; i < _palCount; i++)
        {
            int dr = cr - _palR[i], dg = cg - _palG[i], db = cb - _palB[i];
            int d = dr * dr * 3 + dg * dg * 4 + db * db * 2;   // 초록에 민감한 눈에 맞춘 가중치
            if (d < bestD) { bestD = d; best = i; }
        }
        _nearest[k] = best;
        return best;
    }

    // median cut: 색 상자를 가장 넓은 축의 픽셀 수 중앙값에서 계속 반으로 나눈다 (최대 255색, 255번은 투명)
    private void BuildPalette()
    {
        var bins = new List<int>();
        for (int k = 0; k < 32768; k++) if (_histCount[k] > 0) bins.Add(k);
        var boxes = new List<List<int>> { bins };
        while (boxes.Count < 255)
        {
            int pick = -1; long pickScore = 0; int pickAxis = 0;
            for (int i = 0; i < boxes.Count; i++)
            {
                var box = boxes[i];
                if (box.Count < 2) continue;
                int axis = WidestAxis(box, out int range);
                long pop = 0; foreach (int k in box) pop += _histCount[k];
                long score = range * (long)Math.Sqrt(pop);
                if (score > pickScore) { pickScore = score; pick = i; pickAxis = axis; }
            }
            if (pick < 0) break;
            var target = boxes[pick];
            int sh = pickAxis == 0 ? 10 : pickAxis == 1 ? 5 : 0;
            target.Sort((a, b) => ((a >> sh) & 31).CompareTo((b >> sh) & 31));
            long total = 0; foreach (int k in target) total += _histCount[k];
            long half = total / 2, acc = 0;
            int cut = 1;
            for (int i = 0; i < target.Count - 1; i++)
            {
                acc += _histCount[target[i]];
                if (acc >= half) { cut = i + 1; break; }
                cut = i + 1;
            }
            boxes[pick] = target.GetRange(0, cut);
            boxes.Add(target.GetRange(cut, target.Count - cut));
        }
        _palCount = 0;
        foreach (var box in boxes)
        {
            long n = 0, r = 0, g = 0, b = 0;
            foreach (int k in box) { n += _histCount[k]; r += _histR[k]; g += _histG[k]; b += _histB[k]; }
            if (n == 0) continue;
            _palR[_palCount] = (int)(r / n); _palG[_palCount] = (int)(g / n); _palB[_palCount] = (int)(b / n);
            _palCount++;
        }
        if (_palCount == 0) { _palCount = 1; _palR[0] = _palG[0] = _palB[0] = 0; }
    }

    private int WidestAxis(List<int> box, out int range)
    {
        int rMin = 31, rMax = 0, gMin = 31, gMax = 0, bMin = 31, bMax = 0;
        foreach (int k in box)
        {
            int r = (k >> 10) & 31, g = (k >> 5) & 31, b = k & 31;
            if (r < rMin) rMin = r; if (r > rMax) rMax = r;
            if (g < gMin) gMin = g; if (g > gMax) gMax = g;
            if (b < bMin) bMin = b; if (b > bMax) bMax = b;
        }
        int rr = (rMax - rMin) * 3, gg = (gMax - gMin) * 4, bb = (bMax - bMin) * 2;
        if (rr >= gg && rr >= bb) { range = rr; return 0; }
        if (gg >= bb) { range = gg; return 1; }
        range = bb; return 2;
    }

    private void WriteFrameHeader(int delay, int left, int top, int w, int h)
    {
        // Graphic Control Extension: 지우지 않음(1) + 투명색 사용
        _file.WriteByte(0x21); _file.WriteByte(0xF9); _file.WriteByte(4);
        _file.WriteByte((1 << 2) | 1);
        Short(delay);
        _file.WriteByte(Transparent);
        _file.WriteByte(0);
        // Image Descriptor: 지역 팔레트 256색
        _file.WriteByte(0x2C);
        Short(left); Short(top); Short(w); Short(h);
        _file.WriteByte(0x87);
    }

    private void WritePalette()
    {
        for (int i = 0; i < 256; i++)
        {
            if (i < _palCount) { _file.WriteByte((byte)_palR[i]); _file.WriteByte((byte)_palG[i]); _file.WriteByte((byte)_palB[i]); }
            else { _file.WriteByte(0); _file.WriteByte(0); _file.WriteByte(0); }
        }
    }

    private void WriteHeader()
    {
        var head = System.Text.Encoding.ASCII.GetBytes("GIF89a");
        _file.Write(head, 0, head.Length);
        Short(Width); Short(Height);
        _file.WriteByte(0x70);                 // 전역 팔레트 없음 (프레임마다 지역 팔레트)
        _file.WriteByte(0);                    // 배경색 번호
        _file.WriteByte(0);                    // 화소 비율
        // NETSCAPE2.0: 무한 반복
        _file.WriteByte(0x21); _file.WriteByte(0xFF); _file.WriteByte(11);
        var ns = System.Text.Encoding.ASCII.GetBytes("NETSCAPE2.0");
        _file.Write(ns, 0, ns.Length);
        _file.WriteByte(3); _file.WriteByte(1); Short(0); _file.WriteByte(0);
    }

    private void Short(int v) { _file.WriteByte((byte)(v & 0xFF)); _file.WriteByte((byte)((v >> 8) & 0xFF)); }

    // ── LZW (GIF 변형, 최소 코드 8비트, 최대 12비트). 사전은 열린 주소 해시표 ──

    private const int HashSize = 5003;
    private readonly int[] _hashKey = new int[HashSize];
    private readonly int[] _hashCode = new int[HashSize];
    private readonly byte[] _block = new byte[256];
    private int _blockLen, _bitBuf, _bitCnt, _codeSize, _maxCode, _next;
    private bool _clearFlag;

    private void ClearTable()
    {
        for (int i = 0; i < HashSize; i++) _hashKey[i] = -1;
        _next = 258;
    }

    private void Lzw(byte[] px)
    {
        const int clear = 256, eoi = 257;
        _file.WriteByte(8);
        _blockLen = 0; _bitBuf = 0; _bitCnt = 0;
        _codeSize = 9; _maxCode = 511;
        ClearTable();
        _clearFlag = true;
        Emit(clear);

        int prefix = px[0];
        for (int i = 1; i < px.Length; i++)
        {
            int k = px[i];
            int key = (k << 12) | prefix;
            int h = ((k << 4) ^ prefix) % HashSize;
            int found = -1;
            while (_hashKey[h] >= 0)
            {
                if (_hashKey[h] == key) { found = _hashCode[h]; break; }
                h = h == 0 ? HashSize - 1 : h - 1;
            }
            if (found >= 0) { prefix = found; continue; }
            Emit(prefix);
            if (_next < 4096) { _hashKey[h] = key; _hashCode[h] = _next++; }
            else
            {
                ClearTable();
                _clearFlag = true;
                Emit(clear);
            }
            prefix = k;
        }
        Emit(prefix);
        Emit(eoi);
        if (_bitCnt > 0) PutByte((byte)_bitBuf);
        if (_blockLen > 0) FlushBlock();
        _file.WriteByte(0);
    }

    private void Emit(int code)
    {
        _bitBuf |= code << _bitCnt;
        _bitCnt += _codeSize;
        while (_bitCnt >= 8)
        {
            PutByte((byte)_bitBuf);
            _bitBuf >>= 8;
            _bitCnt -= 8;
        }
        if (_clearFlag)
        {
            _codeSize = 9; _maxCode = 511; _clearFlag = false;
        }
        else if (_next > _maxCode && _codeSize < 12)
        {
            _codeSize++;
            _maxCode = _codeSize == 12 ? 4096 : (1 << _codeSize) - 1;
        }
    }

    private void PutByte(byte b)
    {
        _block[++_blockLen] = b;
        if (_blockLen == 255) FlushBlock();
    }

    private void FlushBlock()
    {
        _block[0] = (byte)_blockLen;
        _file.Write(_block, 0, _blockLen + 1);
        _blockLen = 0;
    }
}
#endif
