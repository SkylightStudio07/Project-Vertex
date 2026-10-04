using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// ============================================================
// filename   : BlessingAmbienceFx.cs
// description: 축복 노드 배경 빛 연출 (보름달·호수·백색 피안화 밭).
//              계속 재생: 달무리, 달빛 줄기, 호수 윤슬, 꽃밭 빛 물결, 떠오르는 빛 입자, 앞쪽 흐린 꽃잎,
//                         캐릭터 림라이트, 깊이감(배경 천천히 확대 + 마우스 시차)
//              순간 연출: 은총 선택 시 꽃밭에서 빛 고리가 퍼짐, 교감 시 달빛이 밝아지고 빛 입자가 캐릭터로 모임
//              항목마다 켜고 끌 수 있다. 위치는 배경 그림 기준 비율(0~1, 왼쪽 아래 원점)이라 해상도와 무관하다.
//              배경에 붙는 빛은 Background의 자식이라 진입 연출(BlessingIntroCinematic)의 카메라 이동을 그대로 따라간다.
// ============================================================
public class BlessingAmbienceFx : MonoBehaviour
{
    [Header("켜고 끄기")]
    [SerializeField] private bool moonHalo = true;
    [SerializeField] private bool moonRays = true;
    [SerializeField] private bool waterShimmer = true;
    [SerializeField] private bool fieldWave = true;
    [SerializeField] private bool lightMotes = true;
    [SerializeField] private bool foregroundPetals = true;
    [SerializeField] private bool characterRim = true;
    [SerializeField] private bool depthMotion = true;

    [Header("재료")]
    [SerializeField] private Material additiveMaterial;   // UI/Additive
    [SerializeField] private Material silhouetteMaterial; // UI/Silhouette (림라이트)
    [SerializeField] private Sprite softCircle, beam, band, mote, ring;
    [SerializeField] private Sprite[] petalSprites;

    [Header("배경 기준 위치 (0~1)")]
    [SerializeField] private Vector2 moonCenter = new(0.51f, 0.786f);
    [SerializeField] private float moonDiameter = 660f;     // 캔버스 단위
    [SerializeField] private Vector2 reflection = new(0.51f, 0.47f); // 호수에 비친 달 기둥
    [SerializeField] private float horizonY = 0.505f;
    [SerializeField] private Vector2 fieldRangeY = new(0.03f, 0.42f);

    [Header("색")]
    [SerializeField] private Color moonLight = new(0.72f, 0.88f, 1f, 1f);
    [SerializeField] private Color moteColor = new(0.55f, 0.9f, 1f, 1f); // 꽃잎(흰색)과 구분되게 청록 기운
    [SerializeField] private Color rimColor = new(0.8f, 0.93f, 1f, 0.55f);

    [Header("세기")]
    [SerializeField, Range(0f, 1f)] private float haloAlpha = 0.4f;
    [SerializeField, Range(0f, 1f)] private float rayAlpha = 0.4f;
    [SerializeField, Min(0)] private int moteCount = 50;
    [SerializeField] private Vector2 rimOffset = new(5f, 4f);   // 달 쪽(오른쪽 위)으로
    [SerializeField, Range(0f, 0.05f)] private float kenBurnsZoom = 0.025f;
    [SerializeField] private Vector2 parallaxBackground = new(14f, 8f);
    [SerializeField] private Vector2 parallaxCharacter = new(26f, 12f);
    [SerializeField] private Vector2 parallaxForeground = new(60f, 24f);

    private RectTransform _bg, _character, _root, _front;
    private BlessingIntroCinematic _intro;
    private readonly List<(Image img, float phase, float angle)> _rays = new();
    private Image _halo;
    private readonly List<Mote> _motes = new();
    private readonly List<(Image img, float phase, float speed)> _glints = new();
    private Image _wave;
    private float _waveT = -1f;
    private readonly List<FrontPetal> _petals = new();
    private RawImage _rim, _rimSource;
    private Vector3 _bgPos, _bgScale, _chPos, _chScale;
    private bool _hasBase;
    private float _motionWeight;
    private Vector2 _mouse;
    private float _boost;       // 교감 순간 밝기 배율 (0 → 1)
    private float _attract;     // 빛 입자가 캐릭터로 모이는 정도

