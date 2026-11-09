using UnityEngine;
using UnityEngine.UI;

// 거점 구역의 움직이는 환경 효과 (눈 · 깜빡이는 불 · 바람 소리). 내용은 BaseZoneData가 정하고, BaseCampView가 구역을 열 때 만든다.
// 효과는 모두 화면 시간(unscaled)으로 돈다. 기획: Docs/기획/거점.md
public static class BaseZoneFx
{
    // 동료 뒤에 그리는 눈 (창밖 · 뚫린 셔터 너머)
    public static void BuildBack(RectTransform layer, BaseZoneData z)
    {
        foreach (var snow in z.snow)
            if (!snow.front) BuildSnow(layer, snow);
    }

    // 동료 앞에 그리는 눈과 조명
    public static void BuildFront(RectTransform layer, BaseZoneData z)
    {
        foreach (var snow in z.snow)
            if (snow.front) BuildSnow(layer, snow);
        if (z.lights.Count == 0) return;

        var go = new GameObject("Lights", typeof(RectTransform), typeof(BaseLightFx));
        go.layer = layer.gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(layer, false);
        Frame(rt, Vector2.zero);
        go.GetComponent<BaseLightFx>().Setup(rt, z);
    }

    public static BaseSnowGraphic BuildSnow(RectTransform layer, BaseZoneData.SnowFx fx)
    {
        var parent = layer;
        var offset = Vector2.zero;
        if (fx.mask != null)
        {
            // 눈은 가림 그림의 불투명한 곳에서만 보인다 (창틀 · 철판 · 주전자 뒤로 숨게)
            var maskGo = new GameObject("SnowMask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            maskGo.layer = layer.gameObject.layer;
            parent = (RectTransform)maskGo.transform;
            parent.SetParent(layer, false);
            parent.anchorMin = parent.anchorMax = parent.pivot = new Vector2(0f, 1f);
            parent.anchoredPosition = new Vector2(fx.maskArea.x, -fx.maskArea.y);
            parent.sizeDelta = fx.maskArea.size;
            var image = maskGo.GetComponent<Image>();
            image.sprite = fx.mask;
            image.raycastTarget = false;
            maskGo.GetComponent<Mask>().showMaskGraphic = false;
            offset = new Vector2(-fx.maskArea.x, fx.maskArea.y);
        }

        var go = new GameObject("Snow", typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = layer.gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        Frame(rt, offset);
        var graphic = go.AddComponent<BaseSnowGraphic>();
        graphic.raycastTarget = false;
        graphic.Setup(fx, fx.sprite != null ? fx.sprite : Resources.Load<Sprite>("BaseCamp/Shadow_Soft"));
        return graphic;
    }

    private static void Frame(RectTransform rt, Vector2 position)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(1920f, 1080f);
    }
}

// 눈송이 묶음을 메시 하나로 그린다. 좌표는 1920×1080 · 왼쪽 위 원점, 속도의 y는 아래쪽이 +
public class BaseSnowGraphic : MaskableGraphic
{
    private struct Flake
    {
        public Vector2 position, velocity;
        public float age, life, size, phase;
    }

    private BaseZoneData.SnowFx _fx;
    private Flake[] _flakes;
    private Sprite _sprite;

    public override Texture mainTexture => _sprite != null ? _sprite.texture : s_WhiteTexture;

    public void Setup(BaseZoneData.SnowFx fx, Sprite sprite)
    {
        _fx = fx;
        _sprite = sprite;
        color = Color.white;
        _flakes = new Flake[Mathf.Max(0, fx.count)];
        for (int i = 0; i < _flakes.Length; i++)
        {
            Spawn(ref _flakes[i]);
            // 구역을 열자마자 이미 내리는 중이도록 흐른 만큼 옮겨 둔다
            float advance = Random.value * _flakes[i].life;
            _flakes[i].age = advance;
            _flakes[i].position += _flakes[i].velocity * advance;
        }
        SetVerticesDirty();
    }

