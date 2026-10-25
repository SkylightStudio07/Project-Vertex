using UnityEngine;
using UnityEngine.UI;

// 전투 유닛(주인공 · 동료 · 적) 그림에 같은 처리를 입힌다. 출처가 다른 그림들의 화풍 차이를 줄이는 용도.
//  - 머티리얼: UI/BattleUnit (외곽선 · 림라이트 · 색 보정). 아군/적 값은 Resources/BattleUnit/BattleUnit_Ally · _Enemy
//  - 발밑 그림자: 보이는 그림(스프라이트 메시) 아래 끝에 같은 모양 · 같은 진하기의 타원
// PartyView · EnemyView가 그림을 정할 때 Apply로 붙인다.
[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
public class BattleUnitLook : MonoBehaviour
{
    public enum Side { Ally, Enemy }

    [SerializeField] private Side side;
    [Header("발밑 그림자")]
    [SerializeField] private bool groundShadow = true;
    [Tooltip("그림자 폭 = max(보이는 폭, 보이는 키 × 0.55) × 이 값. 다리를 모은 좁은 그림도 바닥 자국이 너무 작지 않게")]
    [SerializeField, Range(0.2f, 1.5f)] private float shadowWidth = 0.95f;
    [Tooltip("그림자 폭 대비 높이")]
    [SerializeField, Range(0.05f, 0.5f)] private float shadowFlatness = 0.22f;
    [Tooltip("발끝에서 위로 올리는 정도 (그림자 높이 대비). 발이 그림자 가운데쯤 오게")]
    [SerializeField, Range(0f, 1f)] private float shadowLift = 0.35f;
    [SerializeField] private Color shadowColor = new(0.086f, 0.094f, 0.106f, 0.8f); // Linear 색 공간이라 눈에는 이보다 옅게 보인다

    private Image _image;
    private Image _shadow;
    private Material _instance;      // 유닛마다 복사 — 위 조명 범위(_VRange)가 그림마다 달라서
    private Sprite _rangeSprite;
    private static Sprite _ellipse;

    // 전투 배경(BattleStageView)이 그 배경의 빛에 맞춘 머티리얼을 넣어 준다. 없으면 Resources 기본값
    private static Material _stageAlly, _stageEnemy;

    public static void SetStageMaterials(Material ally, Material enemy)
    {
        _stageAlly = ally; _stageEnemy = enemy;
        foreach (var look in FindObjectsByType<BattleUnitLook>(FindObjectsSortMode.None)) look.Refresh();
    }

    // shadow = false: 떠다니는 동료(드론 등)처럼 발밑 그림자를 따로 두는 경우
    public static BattleUnitLook Apply(Image image, Side side, bool shadow = true)
    {
        if (image == null) return null;
        var look = image.GetComponent<BattleUnitLook>();
        if (look == null) look = image.gameObject.AddComponent<BattleUnitLook>();
        look.side = side;
        look.groundShadow = shadow;
        look.Refresh();
        return look;
    }

    private void OnEnable() => Refresh();

    private void OnDisable()
    {
        if (_shadow != null) _shadow.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_shadow != null) Destroy(_shadow.gameObject);
        if (_instance != null) Destroy(_instance);
    }

    // 위 조명이 그림의 보이는 위 · 아래 끝을 기준으로 걸리도록 텍스처 V 범위를 넘긴다 (시트 프레임이 바뀔 때만)
    private void UpdateLightRange()
    {
        var sprite = _image.sprite;
        if (_instance == null || sprite == null || sprite == _rangeSprite) return;
        _rangeSprite = sprite;
        var outer = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
        float ny0 = 0f, ny1 = 1f;
        var verts = sprite.vertices;
        if (verts != null && verts.Length > 0)
        {
            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var v in verts) { minY = Mathf.Min(minY, v.y); maxY = Mathf.Max(maxY, v.y); }
            float ppu = sprite.pixelsPerUnit, h = sprite.rect.height;
            ny0 = (sprite.pivot.y + minY * ppu) / h; ny1 = (sprite.pivot.y + maxY * ppu) / h;
        }
        _instance.SetVector("_VRange", new Vector4(Mathf.Lerp(outer.y, outer.w, ny0), Mathf.Lerp(outer.y, outer.w, ny1), 0, 0));
    }

    public void Refresh()
    {
        if (_image == null) _image = GetComponent<Image>();
        var mat = side == Side.Enemy ? _stageEnemy : _stageAlly;
        if (mat == null) mat = Resources.Load<Material>(side == Side.Enemy ? "BattleUnit/BattleUnit_Enemy" : "BattleUnit/BattleUnit_Ally");
        if (mat != null)
        {
            if (_instance != null) Destroy(_instance);
            _instance = new Material(mat) { name = mat.name + " (Unit)", hideFlags = HideFlags.DontSave };
            _rangeSprite = null;
            _image.material = _instance;
        }

        if (!groundShadow || _image.transform.parent == null)
        {
            if (_shadow != null) _shadow.gameObject.SetActive(false);
            return;
        }
        // 예전에 따로 둔 그림자(동료 프리팹 "Shadow", 주인공 "PlayerShadow" 등)는 겹치지 않게 그림만 끈다
        foreach (Transform sib in _image.transform.parent)
            if (sib != _image.transform && (_shadow == null || sib != _shadow.transform)
                && sib.name.Contains("Shadow") && !sib.name.EndsWith("_GroundShadow") && sib.TryGetComponent(out Graphic g))
                g.enabled = false;

        if (_shadow == null)
        {
            var go = new GameObject(name + "_GroundShadow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.hideFlags = HideFlags.DontSave;
            go.layer = gameObject.layer;
            go.transform.SetParent(_image.transform.parent, false);
            _shadow = go.GetComponent<Image>();
            _shadow.sprite = Ellipse();
            _shadow.raycastTarget = false;
            var le = go.AddComponent<LayoutElement>(); le.ignoreLayout = true;
        }
        _shadow.color = shadowColor;
        _shadow.gameObject.SetActive(true);
        LateUpdate();
    }

    // 그림이 바뀌거나(시트 애니메이션) 움직여도(공격 돌진) 발밑을 따라간다
    private void LateUpdate()
    {
        if (_image != null) UpdateLightRange();
        if (_shadow == null || _image == null) return;
        var sprite = _image.sprite;
        bool show = groundShadow && _image.enabled && sprite != null && _image.color.a > 0.01f;
        if (_shadow.enabled != show) _shadow.enabled = show;
        if (!show) return;

        var srt = (RectTransform)_shadow.transform;
        int idx = _image.transform.GetSiblingIndex();
        int sIdx = srt.GetSiblingIndex();
        if (sIdx != idx - 1) srt.SetSiblingIndex(sIdx < idx ? idx - 1 : idx); // 언제나 그림 바로 앞 = 그림 뒤에 그려짐

        if (!TryVisibleBottom(out Vector3 bottomLocal, out float widthLocal, out float heightLocal)) return;
        float baseW = Mathf.Max(widthLocal, heightLocal * 0.55f);
        var irt = _image.rectTransform;
        var parent = (RectTransform)srt.parent;
        Vector3 world = irt.TransformPoint(bottomLocal);
        float w = parent.InverseTransformVector(irt.TransformVector(new Vector3(baseW * shadowWidth, 0, 0))).magnitude;
        float h = w * shadowFlatness;
        srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.5f);
        srt.pivot = new Vector2(0.5f, 0.5f);
        srt.sizeDelta = new Vector2(w, h);
        srt.localScale = Vector3.one;
        srt.localRotation = Quaternion.identity;
        Vector3 local = parent.InverseTransformPoint(world);
        srt.localPosition = new Vector3(local.x, local.y + h * shadowLift, 0f);
    }

    // 스프라이트 메시(투명 부분을 깎은 모양) 기준으로 보이는 그림의 아래 끝 가운데와 폭을 Image 로컬 좌표로.
    // preserveAspect로 rect 안에 맞춰 그린 배치까지 반영한다.
    private bool TryVisibleBottom(out Vector3 bottom, out float width, out float height)
        => TryVisibleBottom(_image, out bottom, out width, out height);

    // 보이는 그림의 발끝 (월드 좌표). 진형 배치(PartyView)가 그림마다 다른 여백을 무시하고 발을 맞출 때 쓴다
    public static bool TryVisibleFeetWorld(Image image, out Vector3 feet)
    {
        feet = Vector3.zero;
        if (image == null || image.sprite == null || !TryVisibleBottom(image, out Vector3 local, out _, out _)) return false;
        feet = image.rectTransform.TransformPoint(local);
        return true;
    }

    private static bool TryVisibleBottom(Image _image, out Vector3 bottom, out float width, out float height)
    {
        bottom = Vector3.zero; width = 0f; height = 0f;
        var sprite = _image.sprite;
        var verts = sprite.vertices;
        if (verts == null || verts.Length == 0) return false;
        Vector2 min = new(float.MaxValue, float.MaxValue), max = new(float.MinValue, float.MinValue);
        foreach (var v in verts) { min = Vector2.Min(min, v); max = Vector2.Max(max, v); }

        Rect sr = sprite.rect;
        float ppu = sprite.pixelsPerUnit;
        // 스프라이트 rect 기준 0~1
        float nx0 = (sprite.pivot.x + min.x * ppu) / sr.width, nx1 = (sprite.pivot.x + max.x * ppu) / sr.width;
        float ny0 = (sprite.pivot.y + min.y * ppu) / sr.height, ny1 = (sprite.pivot.y + max.y * ppu) / sr.height;

        Rect r = _image.rectTransform.rect;
        float dw = r.width, dh = r.height;
        if (_image.preserveAspect && sr.width > 0f && sr.height > 0f)
        {
            float aspect = sr.width / sr.height;
            if (r.width / r.height > aspect) dw = r.height * aspect; else dh = r.width / aspect;
        }
        float left = r.center.x - dw * 0.5f, bottomY = r.center.y - dh * 0.5f;
        float x0 = left + nx0 * dw, x1 = left + nx1 * dw;
        bottom = new Vector3((x0 + x1) * 0.5f, bottomY + ny0 * dh, 0f);
        width = Mathf.Abs(x1 - x0);
        height = Mathf.Abs(ny1 - ny0) * dh;
        return width > 0f;
    }

    // 가장자리만 살짝 부드러운 타원 (흐린 그림자가 아니라 바닥에 닿은 자국)
    private static Sprite Ellipse()
    {
        if (_ellipse != null) return _ellipse;
        const int W = 128, H = 64;
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
        var px = new Color32[W * H];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                float dx = (x + 0.5f) / W * 2f - 1f, dy = (y + 0.5f) / H * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01((1f - d) / 0.18f);           // 바깥 18%만 부드럽게
                a *= Mathf.Lerp(0.75f, 1f, Mathf.Clamp01(1f - d));    // 가운데가 조금 더 진하게
                px[y * W + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        tex.SetPixels32(px); tex.Apply();
        _ellipse = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 100f);
        _ellipse.hideFlags = HideFlags.DontSave;
        return _ellipse;
    }
}
