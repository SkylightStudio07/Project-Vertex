using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로비 훈련장 화면. 무기고·의뢰 게시판처럼 로비 버튼이 SetActive(true)로 연다. 기획: Docs/기획/훈련장.md
//   메인: 상대(3회 이상 격퇴한 조우) · 선택한 상대 그림 · 동료(호감도 조건, 최대 3명) · 덱 요약 → 출격
//   덱 편집: 해금 카드(시작 덱 + 런에서 얻은 적 있는 카드) 격자 + 필터, 현재 덱 −/+  (장수 제한 없음, 최소 1장)
// 아트: Assets/Art/Lobby/Training (원본 ArtDirection/TrainingMockup/Extracted, 좌표는 그 layout.json — 1672×941 좌상단)
public class TrainingView : MonoBehaviour
{
    public const int RequiredDefeats = 3;

    [Header("목록 데이터")]
    [Tooltip("훈련 상대 후보 조우 (일반·엘리트·보스)")]
    [SerializeField] private List<EnemyEncounter> encounters = new();
    [Tooltip("덱 구성 후보 카드. 이 중 시작 덱 카드와 런에서 얻은 적 있는 카드만 덱에 넣을 수 있다")]
    [SerializeField] private List<CardData> cardLibrary = new();
    [SerializeField] private List<CoopCharData> companions = new();
    [Tooltip("출격하면 넘어갈 전투 씬 (Build Settings에 있어야 한다)")]
    [SerializeField] private string runSceneName = "DevelopScene - Phase 3";

    [Header("메인")]
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private Button closeButton;
    [SerializeField] private TrainingTargetRow targetRowTemplate;
    [SerializeField] private Transform targetList;
    [SerializeField] private TextMeshProUGUI targetCountText;
    [SerializeField] private Image previewArt;
    [SerializeField] private GameObject previewInfo;
    [SerializeField] private GameObject previewEmpty;
    [SerializeField] private TextMeshProUGUI previewName;
    [SerializeField] private Image previewBadge;
    [SerializeField] private TextMeshProUGUI previewBadgeLabel;
    [SerializeField] private TextMeshProUGUI previewAnalysis;
    [SerializeField] private Sprite badgeNormal;
    [SerializeField] private Sprite badgeElite;
    [SerializeField] private Sprite badgeBoss;
    [SerializeField] private TrainingCompanionCard companionTemplate;
    [SerializeField] private Transform companionList;
    [SerializeField] private TextMeshProUGUI companionCountText;
    [SerializeField] private TrainingDeckRow deckRowTemplate;
    [SerializeField] private Transform deckSummaryList;
    [SerializeField] private TextMeshProUGUI deckCountText;
    [SerializeField] private Button deckEditButton;
    [SerializeField] private Button resetDeckButton;
    [SerializeField] private Button sortieButton;

    [SerializeField] private Button formationButton;

    [Header("동료 편성")]
    [SerializeField] private GameObject companionScreen;
    [SerializeField] private Button companionBackButton;
    [SerializeField] private Button companionDoneButton;
    [SerializeField] private TrainingPortraitCell portraitCellTemplate;
    [SerializeField] private Transform companionGrid;
    [SerializeField] private TextMeshProUGUI companionGridCountText;
    [SerializeField] private TrainingToggle availableOnlyToggle;
    [Tooltip("소속 필터 (Key: 1 / 2 / 3 / event). 아무것도 안 켜면 전체")]
    [SerializeField] private List<TrainingToggle> actToggles = new();
    [SerializeField] private List<TrainingPartySlot> partySlots = new();
    [SerializeField] private TextMeshProUGUI partyCountText;
    [Header("선택한 동료")]
    [SerializeField] private GameObject detailRoot;
    [SerializeField] private UICroppedArt detailPortrait;
    [SerializeField] private TextMeshProUGUI detailName;
    [SerializeField] private TextMeshProUGUI detailAffiliation;
    [SerializeField] private Image detailAccent;
    [SerializeField] private TextMeshProUGUI detailAffinity;
    [SerializeField] private List<Image> detailAffinityCells = new();
    [SerializeField] private Sprite affinityEmpty;
    [SerializeField] private Sprite affinityFilled;
    [SerializeField] private GameObject detailCardRoot;
    [SerializeField] private UICroppedArt detailCardArt;
    [SerializeField] private TextMeshProUGUI detailCardName;
    [SerializeField] private TextMeshProUGUI detailCardEnergy;

