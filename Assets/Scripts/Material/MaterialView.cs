using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로비 마테리얼(시설 05) 화면. 분류 탭(적 / 동료) → 4열 대상 격자 → 오른쪽 대상 머리 + 01~05 색인 탭 + 서류 한 장.
// 적은 격퇴 수(일반 3 · 엘리트 2 · 보스 1회마다), 동료는 호감도 레벨마다 한 편씩 열린다 (MaterialArchive).
// 무기고·훈련장처럼 로비 버튼이 SetActive(true)로 연다. 기획: Docs/기획/마테리얼.md
// 아트: Assets/Art/Lobby/Material (원본 ArtDirection/MaterialMockup/Extracted, 좌표는 그 layout.json — 1672×941 좌상단)
public class MaterialView : MonoBehaviour
{
    private enum Tab { Enemy, Companion }

    [System.Serializable]
    private class CategoryTab
    {
        public Button button;
        public Image frame;
        public TextMeshProUGUI title;
        public TextMeshProUGUI english;
    }

    [Header("대상")]
    [SerializeField] private List<EnemyData> enemies = new();
    [SerializeField] private List<CoopCharData> companions = new();

    [Header("분류 · 필터")]
    [SerializeField] private Button closeButton;
    [SerializeField] private CategoryTab enemyTab;
    [SerializeField] private CategoryTab companionTab;
    [SerializeField] private Sprite tabNormal;
    [SerializeField] private Sprite tabHover;
    [SerializeField] private Sprite tabSelected;
    [SerializeField] private TextMeshProUGUI filterHeading;
    [Tooltip("필터 4칸. 적 탭: 일반 / 엘리트 / 보스 (4번째 숨김), 동료 탭: 1막 / 2막 / 3막 / 이벤트")]
    [SerializeField] private List<TrainingToggle> filters = new();
    [SerializeField] private TextMeshProUGUI ruleText;

    [Header("격자")]
    [SerializeField] private MaterialSubjectCell cellTemplate;
    [SerializeField] private Transform grid;
    [SerializeField] private TextMeshProUGUI gridTitle;
    [SerializeField] private TextMeshProUGUI gridTitleEnglish;
    [SerializeField] private TextMeshProUGUI gridCount;

    [Header("대상 머리")]
    [SerializeField] private GameObject detailRoot;
    [SerializeField] private UICroppedArt headerArt;
    [SerializeField] private TextMeshProUGUI subjectName;
    [SerializeField] private Image rankBadge;
    [SerializeField] private TextMeshProUGUI rankLabel;
    [SerializeField] private Sprite badgeNormal;
    [SerializeField] private Sprite badgeElite;
    [SerializeField] private Sprite badgeBoss;
    [SerializeField] private Image affiliationBadge;
    [SerializeField] private TextMeshProUGUI affiliationLabel;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private List<Image> progressCells = new();
    [SerializeField] private Sprite progressFilled;
    [SerializeField] private Sprite progressEmpty;

    [Header("기록")]
    [SerializeField] private List<MaterialEntryTab> entryTabs = new();
    [SerializeField] private TextMeshProUGUI documentNumber;
    [SerializeField] private TextMeshProUGUI entryTitle;
    [SerializeField] private GameObject bodyRoot;
    [SerializeField] private TextMeshProUGUI entryBody;
    [SerializeField] private ScrollRect bodyScroll;
    [SerializeField] private GameObject lockedRoot;
    [SerializeField] private TextMeshProUGUI lockedMessage;
    [SerializeField] private TextMeshProUGUI lockedCurrent;
    [SerializeField] private TextMeshProUGUI sourceFooter;

    [Header("크롭")]
    [SerializeField] private Vector2 enemyFocus = new(0.5f, 0.75f);
    [SerializeField, Min(1f)] private float enemyZoom = 1.3f;
    [SerializeField] private Vector2 companionFocus = new(0.55f, 0.97f);
    [SerializeField, Min(1f)] private float companionZoom = 3f;

    [Header("글자 색")]
    [SerializeField] private Color inkColor = new(0.086f, 0.094f, 0.106f, 1f);
    [SerializeField] private Color subColor = new(0.42f, 0.44f, 0.47f, 1f);
    [SerializeField] private Color paperColor = new(0.945f, 0.949f, 0.957f, 1f);

    private static readonly string[] EnemyFilterKeys = { "Normal", "Elite", "Boss" };
    private static readonly string[] EnemyFilterLabels = { "일반", "엘리트", "보스" };
    private static readonly string[] CompanionFilterKeys = { "1", "2", "3", "event" };
    private static readonly string[] CompanionFilterLabels = { "1막", "2막", "3막", "이벤트" };

    private readonly List<GameObject> _spawned = new();
    private Tab _tab = Tab.Enemy;
    private ScriptableObject _selected;
    private int _entryIndex;

