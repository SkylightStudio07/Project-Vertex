using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 무기고 카드 목록 탭. 플레이어가 만날 수 있는 카드 풀을 그리드로 보여주고, 고른 카드를 상세 패널에 띄운다.
// 카드 풀 = 시작 덱 + 플레이어 보상 풀 + 합류 캐릭터 카드(합류 획득·보상 풀·호감도 해금).
// 호감도 해금 카드는 현재 호감도 레벨이 모자라면 잠긴 칸으로 보인다.
public class CardCatalogView : MonoBehaviour
{
    [Header("카드 풀")]
    [SerializeField] private TentInteractionHandler startingDeckSource;
    [SerializeField] private PlayerRewardPoolSO playerRewardPool;
    [SerializeField] private List<CoopCharData> coopCharacters = new();

    [Header("그리드")]
    [SerializeField] private CardCatalogEntry entryPrefab;
    [SerializeField] private Transform gridContent;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI collectionText;
    [SerializeField] private Image collectionFill;

    [Header("필터")]
    [SerializeField] private List<FilterRow> filterRows = new();
    [SerializeField] private Sprite rowOn, rowOnHover, rowOff, rowOffHover;

    // group 0 = 소속(하나만: all / player / neutral / 합류 캐릭터 ID), 1 = 종류, 2 = 등급 (여러 개, 아무것도 안 고르면 전체)
    [System.Serializable]
    public class FilterRow
    {
        public Button button;
        public Image image;
        public TextMeshProUGUI countText;
        public int group;
        public string key;
        [System.NonSerialized] public bool on;
    }

    [Header("상세")]
    [SerializeField] private UICroppedArt detailArt;
    [SerializeField] private Sprite lockedArt; // 잠긴 카드 상세에 보여줄 실루엣
    [SerializeField] private TextMeshProUGUI detailName;
    [SerializeField] private TextMeshProUGUI detailType;
    [SerializeField] private TextMeshProUGUI detailCurrent;   // 현재 효과 (없으면 비교 글만)
    [SerializeField] private TextMeshProUGUI detailCompare;
    [SerializeField] private TextMeshProUGUI detailUnlock;
    [SerializeField] private TextMeshProUGUI detailUpgrade;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image upgradeImage;
    [SerializeField] private TextMeshProUGUI upgradeLabel;
    [SerializeField] private Sprite upgradeNormal;
    [SerializeField] private Sprite upgradeDisabled;

    [Header("카드 강화 비용 (탈린 + 파츠, CardUpgrades)")]
    [SerializeField] private GameObject costRoot;
    [SerializeField] private TextMeshProUGUI talinCostText;
    [SerializeField] private TextMeshProUGUI partsCostText;
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private Color paperColor = new(0.933f, 0.941f, 0.949f, 1f);
    [SerializeField] private Color grayColor = new(0.573f, 0.604f, 0.639f, 1f);

    private class Entry { public CardData card; public string source; public string ownerKey; public bool locked; public string lockCondition; }

    private void Awake()
    {
        foreach (var row in filterRows)
        {
            var r = row;
            if (r.button != null) r.button.onClick.AddListener(() => ToggleFilter(r));
            r.on = r.group == 0 && r.key == "all";
        }
        if (upgradeButton != null) upgradeButton.onClick.AddListener(UpgradeSelected);
    }

    private void UpgradeSelected()
    {
        if (_selected == null || _selected.Locked) return;
        if (CardUpgrades.TryUpgrade(_selected.Card) != CardUpgrades.Result.Ok) return;
        _selected.RefreshUpgraded();
        Select(_selected);
    }

    private readonly List<CardCatalogEntry> _views = new();
    private CardCatalogEntry _selected;

    private void OnEnable()
    {
        MetaProgress.OnChanged += RefreshSelected;
        Rebuild();
    }

    private void OnDisable() => MetaProgress.OnChanged -= RefreshSelected;

    private void RefreshSelected() { if (_selected != null) Select(_selected); }

    public void Rebuild()
    {
        foreach (var v in _views) if (v != null) Destroy(v.gameObject);
        _views.Clear();
        _sources.Clear();
        _selected = null;

        var entries = CollectPool();
        int unlocked = 0;
        foreach (var e in entries)
        {
            var view = Instantiate(entryPrefab, gridContent);
            view.Bind(e.card, e.locked, e.lockCondition);
            view.OnClicked += Select;
            _views.Add(view);
            _sources[view] = e;
            if (!e.locked) unlocked++;
        }

        if (countText != null) countText.text = $"전체 <b>{entries.Count}</b>";
        if (collectionText != null) collectionText.text = $"수집 <size=130%>{unlocked}</size> / {entries.Count}";
        if (collectionFill != null) collectionFill.fillAmount = entries.Count > 0 ? (float)unlocked / entries.Count : 0f;

        RefreshCounts();
        ApplyFilters();
    }