    [Header("덱 편집")]
    [SerializeField] private GameObject deckScreen;
    [SerializeField] private Button deckBackButton;
    [SerializeField] private Button doneButton;
    [SerializeField] private CardCatalogEntry libraryEntryPrefab;
    [SerializeField] private GameObject countBadgeTemplate;
    [SerializeField] private Transform libraryGrid;
    [SerializeField] private TextMeshProUGUI libraryCountText;
    [SerializeField] private TrainingDeckRow editRowTemplate;
    [SerializeField] private Transform editList;
    [SerializeField] private TextMeshProUGUI editCountText;
    [Tooltip("종류 필터 (Key: Attack / Skill / Power)")]
    [SerializeField] private List<TrainingToggle> typeToggles = new();
    [Tooltip("코스트 칩 (Key: 0 / 1 / 2 / 3 — 3은 3 이상)")]
    [SerializeField] private List<TrainingToggle> costToggles = new();
    [SerializeField] private TrainingToggle ownerToggleTemplate;
    [SerializeField] private Transform ownerList;

    private readonly List<GameObject> _spawned = new();
    private readonly List<GameObject> _spawnedLibrary = new();
    private readonly List<GameObject> _spawnedEdit = new();
    private readonly List<GameObject> _spawnedCompanion = new();
    private CoopCharData _detailChar;
    private readonly List<TrainingToggle> _ownerToggles = new();
    private readonly List<CardData> _deck = new();
    private readonly List<string> _companions = new();
    private readonly HashSet<string> _startingKeys = new();
    private readonly Dictionary<string, string> _ownerOfCard = new(); // 카드 키 → "player" / "neutral" / charID
    private EnemyEncounter _encounter;
    private string _ownerFilter = "all";

    private void Awake()
    {
        // 비율 유지 그림은 칸보다 좁으면 피벗 쪽으로 붙는다. 적 그림이 원판 가운데 서도록 피벗만 가운데로 옮긴다(칸 위치는 그대로)
        CenterPivot(previewArt.rectTransform);
        targetRowTemplate.gameObject.SetActive(false);
        companionTemplate.gameObject.SetActive(false);
        deckRowTemplate.gameObject.SetActive(false);
        editRowTemplate.gameObject.SetActive(false);
        countBadgeTemplate.SetActive(false);
        portraitCellTemplate.gameObject.SetActive(false);
        companionBackButton.onClick.AddListener(() => ShowScreen(Screen.Main));
        companionDoneButton.onClick.AddListener(() => ShowScreen(Screen.Main));
        formationButton.onClick.AddListener(() => ShowScreen(Screen.Companion));
        foreach (var t in actToggles.Append(availableOnlyToggle))
        {
            t.SetOn(false);
            t.OnClicked += toggle => { toggle.SetOn(!toggle.IsOn); RefreshCompanionScreen(); };
        }
        actToggles.ForEach(t => t.Setup(t.name.Replace("Act_", ""), null));
        ownerToggleTemplate.gameObject.SetActive(false);

        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        sortieButton.onClick.AddListener(Sortie);
        deckEditButton.onClick.AddListener(() => ShowScreen(Screen.Deck));
        resetDeckButton.onClick.AddListener(() => { ResetDeck(); RefreshMain(); });
        deckBackButton.onClick.AddListener(() => ShowScreen(Screen.Main));
        doneButton.onClick.AddListener(() => ShowScreen(Screen.Main));

        foreach (var t in typeToggles.Concat(costToggles))
        {
            t.SetOn(false);
            t.OnClicked += toggle => { toggle.SetOn(!toggle.IsOn); RefreshLibrary(); };
        }
        typeToggles.ForEach(t => t.Setup(t.name.Replace("Type_", ""), null));
        costToggles.ForEach(t => t.Setup(t.name.Replace("Cost_", ""), null));
    }

    private void OnEnable()
    {
        _startingKeys.Clear();
        if (DeckManager.Instance != null)
            foreach (var card in DeckManager.Instance.PlayerDeck) _startingKeys.Add(PlayerRecord.KeyOf(card));
        if (_deck.Count == 0) ResetDeck();
        _companions.RemoveAll(id => !IsCompanionAvailable(companions.Find(c => c != null && c.charID == id)));
        if (_encounter != null && !IsEncounterUnlocked(_encounter)) _encounter = null;
        BuildOwnerMap();
        ShowScreen(Screen.Main);
    }

