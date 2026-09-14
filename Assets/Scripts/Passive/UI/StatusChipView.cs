using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 상태 하나를 표시하는 칩. StatusListView가 생성/재사용한다.
//
// 아이콘이 없는 StatusDefinition이 많아서(현재 Assets/Data/Status의 전부),
// 아이콘이 있으면 아이콘을, 없으면 상태 이름 텍스트를 대신 보여준다.
// 나중에 아이콘 아트가 붙으면 에셋에 연결하는 것만으로 자동 전환된다.
public class StatusChipView : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI labelText; // 아이콘 없을 때의 대체 표시
    [SerializeField] private TextMeshProUGUI stackText;

    [Header("성향별 배경색")]
    [SerializeField] private Color buffColor    = new(0.20f, 0.55f, 0.25f, 0.85f);
    [SerializeField] private Color debuffColor  = new(0.65f, 0.18f, 0.18f, 0.85f);
    [SerializeField] private Color neutralColor = new(0.30f, 0.30f, 0.35f, 0.85f);

    public StatusInstance Bound { get; private set; }

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

        if (background != null)
        {
            background.color = definition.GetDisposition(status.Stacks) switch
            {
                StatusDisposition.Buff   => buffColor,
                StatusDisposition.Debuff => debuffColor,
                _                        => neutralColor,
            };
        }

        // 인스펙터/하이어라키에서 어떤 상태인지 바로 알아보기 위함. 전투 디버깅용.
        gameObject.name = $"Chip_{definition.Id}";
    }

    public void Clear()
    {
        Bound = null;
        gameObject.SetActive(false);
    }
}