    private void Awake()
    {
        cellTemplate.gameObject.SetActive(false);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        enemyTab.button.onClick.AddListener(() => SwitchTab(Tab.Enemy));
        companionTab.button.onClick.AddListener(() => SwitchTab(Tab.Companion));
        foreach (var f in filters)
            f.OnClicked += t => { t.SetOn(!t.IsOn); Refresh(); };
    }

    private void OnEnable() => SwitchTab(_tab, force: true);

    private void SwitchTab(Tab tab, bool force = false)
    {
        if (_tab == tab && !force) return;
        if (_tab != tab) _selected = null;
        _tab = tab;
        ApplyTabVisual(enemyTab, tab == Tab.Enemy);
        ApplyTabVisual(companionTab, tab == Tab.Companion);

        // 필터는 탭마다 다르다: 적 = 등급, 동료 = 소속
        bool enemy = tab == Tab.Enemy;
        filterHeading.text = enemy ? "등급" : "소속";
        var keys = enemy ? EnemyFilterKeys : CompanionFilterKeys;
        var labels = enemy ? EnemyFilterLabels : CompanionFilterLabels;
        for (int i = 0; i < filters.Count; i++)
        {
            bool used = i < keys.Length;
            filters[i].gameObject.SetActive(used);
            if (!used) continue;
            filters[i].Setup(keys[i], labels[i]);
            filters[i].SetOn(false);
        }
        ruleText.text = enemy
            ? "일반 3회 · 엘리트 2회 ·\n보스 1회마다 기록 1편 해금"
            : "동료 호감도 레벨마다\n기록 1편 해금";
        Refresh();
    }

    private void ApplyTabVisual(CategoryTab tab, bool on)
    {
        tab.frame.sprite = on ? tabSelected : tabNormal;
        var ss = tab.button.spriteState;
        ss.highlightedSprite = on ? null : tabHover;
        ss.pressedSprite = ss.highlightedSprite;
        tab.button.spriteState = ss;
        tab.title.color = on ? paperColor : inkColor;
        tab.english.color = on ? paperColor : subColor;
    }

    // ── 대상 ──
    private static int CoopLevel(CoopCharData data)
    {
        int level = CooperationManager.Instance != null ? CooperationManager.Instance.GetCoopLevel(data.charID) : 0;
        if (PlayerRecord.TryGetCoop(data.charID, out int saved, out _)) level = Mathf.Max(level, saved);
        return level;
    }

    private static int Unlocked(ScriptableObject subject) => subject switch
    {
        EnemyData e    => MaterialArchive.UnlockedCount(e),
        CoopCharData c => MaterialArchive.UnlockedCount(CoopLevel(c)),
        _              => 0,
    };

    // 한 번이라도 만난 대상만 이름·그림을 보인다 (적: 격퇴 1회 이상, 동료: 호감도 Lv.1 이상 = 합류한 적 있음)
    private static bool Known(ScriptableObject subject) => subject switch
    {
        EnemyData e    => PlayerRecord.GetDefeatCount(e) > 0,
        CoopCharData c => CoopLevel(c) > 0,
        _              => false,
    };

    private bool PassesFilter(ScriptableObject subject)
    {
        var on = filters.Where(f => f.gameObject.activeSelf && f.IsOn).Select(f => f.Key).ToList();
        if (on.Count == 0) return true;
        return subject switch
        {
            EnemyData e    => on.Contains(e.rank.ToString()),
            CoopCharData c => on.Contains(c.isEventCompanion ? "event" : c.actNumber.ToString()),
            _              => true,
        };
    }

    private IEnumerable<ScriptableObject> Subjects() => _tab == Tab.Enemy
        ? enemies.Where(e => e != null).OrderBy(e => (int)e.rank).ThenBy(e => e.enemyName).Cast<ScriptableObject>()
        : companions.Where(c => c != null).OrderBy(c => c.isEventCompanion).ThenBy(c => c.actNumber).ThenBy(c => c.charName).Cast<ScriptableObject>();

    private void Refresh()
    {
        foreach (var go in _spawned) if (go != null) Destroy(go);
        _spawned.Clear();

        var all = Subjects().ToList();
        // 확인한 대상을 앞에
        var list = all.Where(PassesFilter).OrderByDescending(Known).ToList();
        if (_selected == null || !list.Contains(_selected))
        {
            _selected = list.FirstOrDefault(Known) ?? list.FirstOrDefault();
            _entryIndex = _selected != null ? Mathf.Max(0, Unlocked(_selected) - 1) : 0;
        }

        foreach (var subject in list)
        {
            var cell = Instantiate(cellTemplate, grid);
            cell.gameObject.SetActive(true);
            _spawned.Add(cell.gameObject);
            var target = subject;
            if (subject is EnemyData e)
                cell.Bind(EnemyArt(e), enemyFocus, enemyZoom, e.enemyName, null, Unlocked(e), Known(e), subject == _selected, () => Select(target));
            else if (subject is CoopCharData c)
                cell.Bind(c.charImage != null ? c.charImage : c.standingSprite, c.faceFocus, c.faceZoom, c.charName, null,
                    Unlocked(c), Known(c), subject == _selected, () => Select(target));
        }

        gridTitle.text = _tab == Tab.Enemy ? "적" : "동료";
        gridTitleEnglish.text = _tab == Tab.Enemy ? "/ ENEMY" : "/ COMPANION";
        // 영문 라벨은 제목 폭 바로 뒤에
        gridTitle.ForceMeshUpdate();
        var en = gridTitleEnglish.rectTransform;
        en.anchoredPosition = new Vector2(gridTitle.rectTransform.anchoredPosition.x + gridTitle.preferredWidth + 12f, en.anchoredPosition.y);
        gridCount.text = $"확인 <b>{all.Count(Known)}</b>  ·  전체 <b>{all.Count}</b>";
        RefreshDetail();
    }