    // ── 조건 ──
    // 조우 안의 모든 적이 첫 마테리얼을 열었으면(일반 3 · 엘리트 2 · 보스 1회 격퇴) 훈련 상대로 열린다
    public static bool IsEncounterUnlocked(EnemyEncounter encounter)
        => encounter != null && encounter.enemies != null && encounter.enemies.Count > 0 &&
           encounter.enemies.All(e => e != null && MaterialArchive.IsTrainingUnlocked(e));

    private static int RequiredDefeatsOf(EnemyEncounter encounter)
        => encounter.enemies == null ? RequiredDefeats
           : encounter.enemies.Where(e => e != null).Select(e => MaterialArchive.RequiredDefeats(e, 0)).DefaultIfEmpty(RequiredDefeats).Max();

    private static int LowestDefeatCount(EnemyEncounter encounter)
        => encounter.enemies == null ? 0
           : encounter.enemies.Where(e => e != null).Select(PlayerRecord.GetDefeatCount).DefaultIfEmpty(0).Min();

    private static int CoopLevel(CoopCharData data)
    {
        int level = CooperationManager.Instance != null ? CooperationManager.Instance.GetCoopLevel(data.charID) : 0;
        if (PlayerRecord.TryGetCoop(data.charID, out int saved, out _)) level = Mathf.Max(level, saved);
        return level;
    }

    private static bool IsCompanionAvailable(CoopCharData data) => data != null && CoopLevel(data) >= data.trainingRequiredLevel;

    private bool IsCardUnlocked(CardData card) => card != null &&
        (_startingKeys.Contains(PlayerRecord.KeyOf(card)) || PlayerRecord.HasObtained(card));

    private IEnumerable<CardData> LibraryCards => cardLibrary.Where(c => c != null && !string.IsNullOrWhiteSpace(c.CardName));

    // 기본 덱 = 지금 로비의 시작 덱 (LobbyManager가 로비 진입 때 만들어 둔다)
    private void ResetDeck()
    {
        _deck.Clear();
        if (DeckManager.Instance == null) return;
        foreach (var card in DeckManager.Instance.PlayerDeck)
        {
            string key = PlayerRecord.KeyOf(card);
            _deck.Add(cardLibrary.Find(c => PlayerRecord.KeyOf(c) == key) ?? card);
        }
    }

    private void BuildOwnerMap()
    {
        _ownerOfCard.Clear();
        foreach (var ch in companions.Where(c => c != null))
            foreach (var card in ch.OwnedCards())
                _ownerOfCard[PlayerRecord.KeyOf(card)] = ch.charID;
    }

    private string OwnerOf(CardData card)
    {
        if (_ownerOfCard.TryGetValue(PlayerRecord.KeyOf(card), out string owner)) return owner;
        return card.Owner == CardData.CardOwner.Non_Color ? "neutral" : "player";
    }

    // ── 화면 전환 ──
    private enum Screen { Main, Deck, Companion }

    private void ShowScreen(Screen screen)
    {
        mainScreen.SetActive(screen == Screen.Main);
        deckScreen.SetActive(screen == Screen.Deck);
        companionScreen.SetActive(screen == Screen.Companion);
        switch (screen)
        {
            case Screen.Deck: RebuildOwnerToggles(); RefreshDeckScreen(); break;
            case Screen.Companion: RefreshCompanionScreen(); break;
            default: RefreshMain(); break;
        }
    }

    private T Spawn<T>(T template, Transform parent, List<GameObject> bucket) where T : Component
    {
        var item = Instantiate(template, parent);
        item.gameObject.SetActive(true);
        bucket.Add(item.gameObject);
        return item;
    }

    private static void Clear(List<GameObject> bucket)
    {
        foreach (var go in bucket) if (go != null) Destroy(go);
        bucket.Clear();
    }

