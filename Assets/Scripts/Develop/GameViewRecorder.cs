#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEditor.Media;
using UnityEngine;

// 에디터 플레이 중 게임뷰를 MP4 또는 GIF로 녹화한다 (Unity Recorder 패키지 없이, MP4는 에디터 내장 MediaEncoder, GIF는 GifWriter).
// 켜고 끄는 곳: F10 / 디버그 패널 맨 위 (Editor/ScreenshotTool.cs). 저장: 프로젝트 루트/Recordings/yyyyMMdd_HHmmss.mp4|gif (git 제외)
//  - MP4: 매 프레임 끝에 화면을 읽어 고정 30fps로 넣는다. 게임이 느려지면 같은 프레임을 반복해 실제 시간과 길이를 맞춘다
//  - GIF: 가로 GifMaxWidth(720 / 1080, 디버그 패널에서 고름)로 줄여 15fps로 찍고, GifMaxSeconds(5 / 8 / 15초)가 지나면 저절로 멈춘다
//  - 읽을 크기는 첫 프레임에서 실제 화면을 한 장 찍어 정한다. 게임뷰 해상도를 고정(1920×1080 등)해 두면
//    Screen.width/height(창 크기)와 실제 그려지는 화면 크기가 달라서, Screen 크기로 읽으면 범위를 벗어나 회색만 남는다
//  - 소리는 녹음하지 않는다. 플레이를 끝내면 자동으로 멈추고 파일을 닫는다
public class GameViewRecorder : MonoBehaviour
{
    public enum Format { Mp4, Gif }

    public static GameViewRecorder Active { get; private set; }
    public static bool IsRecording => Active != null;
    public static float Elapsed => Active != null ? Time.unscaledTime - Active._startTime : 0f;
    public static string Folder => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Recordings");

    // 다음 녹화 형식 (에디터 설정에 남는다)
    public static Format NextFormat
    {
        get => (Format)UnityEditor.EditorPrefs.GetInt("Vertex.RecordFormat", 0);
        set => UnityEditor.EditorPrefs.SetInt("Vertex.RecordFormat", (int)value);
    }
    public static Format ActiveFormat => Active != null ? Active._format : NextFormat;

    private const int Fps = 30;
    public const int GifFps = 15;
    public static readonly int[] GifWidths = { 720, 1080 };
    public static readonly int[] GifLengths = { 5, 8, 15 };

    // GIF 가로 크기 · 최대 길이 (에디터 설정에 남는다). 녹화 중에는 시작할 때 값으로 고정
    public static int NextGifWidth
    {
        get => UnityEditor.EditorPrefs.GetInt("Vertex.RecordGifWidth", 1080);
        set => UnityEditor.EditorPrefs.SetInt("Vertex.RecordGifWidth", value);
    }
    public static int NextGifSeconds
    {
        get => UnityEditor.EditorPrefs.GetInt("Vertex.RecordGifSeconds", 8);
        set => UnityEditor.EditorPrefs.SetInt("Vertex.RecordGifSeconds", value);
    }
    public static int GifMaxWidth => Active != null ? Active._gifWidth : NextGifWidth;
    public static float GifMaxSeconds => Active != null ? Active._gifSeconds : NextGifSeconds;
    private int _gifWidth, _gifSeconds;

    private Format _format;
    private MediaEncoder _encoder;
    private GifWriter _gif;
    private Texture2D _frame;
    private string _path;
    private float _startTime;
    private int _written;
    private bool _stopping;