    // ── 필터 ──

    private void ToggleFilter(FilterRow row)
    {
        if (row.group == 0)
        {
            foreach (var r in filterRows) if (r.group == 0) r.on = r == row; // 소속은 하나만
        }
        else row.on = !row.on;
        ApplyFilters();
    }

    private bool Matches(Entry e)
    {
        for (int g = 0; g < 3; g++)
        {
            bool any = false, hit = false;
            foreach (var r in filterRows)
            {
                if (r.group != g || !r.on) continue;
                if (g == 0 && r.key == "all") { any = false; break; }
                any = true;
                if (KeyOf(e, g) == r.key) hit = true;
            }
            if (any && !hit) return false;
        }
        return true;
    }

    private static string KeyOf(Entry e, int group) => group switch
    {
        0 => e.ownerKey,
        1 => e.card.Type.ToString(),
        _ => e.card.Rarity.ToString(),
    };

    private void ApplyFilters()
    {
        foreach (var r in filterRows)
        {
            if (r.image == null) continue;
            r.image.sprite = r.on ? rowOn : rowOff;
            var ss = r.button.spriteState; ss.highlightedSprite = r.on ? rowOnHover : rowOffHover; ss.pressedSprite = ss.highlightedSprite; r.button.spriteState = ss;
        }

        CardCatalogEntry firstVisible = null;
        int shown = 0;
        foreach (var v in _views)
        {
            bool visible = Matches(_sources[v]);
            v.gameObject.SetActive(visible);
            if (!visible) continue;
            shown++;
            if (firstVisible == null) firstVisible = v;
        }
        if (countText != null) countText.text = $"표시 <b>{shown}</b> / {_views.Count}";
        if (_selected == null || !_selected.gameObject.activeSelf) Select(firstVisible);
    }

    // 필터 옆 개수는 카드 풀 전체 기준
    private void RefreshCounts()
    {
        foreach (var r in filterRows)
        {
            if (r.countText == null) continue;
            int n = 0;
            foreach (var e in _sources.Values)
                if (r.group == 0 ? (r.key == "all" || e.ownerKey == r.key) : KeyOf(e, r.group) == r.key) n++;
            r.countText.text = n.ToString();
        }
    }

    private readonly Dictionary<CardCatalogEntry, Entry> _sources = new();

    private List<Entry> CollectPool()
    {
        var list = new List<Entry>();
        var seen = new HashSet<string>();
        string ownerKey = null;
        void Add(CardData card, string source, bool locked = false, string condition = null)
        {
            if (card == null) return;
            string key = card.name.Replace("(Clone)", "").Trim();
            if (!seen.Add(key)) return; // 시작 덱의 같은 카드 여러 장은 한 칸으로
            string owner = ownerKey ?? (card.Owner == CardData.CardOwner.Non_Color ? "neutral" : "player");
            list.Add(new Entry { card = card, source = source, ownerKey = owner, locked = locked, lockCondition = condition });
        }

        if (startingDeckSource != null)
            foreach (var c in startingDeckSource.StartingDeckCards) Add(c, "시작 덱 포함");
        if (playerRewardPool != null)
        {
            foreach (var c in playerRewardPool.commonCards) Add(c, "전투 보상 · 일반");
            foreach (var c in playerRewardPool.rareCards) Add(c, "전투 보상 · 특별");
            foreach (var c in playerRewardPool.uniqueCards) Add(c, "전투 보상 · 희귀");
        }
        var coop = CooperationManager.Instance;
        foreach (var ch in coopCharacters)
        {
            if (ch == null) continue;
            ownerKey = ch.charID;
            Add(ch.joinRewardCard, $"{ch.charName} 합류 시 획득");
            foreach (var c in ch.rewardPoolCommon) Add(c, $"{ch.charName} 합류 후 전투 보상");
            foreach (var c in ch.rewardPoolRare) Add(c, $"{ch.charName} 합류 후 전투 보상");
            foreach (var c in ch.rewardPoolUnique) Add(c, $"{ch.charName} 합류 후 전투 보상");
            int level = coop != null ? coop.GetCoopLevel(ch.charID) : 0;
            for (int i = 0; i < ch.unlockCardCoopLevel.Count; i++)
            {
                string cond = $"{ch.charName} 호감도 {i + 1}";
                Add(ch.unlockCardCoopLevel[i], cond + " 해금", level < i + 1, cond);
            }
        }
        ownerKey = null;
        return list;
    }