    // ── 메인 ──
    private void RefreshMain()
    {
        Clear(_spawned);

        int unlocked = 0;
        // 열린 조우를 먼저, 같은 조건이면 일반 → 엘리트 → 보스
        foreach (var enc in encounters.Where(e => e != null).OrderByDescending(IsEncounterUnlocked).ThenBy(e => (int)e.encounterType))
        {
            bool open = IsEncounterUnlocked(enc);
            if (open) unlocked++;
            var target = enc;
            Spawn(targetRowTemplate, targetList, _spawned).Bind(enc, EncounterName(enc), LowestDefeatCount(enc), RequiredDefeatsOf(enc),
                open, enc == _encounter, () => { _encounter = _encounter == target ? null : target; RefreshMain(); });
        }
        targetCountText.text = $"{unlocked} / {encounters.Count(e => e != null)}";
        RefreshPreview();

        // 메인은 동행 칸 3개만: 편성된 동료 + 빈 칸. 어느 칸이든 누르면 동료 편성 페이지
        for (int i = 0; i < TrainingSession.MaxCompanions; i++)
        {
            var slot = Spawn(companionTemplate, companionList, _spawned);
            var data = i < _companions.Count ? companions.Find(c => c != null && c.charID == _companions[i]) : null;
            if (data != null) slot.Bind(data, CoopLevel(data), true, true, () => ShowScreen(Screen.Companion));
            else slot.BindEmpty(() => ShowScreen(Screen.Companion));
        }
        companionCountText.text = $"{_companions.Count} / {TrainingSession.MaxCompanions}";

        foreach (var group in GroupedDeck())
            Spawn(deckRowTemplate, deckSummaryList, _spawned).Bind(group.card, group.count);
        deckCountText.text = $"{_deck.Count}장";

        sortieButton.interactable = _encounter != null && _deck.Count > 0;
    }

    private static void CenterPivot(RectTransform rt)
    {
        Vector2 delta = new Vector2(0.5f, 0.5f) - rt.pivot;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition += Vector2.Scale(delta, rt.rect.size);
    }

    private void RefreshPreview()
    {
        bool has = _encounter != null;
        previewInfo.SetActive(has);
        previewEmpty.SetActive(!has);
        previewArt.enabled = has;
        if (!has) return;

        var first = _encounter.enemies.FirstOrDefault(e => e != null);
        previewArt.sprite = first == null ? null
            : first.enemyImage != null ? first.enemyImage
            : first.idleFrames != null && first.idleFrames.Length > 0 ? first.idleFrames[0] : null;
        previewArt.enabled = previewArt.sprite != null;
        previewName.text = EncounterName(_encounter);
        previewBadge.sprite = _encounter.encounterType switch
        {
            EnemyEncounterType.Elite => badgeElite,
            EnemyEncounterType.Boss  => badgeBoss,
            _                        => badgeNormal,
        };
        previewBadgeLabel.text = TypeLabel(_encounter.encounterType);
        previewBadgeLabel.color = _encounter.encounterType == EnemyEncounterType.Normal ? new Color(0.086f, 0.094f, 0.106f) : Color.white;
        previewAnalysis.text = $"분석 완료  ·  격퇴 {LowestDefeatCount(_encounter)}회";
    }

    private IEnumerable<(CardData card, int count)> GroupedDeck()
        => _deck.GroupBy(PlayerRecord.KeyOf)
                .Select(g => (g.First(), g.Count()))
                .OrderBy(g => g.Item1.EnergyCost).ThenBy(g => g.Item1.CardName);

    // ── 동료 편성 ──
    // 동료가 많아져도(막당 3명 + 이벤트 동료) 감당하도록 6열 초상 격자 + 오른쪽 선택한 동료·동행 칸으로 나눈다
    private bool PassesCompanionFilter(CoopCharData data)
    {
        if (availableOnlyToggle.IsOn && !IsCompanionAvailable(data)) return false;
        var acts = actToggles.Where(t => t.IsOn).Select(t => t.Key).ToList();
        if (acts.Count == 0) return true;
        return acts.Contains(data.isEventCompanion ? "event" : data.actNumber.ToString());
    }