    public static void Toggle()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[녹화] 플레이 중에만 녹화할 수 있어요."); return; }
        if (IsRecording) Active.Stop();
        else
        {
            var go = new GameObject("__GameViewRecorder") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<GameViewRecorder>().Begin(NextFormat);
        }
    }

    private void Begin(Format format)
    {
        _format = format;
        _gifWidth = NextGifWidth;
        _gifSeconds = NextGifSeconds;
        Directory.CreateDirectory(Folder);
        _path = Path.Combine(Folder, System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + (format == Format.Gif ? ".gif" : ".mp4"));
        _startTime = Time.unscaledTime;
        _written = 0;
        Active = this;
        StartCoroutine(format == Format.Gif ? CaptureGif() : Capture());
    }

    // 첫 프레임 끝에서 화면을 한 장 찍어 실제 크기를 잰다
    private bool Open()
    {
        var probe = ScreenCapture.CaptureScreenshotAsTexture();
        int w = probe.width & ~1, h = probe.height & ~1; // H.264는 짝수 크기만
        Destroy(probe);
        if (w < 2 || h < 2) { Debug.LogWarning("[녹화] 화면 크기를 읽지 못해 멈춰요."); return false; }
        _frame = new Texture2D(w, h, TextureFormat.RGBA32, false);

        if (_format == Format.Gif)
        {
            _gif = new GifWriter(_path, w, h, GifMaxWidth);
            Debug.Log($"[녹화] GIF 시작 {_gif.Width}×{_gif.Height} (화면 {w}×{h}) · {GifFps}fps · 최대 {GifMaxSeconds:0}초 → {_path}");
        }
        else
        {
            var video = new VideoTrackAttributes
            {
                frameRate = new MediaRational(Fps),
                width = (uint)w,
                height = (uint)h,
                includeAlpha = false,
                bitRateMode = UnityEditor.VideoBitrateMode.High,
            };
            _encoder = new MediaEncoder(_path, video);
            Debug.Log($"[녹화] 시작 {w}×{h} · {Fps}fps → {_path}");
        }
        return true;
    }

    private void Read() => _frame.ReadPixels(new Rect(0, 0, _frame.width, _frame.height), 0, 0, false);

    private IEnumerator Capture()
    {
        var eof = new WaitForEndOfFrame();
        yield return eof;
        if (_stopping) yield break;
        if (!Open()) { Stop(); yield break; }
        while (!_stopping)
        {
            // 지금까지 흐른 시간만큼 프레임이 들어가 있어야 한다 — 모자라면 이번 화면으로 채운다
            int due = Mathf.FloorToInt((Time.unscaledTime - _startTime) * Fps) + 1;
            if (_written < due)
            {
                Read();
                _frame.Apply(false);
                while (_written < due) { _encoder.AddFrame(_frame); _written++; }
            }
            yield return eof;
        }
    }

    // GIF: 1/15초마다 화면을 읽어 작업 스레드로 넘긴다. 프레임 표시 시간은 실제 찍은 시각으로 정해서, 게임이 느려져도 길이가 맞는다
    private IEnumerator CaptureGif()
    {
        var eof = new WaitForEndOfFrame();
        yield return eof;
        if (_stopping) yield break;
        if (!Open()) { Stop(); yield break; }
        while (!_stopping)
        {
            float t = Time.unscaledTime - _startTime;
            if (t >= GifMaxSeconds) { Stop(); yield break; }
            if (t >= _written / (float)GifFps && _gif.TryRent(out var buffer))   // 인코딩이 밀리면 이번 프레임은 건너뛴다
            {
                _written = Mathf.FloorToInt(t * GifFps) + 1;
                Read();
                _frame.GetPixelData<Color32>(0).CopyTo(buffer);
                _gif.Submit(buffer, t);
            }
            yield return eof;
        }
    }

    public void Stop()
    {
        if (_stopping) return;
        _stopping = true;
        Finish();
        Destroy(gameObject);
    }

    private void Finish()
    {
        if (_encoder != null)
        {
            _encoder.Dispose();
            _encoder = null;
            Debug.Log($"[녹화] 저장 {_path} ({_written / (float)Fps:0.0}초)");
        }
        if (_gif != null)
        {
            float length = Mathf.Min(Time.unscaledTime - _startTime, GifMaxSeconds);
            _gif.Finish(length);
            Debug.Log($"[녹화] GIF 저장 {_path} ({length:0.0}초 · {_gif.FrameCount}장 · {_gif.Bytes / 1048576f:0.0}MB)");
            _gif = null;
        }
        if (_frame != null) { Destroy(_frame); _frame = null; }
        if (Active == this) Active = null;
    }

    // 플레이를 끝내면 여기로 온다
    private void OnDestroy() => Finish();
}
#endif
