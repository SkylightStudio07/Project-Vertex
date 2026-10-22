using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로비 무기고(03) 입구. 왼쪽 정비 담당 + 대사, 오른쪽 메뉴 두 개(장비 강화 · 카드 강화).
// 안쪽 두 화면은 같은 루트 아래 다른 판(kit)이고, 안쪽 탭으로 서로 오가며 ← 무기고로 입구에 돌아온다.
// 로비 메뉴의 무기고 버튼이 SetActive(true)로 연다. 아트: ArtDirection/ArmoryHubMockup (v2, layout.json 1920×1080)
public class ArmoryHubView : MonoBehaviour
{
    [SerializeField] private GameObject hubKit;
    [SerializeField] private GameObject equipmentKit;
    [SerializeField] private GameObject cardKit;
    [SerializeField] private Button closeButton;
    [SerializeField] private ArchiveMenuEntry equipmentMenu;
    [SerializeField] private ArchiveMenuEntry cardMenu;
    [SerializeField] private Button[] backToHubButtons;   // 안쪽 화면의 ← 무기고
    [SerializeField] private Button[] equipmentTabs;      // 안쪽 화면의 "장비 강화" 탭
    [SerializeField] private Button[] cardTabs;           // 안쪽 화면의 "카드 강화" 탭

    [Header("대사")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private string lineEntry = "어떤 장비를 손봐 줄까?";
    [SerializeField] private string lineEquipment = "총은 손볼수록 손에 붙어. 파츠만 있으면 돼.";
    [SerializeField] private string lineCard = "기술도 정비가 필요하지. 탈린이랑 파츠 조금이면 돼.";

    private void Awake()
    {
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        equipmentMenu.OnHovered += _ => dialogueText.text = lineEquipment;
        cardMenu.OnHovered += _ => dialogueText.text = lineCard;
        equipmentMenu.OnClicked += _ => Show(equipmentKit);
        cardMenu.OnClicked += _ => Show(cardKit);
        foreach (var b in backToHubButtons) if (b != null) b.onClick.AddListener(() => Show(hubKit));
        foreach (var b in equipmentTabs) if (b != null) b.onClick.AddListener(() => Show(equipmentKit));
        foreach (var b in cardTabs) if (b != null) b.onClick.AddListener(() => Show(cardKit));
    }

    private void OnEnable()
    {
        dialogueText.text = lineEntry;
        Show(hubKit);
    }

    private void Show(GameObject kit)
    {
        hubKit.SetActive(kit == hubKit);
        equipmentKit.SetActive(kit == equipmentKit);
        cardKit.SetActive(kit == cardKit);
        if (kit == hubKit) dialogueText.text = lineEntry;
    }
}