    private void Select(CardCatalogEntry view)
    {
        if (_selected != null) _selected.SetSelected(false);
        _selected = view;
        if (view == null) return;
        view.SetSelected(true);
        var e = _sources[view];
        var card = e.card;

        if (e.locked)
        {
            detailArt.SetSprite(lockedArt);
            detailName.text = "???";
            detailType.text = "미해금";
            detailCompare.text = "해금 후 확인할 수 있습니다.";
            if (detailCurrent != null) detailCurrent.text = "—";
            detailUnlock.text = e.lockCondition;
            SetUpgrade(CardUpgrades.Result.InvalidCard, "—");
            return;
        }

        detailArt.SetSprite(card.CardImage);
        detailName.text = card.CardName;
        detailType.text = $"{TypeName(card.Type)}  ·  {RarityName(card.Rarity)}";
        if (detailCurrent != null) detailCurrent.text = card.GetDescription(false);
        detailCompare.text = card.CanUpgrade ? CompareText(card) : "강화 효과 없음";
        detailUnlock.text = e.source;

        var result = CardUpgrades.Check(card);
        SetUpgrade(result, result switch
        {
            CardUpgrades.Result.AlreadyUpgraded => "강화 완료 · 런에서 강화된 상태로 나옵니다",
            CardUpgrades.Result.NotUpgradable => "강화할 수 없는 카드입니다",
            _ => $"탈린 {CardUpgrades.TalinCost} + 파츠 {CardUpgrades.PartsCost}",
        });
    }

    // 강화 전·후 설명을 비교해 강화로 달라진 부분만 청록으로 칠한 강화 후 설명을 보여준다
    private static string CompareText(CardData card)
    {
        if (card.GetDescription(false) == card.GetDescription(true)) return card.GetDescription(false);
        var upgraded = Object.Instantiate(card);
        upgraded.isUpgraded = true;
        string text = CardUpgradeHighlight.Describe(upgraded).Replace(CardUpgradeHighlight.Color, "#0DB8F2"); // 무기고는 청록 강조
        Object.Destroy(upgraded);
        return text;
    }

    private void SetUpgrade(CardUpgrades.Result result, string levelText)
    {
        bool enabled = result == CardUpgrades.Result.Ok;
        if (detailUpgrade != null) detailUpgrade.text = levelText;
        upgradeButton.interactable = enabled;
        upgradeImage.sprite = enabled ? upgradeNormal : upgradeDisabled;
        upgradeLabel.text = result switch
        {
            CardUpgrades.Result.AlreadyUpgraded => "강화 완료",
            CardUpgrades.Result.NotUpgradable or CardUpgrades.Result.InvalidCard => "강화 불가",
            _ => "강화하기  →",
        };
        upgradeLabel.color = enabled ? paperColor : grayColor;

        bool showCost = result is CardUpgrades.Result.Ok or CardUpgrades.Result.NotEnoughTalin or CardUpgrades.Result.NotEnoughParts;
        if (costRoot != null) costRoot.SetActive(showCost);
        if (talinCostText != null) { talinCostText.text = CardUpgrades.TalinCost.ToString(); talinCostText.color = MetaProgress.Talin >= CardUpgrades.TalinCost ? paperColor : grayColor; }
        if (partsCostText != null) { partsCostText.text = CardUpgrades.PartsCost.ToString(); partsCostText.color = MetaProgress.Parts >= CardUpgrades.PartsCost ? paperColor : grayColor; }
        if (balanceText != null) balanceText.text = showCost ? $"보유 탈린 {MetaProgress.Talin:N0} · 파츠 {MetaProgress.Parts:N0}" : "";
    }

    private static string TypeName(CardData.CardType t) => t switch
    {
        CardData.CardType.Attack => "공격", CardData.CardType.Skill => "스킬",
        CardData.CardType.Power => "파워", _ => "상태",
    };

    private static string RarityName(CardData.CardRarity r) => r switch
    {
        CardData.CardRarity.Common => "일반", CardData.CardRarity.Rare => "특별", _ => "희귀",
    };
}
