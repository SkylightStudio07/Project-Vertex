using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TMP 글자 아래 오른쪽으로 떨어지는 그림자. Pretendard 아틀라스는 여백이 좁아 언더레이·외곽선이 잘리므로,
// 바로 뒤 형제로 같은 글자를 하나 더 두고 위치·글자를 따라가게 한다. 레이아웃 그룹 안에서도 자리를 차지하지 않는다.
[RequireComponent(typeof(TextMeshProUGUI))]
public class UITextDropShadow : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new(2f, -2f);
    [SerializeField] private Color color = new(0f, 0f, 0f, 0.45f);

    private TextMeshProUGUI _text;
    private TextMeshProUGUI _shadow;
    private RectTransform _rect, _shadowRect;

    private void OnEnable()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _rect = _text.rectTransform;
        if (_shadow == null) Build();
        _shadow.gameObject.SetActive(true);
        Sync();
    }

    private void OnDisable()
    {
        if (_shadow != null) _shadow.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_shadow != null) Destroy(_shadow.gameObject);
    }

    private void Build()
    {
        var go = new GameObject(name + "_Shadow", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
        go.hideFlags = HideFlags.DontSave;
        go.layer = gameObject.layer;
        go.GetComponent<LayoutElement>().ignoreLayout = true;
        _shadowRect = (RectTransform)go.transform;
        _shadowRect.SetParent(_rect.parent, false);
        _shadow = go.GetComponent<TextMeshProUGUI>();
        _shadow.raycastTarget = false;
    }

    private void LateUpdate() => Sync();

    private void Sync()
    {
        if (_shadow == null) return;
        // 항상 글자 바로 앞(= 뒤에 그려짐)에 둔다
        int textIndex = _rect.GetSiblingIndex(), shadowIndex = _shadowRect.GetSiblingIndex();
        if (shadowIndex != textIndex - 1)
            _shadowRect.SetSiblingIndex(shadowIndex < textIndex ? textIndex - 1 : textIndex);

        _shadowRect.anchorMin = _rect.anchorMin;
        _shadowRect.anchorMax = _rect.anchorMax;
        _shadowRect.pivot = _rect.pivot;
        _shadowRect.sizeDelta = _rect.sizeDelta;
        _shadowRect.anchoredPosition = _rect.anchoredPosition + offset;
        _shadowRect.localScale = _rect.localScale;

        if (_shadow.text != _text.text) _shadow.text = _text.text;
        _shadow.font = _text.font;
        _shadow.fontSize = _text.fontSize;
        _shadow.fontStyle = _text.fontStyle;
        _shadow.alignment = _text.alignment;
        _shadow.textWrappingMode = _text.textWrappingMode;
        _shadow.overflowMode = _text.overflowMode;
        _shadow.color = new Color(color.r, color.g, color.b, color.a * _text.alpha);
    }
}
