using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 전투 배경 위 · 유닛 아래에 까는 움직이는 분위기 층. 정지된 배경 그림을 "살아 있는 장면"으로 만든다.
// 빛 알갱이 · 꽃잎 · 빛줄기 · 낮은 안개 · 깜빡이는 광원 · 유닛 자리 빛 · 아래쪽 어둡게 · 가장자리 어둡게.
// 값은 BattleStageData, 붙이고 고르는 곳은 BattleStageView. 그림은 전부 코드로 만든다 (추가 아트 없음).
// 절제가 핵심: 알갱이가 많아지면 바로 싸구려가 된다. 기본값은 적게.
public class BattleAmbienceFx : MonoBehaviour
{
    [Tooltip("가산 합성 머티리얼 (UI/Additive). 빛 알갱이 · 빛줄기 · 광원에 쓴다")]
    [SerializeField] private Material additive;

    private RectTransform _root;
    private BattleStageData _stage;
    private readonly List<Mote> _motes = new();
    private readonly List<Petal> _petals = new();
    private readonly List<(RawImage img, float speed, float baseAlpha)> _fogs = new();
    private readonly List<(Image img, float baseAlpha, float seed)> _blinks = new();
    private Image _shaft;
    private float _shaftAlpha;

    private static Sprite _dot, _petal, _beam, _vignette, _bottom;
    private static Texture2D _fog;

    private class Mote { public RectTransform rt; public Image img; public Vector2 pos, vel; public float life, maxLife, size, phase; }
    private class Petal { public RectTransform rt; public Image img; public Vector2 pos, vel; public float rot, spin, phase; }