    private class Mote { public RectTransform rt; public Image img; public Vector2 pos; public float speed, phase, size, sway; }
    private class FrontPetal { public RectTransform rt; public Image img; public float t = -1f, dur, y, size, spin; }

    private void Awake()
    {
        _bg = transform.Find("Background") as RectTransform;
        _character = transform.Find("BlessingCharacter") as RectTransform;
        _intro = GetComponent<BlessingIntroCinematic>();
        Build();
    }

    private void OnEnable()
    {
        _motionWeight = 0f;
        _boost = 0f; _attract = 0f;
        for (int i = 0; i < _motes.Count; i++) RespawnMote(_motes[i], true);
    }

    private void OnDisable() => RestoreMotion();

    // ───────── 생성 ─────────
    private void Build()
    {
        if (_bg == null) return;
        _root = NewRect("AmbienceFx", _bg);
        Stretch(_root);

        if (moonHalo)
        {
            _halo = NewImage("MoonHalo", _root, softCircle, additiveMaterial);
            Place(_halo.rectTransform, moonCenter, new Vector2(moonDiameter * 3f, moonDiameter * 3f), new Vector2(0.5f, 0.5f));
        }
        if (moonRays)
        {
            // 달에서 꽃밭 쪽으로 퍼지는 빛줄기 (피벗 위쪽 = 달 가운데)
            float[] angles = { -24f, -6f, 14f };
            float[] widths = { 300f, 420f, 260f };
            for (int i = 0; i < angles.Length; i++)
            {
                var img = NewImage("MoonRay" + i, _root, beam, additiveMaterial);
                Place(img.rectTransform, moonCenter, new Vector2(widths[i], 1250f), new Vector2(0.5f, 1f));
                _rays.Add((img, i * 1.7f, angles[i]));
            }
        }
        if (waterShimmer)
        {
            for (int i = 0; i < 9; i++)
            {
                var img = NewImage("Glint" + i, _root, band, additiveMaterial);
                float y = reflection.y - 0.005f - i * 0.006f;
                float w = Mathf.Lerp(150f, 60f, i / 8f) * Random.Range(0.7f, 1.3f);
                Place(img.rectTransform, new Vector2(reflection.x + Random.Range(-0.03f, 0.03f), y), new Vector2(w, 8f), new Vector2(0.5f, 0.5f));
                _glints.Add((img, Random.Range(0f, 10f), Random.Range(1.2f, 2.6f)));
            }
        }
        if (fieldWave)
        {
            _wave = NewImage("FieldWave", _root, band, additiveMaterial);
            Place(_wave.rectTransform, new Vector2(-0.3f, (fieldRangeY.x + fieldRangeY.y) * 0.5f), new Vector2(900f, 360f), new Vector2(0.5f, 0.5f));
            _wave.rectTransform.localEulerAngles = new Vector3(0f, 0f, 8f);
            _wave.color = Clear(moonLight);
        }
        if (lightMotes)
        {
            for (int i = 0; i < moteCount; i++)
            {
                var img = NewImage("Mote" + i, _root, mote, additiveMaterial);
                var m = new Mote { rt = img.rectTransform, img = img };
                Place(m.rt, Vector2.zero, Vector2.one * 10f, new Vector2(0.5f, 0.5f));
                RespawnMote(m, true);
                _motes.Add(m);
            }
        }

        // 캐릭터 앞 레이어 (대사판보다는 뒤)
        if (foregroundPetals && petalSprites != null && petalSprites.Length > 0 && _character != null)
        {
            _front = NewRect("ForegroundFx", transform);
            Stretch(_front);
            _front.SetSiblingIndex(_character.GetSiblingIndex() + 1);
            for (int i = 0; i < 3; i++)
            {
                var img = NewImage("FrontPetal" + i, _front, petalSprites[i % petalSprites.Length], null);
                img.color = Clear(new Color(0.92f, 0.97f, 1f, 1f));
                _petals.Add(new FrontPetal { rt = img.rectTransform, img = img, t = -Random.Range(1f, 6f) });
            }
        }

        if (characterRim && _character != null)
        {
            _rimSource = _character.GetComponentInChildren<RawImage>(true);
            if (_rimSource != null)
            {
                _rim = NewRect("RimLight", _rimSource.transform.parent).gameObject.AddComponent<RawImage>();
                _rim.raycastTarget = false;
                _rim.material = silhouetteMaterial;
                _rim.transform.SetSiblingIndex(_rimSource.transform.GetSiblingIndex()); // 캐릭터 바로 뒤
            }
        }
    }

