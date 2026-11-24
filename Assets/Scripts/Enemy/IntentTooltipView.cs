using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 의도 하나만 표시하는 프리팹 항목. 생성·배치는 IntentTooltipTrigger가 담당한다.
public class IntentTooltipView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public void Bind(Sprite icon, string title, string description)
    {
        iconImage.sprite = icon;
        iconImage.enabled = icon != null;
        titleText.text = title;
        descriptionText.text = description;
    }
}
