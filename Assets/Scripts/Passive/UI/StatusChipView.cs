using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 상태 하나를 표시하는 칩. StatusListView가 생성/재사용한다.
//
// 아이콘이 없는 StatusDefinition이 많아서(현재 Assets/Data/Status의 전부),
// 아이콘이 있으면 아이콘을, 없으면 상태 이름 텍스트를 대신 보여준다.
// 나중에 아이콘 아트가 붙으면 에셋에 연결하는 것만으로 자동 전환된다.
//
// 마우스 확대(HoverScaleEffect)와 툴팁 표시(이 클래스의 IPointerEnterHandler)는
// 서로 완전히 독립된 컴포넌트다 — 둘 다 같은 칩 오브젝트에 붙어 있고 Unity EventSystem이
// 둘 다 호출해준다. HoverScaleEffect는 툴팁 개념을 전혀 모르는 범용 컴포넌트로 유지한다.
public class StatusChipView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI labelText; // 아이콘 없을 때의 대체 표시
    [SerializeField] private TextMeshProUGUI stackText;

    [Tooltip("스택 숫자 배지(구석). 스택이 0이면 숨긴다")]
    [SerializeField] private GameObject stackBadge;
    [Tooltip("성향(버프/디버프) 색을 칠할 띠. 비워두면 배경 전체에 칠한다(이전 방식)")]
    [SerializeField] private Image accentBar;

    [Header("성향별 색")]
    // 아이콘은 검은 외곽선 + 흰 면 + 시안 포인트로 그려져 있어서, 채도 높은 배경 위에선 흰 면이 묻힌다.
    // 배경은 종이색으로 두고 성향은 하단 띠 색으로만 구분한다(아트 디렉션: 오프화이트 바탕, 색은 최소한).
    [SerializeField] private Color buffColor    = new(0.30f, 0.72f, 0.88f, 1f);
    [SerializeField] private Color debuffColor  = new(0.82f, 0.20f, 0.20f, 1f);
    [SerializeField] private Color neutralColor = new(0.45f, 0.47f, 0.50f, 1f);

    public StatusInstance Bound { get; private set; }

    // 이 칩 때문에 툴팁이 지금 떠 있는지. StatusListView가 칩을 재사용/비활성화할 때
    // 마우스가 그 위에 그대로 있어도 OnPointerExit가 항상 정확히 오는 건 아니라서
    // (오브젝트가 비활성화되거나 Bind()로 다른 상태로 바뀌는 경우), Bind()/Clear() 쪽에서
    // 직접 챙겨야 툴팁이 옛날 내용을 보여준 채 남거나 안 닫히는 문제를 막을 수 있다.
    private bool _isTooltipShown;

    public void Bind(StatusInstance status)
    {
        Bound = status;

        var definition = status?.Definition;
        if (definition == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        Sprite icon = definition.Icon;
        bool hasIcon = icon != null;

        if (iconImage != null)
        {
            iconImage.sprite  = icon;
            iconImage.enabled = hasIcon;
        }

        // 아이콘이 있으면 이름 텍스트는 숨긴다 (칩이 좁아서 둘 다 넣으면 겹침).
        if (labelText != null)
        {
            labelText.gameObject.SetActive(!hasIcon);
            if (!hasIcon) labelText.text = definition.DisplayName;
        }

        if (stackText != null)
            stackText.text = status.Stacks.ToString();
        if (stackBadge != null)
            stackBadge.SetActive(status.Stacks != 0);

        Color dispositionColor = definition.GetDisposition(status.Stacks) switch
        {
            StatusDisposition.Buff   => buffColor,
            StatusDisposition.Debuff => debuffColor,
            _                        => neutralColor,
        };
        if (accentBar != null) accentBar.color = dispositionColor;
        else if (background != null) background.color = dispositionColor;

        // 인스펙터/하이어라키에서 어떤 상태인지 바로 알아보기 위함. 전투 디버깅용.
        gameObject.name = $"Chip_{definition.Id}";

        // 마우스가 그대로 위에 있는 채로 칩이 다른 상태로 재바인딩된 경우(풀링 재사용) —
        // 툴팁이 켜져 있었다면 내용을 새 상태로 갱신한다. 포인터가 실제로 움직이지 않아서
        // OnPointerEnter가 다시 오지 않으므로 여기서 직접 새로고침해야 한다.
        if (_isTooltipShown) StatusTooltipView.Instance?.Show(definition);
    }

    public void Clear()
    {
        Bound = null;
        gameObject.SetActive(false);

        // 비활성화되는 순간 OnPointerExit가 보장되지 않으므로 직접 닫는다.
        if (_isTooltipShown)
        {
            StatusTooltipView.Instance?.Hide();
            _isTooltipShown = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Bound?.Definition == null) return;
        _isTooltipShown = true;
        StatusTooltipView.Instance?.Show(Bound.Definition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isTooltipShown = false;
        StatusTooltipView.Instance?.Hide();
    }
}
