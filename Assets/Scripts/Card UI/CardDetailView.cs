using UnityEngine;
using UnityEngine.UI;

// 카드 상세 UI. 카드 우클릭 시 띄워지는 뷰.
public class CardDetailView : MonoBehaviour
{
    public static CardDetailView Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private CardView detailCardView;
    [SerializeField] private Button backdropButton; // 바깥 클릭 시 닫기

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (backdropButton != null) backdropButton.onClick.AddListener(Hide);
        if (panel != null) panel.SetActive(false);
    }

    public void Show(CardData card)
    {
        if (card == null) 
        { 
            Debug.LogWarning("CardDetailView.Show called with null card.");
            return;
        }
        if (panel != null) panel.SetActive(true);
        detailCardView.SetCard(card);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}