    private void Spawn(ref Flake f)
    {
        var a = _fx.spawn;
        f.position = new Vector2(a.x + Random.value * a.width, a.y + Random.value * a.height);
        f.velocity = _fx.velocity + new Vector2(Random.Range(-_fx.jitter.x, _fx.jitter.x), Random.Range(-_fx.jitter.y, _fx.jitter.y));
        f.age = 0f;
        f.life = Mathf.Max(0.05f, Random.Range(_fx.life.x, _fx.life.y));
        f.size = Random.Range(_fx.size.x, _fx.size.y);
        f.phase = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        if (_flakes == null) return;
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        for (int i = 0; i < _flakes.Length; i++)
        {
            ref var f = ref _flakes[i];
            f.age += dt;
            if (f.age >= f.life) Spawn(ref f);
            else f.position += f.velocity * dt;
        }
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (_flakes == null) return;
        var uv = _sprite != null ? UnityEngine.Sprites.DataUtility.GetOuterUV(_sprite) : new Vector4(0f, 0f, 1f, 1f);
        float time = Time.unscaledTime;

        foreach (var f in _flakes)
        {
            float t = f.age / f.life;
            // 태어날 때 · 사라질 때 부드럽게
            float alpha = _fx.alpha * Mathf.Clamp01(t / 0.12f) * Mathf.Clamp01((1f - t) / 0.25f);
            if (alpha <= 0.004f) continue;

            float size = f.size * (1f + _fx.grow * t);
            var along = f.velocity.sqrMagnitude > 1f ? f.velocity.normalized : Vector2.down;
            var across = new Vector2(-along.y, along.x);
            var center = f.position + across * (Mathf.Sin(time * 2.1f + f.phase) * _fx.wobble);
            along *= size * Mathf.Max(1f, _fx.stretch);
            across *= size;

            var c = color;
            c.a = alpha;
            int start = vh.currentVertCount;
            vh.AddVert(Local(center - along - across), c, new Vector2(uv.x, uv.y));
            vh.AddVert(Local(center + along - across), c, new Vector2(uv.x, uv.w));
            vh.AddVert(Local(center + along + across), c, new Vector2(uv.z, uv.w));
            vh.AddVert(Local(center - along + across), c, new Vector2(uv.z, uv.y));
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start + 2, start + 3, start);
        }
    }

    private static Vector3 Local(Vector2 p) => new(p.x, -p.y, 0f);
}

// 등 · 난로 같은 불빛. 빛 번짐(가산)과, 불이 약해질 때 배경에 그려진 불빛을 눌러 주는 어두운 판을 함께 움직인다
public class BaseLightFx : MonoBehaviour
{
    // 고장 난 등이 깜빡이는 빠르기의 하한. 초당 3번을 넘겨 번쩍이지 않게 한다
    private const float MinToggleSeconds = 0.17f;

    private class Item
    {
        public BaseZoneData.LightFx fx;
        public Image glow, dark;
        public float seed, nextBurst, burstEnd, nextToggle;
        public bool lit = true;
    }

    private Item[] _items;
    private Image _room;
    private float _roomDim;

    public void Setup(RectTransform root, BaseZoneData z)
    {
        var soft = z.lightSprite != null ? z.lightSprite : Resources.Load<Sprite>("BaseCamp/Shadow_Soft");
        var additive = Resources.Load<Material>("BaseCamp/Fx_Additive");
        _roomDim = z.faultyRoomDim;
        if (_roomDim > 0f)
        {
            _room = New(root, "RoomDim", null, null);
            var rt = _room.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(1920f, 1080f);
        }

        _items = new Item[z.lights.Count];
        for (int i = 0; i < _items.Length; i++)
        {
            var fx = z.lights[i];
            var item = new Item { fx = fx, seed = Random.value * 100f, nextBurst = Time.unscaledTime + Random.Range(1f, 3f) };
            if (fx.darken > 0f) item.dark = Spot(root, "Dark", soft, null, fx);
            item.glow = Spot(root, "Glow", soft, additive, fx);
            _items[i] = item;
        }
        Update();
    }

    private Image Spot(RectTransform root, string name, Sprite sprite, Material material, BaseZoneData.LightFx fx)
    {
        var image = New(root, name, sprite, material);
        var rt = image.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(fx.center.x, -fx.center.y);
        rt.sizeDelta = fx.size;
        return image;
    }