    // ───────── 매 프레임 ─────────
    private void Update()
    {
        float t = Time.unscaledTime, dt = Time.unscaledDeltaTime;
        _boost = Mathf.MoveTowards(_boost, 0f, dt * 0.6f);
        float boost = 1f + _boost * 1.6f;

        if (_halo != null)
            _halo.color = WithAlpha(moonLight, haloAlpha * (0.85f + 0.15f * Mathf.Sin(t * 0.9f)) * boost);

        foreach (var (img, phase, angle) in _rays)
        {
            // 밝기는 숨 쉬듯, 각도는 천천히 ±2.5° 흔들린다
            float breath = 0.6f + 0.4f * Mathf.Sin(t * 0.55f + phase);
            img.color = WithAlpha(moonLight, rayAlpha * breath * boost);
            img.rectTransform.localEulerAngles = new Vector3(0f, 0f, angle + Mathf.Sin(t * 0.23f + phase) * 2.5f);
        }

        foreach (var (img, phase, speed) in _glints)
        {
            float s = Mathf.Max(0f, Mathf.Sin(t * speed + phase));
            img.color = WithAlpha(Color.white, 0.95f * s * s * boost);
        }

        UpdateWave(dt);
        UpdateMotes(t, dt);
        UpdateFrontPetals(dt);
        UpdateRim();
    }

    private void LateUpdate() => UpdateMotion();

    private void UpdateWave(float dt)
    {
        if (_wave == null) return;
        // 약 7초마다 꽃밭을 왼쪽에서 오른쪽으로 한 번 쓸고 지나간다 (4.5초 지나가고 2.7초 쉼)
        _waveT += dt / 4.5f;
        if (_waveT > 1.6f) _waveT = 0f;
        float k = Mathf.Clamp01(_waveT);
        var a = _wave.rectTransform;
        a.anchorMin = a.anchorMax = new Vector2(Mathf.Lerp(-0.25f, 1.25f, k), a.anchorMin.y);
        _wave.color = WithAlpha(moonLight, 0.3f * Mathf.Sin(k * Mathf.PI) * (_waveT <= 1f ? 1f : 0f));
    }

    private void UpdateMotes(float t, float dt)
    {
        if (_motes.Count == 0) return;
        Vector2 size = _root.rect.size;
        Vector2 target = Vector2.zero;
        if (_attract > 0f && _character != null)
        {
            // 캐릭터 가슴께 (배경 기준 비율)
            Vector3 local = _root.InverseTransformPoint(_character.TransformPoint(new Vector3(0f, _character.rect.height * 0.15f, 0f)));
            target = new Vector2(local.x / size.x + 0.5f, local.y / size.y + 0.5f);
        }
        _attract = Mathf.MoveTowards(_attract, 0f, dt * 0.35f);
        foreach (var m in _motes)
        {
            m.pos.y += m.speed * dt;
            m.pos.x += Mathf.Sin(t * 0.7f + m.phase) * m.sway * dt;
            if (_attract > 0f) m.pos = Vector2.Lerp(m.pos, target, dt * 2.2f * _attract);
            if (m.pos.y > horizonY + 0.25f) RespawnMote(m, false);
            m.rt.anchorMin = m.rt.anchorMax = m.pos;
            float twinkle = 0.5f + 0.5f * Mathf.Sin(t * 2.3f + m.phase * 3f);
            float fade = Mathf.Clamp01((horizonY + 0.25f - m.pos.y) / 0.12f);
            m.img.color = WithAlpha(moteColor, (0.6f + 0.4f * twinkle) * fade * (1f + _attract));
        }
    }

