using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 출정 준비(메뉴 01) 화면 내용: 선택 무기(그림 · 수치) · 무기 칸 4개 · 시작 덱 줄 · 특전 카드 · 진행 중 의뢰.
// 출정·닫기는 TentInteractionHandler가 맡고, 이 컴포넌트는 보여 주기만 한다. 아트: ArtDirection/LoadoutMockup (v3, layout.json 1920×1080)
// 무기 선택은 아직 권총 고정 (전투 기본 무기). 다른 무기는 아티팩트 해금 표시만.
public class LoadoutView : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public WeaponData weapon;
        public Sprite art;
        public Image frame;
        public Image artImage;
        public TextMeshProUGUI nameText;
        public GameObject lockIcon;
        public GameObject costRoot;
        public TextMeshProUGUI costText;
        [Tooltip("큰 무기 그림 옆 도면 주석 (예: CAL .357 MAG  /  CYL 6)")]
        public string spec;
        public string length;   // 도면 가로 치수 (예: 318 mm)
        public string height;   // 도면 세로 치수
    }

    [Header("무기")]
    [SerializeField] private WeaponData selectedWeapon;
    [SerializeField] private Sprite selectedArt;
    [SerializeField] private Image heroArt;
    [SerializeField] private TextMeshProUGUI weaponName;
    [SerializeField] private TextMeshProUGUI weaponSubtitle;
    [SerializeField] private TextMeshProUGUI ammoValue, shotValue, upgradeValue;
    [SerializeField] private TextMeshProUGUI specText;

    [Header("도면 (명일방주 오퍼레이터 파일 느낌)")]
    [SerializeField] private RectTransform dimBottom;        // 가로 치수선 (그려진 무기 폭에 맞춤)
    [SerializeField] private RectTransform dimLeft;          // 세로 치수선
    [SerializeField] private TextMeshProUGUI dimBottomLabel, dimLeftLabel;
    [SerializeField] private float dimGap = 26f;
    [SerializeField] private List<Slot> slots = new();
    [SerializeField] private Sprite slotNormal, slotSelected, slotLocked, slotEmpty;
    [SerializeField] private Color paperColor = new(0.933f, 0.941f, 0.949f, 1f);
    [SerializeField] private Color inkColor = new(0.086f, 0.094f, 0.106f, 1f);

    [Header("무기고로")]
    [SerializeField] private Button armoryLink;
    [SerializeField] private Button closeButton;        // 출정 준비 닫기 (TentInteractionHandler.closeButton)
    [SerializeField] private Button armoryMenuButton;   // 로비 메뉴의 무기고 버튼

    [Header("시작 덱")]
    [SerializeField] private TextMeshProUGUI deckTitle;
    [Tooltip("스크롤 목록 안의 줄 틀 (세로 레이아웃이 줄을 쌓는다. 판은 3줄 높이 고정, 넘치면 스크롤)")]
    [SerializeField] private RectTransform rowTemplate;
    [SerializeField] private Sprite iconAttack, iconDefense, iconReload;

    [Header("진행 중 의뢰")]
    [SerializeField] private TextMeshProUGUI questHeader;
    [SerializeField] private List<GameObject> questRows = new();
    [SerializeField] private List<TextMeshProUGUI> questNames = new();

    private readonly List<GameObject> _rows = new();

    private void Awake()
    {
        rowTemplate.gameObject.SetActive(false);
        if (armoryLink != null) armoryLink.onClick.AddListener(() =>
        {
            if (closeButton != null) closeButton.onClick.Invoke();
            if (armoryMenuButton != null && armoryMenuButton.interactable) armoryMenuButton.onClick.Invoke();
        });
    }

    private void OnEnable()
    {
        if (QuestManager.Instance != null) QuestManager.Instance.OnStateChanged += RefreshQuests;
        Refresh();
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null) QuestManager.Instance.OnStateChanged -= RefreshQuests;
    }

    public void Refresh()
    {
        RefreshWeapon();
        RefreshDeck();
        RefreshQuests();
    }

    private void RefreshWeapon()
    {
        var w = selectedWeapon;
        heroArt.sprite = selectedArt;
        heroArt.enabled = selectedArt != null;
        if (w != null)
        {
            int level = WeaponUpgrades.GetLevel(w);
            weaponName.text = w.WeaponName;
            weaponSubtitle.text = "기본 시작 무기";
            ammoValue.text = (w.MaxAmmo + w.MaxAmmoBonusAt(level)).ToString();
            int shot = w.ShotDamageBonusAt(level);
            shotValue.text = shot > 0 ? $"+{shot}" : "+0";
            upgradeValue.text = w.MaxUpgradeLevel > 0 ? $"{level} / {w.MaxUpgradeLevel}" : "—";
        }

        var sel = slots.Find(x => x.weapon != null && x.weapon == w);
        if (specText != null) specText.text = sel != null ? sel.spec : "";
        PlaceDimensions(sel);

        foreach (var s in slots)
        {
            bool empty = s.weapon == null;
            bool selected = !empty && s.weapon == selectedWeapon;
            bool locked = !empty && !selected;
            s.frame.sprite = empty ? slotEmpty : selected ? slotSelected : locked ? slotLocked : slotNormal;
            s.artImage.gameObject.SetActive(!empty);
            if (!empty) { s.artImage.sprite = s.art; s.artImage.color = locked ? new Color(1f, 1f, 1f, 0.55f) : Color.white; }
            if (s.nameText != null)
            {
                s.nameText.gameObject.SetActive(!empty);
                s.nameText.text = empty ? "" : s.weapon.WeaponName;
                s.nameText.color = selected ? paperColor : inkColor;
                s.nameText.alignment = selected ? TextAlignmentOptions.Center : TextAlignmentOptions.MidlineLeft;
            }
            if (s.lockIcon != null) s.lockIcon.SetActive(locked);
            if (s.costRoot != null) s.costRoot.SetActive(locked);
            if (s.costText != null) s.costText.text = "1";
        }
    }

    // 비율을 지킨 채 그려진 그림의 실제 사각형에 치수선을 붙인다
    private void PlaceDimensions(Slot sel)
    {
        if (dimBottom == null || dimLeft == null || heroArt.sprite == null) return;
        var box = heroArt.rectTransform;
        var spr = heroArt.sprite.rect;
        float scale = Mathf.Min(box.rect.width / spr.width, box.rect.height / spr.height);
        float w = spr.width * scale, h = spr.height * scale;
        Vector2 c = box.anchoredPosition; // 피벗 가운데
        dimBottom.anchoredPosition = new Vector2(c.x - w / 2f, c.y - h / 2f - dimGap);
        dimBottom.sizeDelta = new Vector2(w, dimBottom.sizeDelta.y);
        dimLeft.anchoredPosition = new Vector2(c.x - w / 2f - dimGap, c.y - h / 2f);
        dimLeft.sizeDelta = new Vector2(dimLeft.sizeDelta.x, h);
        if (dimBottomLabel != null) dimBottomLabel.text = sel != null ? sel.length : "";
        if (dimLeftLabel != null) dimLeftLabel.text = sel != null ? sel.height : "";
    }

    // 지금 덱(DeckManager)을 카드별로 묶어 한 줄씩. 무기고에서 강화한 카드는 청록 + 표식
    private void RefreshDeck()
    {
        foreach (var r in _rows) if (r != null) Destroy(r);
        _rows.Clear();

        var deck = DeckManager.Instance != null ? DeckManager.Instance.PlayerDeck : null;
        var order = new List<string>();
        var groups = new Dictionary<string, (CardData card, int count)>();
        if (deck != null)
            foreach (var c in deck)
            {
                if (c == null) continue;
                string key = PlayerRecord.KeyOf(c) + (c.isUpgraded ? "+" : "");
                if (groups.TryGetValue(key, out var g)) groups[key] = (g.card, g.count + 1);
                else { groups[key] = (c, 1); order.Add(key); }
            }

        int total = deck?.Count ?? 0;
        deckTitle.text = $"시작 덱 ({total})";
        for (int i = 0; i < order.Count; i++)
        {
            var (card, count) = groups[order[i]];
            var row = Instantiate(rowTemplate, rowTemplate.parent);
            row.gameObject.SetActive(true);
            row.Find("Icon").GetComponent<Image>().sprite = IconOf(card);
            row.Find("Txt_Name").GetComponent<TextMeshProUGUI>().text = card.CardName;
            row.Find("Txt_Effect").GetComponent<TextMeshProUGUI>().text = card.GetDescription(card.isUpgraded).Replace("\n", " ");
            row.Find("Txt_Count").GetComponent<TextMeshProUGUI>().text = $"×{count}";
            row.Find("Mark_Upgraded").gameObject.SetActive(card.isUpgraded);
            _rows.Add(row.gameObject);
        }
    }

    private Sprite IconOf(CardData c)
    {
        if (c.BaseCardName.Contains("재장전")) return iconReload;
        return c.Type == CardData.CardType.Attack ? iconAttack : iconDefense;
    }

    private void RefreshQuests()
    {
        var q = QuestManager.Instance;
        var accepted = q != null ? new List<QuestData>(q.Accepted) : new List<QuestData>();
        questHeader.text = accepted.Count == 0 ? "진행 중 의뢰 없음" : $"진행 중 의뢰  <b>{accepted.Count} / {q.MaxActive}</b>";
        for (int i = 0; i < questRows.Count; i++)
        {
            bool on = i < accepted.Count;
            questRows[i].SetActive(on);
            if (on) questNames[i].text = accepted[i].title;
        }
    }
}
