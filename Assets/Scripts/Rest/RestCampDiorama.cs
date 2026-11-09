using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 휴식 노드의 야영지. 배경 · 텐트 · 모닥불 · 동료 자리를 조립하고 불 · 불씨 · 연기 · 불빛을 돌린다.
// 좌표는 1920×1080 · 왼쪽 위 원점이고, 앞뒤는 발밑 y로 정렬한다. 그림은 Resources/RestCamp에 있다.
public sealed class RestCampDiorama : MonoBehaviour
{
    private const float DesignWidth = 1920f, DesignHeight = 1080f;
    private const float FireFeetY = 740f;
    private const float AmbienceVolume = 0.45f;
    private static readonly Vector2 FlameCenter = new(966f, 632f); // 모닥불 그림의 장작 불씨 중심

    private static readonly (string sprite, Vector2 feet, float height, bool mirror)[] BackProps =
    {
        ("Prop_Tent", new(330f, 400f), 310f, false),
        ("Prop_Tarp", new(1600f, 410f), 300f, true),
        ("Prop_Lantern", new(640f, 400f), 300f, false),
    };

    // 합류 순서대로 채우는 자리. 그림은 불을 오른쪽에 두고 그렸으므로 불 오른쪽 자리는 뒤집는다
    private static readonly (Vector2 feet, bool mirror)[] Slots =
    {
        (new(1340f, 930f), true),
        (new(240f, 780f), false),
        (new(1660f, 790f), true),
        (new(590f, 590f), false),
    };

    private static readonly Vector2 ProtagonistFeet = new(590f, 930f);

    // 동료가 손에 든 것에서 나오는 작은 효과. 위치는 포즈 그림 안의 비율 (왼쪽 위 원점)
    private enum HandFx { Steam, Sparks, Glow }

    private static readonly Dictionary<string, (HandFx kind, Vector2 point)> HandEffects = new()
    {
        { "Cp_01", (HandFx.Sparks, new Vector2(0.80f, 0.50f)) },
        { "Cp_02", (HandFx.Glow, new Vector2(0.68f, 0.41f)) },
        { "Cp_03", (HandFx.Glow, new Vector2(0.92f, 0.29f)) },
        { "Cp_Event_02", (HandFx.Steam, new Vector2(0.59f, 0.55f)) },
    };

    private RectTransform _camp;
    private RectTransform _shadows;
    private RectTransform _backFx;
    private RectTransform _frontFx;
    private RectTransform _handFx;
    private RectTransform _fire;
    private readonly List<(RectTransform rect, float y)> _depth = new();
    private readonly List<RestCampStation> _stations = new();
    private readonly List<Image> _stationShadows = new();
    private readonly List<RestFlameGraphic> _flames = new();
    private GameObject _actionPanel;
    private GameObject _menuBackdrop;
    private TextMeshProUGUI _fireLabel;
    private Image _fireTag;
    private Image _protagonist;
    private BaseZoneData _groundLights;
    private BaseZoneData _fireLights;
    private BaseZoneData _handLights;
    private BaseAmbiencePlayer _ambience;
    private AudioClip _ambienceClip;
    private Sprite _soft;
    private Sprite _glow;
    private Material _additive;
    private bool _fireHovered;
    private bool _built;

    public void Build(Transform camp, Image background, IReadOnlyList<RestCampStation> stations, Button fireButton, GameObject actionPanel)
    {
        if (_built) return;
        _built = true;
        _camp = (RectTransform)camp;
        _actionPanel = actionPanel;
        _soft = Resources.Load<Sprite>("BaseCamp/Shadow_Soft");
        _glow = Resources.Load<Sprite>("RestCamp/Fx_Glow");
        _additive = Resources.Load<Material>("BaseCamp/Fx_Additive");
        _ambienceClip = Resources.Load<AudioClip>("Audio/Ambience/amb_stove_quarters");

        if (background != null) background.sprite = Resources.Load<Sprite>("RestCamp/BG_Camp");

        foreach (RestCampStation station in stations)
            if (station != null) _stations.Add(station);

        _shadows = Stage("Shadows");
        _backFx = Stage("BackFx");
        _frontFx = Stage("FrontFx");
        _handFx = Stage("HandFx");

        foreach (var prop in BackProps)
        {
            Image image = Sprite2D(_camp, prop.sprite, prop.feet, prop.height, prop.mirror);
            AddShadow(prop.feet, image.rectTransform.sizeDelta.x * 0.9f, 46f, 0.45f);
            _depth.Add((image.rectTransform, prop.feet.y));
        }

        BuildFire();
        BuildProtagonist();
        BuildFx();
        SetupFireButton(fireButton);
        BuildMenuBackdrop();

        foreach (RestCampStation station in _stations)
        {
            Image shadow = NewImage(_shadows, "StationShadow", _soft, null);
            shadow.gameObject.SetActive(false);
            _stationShadows.Add(shadow);
        }
    }