    private void RefreshCompanionScreen()
    {
        Clear(_spawnedCompanion);
        var list = companions.Where(c => c != null).Where(PassesCompanionFilter)
                             .OrderByDescending(IsCompanionAvailable).ThenBy(c => c.isEventCompanion).ThenBy(c => c.actNumber).ThenBy(c => c.charName)
                             .ToList();
        foreach (var data in list)
        {
            string id = data.charID;
            var cell = Spawn(portraitCellTemplate, companionGrid, _spawnedCompanion);
            cell.Bind(data, CoopLevel(data), IsCompanionAvailable(data), _companions.Contains(id), () =>
            {
                if (_companions.Contains(id)) _companions.Remove(id);
                else if (_companions.Count < TrainingSession.MaxCompanions) _companions.Add(id);
                _detailChar = companions.Find(c => c != null && c.charID == id);
                RefreshCompanionScreen();
            });
            cell.OnHovered += c => ShowCompanionDetail(c.Data);
        }
        int available = companions.Count(c => c != null && IsCompanionAvailable(c));
        companionGridCountText.text = $"동행 가능 <b>{available}</b>  ·  전체 <b>{companions.Count(c => c != null)}</b>";

        for (int i = 0; i < partySlots.Count; i++)
        {
            var data = i < _companions.Count ? companions.Find(c => c != null && c.charID == _companions[i]) : null;
            string id = data != null ? data.charID : null;
            partySlots[i].Bind(data, () => { _companions.Remove(id); RefreshCompanionScreen(); });
        }
        partyCountText.text = $"{_companions.Count} / {TrainingSession.MaxCompanions}";

        // 상세: 마지막으로 누른 동료 → 첫 동행 → 첫 동행 가능 동료
        if (_detailChar == null)
            _detailChar = companions.Find(c => c != null && _companions.Count > 0 && c.charID == _companions[0])
                          ?? list.FirstOrDefault(IsCompanionAvailable) ?? list.FirstOrDefault();
        ShowCompanionDetail(_detailChar);
    }

    private void ShowCompanionDetail(CoopCharData data)
    {
        detailRoot.SetActive(data != null);
        if (data == null) return;
        bool available = IsCompanionAvailable(data);
        int level = CoopLevel(data);

        detailPortrait.SetSprite(available ? (data.charImage != null ? data.charImage : data.standingSprite) : data.standingSprite, new Vector2(0.55f, 0.92f), 1.7f); // 허리 위
        var raw = detailPortrait.GetComponent<RawImage>();
        raw.color = available ? Color.white : new Color(0.62f, 0.64f, 0.67f, 1f);
        detailName.text = available ? data.charName : "잠긴 동료";
        detailAffiliation.text = available ? data.affiliation : "기록 없음";
        detailAccent.color = data.themeColor;
        detailAccent.enabled = available;
        detailAffinity.text = available ? $"호감도 Lv.{level}" : $"호감도 Lv.{data.trainingRequiredLevel} 필요  <size=80%>(현재 Lv.{level})</size>";
        for (int i = 0; i < detailAffinityCells.Count; i++)
            detailAffinityCells[i].sprite = i < level ? affinityFilled : affinityEmpty;

        var card = data.joinRewardCard;
        detailCardRoot.SetActive(available && card != null);
        if (available && card != null)
        {
            detailCardArt.SetSprite(card.CardImage);
            detailCardName.text = card.CardName;
            detailCardEnergy.text = $"에너지 {card.EnergyCost}";
        }
    }

    // ── 덱 편집 ──
    private void RebuildOwnerToggles()
    {
        foreach (var t in _ownerToggles) if (t != null) Destroy(t.gameObject);
        _ownerToggles.Clear();

        var owners = new List<(string key, string label)> { ("all", "전체"), ("player", "주인공") };
        foreach (var ch in companions.Where(c => c != null))
            if (LibraryCards.Any(c => OwnerOf(c) == ch.charID && IsCardUnlocked(c)))
                owners.Add((ch.charID, string.IsNullOrWhiteSpace(ch.charName) ? ch.charID : ch.charName));
        owners.Add(("neutral", "무색"));

        foreach (var (key, label) in owners)
        {
            var toggle = Instantiate(ownerToggleTemplate, ownerList);
            toggle.gameObject.SetActive(true);
            toggle.Setup(key, label);
            toggle.SetOn(key == _ownerFilter);
            toggle.OnClicked += t => { _ownerFilter = t.Key; _ownerToggles.ForEach(o => o.SetOn(o.Key == _ownerFilter)); RefreshLibrary(); };
            _ownerToggles.Add(toggle);
        }
        if (_ownerToggles.All(t => !t.IsOn)) { _ownerFilter = "all"; _ownerToggles[0].SetOn(true); }
    }

