using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 무기고 > 장비 강화. 왼쪽 무기 목록 → 가운데 무기 판(그림 · 수치 · 다음 강화 · 기본 카드 강화) → 오른쪽 강화 단계 · 비용 · 강화 버튼.
// 무기 강화는 파츠(WeaponUpgrades, 마지막 단계는 무기고 시설 Lv.2), 기본 카드 강화는 탈린 + 파츠(CardUpgrades).
public class ArmoryEquipmentView : MonoBehaviour
{
    [System.Serializable]
    public class WeaponSlot
    {
        public WeaponData weapon;
        public Sprite art;          // 목록 칸 (먹색 칸 위 — 밝은 판)
        public Sprite detailArt;    // 가운데 회백 판 (먹색 판). 비우면 art
        public Button button;
        public Image frame;
        public Image thumb;
        public TextMeshProUGUI nameText;
        public GameObject check;
    }

    [System.Serializable]
    public class BaseCardRow
    {
        public CardData card;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI effectText;
        public Button button;
        public Image buttonImage;
        public TextMeshProUGUI talinText;
        public TextMeshProUGUI partsText;
        public TextMeshProUGUI actionText;
        public GameObject costRoot;
    }

    [System.Serializable]
    public class Step
    {
        public Image node;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI descText;
        public GameObject costRoot;
        public TextMeshProUGUI costText;
    }

    [SerializeField] private List<WeaponSlot> slots = new();
    [SerializeField] private Sprite slotNormal, slotSelected;

    [Header("가운데")]
    [SerializeField] private TextMeshProUGUI weaponTitle;
    [SerializeField] private TextMeshProUGUI levelBadge;
    [SerializeField] private Image weaponArt;
    [SerializeField] private TextMeshProUGUI[] statLabels = new TextMeshProUGUI[3];
    [SerializeField] private TextMeshProUGUI[] statValues = new TextMeshProUGUI[3];
    [SerializeField] private TextMeshProUGUI nextText;
    [SerializeField] private List<BaseCardRow> baseCards = new();

    [Header("오른쪽")]
    [SerializeField] private TextMeshProUGUI facilityText;
    [SerializeField] private List<Step> steps = new();
    [SerializeField] private RectTransform stepLine;   // 단계 잇는 세로선 (단계 수에 맞춰 길이)
    [SerializeField] private Sprite stepDone, stepCurrent, stepLocked;
    [SerializeField] private GameObject costRoot;
    [SerializeField] private TextMeshProUGUI costValue;
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image upgradeImage;
    [SerializeField] private TextMeshProUGUI upgradeLabel;
    [SerializeField] private Sprite upgradeNormal, upgradeDisabled;
    [SerializeField] private Sprite smallNormal, smallDisabled;

    [Header("색")]
    [SerializeField] private Color paper = new(0.933f, 0.941f, 0.949f, 1f);
    [SerializeField] private Color gray = new(0.573f, 0.604f, 0.639f, 1f);
    [SerializeField] private Color cyan = new(0.051f, 0.722f, 0.949f, 1f);
    [SerializeField] private Color ink = new(0.086f, 0.094f, 0.106f, 1f);

    private WeaponSlot _selected;

    private void Awake()
    {
        foreach (var s in slots)
        {
            var slot = s;
            if (slot.button != null) slot.button.onClick.AddListener(() => Select(slot));
        }
        upgradeButton.onClick.AddListener(UpgradeWeapon);
        foreach (var r in baseCards)
        {
            var row = r;
            row.button.onClick.AddListener(() => { CardUpgrades.TryUpgrade(row.card); Refresh(); });
        }
    }

    private void OnEnable()
    {
        MetaProgress.OnChanged += Refresh;
        if (_selected == null) _selected = slots.Find(s => s.weapon != null);
        Refresh();
    }

    private void OnDisable() => MetaProgress.OnChanged -= Refresh;

    private static bool FacilityUpgraded()
    {
        var fm = LobbyManager.Instance != null ? LobbyManager.Instance.FacilityManager : null;
        return fm != null && fm.IsFacilityUpgraded(FacilityType.Armory);
    }

    private void Select(WeaponSlot slot)
    {
        _selected = slot;
        Refresh();
    }

    private void UpgradeWeapon()
    {
        if (_selected == null) return;
        WeaponUpgrades.TryUpgrade(_selected.weapon, FacilityUpgraded());
        Refresh();
    }

