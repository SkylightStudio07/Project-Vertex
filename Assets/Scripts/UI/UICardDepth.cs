using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 로비 2.5D 메뉴 칸의 옆면 두께(명일방주식). 칸 그림과 똑같은 모양의 먹색 실루엣을 칸 바로 뒤 형제로 만들어
// 아래·왼쪽으로 조금 비켜 깔고, 호버하면 칸이 살짝 커지며 두께가 길어진다(앞으로 튀어나오는 느낌).
// 실루엣은 UI/Silhouette 셰이더(스프라이트 알파 + 단색)를 쓰므로 칸 모양이 바뀌어도 따로 그릴 것이 없다.
// 등장 연출이 칸을 움직이고 페이드해도 실루엣이 위치·크기·알파를 매 프레임 따라간다.
[RequireComponent(typeof(Image))]
public class UICardDepth : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Material silhouetteMaterial;            // Mat_Silhouette (UI/Silhouette)
    [SerializeField] private Color depthColor = new(0.086f, 0.094f, 0.106f, 1f); // #16181B
    [SerializeField] private Vector2 offset = new(-5f, -7f);
    [SerializeField] private Vector2 hoverOffset = new(-9f, -12f);
    [SerializeField, Min(1f)] private float hoverScale = 1.03f;
    [SerializeField, Min(0f)] private float hoverDuration = 0.15f;

    private RectTransform _rect;
    private Image _image;
    private CanvasGroup _group;
    private RectTransform _depth;
    private Image _depthImage;
    private Vector2 _currentOffset;
    private Vector3 _baseScale;
    private float _hover; // 0 → 1
    private Tween _tween;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _image = GetComponent<Image>();
        _baseScale = _rect.localScale;
        _currentOffset = offset;
    }

    private void OnEnable()
    {
        EnsureDepth();
        if (_depth != null) _depth.gameObject.SetActive(true);
        Sync();
    }

    private void OnDisable()
    {
        _tween?.Kill();
        _hover = 0f;
        _rect.localScale = _baseScale;
        if (_depth != null) _depth.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_depth != null) Destroy(_depth.gameObject);
    }

    private void LateUpdate() => Sync();

    public void OnPointerEnter(PointerEventData eventData) => TweenHover(1f);
    public void OnPointerExit(PointerEventData eventData) => TweenHover(0f);

    private void TweenHover(float to)
    {
        _tween?.Kill();
        _tween = DOTween.To(() => _hover, v =>
        {
            _hover = v;
            _rect.localScale = _baseScale * Mathf.Lerp(1f, hoverScale, v);
        }, to, hoverDuration).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(gameObject);
    }

    private void EnsureDepth()
    {
        if (_depth != null || _rect.parent == null) return;
        var go = new GameObject(name + "_Depth", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = gameObject.layer;
        go.hideFlags = HideFlags.DontSave; // 런타임에만 만든다 (씬에 저장하지 않음)
        _depth = (RectTransform)go.transform;
        _depth.SetParent(_rect.parent, false);
        _depth.SetSiblingIndex(_rect.GetSiblingIndex()); // 칸 바로 뒤
        _depthImage = go.GetComponent<Image>();
        _depthImage.raycastTarget = false;
        _depthImage.material = silhouetteMaterial;
    }

    private void Sync()
    {
        if (_depth == null) return;
        if (_depth.GetSiblingIndex() != _rect.GetSiblingIndex() - 1) _depth.SetSiblingIndex(_rect.GetSiblingIndex());
        _depth.anchorMin = _rect.anchorMin;
        _depth.anchorMax = _rect.anchorMax;
        _depth.pivot = _rect.pivot;
        _depth.sizeDelta = _rect.sizeDelta;
        _depth.localScale = _rect.localScale;
        _depth.localRotation = _rect.localRotation;
        _currentOffset = Vector2.Lerp(offset, hoverOffset, _hover);
        _depth.anchoredPosition = _rect.anchoredPosition + _currentOffset;

        _depthImage.sprite = _image.sprite;
        _depthImage.type = _image.type;
        _depthImage.preserveAspect = _image.preserveAspect;
        float alpha = _group != null || TryGetComponent(out _group) ? _group.alpha : 1f;
        _depthImage.color = new Color(depthColor.r, depthColor.g, depthColor.b, depthColor.a * alpha * _image.color.a);
    }
}