    private Image New(RectTransform root, string name, Sprite sprite, Material material)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = gameObject.layer;
        go.transform.SetParent(root, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.material = material;
        image.raycastTarget = false;
        return image;
    }

    private void Update()
    {
        if (_items == null) return;
        float now = Time.unscaledTime;
        float darkest = 1f;
        foreach (var item in _items)
        {
            var fx = item.fx;
            float level;
            switch (fx.mode)
            {
                case BaseZoneData.LightFxMode.Breathe:
                    level = Mathf.Lerp(fx.min, 1f, 0.5f + 0.5f * Mathf.Sin(now * fx.speed * Mathf.PI * 2f + item.seed));
                    break;
                case BaseZoneData.LightFxMode.Flame:
                    // 느린 일렁임 위에 빠른 떨림
                    float wave = Mathf.PerlinNoise(now * fx.speed * 1.3f, item.seed) * 0.65f + Mathf.PerlinNoise(now * fx.speed * 6f, item.seed + 9f) * 0.35f;
                    level = Mathf.Lerp(fx.min, 1f, Mathf.Clamp01(wave));
                    break;
                default:
                    level = Faulty(item, now);
                    darkest = Mathf.Min(darkest, level);
                    break;
            }

            var c = fx.color;
            c.a *= level;
            item.glow.color = c;
            if (item.dark != null) item.dark.color = new Color(0f, 0f, 0f, fx.darken * (1f - level));
        }
        if (_room != null) _room.color = new Color(0f, 0f, 0f, _roomDim * (1f - darkest));
    }

    // 평소엔 켜져 있다가 가끔 몇 번 끊긴다
    private static float Faulty(Item item, float now)
    {
        if (now >= item.burstEnd && now >= item.nextBurst)
        {
            item.burstEnd = now + Random.Range(0.45f, 1.0f) / Mathf.Max(0.1f, item.fx.speed);
            item.nextBurst = item.burstEnd + Random.Range(3.5f, 8f) / Mathf.Max(0.1f, item.fx.speed);
            item.nextToggle = now;
            item.lit = true;
        }
        if (now >= item.burstEnd)
        {
            item.lit = true;
            return 1f;
        }
        if (now >= item.nextToggle)
        {
            item.lit = !item.lit;
            item.nextToggle = now + Random.Range(MinToggleSeconds, 0.3f);
        }
        return item.lit ? 1f : item.fx.min;
    }
}

// 구역 환경음. 거점 화면이 꺼져도 페이드가 끝날 때까지 돌도록 화면 밖 오브젝트에 붙는다. 같은 소리면 이어서 틀고 음량 · 먹먹함만 옮겨 간다
[RequireComponent(typeof(AudioSource), typeof(AudioLowPassFilter))]
public class BaseAmbiencePlayer : MonoBehaviour
{
    private const float FadeSeconds = 0.8f;

    private AudioSource _source;
    private AudioLowPassFilter _filter;
    private AudioClip _next;
    private float _volume, _level, _cutoff = 22000f;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _filter = GetComponent<AudioLowPassFilter>();
        _source.playOnAwake = false;
        _source.loop = true;
        _source.spatialBlend = 0f;
    }

    public void Play(AudioClip clip, float volume, float cutoff)
    {
        _next = clip;
        if (clip != null)
        {
            _volume = volume;
            _cutoff = cutoff;
        }
    }

    private void Update()
    {
        float step = Time.unscaledDeltaTime / FadeSeconds;
        bool swap = _source.clip != _next;
        bool leaving = swap || _next == null || SceneTransition.IsTransitioning;
        _level = Mathf.MoveTowards(_level, leaving ? 0f : 1f, step);
        if (swap && _level <= 0f)
        {
            _source.Stop();
            _source.clip = _next;
            if (_next != null) _source.Play();
        }
        _source.volume = _volume * _level * UISfx.Volume;
        // 소리가 작아진 뒤에는 바로, 들리는 동안에는 부드럽게 맞춘다
        _filter.cutoffFrequency = _level <= 0f ? _cutoff : Mathf.Lerp(_filter.cutoffFrequency, _cutoff, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 4f));
    }
}
