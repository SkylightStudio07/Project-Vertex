using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// ============================================================
// filename   : BlessingResponsiveUI.cs
// description: 축복 화면의 화면 비율 대응과 입력 반응.
//              비율: 화면 높이를 1080으로 보고 키운다. 대사판 · 선택지는 오른쪽 위, 이름판은 왼쪽 아래에 붙고,
//                    화면이 좁으면 캐릭터와 겹치지 않게 오른쪽 묶음을 더 줄인다. 배경은 늘리지 않고 화면을 덮는다.
//              반응: 마우스를 따라 UI가 시차로 움직이고 살짝 기운다. 대사판 · 답변 버튼은 호버에 밀리고 누르면 눌린다.
//              씬의 배치 값(1920×1080 기준)을 읽어 쓰므로 인스펙터에서 옮기면 그대로 따라간다. BlessingView가 자동으로 붙인다.
// ============================================================
public class BlessingResponsiveUI : MonoBehaviour
{
    private const float DesignWidth = 1920f, DesignHeight = 1080f;

    [Header("화면 비율")]
    [Tooltip("캐릭터 가로 위치 (화면 너비 대비, 가운데 기준)")]
    [SerializeField] private float characterX = -0.245f;
    [Tooltip("오른쪽 묶음이 침범하면 안 되는 캐릭터 오른쪽 폭 (1080 기준 px)")]
    [SerializeField] private float characterHalfWidth = 340f;
    [SerializeField, Range(0.5f, 1f)] private float minColumnScale = 0.7f;

    [Header("입력 반응")]
    [SerializeField, Min(0f)] private float smoothing = 5f;
    [Tooltip("마우스가 화면 끝에 있을 때 UI 전체가 기우는 각도 (x 위아래, y 좌우)")]
    [SerializeField] private Vector2 tiltAngle = new(1.2f, 1.8f);
    [SerializeField] private float hoverShift = -8f;
    [SerializeField, Range(0.9f, 1f)] private float pressScale = 0.985f;

    private class Item
    {
        public RectTransform rt;
        public Vector2 design;   // 1080 기준 위치 (anchor에서의 거리)
        public float depth;      // 시차 크기 (px)
        public bool column;      // 오른쪽 묶음
        public Vector2 basePos;
        public float scale = 1f;
        public BlessingHoverProbe probe;
        public float hover, press;
    }

    private Item[] _items;
    private RectTransform _rect, _overlay, _micro, _character;
    private Vector2 _lastSize;
    private Vector2 _mouse;
    private float _weight;
    private float _characterY, _characterScale = 1f;
    private BlessingIntroCinematic _intro;
    private BlessingAmbienceFx _ambience;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _intro = GetComponent<BlessingIntroCinematic>();
        _ambience = GetComponent<BlessingAmbienceFx>();
        _overlay = transform.Find("ChoiceOverlay") as RectTransform;
        _character = transform.Find("BlessingCharacter") as RectTransform;
        if (_character != null)
        {
            _characterY = _character.anchoredPosition.y;
            _characterScale = _character.localScale.x;
        }

        // 배경은 비율을 지켜 화면을 덮는다 (늘리면 달이 찌그러진다)
        var bg = transform.Find("Background");
        if (bg != null && bg.TryGetComponent<Image>(out var bgImage) && bgImage.sprite != null)
        {
            var fitter = bg.GetComponent<AspectRatioFitter>();
            if (fitter == null) fitter = bg.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = bgImage.sprite.rect.width / bgImage.sprite.rect.height;
        }

