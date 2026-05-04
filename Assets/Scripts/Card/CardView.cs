// ============================================================
// 파일명    : CardView.cs
// 설명      : CardData를 카드 UI 프리팹에 바인딩하는 뷰 컴포넌트.
//             카드 프리팹 루트에 부착하고, Inspector에서 각 UI 슬롯을 연결.
// ============================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI energyCostText;
    [SerializeField] private TextMeshProUGUI ammoCostText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("이미지")]
    [SerializeField] private Image artworkImage;
    [SerializeField] private Image backgroundImage;

    public CardData Data { get; private set; }

    public void SetCard(CardData card)
    {
        Data = card;

        nameText.text        = card.CardName;
        energyCostText.text  = card.EnergyCost.ToString();
        ammoCostText.text    = card.AmmoCost.ToString();
        descriptionText.text = card.CardDescription;

        artworkImage.sprite  = card.CardImage;
        artworkImage.enabled = card.CardImage != null;

        backgroundImage.sprite = card.CardBackground;
        backgroundImage.enabled = card.CardBackground != null;
    }
}
