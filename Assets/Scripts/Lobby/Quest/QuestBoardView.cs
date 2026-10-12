using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로비 의뢰 게시판. QuestManager 데이터로 탭(게시 중·진행 중·완료 기록), 분류·유형 필터, 카드 목록, 상세를 채운다.
// 수주하면 카드에 도장이 쾅 찍히고, 로비 복귀 후 처음 열 때 지난 런에서 완료한 의뢰가 완료 기록 탭에서 완료 도장과 함께 보인다.
public class QuestBoardView : MonoBehaviour
{
    public enum Tab { Posted, Active, History }

    [System.Serializable]
    public class TabButton
    {
        public Tab tab;
        public Button button;
        public Image image;
        public TextMeshProUGUI label;
        public Sprite normal, normalHover, selected, selectedHover;
    }

    [System.Serializable]
    public class FilterRow
    {
        public Button button;
        public Image image;
        public TextMeshProUGUI countText;
        public int group;   // 0 = 분류(하나만: all / story / rescue / normal), 1 = 유형(여러 개: recovery / delivery / hunt)
        public string key;
        [System.NonSerialized] public bool on;
    }

    [Header("헤더")]
    [SerializeField] private TextMeshProUGUI expValue;
    [SerializeField] private Image expFill;

    [Header("탭·필터")]
    [SerializeField] private List<TabButton> tabs = new();
    [SerializeField] private List<FilterRow> filterRows = new();
    [SerializeField] private Sprite rowOn, rowOnHover, rowOff, rowOffHover;
    [SerializeField] private TextMeshProUGUI activeText;
    [SerializeField] private Image activeFill;

    [Header("목록")]
    [SerializeField] private QuestCardEntry cardPrefab;
    [SerializeField] private Transform listContent;
    [SerializeField] private TextMeshProUGUI listTitle;
    [SerializeField] private TextMeshProUGUI listCount;
    [SerializeField] private GameObject emptyMessage;

    [Header("상세")]
    [SerializeField] private GameObject detailRoot;
    [SerializeField] private UICroppedArt detailEmblem;
    [SerializeField] private TextMeshProUGUI detailTitle;
    [SerializeField] private Image detailBadge;
    [SerializeField] private TextMeshProUGUI detailBadgeText;
    [SerializeField] private TextMeshProUGUI detailClient;
    [SerializeField] private TextMeshProUGUI detailDesc;
    [SerializeField] private List<Image> objectiveChecks = new();
    [SerializeField] private List<TextMeshProUGUI> objectiveTexts = new();
    [SerializeField] private GameObject itemGroup;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI rewardValue;
    [SerializeField] private TextMeshProUGUI rewardExtra;
    [SerializeField] private Sprite checkOn, checkOff, badgeStory, badgeRescue;

    [Header("수주 버튼")]
    [SerializeField] private Button actionButton;
    [SerializeField] private Image actionImage;
    [SerializeField] private TextMeshProUGUI actionLabel;
    [SerializeField] private Sprite acceptNormal, acceptHover, acceptDisabled, cancelNormal, cancelHover;

    private readonly List<QuestCardEntry> _cards = new();
    private QuestCardEntry _selected;
    private Tab _tab = Tab.Posted;
    private List<string> _justCompleted = new();

    private QuestManager Q => QuestManager.Instance;

    private void Awake()
    {
        foreach (var t in tabs) { var tb = t; tb.button.onClick.AddListener(() => SwitchTab(tb.tab)); }
        foreach (var r in filterRows) { var fr = r; fr.button.onClick.AddListener(() => ToggleFilter(fr)); fr.on = fr.group == 0 && fr.key == "all"; }
        actionButton.onClick.AddListener(OnAction);
    }

    private void OnEnable()
    {
        // 지난 런에서 완료한 의뢰가 있으면 완료 기록 탭으로 열어 도장을 찍는다
        _justCompleted = Q.ConsumeJustCompleted();
        _tab = _justCompleted.Count > 0 ? Tab.History : Tab.Posted;
        Refresh();
    }

    private void SwitchTab(Tab tab)
    {
        if (_tab == tab) return;
        _tab = tab;
        _justCompleted.Clear();
        Refresh();
    }

    public void Refresh()
    {
        RefreshHeader();
        RefreshTabs();
        RebuildList();
    }

