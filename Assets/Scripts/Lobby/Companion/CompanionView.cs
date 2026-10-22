using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로비 동료(04) 화면. ① 목록: 동료 카드 6열(정렬 합류순 / 호감도 / 막) → 합류한 동료를 누르면 ② 상세.
// 상세: 전신 + 대사 한 줄(campLines, ‹ ›로 넘김) + 이름 블록 + 오른쪽 정보 판 4개(호감도 · 지시 스킬 · 동료 카드 · 단계별 해금).
// 대화·선물·랭크업 이벤트는 베이스캠프 몫이라 여기엔 없다. 좌하단 동료 버튼이 SetActive(true)로 연다.
// 아트: Assets/Art/Lobby/Companion (원본 ArtDirection/CompanionTabMockup/Extracted_Draft, layout.json 1920×1080)
public class CompanionView : MonoBehaviour
{
    private enum Sort { Joined, Bond, Act }

    [SerializeField] private List<CoopCharData> companions = new();
    [SerializeField] private Button closeButton;

    [Header("목록")]
    [SerializeField] private GameObject listPage;
    [SerializeField] private CompanionCard cardTemplate;
    [SerializeField] private Transform grid;
    [SerializeField, Min(6)] private int minSlots = 12;
    [SerializeField] private Button[] sortButtons = new Button[3];
    [SerializeField] private TextMeshProUGUI[] sortLabels = new TextMeshProUGUI[3];
    [SerializeField] private RectTransform sortUnderline;

    [Header("상세")]
    [SerializeField] private GameObject detailPage;
    [SerializeField] private Button detailBackButton;
    [SerializeField] private Image fullArt;
    [SerializeField] private TextMeshProUGUI quoteText;
    [SerializeField] private TextMeshProUGUI quoteIndex;
    [SerializeField] private Button quotePrev, quoteNext;
    [SerializeField] private Button charPrev, charNext;
    [SerializeField] private TextMeshProUGUI factionText, nameText, englishText;
    [SerializeField] private TextMeshProUGUI bondValue, bondMax, bondRemaining;
    [SerializeField] private Image[] bondSegments;
    [SerializeField] private Sprite segEmpty, segFilled;
    [SerializeField] private Image skillIcon;
    [SerializeField] private Sprite skillIconFallback;   // 스킬 데이터에 아이콘이 없을 때
    [SerializeField] private TextMeshProUGUI skillName, skillDesc, skillCharge;
    [SerializeField] private GameObject skillEnhanceTag;
    [SerializeField] private Image[] cardFrames;
    [SerializeField] private Image[] cardArts;
    [SerializeField] private TextMeshProUGUI[] cardNames;
    [SerializeField] private GameObject[] cardLocks;
    [SerializeField] private Sprite cardFrameOpen, cardFrameLocked;
    [SerializeField] private Image[] unlockMarks;
    [SerializeField] private TextMeshProUGUI[] unlockLevels, unlockContents;
    [SerializeField] private Sprite markOpen, markLocked;
    [SerializeField] private Button recordButton, campButton;
    [SerializeField] private MaterialView materialView;
    [SerializeField] private Button campMenuButton;   // 로비 메뉴의 베이스캠프 버튼

    [Header("색")]
    [SerializeField] private Color inkColor = new(0.086f, 0.094f, 0.106f, 1f);
    [SerializeField] private Color grayColor = new(0.573f, 0.6f, 0.635f, 1f);

    private static readonly string[] Roman = { "I", "II", "III", "IV", "V", "VI", "VII" };
    private readonly List<CompanionCard> _cards = new();
    private Sort _sort = Sort.Joined;
    private List<CoopCharData> _joined = new();
    private CoopCharData _current;
    private int _line;