    private void Select(ScriptableObject subject)
    {
        _selected = subject;
        // 고르면 가장 최근에 열린 편부터 보여 준다
        _entryIndex = Mathf.Max(0, Unlocked(subject) - 1);
        Refresh();
    }

    private static Sprite EnemyArt(EnemyData e)
        => e.enemyImage != null ? e.enemyImage
           : e.idleFrames != null && e.idleFrames.Length > 0 ? e.idleFrames[0] : null;

    // ── 기록 ──
    private void RefreshDetail()
    {
        detailRoot.SetActive(_selected != null);
        if (_selected == null) return;

        bool known = Known(_selected);
        int unlocked = Unlocked(_selected);
        var enemy = _selected as EnemyData;
        var coop = _selected as CoopCharData;
        var file = MaterialArchive.Load(enemy != null ? enemy.materialJson : coop.materialJson);

        // 대상 머리
        headerArt.SetSprite(enemy != null ? EnemyArt(enemy) : (coop.charImage != null ? coop.charImage : coop.standingSprite),
            enemy != null ? enemyFocus : coop.faceFocus, enemy != null ? enemyZoom : Mathf.Max(1f, coop.faceZoom * 0.8f));
        headerArt.GetComponent<RawImage>().color = known ? Color.white : new Color(0.62f, 0.64f, 0.67f, 1f);
        subjectName.text = !known ? "???" : enemy != null ? enemy.enemyName : coop.charName;
        rankBadge.gameObject.SetActive(enemy != null);
        affiliationBadge.gameObject.SetActive(coop != null && known);
        if (enemy != null)
        {
            rankBadge.sprite = enemy.rank switch { EnemyEncounterType.Elite => badgeElite, EnemyEncounterType.Boss => badgeBoss, _ => badgeNormal };
            rankLabel.text = enemy.rank switch { EnemyEncounterType.Elite => "엘리트", EnemyEncounterType.Boss => "보스", _ => "일반" };
            rankLabel.color = enemy.rank == EnemyEncounterType.Normal ? inkColor : Color.white;
            progressText.text = $"격퇴 {PlayerRecord.GetDefeatCount(enemy)}회  ·  기록 {unlocked} / {MaterialArchive.EntryCount}";
        }
        else
        {
            affiliationLabel.text = coop.affiliation;
            progressText.text = $"호감도 Lv.{CoopLevel(coop)}  ·  기록 {unlocked} / {MaterialArchive.EntryCount}";
        }
        for (int i = 0; i < progressCells.Count; i++) progressCells[i].sprite = i < unlocked ? progressFilled : progressEmpty;

        // 색인 탭
        for (int i = 0; i < entryTabs.Count; i++)
        {
            int index = i;
            entryTabs[i].Bind(i, i < unlocked, i == _entryIndex, () => { _entryIndex = index; RefreshDetail(); });
        }

        // 서류
        bool open = _entryIndex < unlocked;
        var entry = file != null && _entryIndex < file.entries.Count ? file.entries[_entryIndex] : null;
        documentNumber.text = $"ARCHIVE-{_entryIndex + 1:00}";
        entryTitle.text = entry != null && !string.IsNullOrWhiteSpace(entry.title)
            ? entry.title
            : $"{(enemy != null ? "관측" : "인물")} 기록 {_entryIndex + 1}";
        sourceFooter.text = enemy != null ? "관측 자료 / 전투 기록 기반" : "인연 자료 / 호감도 기록 기반";
        bodyRoot.SetActive(open);
        lockedRoot.SetActive(!open);
        if (open)
        {
            entryBody.text = entry != null && !string.IsNullOrWhiteSpace(entry.body) ? entry.body : "아직 기록이 작성되지 않았습니다.";
            if (bodyScroll != null) bodyScroll.verticalNormalizedPosition = 1f;
        }
        else if (enemy != null)
        {
            lockedMessage.text = $"격퇴 {MaterialArchive.RequiredDefeats(enemy, _entryIndex)}회에 열립니다";
            lockedCurrent.text = $"(현재 {PlayerRecord.GetDefeatCount(enemy)}회)";
        }
        else
        {
            lockedMessage.text = $"호감도 Lv.{_entryIndex + 1}에 열립니다";
            lockedCurrent.text = $"(현재 Lv.{CoopLevel(coop)})";
        }
    }
}