    private void Refresh()
    {
        foreach (var s in slots)
        {
            bool has = s.weapon != null;
            s.frame.gameObject.SetActive(has);
            if (!has) continue;
            bool on = s == _selected;
            s.frame.sprite = on ? slotSelected : slotNormal;
            s.nameText.text = s.weapon.WeaponName;
            s.thumb.sprite = s.art;
            s.thumb.enabled = s.art != null;
            s.thumb.color = on ? Color.white : new Color(1f, 1f, 1f, 0.6f);
            if (s.check != null) s.check.SetActive(on);
        }
        if (_selected == null || _selected.weapon == null) return;

        var w = _selected.weapon;
        int level = WeaponUpgrades.GetLevel(w);
        int max = w.MaxUpgradeLevel;
        bool facility = FacilityUpgraded();

        weaponTitle.text = w.WeaponName;
        levelBadge.text = $"Lv.{level + 1}";
        weaponArt.sprite = _selected.detailArt != null ? _selected.detailArt : _selected.art;
        weaponArt.enabled = weaponArt.sprite != null;

        // 수치: 탄창 / 사격 보너스 / 강화 단계 (다음 단계 값은 청록)
        int nextLevel = Mathf.Min(level + 1, max);
        bool canNext = level < max;
        statLabels[0].text = "탄창";
        statValues[0].text = Arrow(w.MaxAmmo + w.MaxAmmoBonusAt(level), w.MaxAmmo + w.MaxAmmoBonusAt(nextLevel), canNext && w.UpgradeType == WeaponUpgradeType.MaxAmmo);
        statLabels[1].text = "사격 추가 피해";
        statValues[1].text = Arrow(w.ShotDamageBonusAt(level), w.ShotDamageBonusAt(nextLevel), canNext && w.UpgradeType == WeaponUpgradeType.ShotDamage, plus: true);
        statLabels[2].text = "강화";
        statValues[2].text = max > 0 ? $"{level} / {max}" : "—";

        nextText.text = max == 0 ? "이 무기는 무기고 강화가 없습니다."
            : !canNext ? "최대 강화 완료"
            : $"다음 강화: {StepEffect(w, level + 1)}";

        // 강화 단계 (Lv.1 = 기본, Lv.n+1 = n단계)
        facilityText.text = $"시설 Lv.{(facility ? 2 : 1)}  |  최대 강화 Lv.{max + 1}";
        for (int i = 0; i < steps.Count; i++)
        {
            var st = steps[i];
            bool used = i <= max;
            st.node.gameObject.SetActive(used);
            st.levelText.gameObject.SetActive(used);
            st.descText.gameObject.SetActive(used);
            if (st.costRoot != null) st.costRoot.SetActive(used && i > 0 && i > level);
            if (!used) continue;
            bool done = i <= level, current = i == level + 1;
            bool needsFacility = i == max && max >= 2 && !facility && !done;
            st.node.sprite = done ? stepDone : current && !needsFacility ? stepCurrent : stepLocked;
            st.levelText.text = $"Lv.{i + 1}";
            st.levelText.color = current && !needsFacility ? cyan : paper;
            st.descText.text = i == 0 ? "기본 성능" : needsFacility ? "시설 Lv.2 필요" : StepEffect(w, i);
            st.descText.color = needsFacility ? gray : paper;
            if (st.costText != null) st.costText.text = w.UpgradeCostTo(i).ToString();
        }

        if (stepLine != null)
        {
            stepLine.gameObject.SetActive(max > 0);
            stepLine.sizeDelta = new Vector2(stepLine.sizeDelta.x, 154f * max - 14f);
        }

        // 비용 · 버튼
        var check = WeaponUpgrades.Check(w, facility);
        int cost = WeaponUpgrades.NextCost(w);
        costRoot.SetActive(canNext);
        costValue.text = cost.ToString();
        costValue.color = check == WeaponUpgrades.Result.NotEnoughParts ? gray : paper;
        balanceText.text = !canNext ? "" : check == WeaponUpgrades.Result.NotEnoughParts
            ? $"보유 {MetaProgress.Parts} · 파츠 부족"
            : $"보유 {MetaProgress.Parts} → 강화 후 {MetaProgress.Parts - cost}";
        bool ok = check == WeaponUpgrades.Result.Ok;
        upgradeButton.interactable = ok;
        upgradeImage.sprite = ok ? upgradeNormal : upgradeDisabled;
        upgradeLabel.text = check switch
        {
            WeaponUpgrades.Result.MaxLevel => max == 0 ? "강화 없음" : "최대 강화",
            WeaponUpgrades.Result.NeedsFacility => "시설 Lv.2 필요",
            _ => "강화  →",
        };
        upgradeLabel.color = ok ? paper : gray;

        // 기본 카드 강화
        foreach (var r in baseCards)
        {
            if (r.card == null) continue;
            var res = CardUpgrades.Check(r.card);
            r.nameText.text = r.card.BaseCardName;
            r.effectText.text = EffectLine(r.card);
            bool can = res == CardUpgrades.Result.Ok;
            r.button.interactable = can;
            r.buttonImage.sprite = can ? smallNormal : smallDisabled;
            bool showCost = res != CardUpgrades.Result.AlreadyUpgraded && res != CardUpgrades.Result.NotUpgradable;
            r.costRoot.SetActive(showCost);
            r.talinText.text = CardUpgrades.TalinCost.ToString();
            r.partsText.text = CardUpgrades.PartsCost.ToString();
            r.talinText.color = MetaProgress.Talin >= CardUpgrades.TalinCost ? paper : gray;
            r.partsText.color = MetaProgress.Parts >= CardUpgrades.PartsCost ? paper : gray;
            r.actionText.text = res == CardUpgrades.Result.AlreadyUpgraded ? "강화 완료" : res == CardUpgrades.Result.NotUpgradable ? "강화 불가" : "강화";
            r.actionText.color = can ? paper : gray;
        }
    }

    private string Arrow(int now, int next, bool show, bool plus = false)
    {
        string f(int v) => plus ? (v > 0 ? $"+{v}" : "0") : v.ToString();
        return show && next != now ? $"{f(now)} → <color=#{ColorUtility.ToHtmlStringRGB(cyan)}>{f(next)}</color>" : f(now);
    }

    private static string StepEffect(WeaponData w, int level) => w.UpgradeType switch
    {
        WeaponUpgradeType.MaxAmmo => $"탄창 +{w.UpgradeValueAt(level)}",
        WeaponUpgradeType.ShotDamage => $"사격 피해 +{w.UpgradeValueAt(level)}",
        _ => "",
    };

    // 기본 카드: 강화 전 → 강화 후 첫 수치 비교가 어려우니 설명 대신 상태만 짧게
    private static string EffectLine(CardData card)
        => CardUpgrades.IsUpgraded(card) ? "강화됨 · 시작 덱에 반영" : "강화하면 시작 덱부터 강화 상태";
}