    private void Awake()
    {
        cardTemplate.gameObject.SetActive(false);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        for (int i = 0; i < sortButtons.Length; i++) { var s = (Sort)i; sortButtons[i].onClick.AddListener(() => { _sort = s; BuildList(); }); }
        detailBackButton.onClick.AddListener(ShowList);
        quotePrev.onClick.AddListener(() => StepLine(-1));
        quoteNext.onClick.AddListener(() => StepLine(+1));
        charPrev.onClick.AddListener(() => StepChar(-1));
        charNext.onClick.AddListener(() => StepChar(+1));
        recordButton.onClick.AddListener(() => { if (materialView != null) materialView.Open(companions: true); });
        campButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            if (campMenuButton != null && campMenuButton.interactable) campMenuButton.onClick.Invoke();
        });
    }

    private void OnEnable() => ShowList();

    // ── 진행 정보 ──
    private static int Level(CoopCharData d)
    {
        int level = CooperationManager.Instance != null ? CooperationManager.Instance.GetCoopLevel(d.charID) : 0;
        if (PlayerRecord.TryGetCoop(d.charID, out int saved, out _)) level = Mathf.Max(level, saved);
        return level;
    }

    private static int Point(CoopCharData d) => PlayerRecord.TryGetCoop(d.charID, out _, out int p) ? p : 0;

    private static bool IsJoined(CoopCharData d) => Level(d) > 0 || MetaProgress.HasMilestone("join:" + d.charID);

    private static string Condition(CoopCharData d) => d.isEventCompanion ? "특수 이벤트" : $"{d.actNumber}막 합류";

    // ── 목록 ──
    private void ShowList()
    {
        detailPage.SetActive(false);
        listPage.SetActive(true);
        BuildList();
    }

    private void BuildList()
    {
        foreach (var c in _cards) if (c != null) Destroy(c.gameObject);
        _cards.Clear();

        var all = companions.Where(c => c != null).ToList();
        IEnumerable<CoopCharData> ordered = _sort switch
        {
            Sort.Bond => all.OrderByDescending(IsJoined).ThenByDescending(Level),
            Sort.Act => all.OrderBy(c => c.isEventCompanion ? 99 : c.actNumber),
            _ => all.OrderByDescending(IsJoined),
        };
        _joined = ordered.Where(IsJoined).ToList();

        int n = 0;
        foreach (var d in ordered)
        {
            var card = NewCard();
            bool joined = IsJoined(d);
            card.Bind(d, ++n, joined, Level(d), Condition(d));
            card.OnClicked += c => ShowDetail(c.Data);
        }
        int slots = Mathf.Max(minSlots, Mathf.CeilToInt(n / 6f) * 6);
        while (n < slots) NewCard().BindEmpty(++n);

        for (int i = 0; i < sortLabels.Length; i++)
            sortLabels[i].fontStyle = i == (int)_sort ? FontStyles.Bold : FontStyles.Normal;
        if (sortUnderline != null && sortButtons[(int)_sort] != null)
        {
            var target = (RectTransform)sortButtons[(int)_sort].transform;
            sortUnderline.anchoredPosition = new Vector2(target.anchoredPosition.x, sortUnderline.anchoredPosition.y);
        }
    }

    private CompanionCard NewCard()
    {
        var card = Instantiate(cardTemplate, grid);
        card.gameObject.SetActive(true);
        _cards.Add(card);
        return card;
    }

    // ── 상세 ──
    private void StepChar(int dir)
    {
        if (_joined.Count == 0 || _current == null) return;
        int i = _joined.IndexOf(_current);
        ShowDetail(_joined[((i + dir) % _joined.Count + _joined.Count) % _joined.Count]);
    }

    private void StepLine(int dir)
    {
        if (_current == null || _current.campLines.Count == 0) return;
        _line = ((_line + dir) % _current.campLines.Count + _current.campLines.Count) % _current.campLines.Count;
        ApplyLine();
    }

    private void ApplyLine()
    {
        var lines = _current.campLines;
        bool has = lines != null && lines.Count > 0;
        quoteText.text = has ? $"「{lines[_line]}」" : "";
        quoteIndex.text = has ? $"LINE {_line + 1:00} / {lines.Count:00}" : "";
        quotePrev.gameObject.SetActive(has && lines.Count > 1);
        quoteNext.gameObject.SetActive(has && lines.Count > 1);
    }

    private void ShowDetail(CoopCharData d)
    {
        if (d == null) return;
        _current = d;
        _line = 0;
        listPage.SetActive(false);
        detailPage.SetActive(true);
        charPrev.gameObject.SetActive(_joined.Count > 1);
        charNext.gameObject.SetActive(_joined.Count > 1);

        var art = d.RosterFullArt != null ? d.RosterFullArt : d.charImage;
        fullArt.sprite = art;
        fullArt.enabled = art != null;
        ApplyLine();

        factionText.text = d.affiliation;
        nameText.text = d.charName;
        englishText.text = EnglishName(d);

        // 호감도
        int level = Level(d), max = Mathf.Max(1, d.maxCoopLevel);
        bondValue.text = level.ToString();
        bondMax.text = $"/ {max}";
        for (int i = 0; i < bondSegments.Length; i++)
        {
            bondSegments[i].gameObject.SetActive(i < max);
            bondSegments[i].sprite = i < level ? segFilled : segEmpty;
        }
        var next = d.rankEventDatas?.Find(r => r != null && r.targetLevel == level + 1);
        bondRemaining.text = level >= max ? "최대 호감도" : next != null ? $"다음 단계까지 <b>{Mathf.Max(0, next.requiredPoint - Point(d))} PT</b>" : "";

        // 지시 스킬
        var skill = d.skill;
        bool enhanced = level >= max && d.maxCoopLevel > 0;
        skillIcon.sprite = skill != null && skill.icon != null ? skill.icon : skillIconFallback;
        skillIcon.enabled = skillIcon.sprite != null;
        skillName.text = skill != null ? skill.skillName : "없음";
        skillDesc.text = skill != null ? skill.GetDescription(enhanced) : "";
        skillCharge.text = skill != null ? ChargeText(skill, enhanced) : "";
        skillEnhanceTag.SetActive(skill != null && skill.HasEnhancement);

        // 동료 카드: 합류 카드 + 호감도 레벨 카드(Lv1부터 한 장씩)
        var cards = new List<(CardData card, bool open)>();
        if (d.joinRewardCard != null) cards.Add((d.joinRewardCard, true));
        for (int i = 0; i < d.unlockCardCoopLevel.Count; i++)
            if (d.unlockCardCoopLevel[i] != null) cards.Add((d.unlockCardCoopLevel[i], level >= i + 1));
        for (int i = 0; i < cardFrames.Length; i++)
        {
            bool has = i < cards.Count;
            cardFrames[i].gameObject.SetActive(has);
            if (!has) continue;
            bool open = cards[i].open;
            cardFrames[i].sprite = open ? cardFrameOpen : cardFrameLocked;
            cardArts[i].sprite = cards[i].card.CardImage;
            cardArts[i].enabled = open && cards[i].card.CardImage != null;
            cardNames[i].text = open ? cards[i].card.BaseCardName : "???";
            cardNames[i].color = open ? inkColor : grayColor;
            cardLocks[i].SetActive(!open);
        }

        // 단계별 해금: 매 레벨 기록 1편 + 그 레벨 카드 + 최대 레벨 스킬 강화
        for (int i = 0; i < unlockLevels.Length; i++)
        {
            int lv = i + 1;
            bool used = lv <= max;
            unlockMarks[i].gameObject.SetActive(used);
            unlockLevels[i].gameObject.SetActive(used);
            unlockContents[i].gameObject.SetActive(used);
            if (!used) continue;
            bool open = level >= lv;
            var parts = new List<string> { $"동료 기록 {Roman[Mathf.Min(i, Roman.Length - 1)]}" };
            if (i < d.unlockCardCoopLevel.Count && d.unlockCardCoopLevel[i] != null) parts.Add($"카드 「{d.unlockCardCoopLevel[i].BaseCardName}」");
            if (lv == max && skill != null && skill.HasEnhancement) parts.Add("지시 스킬 강화");
            unlockLevels[i].text = $"Lv.{lv}";
            unlockContents[i].text = string.Join("  ·  ", parts);
            unlockMarks[i].sprite = open ? markOpen : markLocked;
            var col = open ? inkColor : grayColor;
            unlockLevels[i].color = col;
            unlockContents[i].color = col;
            unlockContents[i].fontStyle = open ? FontStyles.Bold : FontStyles.Normal;
        }
    }

    private static string ChargeText(CompanionSkillData s, bool enhanced)
    {
        int n = s.ChargeOf(enhanced);
        return s.chargeType == CompanionSkillCharge.Battles ? $"{n}전투마다 사용 가능" : $"{n}턴마다 사용 가능";
    }

    // 영문 이름: 에셋에 따로 없으니 로마자 표기 표를 둔다 (없으면 charID)
    private static string EnglishName(CoopCharData d) => d.charID switch
    {
        "Cp_01" => "C H I H A R A   S H O",
        "Cp_Event_01" => "I C A R U S",
        _ => d.charID,
    };
}
