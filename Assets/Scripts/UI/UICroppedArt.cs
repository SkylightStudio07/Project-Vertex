using UnityEngine;
using UnityEngine.UI;

// 스프라이트를 이 칸의 비율에 맞춰 잘라서 꽉 채운다 (CSS object-fit: cover).
// 카드 아트(가로로 긴 788×301)를 무기고 카드 목록처럼 정사각형에 가까운 칸에 넣을 때 쓴다.
// 원본 스프라이트는 건드리지 않고 RawImage의 uvRect만 바꾼다. focus로 어느 쪽을 남길지 정한다.
[RequireComponent(typeof(RawImage))]
public class UICroppedArt : MonoBehaviour
{
    [Tooltip("남길 부분의 중심 (0~1). (0.5, 0.5)는 가운데")]
    [SerializeField] private Vector2 focus = new(0.5f, 0.5f);
    [Tooltip("1보다 크면 더 확대해서 잘라낸다")]
    [SerializeField, Min(1f)] private float zoom = 1f;

    private RawImage _image;
    private Sprite _sprite;

    private RawImage Image => _image != null ? _image : (_image = GetComponent<RawImage>());

    public void SetSprite(Sprite sprite, Vector2? focusOverride = null, float? zoomOverride = null)
    {
        _sprite = sprite;
        if (focusOverride.HasValue) focus = focusOverride.Value;
        if (zoomOverride.HasValue) zoom = Mathf.Max(1f, zoomOverride.Value);
        Image.texture = sprite != null ? sprite.texture : null;
        Image.enabled = sprite != null;
        Apply();
    }

    private void OnRectTransformDimensionsChange() => Apply();

    private void Apply()
    {
        if (_sprite == null || _sprite.texture == null) return;
        var rect = ((RectTransform)transform).rect;
        if (rect.width <= 0f || rect.height <= 0f) return;

        // 아틀라스·스프라이트 시트 안의 스프라이트도 그 영역 안에서만 자른다
        var tex = _sprite.texture;
        Rect src = _sprite.textureRect;
        float boxAspect = rect.width / rect.height;
        float srcAspect = src.width / src.height;

        float w = src.width, h = src.height;
        if (srcAspect > boxAspect) w = src.height * boxAspect; // 원본이 더 넓다 → 좌우를 자른다
        else h = src.width / boxAspect;                        // 원본이 더 높다 → 위아래를 자른다
        w /= zoom; h /= zoom;

        float x = src.x + Mathf.Clamp((src.width - w) * focus.x, 0f, src.width - w);
        float y = src.y + Mathf.Clamp((src.height - h) * focus.y, 0f, src.height - h);
        Image.uvRect = new Rect(x / tex.width, y / tex.height, w / tex.width, h / tex.height);
    }
}