    private void RespawnMote(Mote m, bool anywhere)
    {
        m.pos = new Vector2(Random.Range(0.02f, 0.98f), anywhere ? Random.Range(fieldRangeY.x, horizonY + 0.2f) : Random.Range(fieldRangeY.x, fieldRangeY.y));
        m.speed = Random.Range(0.012f, 0.035f);
        m.phase = Random.Range(0f, 10f);
        m.sway = Random.Range(0.004f, 0.012f);
        m.size = Random.Range(16f, 40f);
        m.rt.sizeDelta = Vector2.one * m.size;
    }

    private void UpdateFrontPetals(float dt)
    {
        if (_petals.Count == 0) return;
        Vector2 size = ((RectTransform)transform).rect.size;
        foreach (var p in _petals)
        {
            p.t += dt;
            if (p.t < 0f) { p.img.color = Clear(p.img.color); continue; }
            if (p.dur <= 0f)
            {
                // 화면 앞을 크게 흐린 꽃잎이 가끔 빠르게 스쳐 간다 (초점 밖 느낌)
                p.dur = Random.Range(2.2f, 3.4f);
                p.y = Random.Range(0.15f, 0.85f);
                p.size = Random.Range(160f, 280f);
                p.spin = Random.Range(-90f, 90f);
                p.rt.sizeDelta = Vector2.one * p.size;
            }
            float k = p.t / p.dur;
            p.rt.anchorMin = p.rt.anchorMax = new Vector2(Mathf.Lerp(-0.1f, 1.1f, k), p.y - k * 0.25f);
            p.rt.localEulerAngles = new Vector3(0f, 0f, p.spin * p.t);
            p.img.color = WithAlpha(p.img.color, 0.35f * Mathf.Sin(Mathf.Clamp01(k) * Mathf.PI));
            if (k >= 1f) { p.t = -Random.Range(3f, 8f); p.dur = 0f; }
        }
    }

    private void UpdateRim()
    {
        if (_rim == null || _rimSource == null) return;
        _rim.texture = _rimSource.texture;
        _rim.uvRect = _rimSource.uvRect;
        var s = _rimSource.rectTransform; var r = _rim.rectTransform;
        r.anchorMin = s.anchorMin; r.anchorMax = s.anchorMax; r.pivot = s.pivot;
        r.sizeDelta = s.sizeDelta; r.localScale = s.localScale; r.localRotation = s.localRotation;
        r.anchoredPosition = s.anchoredPosition + rimOffset;
        _rim.enabled = _rimSource.enabled && _rimSource.texture != null;
        _rim.color = WithAlpha(rimColor, rimColor.a * _rimSource.color.a * (1f + _boost * 0.6f));
    }