        if (_overlay == null) { _items = new Item[0]; return; }
        _micro = _overlay.Find("Micro") as RectTransform;
        if (_micro != null)
        {
            _micro.pivot = new Vector2(0f, 1f);
            _micro.offsetMin = _micro.offsetMax = Vector2.zero;
        }
        _items = new[]
        {
            Plate("Nameplate", 8f),
            Column("DialogueBubble", 14f, true),
            Column("ChoicePillList", 10f, false),
            Column("PlayerAnswerButton", 14f, true),
        };
        ApplyLayout();
    }

    // 왼쪽 위 기준 좌표를 왼쪽 아래 기준으로 옮긴다
    private Item Plate(string name, float depth)
    {
        var rt = _overlay.Find(name) as RectTransform;
        if (rt == null) return null;
        var item = new Item { rt = rt, depth = depth, design = new Vector2(rt.anchoredPosition.x, DesignHeight + rt.anchoredPosition.y) };
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        return item;
    }

    // 왼쪽 위 기준 좌표를 오른쪽 위 기준으로 옮긴다. design = (오른쪽 여백, 위 여백)
    private Item Column(string name, float depth, bool probe)
    {
        var rt = _overlay.Find(name) as RectTransform;
        if (rt == null) return null;
        var item = new Item { rt = rt, depth = depth, column = true, design = new Vector2(DesignWidth - rt.anchoredPosition.x - rt.sizeDelta.x, -rt.anchoredPosition.y) };
        rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.one;
        if (probe) item.probe = rt.gameObject.AddComponent<BlessingHoverProbe>();
        return item;
    }

    private void OnRectTransformDimensionsChange()
    {
        if (_items != null && isActiveAndEnabled) ApplyLayout();
    }

    private void OnDisable()
    {
        if (_overlay != null) _overlay.localRotation = Quaternion.identity;
        _weight = 0f;
    }

    private void ApplyLayout()
    {
        Vector2 size = _rect.rect.size;
        if (size.y <= 0f || size == _lastSize) return;
        _lastSize = size;

        float s = size.y / DesignHeight;
        // 캐릭터 가운데는 화면 왼쪽에서 (0.5 + characterX) 지점. 그 오른쪽 폭을 뺀 나머지가 오른쪽 묶음 자리다
        float charCenter = size.x * (0.5f + characterX);
        float margin = 34f * s;
        float available = size.x - margin - (charCenter + characterHalfWidth * s);
        float columnScale = s * Mathf.Clamp(available / (920f * s), minColumnScale, 1f);

        if (_character != null)
        {
            _character.anchoredPosition = new Vector2(characterX * size.x, _characterY * s);
            _character.localScale = new Vector3(s * _characterScale, s * _characterScale, 1f);
            _ambience?.SetCharacterBase(_character.localPosition, _character.localScale);
            _intro?.SetCharacterBase(_character.localPosition, _character.localScale);
        }
        if (_micro != null) _micro.localScale = new Vector3(s, s, 1f);

        foreach (var item in _items)
        {
            if (item == null) continue;
            item.scale = item.column ? columnScale : s;
            item.basePos = item.column ? new Vector2(-item.design.x * s, -item.design.y * columnScale) : item.design * s;
            item.rt.anchoredPosition = item.basePos;
            item.rt.localScale = new Vector3(item.scale, item.scale, 1f);
        }
    }

    private void LateUpdate()
    {
        if (_overlay == null || _items == null) return;
        float dt = Time.unscaledDeltaTime;
        bool busy = _intro != null && _intro.IsPlaying;
        _weight = Mathf.MoveTowards(_weight, busy ? 0f : 1f, dt / 0.6f);

        Vector2 target = Vector2.zero;
        if (Mouse.current != null && Screen.width > 0 && Screen.height > 0)
        {
            Vector2 m = Mouse.current.position.ReadValue();
            target = new Vector2(Mathf.Clamp(m.x / Screen.width * 2f - 1f, -1f, 1f), Mathf.Clamp(m.y / Screen.height * 2f - 1f, -1f, 1f));
        }
        float k = 1f - Mathf.Exp(-smoothing * dt);
        _mouse = Vector2.Lerp(_mouse, target, k);

        _overlay.localRotation = Quaternion.Euler(-_mouse.y * tiltAngle.x * _weight, _mouse.x * tiltAngle.y * _weight, 0f);

        float s = _lastSize.y / DesignHeight;
        foreach (var item in _items)
        {
            if (item == null) continue;
            bool hovering = item.probe != null && item.probe.Hovering, pressing = item.probe != null && item.probe.Pressing;
            item.hover = Mathf.Lerp(item.hover, hovering ? 1f : 0f, 1f - Mathf.Exp(-16f * dt));
            item.press = Mathf.Lerp(item.press, pressing ? 1f : 0f, 1f - Mathf.Exp(-30f * dt));
            item.rt.anchoredPosition = item.basePos - _mouse * (item.depth * s * _weight) + new Vector2(hoverShift * s * item.hover, 0f);
            float scale = item.scale * Mathf.Lerp(1f, pressScale, item.press);
            item.rt.localScale = new Vector3(scale, scale, 1f);
        }
    }
}

// 호버 · 누름 상태만 알려 주는 표식. 움직임은 BlessingResponsiveUI가 맡는다
public class BlessingHoverProbe : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public bool Hovering { get; private set; }
    public bool Pressing { get; private set; }

    public void OnPointerEnter(PointerEventData eventData) => Hovering = true;
    public void OnPointerExit(PointerEventData eventData) { Hovering = false; Pressing = false; }
    public void OnPointerDown(PointerEventData eventData) => Pressing = true;
    public void OnPointerUp(PointerEventData eventData) => Pressing = false;
    private void OnDisable() { Hovering = false; Pressing = false; }
}
