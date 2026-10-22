using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로비 아카이브(기록관) 입구. 가운데 기록관 담당 + 오른쪽 기록 메뉴 4개(적 · 동료 · 아이템 · 플레이).
//   - 적 / 동료 기록: 마테리얼 화면을 해당 탭으로 연다 (이 화면 위에 덮이고, 닫으면 여기로 돌아온다)
//   - 아이템 도감 / 플레이 기록: 이 화면 안의 쪽(page)을 연다
// 로비 좌하단 아카이브 버튼이 SetActive(true)로 연다. 아트: ArtDirection/ArchiveHubMockup (B안, layout.json 1920×1080)
public class ArchiveView : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private ArchiveMenuEntry enemyMenu;
    [SerializeField] private ArchiveMenuEntry companionMenu;
    [SerializeField] private ArchiveMenuEntry itemMenu;
    [SerializeField] private ArchiveMenuEntry playMenu;

    [Header("이동")]
    [SerializeField] private MaterialView materialView;
    [SerializeField] private ArchiveItemCodex itemPage;
    [SerializeField] private ArchivePlayRecords playPage;
    [SerializeField] private List<GameObject> hubObjects = new();   // 쪽을 열 때 숨기는 입구 요소(캐릭터·메뉴·대사·로비 버튼)

    [Header("수집 수")]
    [SerializeField] private List<EnemyData> enemies = new();
    [SerializeField] private List<CoopCharData> companions = new();
    [SerializeField] private ItemCatalog items;

    [Header("대사")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private string lineEntry = "어떤 이야기를 펼쳐 볼까요?";
    [SerializeField] private string lineEnemy = "쓰러뜨린 것들의 기록이에요. 무서운 얼굴도 많죠.";
    [SerializeField] private string lineCompanion = "함께 걸은 이들의 이야기예요.";
    [SerializeField] private string lineItem = "손에 넣었던 물건들. 하나하나 사연이 있답니다.";
    [SerializeField] private string linePlay = "지나온 길을 돌아볼까요?";

    private void Awake()
    {
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        Bind(enemyMenu, lineEnemy, () => materialView.Open(companions: false));
        Bind(companionMenu, lineCompanion, () => materialView.Open(companions: true));
        Bind(itemMenu, lineItem, () => OpenPage(itemPage.gameObject));
        Bind(playMenu, linePlay, () => OpenPage(playPage.gameObject));
        itemPage.OnClosed += ClosePage;
        playPage.OnClosed += ClosePage;
    }

    private void Bind(ArchiveMenuEntry entry, string line, System.Action open)
    {
        entry.OnHovered += _ => dialogueText.text = line;
        entry.OnClicked += _ => open();
    }

    private void OnEnable()
    {
        dialogueText.text = lineEntry;
        itemPage.gameObject.SetActive(false);
        playPage.gameObject.SetActive(false);
        SetHub(true);
        RefreshCounts();
    }

    private void SetHub(bool on)
    {
        foreach (var o in hubObjects) if (o != null) o.SetActive(on);
    }

    private void OpenPage(GameObject page)
    {
        SetHub(false);
        page.SetActive(true);
    }

    private void ClosePage()
    {
        SetHub(true);
        dialogueText.text = lineEntry;
        RefreshCounts();
    }

    private void RefreshCounts()
    {
        int foundEnemies = 0;
        foreach (var e in enemies) if (e != null && PlayerRecord.GetDefeatCount(e) > 0) foundEnemies++;
        enemyMenu.SetCount($"수집 {foundEnemies} / {enemies.Count}");

        int foundCompanions = 0;
        foreach (var c in companions)
            if (c != null && PlayerRecord.TryGetCoop(c.charID, out int level, out _) && level > 0) foundCompanions++;
        companionMenu.SetCount($"수집 {foundCompanions} / {companions.Count}");

        int foundItems = 0, totalItems = 0;
        if (items != null)
            foreach (var i in items.items)
            {
                if (i == null) continue;
                totalItems++;
                if (PlayerRecord.HasObtained(i)) foundItems++;
            }
        itemMenu.SetCount($"수집 {foundItems} / {totalItems}");

        playMenu.SetCount($"런 {PlayerRecord.RunCount}회");
    }
}