    private void RefreshHeader()
    {
        var exp = UserExpManager.Instance;
        int cur = exp != null ? exp.Experience : 0, max = exp != null ? Mathf.Max(1, exp.MaxExperience) : 100;
        if (expValue != null) expValue.text = $"{cur} <size=70%>/ {max}</size>";
        if (expFill != null) expFill.fillAmount = Mathf.Clamp01((float)cur / max);
        if (activeText != null) activeText.text = $"진행 중  <size=120%>{Q.ActiveCount}</size> / {Q.MaxActive}";
        if (activeFill != null) activeFill.fillAmount = Q.MaxActive > 0 ? (float)Q.ActiveCount / Q.MaxActive : 0f;
    }

    private void RefreshTabs()
    {
        foreach (var t in tabs)
        {
            bool sel = t.tab == _tab;
            t.image.sprite = sel ? t.selected : t.normal;
            var ss = t.button.spriteState; ss.highlightedSprite = sel ? t.selectedHover : t.normalHover; ss.pressedSprite = ss.highlightedSprite; t.button.spriteState = ss;
            t.label.fontStyle = sel ? FontStyles.Bold : FontStyles.Normal;
            t.label.text = t.tab switch
            {
                Tab.Posted => $"게시 중 ({Q.Posted().Count})",
                Tab.Active => $"진행 중 ({Q.ActiveCount}/{Q.MaxActive})",
                _ => "완료 기록",
            };
        }
        if (listTitle != null) listTitle.text = _tab switch { Tab.Posted => "의뢰 목록", Tab.Active => "진행 중인 의뢰", _ => "완료한 의뢰" };
    }

    private List<QuestData> Source() => _tab switch
    {
        Tab.Posted => Q.Posted(),
        Tab.Active => new List<QuestData>(Q.Accepted),
        _ => Q.History(),
    };

    private void RebuildList()
    {
        foreach (var c in _cards) if (c != null) Destroy(c.gameObject);
        _cards.Clear();
        _selected = null;

        var source = Source();
        RefreshCounts(source);
        foreach (var quest in source)
        {
            var card = Instantiate(cardPrefab, listContent);
            bool completed = _tab == Tab.History;
            card.Bind(quest, !completed && !Q.IsUnlocked(quest), Q.IsAccepted(quest.questId), completed);
            card.OnClicked += Select;
            _cards.Add(card);
        }
        ApplyFilters();

        // 방금 완료된 의뢰는 차례로 완료 도장을 찍는다
        float delay = 0.55f;
        foreach (var c in _cards)
            if (_justCompleted.Contains(c.Quest.questId)) { c.PlayStamp(true, delay); delay += 0.25f; }
    }

    // ── 필터 ──

    private void ToggleFilter(FilterRow row)
    {
        if (row.group == 0) { foreach (var r in filterRows) if (r.group == 0) r.on = r == row; }
        else row.on = !row.on;
        ApplyFilters();
    }

    private static string KeyOf(QuestData q, int group) => group == 0
        ? (q.tag == QuestTag.Story ? "story" : q.tag == QuestTag.Rescue ? "rescue" : "normal")
        : (q.goal == QuestGoal.DeliverToNode ? "delivery" : q.goal == QuestGoal.DefeatCount ? "hunt" : "recovery");

    private bool Matches(QuestData q)
    {
        for (int g = 0; g < 2; g++)
        {
            bool any = false, hit = false;
            foreach (var r in filterRows)
            {
                if (r.group != g || !r.on) continue;
                if (g == 0 && r.key == "all") { any = false; break; }
                any = true;
                if (KeyOf(q, g) == r.key) hit = true;
            }
            if (any && !hit) return false;
        }
        return true;
    }

    private void RefreshCounts(List<QuestData> source)
    {
        foreach (var r in filterRows)
        {
            if (r.countText == null) continue;
            int n = 0;
            foreach (var q in source) if (r.key == "all" || KeyOf(q, r.group) == r.key) n++;
            r.countText.text = n.ToString();
        }
    }

    private void ApplyFilters()
    {
        foreach (var r in filterRows)
        {
            r.image.sprite = r.on ? rowOn : rowOff;
            var ss = r.button.spriteState; ss.highlightedSprite = r.on ? rowOnHover : rowOffHover; ss.pressedSprite = ss.highlightedSprite; r.button.spriteState = ss;
        }
        QuestCardEntry first = null, firstUnlocked = null;
        int shown = 0;
        foreach (var c in _cards)
        {
            bool v = Matches(c.Quest);
            c.gameObject.SetActive(v);
            if (!v) continue;
            shown++;
            first ??= c;
            if (!c.Locked) firstUnlocked ??= c;
        }
        if (listCount != null) listCount.text = _tab == Tab.Posted ? $"게시 <b>{shown}</b>" : $"<b>{shown}</b>건";
        if (emptyMessage != null) emptyMessage.SetActive(shown == 0);
        if (_selected == null || !_selected.gameObject.activeSelf) Select(firstUnlocked ?? first);
    }

