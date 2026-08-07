using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartingDeckSummaryItem : MonoBehaviour
{
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardDescriptionText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text costText;

    private string fallbackString = "N/A";

    public void Bind(CardData card, int count)
    {
        string cardName = card != null ? card.CardName : fallbackString;

        if (cardNameText != null)
            cardNameText.text = cardName;

        if (cardDescriptionText != null)
            cardDescriptionText.text = card != null ? card.CardDescription : string.Empty;

        if (countText != null)
            countText.text = $"x{Mathf.Max(0, count)}";

        if (costText != null)
            costText.text = card != null ? card.EnergyCost.ToString() : "-";

        if (artworkImage == null)
            return;

        artworkImage.sprite = card != null ? card.CardImage : null;
        artworkImage.enabled = artworkImage.sprite != null;
    }

    public void SetReferences(
        Image artwork,
        TMP_Text cardName,
        TMP_Text cardDescription,
        TMP_Text count,
        TMP_Text cost)
    {
        artworkImage = artwork;
        cardNameText = cardName;
        cardDescriptionText = cardDescription;
        countText = count;
        costText = cost;
    }
}