    // RestView가 동료를 자리에 묶은 뒤 부른다
    public void ArrangeStations()
    {
        if (!_built) return;

        ClearHandFx();
        _handLights = NewLights();
        _depth.RemoveAll(entry => entry.rect.TryGetComponent<RestCampStation>(out _));

        for (int i = 0; i < _stations.Count; i++)
        {
            RestCampStation station = _stations[i];
            Image shadow = _stationShadows[i];
            bool used = station.State != null && i < Slots.Length;
            shadow.gameObject.SetActive(used);
            if (!used) continue;

            var slot = Slots[i];
            float height = HeightAt(slot.feet.y) * station.State.charData.campHeightScale;
            bool floats = station.State.charData.floatAmplitude > 0f;
            station.Place(slot.feet, height, slot.mirror, floats);
            _depth.Add(((RectTransform)station.transform, slot.feet.y));

            float width = station.Size.x;
            PlaceShadow(shadow, slot.feet, width * (floats ? 0.55f : 0.85f), floats ? 34f : 52f, floats ? 0.32f : 0.55f);
            AddHandFx(station, slot.feet, slot.mirror);
        }

        if (_handLights.lights.Count > 0) BaseZoneFx.BuildFront(_handFx, _handLights);
        SortDepth();
    }

    private void OnEnable()
    {
        if (_ambienceClip == null) _ambienceClip = Resources.Load<AudioClip>("Audio/Ambience/amb_stove_quarters");
        if (_ambience == null)
        {
            var go = new GameObject("RestAmbience", typeof(AudioSource), typeof(AudioLowPassFilter), typeof(BaseAmbiencePlayer));
            go.transform.SetParent(transform.parent, false); // 화면이 꺼져도 페이드가 끝나게 화면 밖에
            _ambience = go.GetComponent<BaseAmbiencePlayer>();
        }
        _ambience.Play(_ambienceClip, AmbienceVolume, 22000f);
    }

    private void OnDisable()
    {
        if (_ambience != null) _ambience.Play(null, 0f, 22000f);
    }

    private void OnDestroy()
    {
        if (_groundLights != null) Destroy(_groundLights);
        if (_fireLights != null) Destroy(_fireLights);
        if (_handLights != null) Destroy(_handLights);
    }

    private void Update()
    {
        if (!_built) return;

        float now = Time.unscaledTime;
        // 동료 얼굴에 어른거리는 불빛
        float flicker = Mathf.Clamp01(Mathf.PerlinNoise(now * 1.5f, 3.1f) * 0.7f + Mathf.PerlinNoise(now * 7f, 8.2f) * 0.3f);
        foreach (RestCampStation station in _stations)
            if (station.State != null) station.Tick(flicker, now);

        if (_protagonist != null)
        {
            _protagonist.color = RestCampStation.FireTint(flicker);
            float breath = Mathf.Sin(now * 1.6f) * RestCampStation.BreathAmount;
            _protagonist.rectTransform.localScale = new Vector3(1f - breath * 0.4f, 1f + breath, 1f);
        }

        if (_menuBackdrop != null && _actionPanel != null && _menuBackdrop.activeSelf != _actionPanel.activeSelf)
            _menuBackdrop.SetActive(_actionPanel.activeSelf);

        float boost = _fireHovered ? 1.22f : 1f;
        foreach (RestFlameGraphic flame in _flames) flame.Boost = boost;

        if (_fireTag != null) _fireTag.color = _fireHovered ? Color.white : new Color(0.86f, 0.86f, 0.86f, 1f);
    }

    // ── 조립 ──