    // ── 상세·수주 ──

    private void Select(QuestCardEntry card)
    {
        if (_selected != null) _selected.SetSelected(false);
        _selected = card;
        detailRoot.SetActive(card != null);
        if (card == null) { SetAction(ActionState.Disabled, "의뢰를 선택하세요"); return; }
        card.SetSelected(true);
        var q = card.Quest;

        detailEmblem.gameObject.SetActive(q.clientEmblem != null);
        if (q.clientEmblem != null) detailEmblem.SetSprite(q.clientEmblem);
        detailTitle.text = card.Locked ? "???" : q.title;
        detailBadge.gameObject.SetActive(q.tag != QuestTag.None);
        if (q.tag != QuestTag.None)
        {
            bool story = q.tag == QuestTag.Story;
            detailBadge.sprite = story ? badgeStory : badgeRescue;
            detailBadgeText.text = story ? "STORY" : "구출";
            detailBadgeText.color = story ? Color.white : new Color(0.05f, 0.72f, 0.95f, 1f);
        }
        detailClient.text = $"{q.client}  ·  {q.TypeName}";
        detailDesc.text = card.Locked ? (string.IsNullOrEmpty(q.lockedHint) ? "게시 조건을 채우면 열립니다." : q.lockedHint) : q.description;

        var objectives = card.Locked ? new List<string>() : q.Objectives();
        bool done = _tab == Tab.History;
        for (int i = 0; i < objectiveTexts.Count; i++)
        {
            bool has = i < objectives.Count;
            objectiveTexts[i].gameObject.SetActive(has);
            objectiveChecks[i].gameObject.SetActive(has);
            if (!has) continue;
            objectiveTexts[i].text = objectives[i];
            objectiveChecks[i].sprite = done ? checkOn : checkOff;
        }

        bool hasItem = !card.Locked && q.questItem != null;
        itemGroup.SetActive(hasItem);
        if (hasItem) { itemIcon.sprite = q.questItem.ItemIcon; itemName.text = q.questItem.ItemName; }

        rewardValue.text = card.Locked ? "???" : $"{q.rewardExp} <size=75%>EXP</size>";
        rewardExtra.text = q.tag == QuestTag.Story ? "+ 스토리 단서" : q.tag == QuestTag.Rescue ? "+ 동료 구출" : "";

        if (done) SetAction(ActionState.Disabled, "완료한 의뢰");
        else if (card.Locked) SetAction(ActionState.Disabled, "게시 조건 미달");
        else if (Q.IsAccepted(q.questId)) SetAction(ActionState.Cancel);
        else if (Q.ActiveCount >= Q.MaxActive) SetAction(ActionState.Disabled, "수주 한도 도달");
        else SetAction(ActionState.Accept);
    }

    private enum ActionState { Accept, Cancel, Disabled, Hidden }
    private ActionState _action;

    private void SetAction(ActionState state, string disabledText = null)
    {
        _action = state;
        actionButton.gameObject.SetActive(state != ActionState.Hidden);
        actionButton.interactable = state != ActionState.Disabled;
        bool cancel = state == ActionState.Cancel;
        actionImage.sprite = state == ActionState.Disabled ? acceptDisabled : cancel ? cancelNormal : acceptNormal;
        var ss = actionButton.spriteState; ss.highlightedSprite = cancel ? cancelHover : acceptHover; ss.pressedSprite = ss.highlightedSprite; ss.disabledSprite = acceptDisabled; actionButton.spriteState = ss;
        actionLabel.text = state == ActionState.Disabled ? disabledText : cancel ? "수주 취소" : "의뢰 수주";
        actionLabel.color = cancel ? new Color(0.1f, 0.11f, 0.12f, 1f) : state == ActionState.Disabled ? new Color(0.45f, 0.46f, 0.48f, 1f) : Color.white;
    }

    private void OnAction()
    {
        if (_selected == null) return;
        var q = _selected.Quest;
        if (_action == ActionState.Accept && Q.Accept(q))
        {
            _selected.PlayStamp(false);
        }
        else if (_action == ActionState.Cancel)
        {
            Q.Abandon(q);
            if (_tab == Tab.Active) { RefreshHeader(); RefreshTabs(); RebuildList(); return; }
            _selected.Bind(q, false, false, false);
            _selected.SetSelected(true);
        }
        RefreshHeader();
        RefreshTabs();
        Select(_selected);
    }
}