    private void RefreshDeckScreen()
    {
        RefreshLibrary();
        RefreshEditList();
    }

    private bool PassesFilter(CardData card)
    {
        var types = typeToggles.Where(t => t.IsOn).Select(t => t.Key).ToList();
        if (types.Count > 0 && !types.Contains(card.Type.ToString())) return false;
        var costs = costToggles.Where(t => t.IsOn).Select(t => int.Parse(t.Key)).ToList();
        if (costs.Count > 0 && !costs.Contains(Mathf.Min(card.EnergyCost, 3))) return false;
        return _ownerFilter == "all" || OwnerOf(card) == _ownerFilter;
    }

    private void RefreshLibrary()
    {
        Clear(_spawnedLibrary);
        int shown = 0, hidden = 0;
        // 해금 카드 먼저, 미획득은 무기고처럼 "???" 칸으로 뒤에
        foreach (var card in LibraryCards.Where(PassesFilter)
                     .OrderByDescending(IsCardUnlocked).ThenBy(c => c.EnergyCost).ThenBy(c => c.CardName))
        {
            bool open = IsCardUnlocked(card);
            if (open) shown++; else hidden++;
            var entry = Spawn(libraryEntryPrefab, libraryGrid, _spawnedLibrary);
            entry.Bind(card, !open, "런에서 획득");
            if (!open) continue;

            var target = card;
            entry.OnClicked += _ => { _deck.Add(target); RefreshDeckScreen(); };
            int inDeck = _deck.Count(c => PlayerRecord.KeyOf(c) == PlayerRecord.KeyOf(card));
            if (inDeck > 0)
            {
                var badge = Instantiate(countBadgeTemplate, entry.transform);
                badge.SetActive(true);
                badge.GetComponentInChildren<TextMeshProUGUI>().text = $"× {inDeck}";
            }
        }
        libraryCountText.text = hidden > 0 ? $"{shown}종  ·  미획득 {hidden}" : $"{shown}종";
    }

    private void RefreshEditList()
    {
        Clear(_spawnedEdit);
        foreach (var group in GroupedDeck())
        {
            var card = group.card;
            var row = Spawn(editRowTemplate, editList, _spawnedEdit);
            // 마지막 1장은 뺄 수 없다 (덱 최소 1장)
            row.Bind(card, group.count,
                _deck.Count > 1 ? () => { _deck.Remove(_deck.First(c => PlayerRecord.KeyOf(c) == PlayerRecord.KeyOf(card))); RefreshDeckScreen(); } : null,
                () => { _deck.Add(card); RefreshDeckScreen(); });
        }
        editCountText.text = $"{_deck.Count}장";
    }

    // ── 표시 ──
    private static string EncounterName(EnemyEncounter enc)
    {
        if (!string.IsNullOrWhiteSpace(enc.bossTitle))
        {
            // "차원의 공포 (Dimensional Horror)" → 목록에는 한글 이름만
            int paren = enc.bossTitle.IndexOf(" (", System.StringComparison.Ordinal);
            return paren > 0 ? enc.bossTitle.Substring(0, paren) : enc.bossTitle;
        }
        var names = enc.enemies?.Where(e => e != null).Select(e => e.enemyName).Distinct().ToList();
        return names != null && names.Count > 0 ? string.Join(" · ", names) : enc.name;
    }

    private static string TypeLabel(EnemyEncounterType type) => type switch
    {
        EnemyEncounterType.Elite => "엘리트",
        EnemyEncounterType.Boss  => "보스",
        _                        => "일반",
    };

    // ── 출격 ──
    private void Sortie()
    {
        if (_encounter == null || _deck.Count == 0) return;
        if (!Application.CanStreamedLevelBeLoaded(runSceneName))
        {
            Debug.LogWarning($"[TrainingView] 전투 씬 '{runSceneName}'이 Build Settings에 없음");
            return;
        }
        sortieButton.interactable = false;
        TrainingSession.Begin(_encounter, _deck, _companions);
        SceneTransition.Load(runSceneName, "훈련", "모의 전투를 시작합니다", "TRAINING  //  SIMULATION", "SIM");
    }
}