    // 배경 천천히 확대 + 마우스 시차. 진입 연출이 카메라를 쥐고 있는 동안은 손대지 않고, 끝나면 서서히 섞어 든다
    private void UpdateMotion()
    {
        if (!depthMotion || _bg == null) return;
        bool introPlaying = _intro != null && _intro.IsPlaying;
        if (!_hasBase)
        {
            if (introPlaying) return;
            _bgPos = _bg.localPosition; _bgScale = _bg.localScale;
            if (_character != null) { _chPos = _character.localPosition; _chScale = _character.localScale; }
            _hasBase = true;
        }
        if (introPlaying) { _motionWeight = 0f; return; }

        float dt = Time.unscaledDeltaTime, t = Time.unscaledTime;
        _motionWeight = Mathf.MoveTowards(_motionWeight, 1f, dt / 1.5f);
        // 마우스가 없으면(패드·터치) 가운데에 둔다
        Vector2 m = Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(Screen.width, Screen.height) * 0.5f;
        Vector2 target = Screen.width > 0 ? new Vector2(m.x / Screen.width * 2f - 1f, m.y / Screen.height * 2f - 1f) : Vector2.zero;
        target = Vector2.ClampMagnitude(target, 1.4f);
        _mouse = Vector2.Lerp(_mouse, target, 1f - Mathf.Exp(-3f * dt));

        float zoom = 1f + kenBurnsZoom * (0.5f - 0.5f * Mathf.Cos(t * 2f * Mathf.PI / 24f)) * _motionWeight;
        Vector3 bgOffset = -(Vector3)Vector2.Scale(_mouse, parallaxBackground) * _motionWeight;
        _bg.localPosition = _bgPos + bgOffset;
        _bg.localScale = _bgScale * zoom;
        if (_character != null)
        {
            Vector3 chOffset = -(Vector3)Vector2.Scale(_mouse, parallaxCharacter) * _motionWeight;
            _character.localPosition = _chPos + chOffset;
            _character.localScale = _chScale * (1f + (zoom - 1f) * 0.5f);
        }
        if (_front != null) _front.localPosition = -(Vector3)Vector2.Scale(_mouse, parallaxForeground) * _motionWeight;
    }

    private void RestoreMotion()
    {
        if (!_hasBase) return;
        _bg.localPosition = _bgPos; _bg.localScale = _bgScale;
        if (_character != null) { _character.localPosition = _chPos; _character.localScale = _chScale; }
        if (_front != null) _front.localPosition = Vector3.zero;
        _motionWeight = 0f;
    }

    // ───────── 순간 연출 ─────────
    // 은총 선택: 꽃밭 가운데에서 빛 고리가 퍼지고 달무리가 잠깐 밝아진다
    public void PlayBlessingBurst()
    {
        if (_root == null || ring == null) return;
        var img = NewImage("BlessRing", _root, ring, additiveMaterial);
        Place(img.rectTransform, new Vector2(0.62f, 0.22f), new Vector2(420f, 150f), new Vector2(0.5f, 0.5f));
        img.color = WithAlpha(Color.white, 1f);
        img.rectTransform.localScale = Vector3.one * 0.3f;
        var flash = NewImage("BlessGlow", _root, softCircle, additiveMaterial);
        Place(flash.rectTransform, new Vector2(0.62f, 0.22f), new Vector2(1600f, 600f), new Vector2(0.5f, 0.5f));
        flash.color = WithAlpha(moonLight, 0f);
        DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
            .Append(img.rectTransform.DOScale(6f, 1.2f).SetEase(Ease.OutCubic))
            .Join(img.DOFade(0f, 1.2f).SetEase(Ease.InQuad))
            .Join(flash.DOFade(0.7f, 0.15f))
            .Insert(0.15f, flash.DOFade(0f, 0.9f))
            .OnComplete(() => { Destroy(img.gameObject); Destroy(flash.gameObject); });
        _boost = 1f;
        foreach (var m in _motes) m.speed *= 2.2f; // 입자가 한꺼번에 솟구친다 (다음 리스폰부터 원래 속도)
    }

    // 교감: 달빛이 밝아지고 빛 입자가 캐릭터 쪽으로 모인다
    public void PlayAffinityGlow()
    {
        _boost = 1f;
        _attract = 1f;
    }

    // ───────── 도우미 ─────────
    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite, Material mat)
    {
        var rt = NewRect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.material = mat;
        img.raycastTarget = false;
        return img;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static void Place(RectTransform rt, Vector2 anchor, Vector2 size, Vector2 pivot)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
    }

    private static Color WithAlpha(Color c, float a) => new(c.r, c.g, c.b, Mathf.Clamp01(a));
    private static Color Clear(Color c) => new(c.r, c.g, c.b, 0f);
}