    private void BuildFire()
    {
        _fire = Stage("Fire");
        Vector2 ringFeet = new(960f, FireFeetY);
        AddShadow(ringFeet, 360f, 60f, 0.4f);
        Image ring = Sprite2D(_fire, "Prop_FireRing", ringFeet, 190f, false);
        ring.raycastTarget = false;

        Sprite tongue = Resources.Load<Sprite>("RestCamp/Fx_FlameTongue");
        _flames.Add(NewFlame("FlameBody", tongue, 30, false));
        _flames.Add(NewFlame("FlameCore", tongue, 16, true));

        Sprite2D(_fire, "Prop_Tripod", new Vector2(965f, 690f), 390f, false); // 냄비 바닥이 장작 위에 뜨는 높이
        _depth.Add((_fire, FireFeetY));
    }

    private RestFlameGraphic NewFlame(string name, Sprite sprite, int count, bool core)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = gameObject.layer;
        var rect = (RectTransform)go.transform;
        rect.SetParent(_fire, false);
        Frame(rect);
        var flame = go.AddComponent<RestFlameGraphic>();
        flame.raycastTarget = false;
        if (core) flame.material = _additive;
        flame.Setup(sprite, FlameCenter, core ? 40f : 62f, count, core);
        return flame;
    }

    private void BuildProtagonist()
    {
        _protagonist = Sprite2D(_camp, "Pose_Protagonist", ProtagonistFeet, HeightAt(ProtagonistFeet.y), false);
        AddShadow(ProtagonistFeet + new Vector2(-30f, -24f), _protagonist.rectTransform.sizeDelta.x * 0.95f, 54f, 0.55f);
        _depth.Add((_protagonist.rectTransform, ProtagonistFeet.y));
    }

    private void BuildFx()
    {
        Sparkles(_backFx, new Rect(0f, 60f, 1920f, 620f), 34, new(10f, -6f), new(14f, 10f), new(1.5f, 3.2f), new(4f, 8f), 0.5f, 22f, new Color(0.72f, 0.86f, 1f), true);
        Sparkles(_backFx, new Rect(936f, 440f, 60f, 30f), 14, new(14f, -55f), new(10f, 12f), new(22f, 44f), new(3f, 5.5f), 0.10f, 16f, new Color(0.62f, 0.60f, 0.62f), false, 1.6f);

        Sparkles(_frontFx, new Rect(945f, 495f, 40f, 12f), 8, new(6f, -45f), new(6f, 8f), new(10f, 20f), new(1.4f, 2.6f), 0.16f, 8f, Color.white, false, 1.2f);
        Sparkles(_frontFx, new Rect(886f, 585f, 160f, 60f), 28, new(8f, -115f), new(32f, 50f), new(1.5f, 3.6f), new(1f, 2.4f), 0.95f, 14f, new Color(1f, 0.58f, 0.16f), true);

        // 땅을 비추는 불빛은 동료 뒤에, 불 바로 옆 번짐과 등불은 앞에 그린다
        _groundLights = NewLights();
        _groundLights.lights.Add(Light(BaseZoneData.LightFxMode.Flame, new(966f, 640f), new(1750f, 1150f), new Color(1f, 0.62f, 0.27f, 0.26f), 0.68f, 0.9f));
        BaseZoneFx.BuildFront(_backFx, _groundLights);

        _fireLights = NewLights();
        _fireLights.lights.Add(Light(BaseZoneData.LightFxMode.Flame, new(966f, 600f), new(520f, 440f), new Color(1f, 0.66f, 0.26f, 0.42f), 0.5f, 1.3f));
        _fireLights.lights.Add(Light(BaseZoneData.LightFxMode.Flame, new(621f, 195f), new(250f, 250f), new Color(1f, 0.70f, 0.30f, 0.40f), 0.55f, 0.5f));
        _fireLights.lights.Add(Light(BaseZoneData.LightFxMode.Breathe, new(322f, 300f), new(150f, 130f), new Color(1f, 0.68f, 0.3f, 0.34f), 0.65f, 0.25f));
        BaseZoneFx.BuildFront(_frontFx, _fireLights);
    }

    private BaseZoneData NewLights()
    {
        var lights = ScriptableObject.CreateInstance<BaseZoneData>();
        lights.lightSprite = _glow;
        return lights;
    }

    private void SetupFireButton(Button button)
    {
        if (button == null) return;

        var rect = (RectTransform)button.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -50f);
        rect.sizeDelta = new Vector2(360f, 380f);
        if (button.TryGetComponent(out Image image)) image.color = new Color(1f, 1f, 1f, 0f);
        button.gameObject.AddComponent<RestFireHover>().Changed = hovered => _fireHovered = hovered;

        _fireLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (_fireLabel != null)
        {
            _fireTag = NewImage(rect, "FireTag", RestCampMenu.Load("Tag_Fire"), null);
            _fireTag.rectTransform.sizeDelta = new Vector2(200f, 68f);
            _fireTag.rectTransform.anchoredPosition = new Vector2(5f, 334f);
            _fireTag.transform.SetAsFirstSibling();

            var subtitle = Instantiate(_fireLabel, _fireLabel.transform.parent);
            subtitle.name = "Subtitle";
            subtitle.rectTransform.sizeDelta = new Vector2(165f, 22f);
            subtitle.rectTransform.anchoredPosition = new Vector2(5.5f, 316f);
            RestCampMenu.Style(subtitle, "휴식 · 정비", 15f, 4f, FontStyles.Bold, TextAlignmentOptions.Center, RestCampMenu.Hex(0xD1D0CC));

            _fireLabel.rectTransform.sizeDelta = new Vector2(166f, 29f);
            _fireLabel.rectTransform.anchoredPosition = new Vector2(5f, 349.5f);
            RestCampMenu.Style(_fireLabel, "모닥불", 25f, 0f, FontStyles.Bold, TextAlignmentOptions.Center, RestCampMenu.Hex(0xD1D0CC));
        }
        _depth.Add((rect, FireFeetY + 0.5f));
    }

    // 메뉴가 열리면 뒤를 어둡게 하고, 메뉴 밖을 누르면 닫는다
    private void BuildMenuBackdrop()
    {
        if (_actionPanel == null) return;

        var go = new GameObject("MenuBackdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.layer = gameObject.layer;
        var rect = (RectTransform)go.transform;
        rect.SetParent(_actionPanel.transform.parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.SetSiblingIndex(_actionPanel.transform.GetSiblingIndex());
        go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.30f);
        var button = go.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => _actionPanel.SetActive(false));
        go.SetActive(false);
        _menuBackdrop = go;
    }

    private void AddHandFx(RestCampStation station, Vector2 feet, bool mirror)
    {
        if (!HandEffects.TryGetValue(station.State.charID, out var effect)) return;

        Vector2 size = station.Size;
        float x = feet.x + (effect.point.x - 0.5f) * size.x * (mirror ? -1f : 1f);
        float y = feet.y - (1f - effect.point.y) * size.y;

        switch (effect.kind)
        {
            case HandFx.Steam:
                Sparkles(_handFx, new Rect(x - 8f, y - 10f, 16f, 8f), 9, new(4f, -34f), new(6f, 8f), new(4f, 8f), new(1f, 2f), 0.22f, 6f, Color.white, false, 1.2f);
                break;
            case HandFx.Sparks:
                float direction = mirror ? -1f : 1f;
                Sparkles(_handFx, new Rect(x - 4f, y - 4f, 8f, 8f), 6, new(40f * direction, -30f), new(60f, 40f), new(1f, 2f), new(0.15f, 0.5f), 1f, 0f, new Color(1f, 0.72f, 0.3f), true, 0f, 3f);
                break;
            case HandFx.Glow:
                _handLights.lights.Add(Light(BaseZoneData.LightFxMode.Breathe, new(x, y), new(130f, 130f), new Color(0.75f, 0.95f, 0.75f, 0.30f), 0.45f, 0.6f));
                Sparkles(_handFx, new Rect(x - 12f, y - 12f, 24f, 24f), 5, new(0f, -22f), new(10f, 6f), new(1.2f, 2.4f), new(0.6f, 1.3f), 0.8f, 4f, new Color(0.8f, 1f, 0.8f), true);
                break;
        }
    }

    private void ClearHandFx()
    {
        for (int i = _handFx.childCount - 1; i >= 0; i--) Destroy(_handFx.GetChild(i).gameObject);
        if (_handLights != null) Destroy(_handLights);
        _handLights = null;
    }

    private void SortDepth()
    {
        _depth.Sort((a, b) => a.y.CompareTo(b.y));
        int index = 0;
        _shadows.SetSiblingIndex(index++);
        _backFx.SetSiblingIndex(index++);
        foreach (var entry in _depth) entry.rect.SetSiblingIndex(index++);
        _frontFx.SetSiblingIndex(index++);
        _handFx.SetSiblingIndex(index);
    }

    // ── 도우미 ──

    private static float HeightAt(float feetY) => 300f + 0.294f * (feetY - 590f);

    private RectTransform Stage(string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = gameObject.layer;
        var rect = (RectTransform)go.transform;
        rect.SetParent(_camp, false);
        Frame(rect);
        return rect;
    }

    // 넓어진 화면에서도 1920×1080 무대가 가운데에 오도록 가운데 기준으로 잡는다
    private static void Frame(RectTransform rect)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(DesignWidth, DesignHeight);
    }

    private Image Sprite2D(RectTransform parent, string spriteName, Vector2 feet, float height, bool mirror)
    {
        Sprite sprite = Resources.Load<Sprite>("RestCamp/" + spriteName);
        Image image = NewImage(parent, spriteName, sprite, null);
        RectTransform rect = image.rectTransform;
        float width = sprite != null ? height * sprite.rect.width / sprite.rect.height : height;
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = StagePoint(feet);
        rect.sizeDelta = new Vector2(width, height);
        if (mirror) rect.localScale = new Vector3(-1f, 1f, 1f);
        return image;
    }

    private void AddShadow(Vector2 feet, float width, float height, float alpha)
    {
        PlaceShadow(NewImage(_shadows, "Shadow", _soft, null), feet, width, height, alpha);
    }

    private static void PlaceShadow(Image image, Vector2 feet, float width, float height, float alpha)
    {
        RectTransform rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = StagePoint(feet) + new Vector2(0f, 4f);
        rect.sizeDelta = new Vector2(width, height);
        image.color = new Color(0f, 0f, 0f, alpha);
    }

    private static Vector2 StagePoint(Vector2 p) => new(p.x - DesignWidth * 0.5f, DesignHeight * 0.5f - p.y);

    private Image NewImage(RectTransform parent, string name, Sprite sprite, Material material)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = gameObject.layer;
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.material = material;
        image.raycastTarget = false;
        return image;
    }

    private void Sparkles(RectTransform layer, Rect spawn, int count, Vector2 velocity, Vector2 jitter, Vector2 size, Vector2 life, float alpha, float wobble, Color color, bool additive, float grow = 0f, float stretch = 1f)
    {
        var fx = new BaseZoneData.SnowFx
        {
            sprite = _glow,
            spawn = spawn,
            count = count,
            velocity = velocity,
            jitter = jitter,
            size = size,
            life = life,
            alpha = alpha,
            wobble = wobble,
            grow = grow,
            stretch = stretch,
        };
        BaseSnowGraphic graphic = BaseZoneFx.BuildSnow(layer, fx);
        graphic.color = color;
        if (additive) graphic.material = _additive;
    }

    private static BaseZoneData.LightFx Light(BaseZoneData.LightFxMode mode, Vector2 center, Vector2 size, Color color, float min, float speed) => new()
    {
        mode = mode,
        center = center,
        size = size,
        color = color,
        min = min,
        speed = speed,
    };
}