    public void Apply(BattleStageData stage)
    {
        _stage = stage;
        Clear();
        if (stage == null) return;
        _root = (RectTransform)transform;
        var area = _root.rect;

        // 아래 → 위 순서로 쌓는다 (나중 것이 위)
        foreach (var p in stage.standPools)
        {
            var pool = Make("StandPool", Dot(), additive);
            pool.color = stage.standPoolColor;
            Place(pool.rectTransform, p, new Vector2(area.height * 0.42f, area.height * 0.09f));
        }
        if (stage.spotlightColor.a > 0f)
            foreach (var p in stage.standPools)
            {
                // 유닛 머리 위에서 발밑까지 내려오는 좁은 빛기둥 (위가 진하고 아래로 옅어짐)
                var spot = Make("Spotlight", Beam(), additive);
                spot.color = stage.spotlightColor;
                var srt = spot.rectTransform; srt.pivot = new Vector2(0.5f, 1f);
                Place(srt, new Vector2(p.x, 1.02f), new Vector2(area.height * 0.32f, (1.02f - p.y + 0.03f) * area.height));
            }
        if (stage.lightShaft)
        {
            _shaft = Make("LightShaft", Beam(), additive);
            _shaft.color = stage.shaftColor;
            _shaftAlpha = stage.shaftColor.a;
            var rt = _shaft.rectTransform;
            rt.pivot = new Vector2(0.5f, 1f);
            Place(rt, stage.shaftOrigin, new Vector2(stage.shaftSize.x * area.width, stage.shaftSize.y * area.height));
            rt.localRotation = Quaternion.Euler(0, 0, stage.shaftAngle);
        }
        for (int i = 0; i < stage.fogBands.Count; i++)
        {
            var go = new GameObject("Fog", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            go.layer = gameObject.layer; go.hideFlags = HideFlags.DontSave;
            go.transform.SetParent(transform, false);
            var raw = go.GetComponent<RawImage>(); raw.texture = Fog(); raw.raycastTarget = false;
            raw.color = stage.fogColor;
            Place(raw.rectTransform, new Vector2(0.5f, stage.fogBands[i]), new Vector2(area.width * 1.02f, area.height * 0.2f));
            raw.uvRect = new Rect(Random.value, 0, 1.4f, 1);
            _fogs.Add((raw, (i % 2 == 0 ? 1f : -0.7f) * 0.008f, stage.fogColor.a));
        }
        foreach (var b in stage.blinkLights)
        {
            var img = Make("Blink", Dot(), additive);
            img.color = b.color;
            Place(img.rectTransform, b.position, Vector2.one * b.size * area.height);
            _blinks.Add((img, b.color.a, Random.value * 10f));
        }
        for (int i = 0; i < stage.moteCount; i++)
        {
            var m = new Mote { img = Make("Mote", Dot(), additive) };
            m.rt = m.img.rectTransform;
            Respawn(m, true);
            _motes.Add(m);
        }
        for (int i = 0; i < stage.petalCount; i++)
        {
            var p = new Petal { img = Make("Petal", PetalSprite(), null) };
            p.rt = p.img.rectTransform;
            p.img.color = stage.petalColor;
            RespawnPetal(p, true);
            _petals.Add(p);
        }
        if (stage.screenBottomShade > 0f)
        {
            var img = Make("BottomShade", Bottom(), null);
            img.color = new Color(0.02f, 0.03f, 0.05f, stage.screenBottomShade);
            var rt = img.rectTransform; rt.anchorMin = Vector2.zero; rt.anchorMax = new Vector2(1f, 0.42f); rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
        if (stage.vignette > 0f)
        {
            var img = Make("Vignette", Vignette(), null);
            img.color = new Color(0.01f, 0.02f, 0.04f, stage.vignette);
            var rt = img.rectTransform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }

    private void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
        _motes.Clear(); _petals.Clear(); _fogs.Clear(); _blinks.Clear(); _shaft = null;
    }

    private void Update()
    {
        if (_stage == null || _root == null) return;
        float dt = Time.unscaledDeltaTime, t = Time.unscaledTime;
        var area = _root.rect;

        foreach (var m in _motes)
        {
            m.life += dt;
            if (m.life >= m.maxLife) { Respawn(m, false); continue; }
            m.pos += m.vel * dt;
            m.pos.x += Mathf.Sin(t * 0.7f + m.phase) * 0.004f * dt;
            float k = m.life / m.maxLife;
            float fade = Mathf.Clamp01(k / 0.2f) * Mathf.Clamp01((1f - k) / 0.3f);
            float twinkle = 0.65f + 0.35f * Mathf.Sin(t * 3.1f + m.phase * 5f);
            var c = _stage.moteColor; c.a *= fade * twinkle;
            m.img.color = c;
            Place(m.rt, m.pos, Vector2.one * m.size * area.height);
        }
        foreach (var p in _petals)
        {
            p.pos += p.vel * dt;
            p.pos.y += Mathf.Sin(t * 1.3f + p.phase) * 0.006f * dt;
            p.rot += p.spin * dt;
            if (p.pos.x > 1.08f || p.pos.y < -0.05f) RespawnPetal(p, false);
            Place(p.rt, p.pos, new Vector2(0.012f, 0.007f) * area.height * 1.6f);
            p.rt.localRotation = Quaternion.Euler(0, 0, p.rot);
            p.rt.localScale = new Vector3(1f, Mathf.Abs(Mathf.Sin(t * 2f + p.phase)) * 0.7f + 0.3f, 1f); // 뒤집히며 날리는 느낌
        }
        foreach (var (img, speed, baseAlpha) in _fogs)
        {
            var uv = img.uvRect; uv.x += speed * dt; img.uvRect = uv;
            var c = img.color; c.a = baseAlpha * (0.8f + 0.2f * Mathf.Sin(t * 0.21f + speed * 300f)); img.color = c;
        }
        if (_shaft != null)
        {
            // 구름이 지나가듯 아주 느리게 숨 쉰다
            float k = 0.65f + 0.35f * (0.5f + 0.5f * Mathf.Sin(t * 0.23f)) * (0.85f + 0.15f * Mathf.Sin(t * 0.61f));
            var c = _shaft.color; c.a = _shaftAlpha * k; _shaft.color = c;
        }
        foreach (var (img, baseAlpha, seed) in _blinks)
        {
            // 꺼져 가는 경고등: 대체로 켜져 있다가 가끔 툭 끊긴다
            float n = Mathf.PerlinNoise(t * 1.7f, seed);
            float on = n > 0.32f ? 1f : 0.15f;
            float pulse = 0.75f + 0.25f * Mathf.Sin(t * 4f + seed);
            var c = img.color; c.a = baseAlpha * on * pulse; img.color = c;
        }
    }

    private void Respawn(Mote m, bool anywhere)
    {
        var a = _stage.moteArea;
        m.pos = new Vector2(a.x + Random.value * a.width, a.y + (anywhere ? Random.value : Random.value * 0.35f) * a.height);
        m.vel = new Vector2(Random.Range(-0.006f, 0.006f), Random.Range(0.006f, 0.018f));
        m.maxLife = Random.Range(5f, 10f);
        m.life = anywhere ? Random.value * m.maxLife : 0f;
        m.size = Random.Range(0.006f, 0.016f) * (Random.value < 0.15f ? 2.2f : 1f); // 가끔 앞쪽(크고 흐린) 알갱이
        m.phase = Random.value * 10f;
    }

    private void RespawnPetal(Petal p, bool anywhere)
    {
        p.pos = anywhere ? new Vector2(Random.value, Random.Range(0.25f, 0.95f)) : new Vector2(-0.05f, Random.Range(0.4f, 0.95f));
        p.vel = new Vector2(Random.Range(0.02f, 0.045f), Random.Range(-0.02f, -0.008f));
        p.rot = Random.value * 360f; p.spin = Random.Range(-60f, 60f); p.phase = Random.value * 10f;
    }

    private Image Make(string name, Sprite sprite, Material mat)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = gameObject.layer; go.hideFlags = HideFlags.DontSave;
        go.transform.SetParent(transform, false);
        var img = go.GetComponent<Image>(); img.sprite = sprite; img.raycastTarget = false;
        if (mat != null) img.material = mat;
        return img;
    }

    // 화면 비율(좌하단 원점) → 이 판의 로컬 좌표
    private void Place(RectTransform rt, Vector2 normalized, Vector2 size)
    {
        var r = _root.rect;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(r.xMin + normalized.x * r.width, r.yMin + normalized.y * r.height);
        rt.sizeDelta = size;
    }

    // ── 코드로 만드는 그림 ──
    private static Sprite Gen(int w, int h, System.Func<float, float, float> alpha, ref Sprite cache)
    {
        if (cache != null) return cache;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                px[y * w + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(alpha((x + 0.5f) / w, (y + 0.5f) / h)) * 255));
        tex.SetPixels32(px); tex.Apply();
        cache = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        cache.hideFlags = HideFlags.DontSave;
        return cache;
    }
    private static Sprite Dot() => Gen(64, 64, (u, v) => { float d = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f; return Mathf.Exp(-d * d * 4.5f); }, ref _dot);
    private static Sprite PetalSprite() => Gen(48, 24, (u, v) => { float x = u * 2f - 1f, y = (v * 2f - 1f) / (1f - 0.45f * (x + 1f) * 0.5f + 0.05f); float d = x * x + y * y; return Mathf.Clamp01((1f - d) * 3f); }, ref _petal);
    private static Sprite Beam() => Gen(64, 256, (u, v) => { float edge = Mathf.Exp(-Mathf.Pow((u - 0.5f) * 2.4f, 2f) * 2.2f); return edge * Mathf.Pow(v, 1.4f); }, ref _beam);
    private static Sprite Vignette() => Gen(128, 72, (u, v) => { float x = (u - 0.5f) * 2f, y = (v - 0.5f) * 2f; float d = Mathf.Sqrt(x * x * 0.8f + y * y); return Mathf.SmoothStep(0f, 1f, (d - 0.55f) / 0.75f); }, ref _vignette);
    private static Sprite Bottom() => Gen(8, 128, (u, v) => Mathf.Pow(1f - v, 1.6f), ref _bottom);

    // 가로로 이어지는 안개 띠 (가로 반복 가능한 값 노이즈, 위아래로 부드럽게 사라짐)
    private static Texture2D Fog()
    {
        if (_fog != null) return _fog;
        const int W = 256, H = 64;
        _fog = new Texture2D(W, H, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, hideFlags = HideFlags.DontSave };
        var px = new Color32[W * H];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                float u = (float)x / W, v = (float)y / H;
                float n = 0f, amp = 0.55f;
                for (int o = 1; o <= 3; o++, amp *= 0.5f)
                {
                    float f = 2f * o;
                    // 가로는 원 위에서 노이즈를 읽어 끝과 처음이 이어지게
                    float cx = Mathf.Cos(u * Mathf.PI * 2f) * f, cy = Mathf.Sin(u * Mathf.PI * 2f) * f;
                    n += amp * Mathf.PerlinNoise(cx + 13.1f * o + v * f * 0.6f, cy + 7.7f * o);
                }
                float vertical = Mathf.Sin(v * Mathf.PI); vertical *= vertical;
                px[y * W + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01((n - 0.25f) * 1.6f) * vertical * 255));
            }
        _fog.SetPixels32(px); _fog.Apply();
        return _fog;
    }
}
