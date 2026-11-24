using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 의도 하나만 표시하는 프리팹 항목. 생성·배치는 IntentTooltipTrigger가 담당한다.
public class IntentTooltipView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public void Bind(IntentDefinition definition, int? damage)
    {
        iconImage.sprite = definition.icon;
        iconImage.enabled = definition.icon != null;
        titleText.text = definition.title;
        descriptionText.text = definition.GetDescription(damage);
    }
}
