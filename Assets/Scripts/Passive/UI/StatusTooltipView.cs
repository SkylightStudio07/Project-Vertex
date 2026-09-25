using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 상태(버프/디버프) 설명 툴팁. 화면 전체에 하나만 존재하는 싱글톤 패널로,
// 마우스를 어느 상태 칩(플레이어/적 공용 StatusChipView)에 올리든 이 패널 하나를 채워서 보여준다.
//
// 마우스 커서를 따라다니지 않고 씬에 미리 배치해둔 고정 위치(아군 스프라이트 앞)에 뜬다 —
// 좁은 칩 옆에 바로 띄우면 화면 밖으로 나가거나 다른 UI에 가릴 수 있어서, 항상 같은
// 자리에서 읽을 수 있게 고정했다. 위치를 바꾸고 싶으면 이 오브젝트 자체를 씬에서 옮기면 된다.
public class StatusTooltipView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public static StatusTooltipView Instance { get; private set; }

    private RectTransform _rect;
    private Vector3 _homePosition; // 씬에 배치된 고정 위치. ShowNear로 옮겼다가 Show/Hide 때 되돌린다.

    private void Awake()
    {
        // 씬에 하나만 있다고 가정한다(여러 개면 마지막에 Awake된 것이 우선).
        Instance = this;
        _rect = (RectTransform)transform;
        _homePosition = _rect.position;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(StatusDefinition definition)
    {
        if (definition == null) return;

        gameObject.SetActive(true);
        if (_rect != null) _rect.position = _homePosition;

        if (iconImage != null)
        {
            iconImage.sprite  = definition.Icon;
            iconImage.enabled = definition.Icon != null;
        }

        if (titleText != null) titleText.text = definition.DisplayName;
        if (descriptionText != null) descriptionText.text = definition.Description;
    }

    // 상태 정의 없이 임의 내용(적 인텐트 등)을 anchor 왼쪽에 띄운다. 화면 밖으로 나가면 안쪽으로 민다.
    public void ShowNear(RectTransform anchor, Sprite icon, string title, string description)
    {
        gameObject.SetActive(true);
        if (iconImage != null)
        {
            iconImage.sprite  = icon;
            iconImage.enabled = icon != null;
        }
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;
        if (anchor == null || _rect == null) return;

        Canvas.ForceUpdateCanvases();
        var root = _rect.GetComponentInParent<Canvas>().rootCanvas.transform as RectTransform;
        var a = RectTransformUtility.CalculateRelativeRectTransformBounds(root, anchor);
        var t = RectTransformUtility.CalculateRelativeRectTransformBounds(root, _rect);
        Vector3 pivotInRoot = root.InverseTransformPoint(_rect.position);

        // 툴팁 오른쪽 끝을 아이콘 왼쪽 끝 - 여백에, 세로 가운데를 아이콘 가운데에 맞춘다
        const float margin = 16f;
        Vector2 shift = new(a.min.x - margin - t.max.x, a.center.y - t.center.y);
        Rect area = root.rect;
        float minX = t.min.x + shift.x, maxY = t.max.y + shift.y, minY = t.min.y + shift.y;
        if (minX < area.xMin + margin) shift.x += area.xMin + margin - minX;          // 왼쪽 끝이면 오른쪽으로
        if (maxY > area.yMax - margin) shift.y -= maxY - (area.yMax - margin);        // 위로 넘치면 아래로
        if (minY < area.yMin + margin) shift.y += area.yMin + margin - minY;
        _rect.position = root.TransformPoint(pivotInRoot + (Vector3)shift);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (_rect != null) _rect.position = _homePosition;
    }
}