// 모닥불 위에 마우스가 있는지 알려 준다
public class RestFireHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public System.Action<bool> Changed;

    public void OnPointerEnter(PointerEventData eventData) => Changed?.Invoke(true);
    public void OnPointerExit(PointerEventData eventData) => Changed?.Invoke(false);
    private void OnDisable() => Changed?.Invoke(false);
}

// 불꽃 혀 묶음을 메시 하나로 그린다. 좌표는 1920×1080 · 왼쪽 위 원점
public class RestFlameGraphic : MaskableGraphic
{
    private struct Tongue
    {
        public Vector2 position, velocity;
        public float age, life, size, phase;
    }

    private Sprite _sprite;
    private Tongue[] _tongues;
    private Vector2 _center;
    private float _halfWidth;
    private bool _core;

    public float Boost { get; set; } = 1f;

    public override Texture mainTexture => _sprite != null ? _sprite.texture : s_WhiteTexture;

    public void Setup(Sprite sprite, Vector2 center, float halfWidth, int count, bool core)
    {
        _sprite = sprite;
        _center = center;
        _halfWidth = halfWidth;
        _core = core;
        color = Color.white;
        _tongues = new Tongue[count];
        for (int i = 0; i < count; i++)
        {
            Spawn(ref _tongues[i]);
            float advance = Random.value * _tongues[i].life;
            _tongues[i].age = advance;
            _tongues[i].position += _tongues[i].velocity * advance;
        }
        SetVerticesDirty();
    }

    private void Spawn(ref Tongue t)
    {
        // 가운데에 몰리도록 두 난수를 평균 낸다
        float offset = Random.value + Random.value - 1f;
        float edge = Mathf.Abs(offset);
        t.position = _center + new Vector2(offset * _halfWidth, Random.Range(-6f, 10f));
        t.velocity = new Vector2(-offset * 22f, -Random.Range(90f, 170f) * (1f - edge * 0.5f));
        t.age = 0f;
        t.life = Random.Range(0.5f, 0.9f) * (1f - edge * 0.35f);
        t.size = Random.Range(22f, 42f) * (1f - edge * 0.4f) * (_core ? 0.62f : 1.2f);
        t.phase = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        if (_tongues == null) return;
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        for (int i = 0; i < _tongues.Length; i++)
        {
            ref Tongue t = ref _tongues[i];
            t.age += dt;
            if (t.age >= t.life) Spawn(ref t);
            else t.position += t.velocity * dt * Boost;
        }
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (_tongues == null) return;

        Vector4 uv = _sprite != null ? UnityEngine.Sprites.DataUtility.GetOuterUV(_sprite) : new Vector4(0f, 0f, 1f, 1f);
        float now = Time.unscaledTime;
        float wind = (Mathf.PerlinNoise(now * 0.7f, 1.7f) - 0.5f) * 90f;

        foreach (Tongue t in _tongues)
        {
            float k = t.age / t.life;
            float alpha = Mathf.Clamp01(k / 0.08f) * Mathf.Clamp01((1f - k) / 0.5f);
            if (alpha <= 0.004f) continue;

            float sway = wind * k + Mathf.Sin(now * 7f + t.phase) * 7f * k;
            // 좌표의 y가 아래쪽이 +라서 화면 위쪽은 -y다
            Vector2 up = new Vector2(sway * 0.004f, -1f).normalized;
            Vector2 right = new(-up.y, up.x);
            float size = t.size * Boost;
            Vector2 halfUp = up * (size * 1.9f * (1f - 0.25f * k));
            Vector2 halfRight = right * (size * 0.72f * (1f - 0.55f * k));
            // 혀의 아랫단이 장작에 붙도록 위치를 아랫단으로 쓴다
            Vector2 center = t.position + new Vector2(sway, 0f) + halfUp;

            Color c = Ramp(k);
            c.a *= alpha;
            int start = vh.currentVertCount;
            vh.AddVert(Local(center - halfUp - halfRight), c, new Vector2(uv.x, uv.y));
            vh.AddVert(Local(center + halfUp - halfRight), c, new Vector2(uv.x, uv.w));
            vh.AddVert(Local(center + halfUp + halfRight), c, new Vector2(uv.z, uv.w));
            vh.AddVert(Local(center - halfUp + halfRight), c, new Vector2(uv.z, uv.y));
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start + 2, start + 3, start);
        }
    }

    private Color Ramp(float k)
    {
        if (_core) return Color.Lerp(new Color(1f, 0.92f, 0.62f, 0.85f), new Color(1f, 0.55f, 0.14f, 0.5f), k);
        return k < 0.45f
            ? Color.Lerp(new Color(1f, 0.72f, 0.24f, 0.95f), new Color(1f, 0.42f, 0.08f, 0.9f), k / 0.45f)
            : Color.Lerp(new Color(1f, 0.42f, 0.08f, 0.9f), new Color(0.72f, 0.12f, 0.03f, 0.6f), (k - 0.45f) / 0.55f);
    }

    // 1920×1080 왼쪽 위 원점 → 가운데 기준 프레임의 로컬 좌표
    private static Vector3 Local(Vector2 p) => new(p.x - 960f, 540f - p.y, 0f);
}
