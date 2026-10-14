using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using K = CompanionSkillUIKit;

// 전투 HUD 왼쪽 아래 "지시 / ORDER" 버튼과 동료 스킬 목록 (FGO 마스터 스킬식). 기획: Docs/기획/동료_스킬.md
//   버튼을 누르면 위로 동행 동료 줄이 펼쳐진다. 준비된 줄을 누르면 스킬을 쓴다(적 지정 스킬은 화살표로 대상 지정).
//   줄에 마우스를 올리면 오른쪽에 툴팁(스킬 설명·충전 규칙·인연 강화).
// UI는 Awake에서 코드로 만든다. 좌표는 layout.json(1920×1080 좌상단) 값에 yShift를 더한 것.
public class CompanionOrderView : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private int sortingOrder = 50;
    [Tooltip("layout.json 좌표에서 세로로 옮길 양. 상태 아이콘 줄(y 868~916)을 가리지 않게 위로 올린다")]
    [SerializeField] private float yShift = -28f;
    [SerializeField] private ItemTargetingController targeting; // 비우면 씬에서 찾는다

    private const float ButtonY = 848f, RowStep = 62f;

    private BattleManager _battle;
    private RectTransform _root, _list, _rowsRoot;
    private Image _button, _diamond, _badge, _panel;
    private TextMeshProUGUI _badgeText, _countText;
    private readonly List<Row> _rows = new();
    private bool _open;
    private int _stateHash;

    private void Awake()
    {
        _root = (RectTransform)transform;
        var canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
        if (font == null) font = TMP_Settings.defaultFontAsset;
        BuildButton();
        BuildList();
        SetVisible(false);
    }

    private void Start()
    {
        _battle = BattleManager.Instance;
        if (targeting == null) targeting = FindFirstObjectByType<ItemTargetingController>(FindObjectsInactive.Include);
        if (_battle == null) return;
        _battle.OnBattleStarted += Rebuild;
        _battle.OnCompanionSkillsChanged += Rebuild;
        Rebuild();
    }

    private void OnDestroy()
    {
        if (_battle == null) return;
        _battle.OnBattleStarted -= Rebuild;
        _battle.OnCompanionSkillsChanged -= Rebuild;
    }

    private IReadOnlyList<CompanionSkillSlot> Slots =>
        _battle != null && _battle.State != null ? _battle.State.CompanionSkills : Array.Empty<CompanionSkillSlot>();

    private bool CanAct => _battle != null && _battle.IsInBattle && _battle.CanUseItemNow;

    private void Update()
    {
        if (_battle == null) return;
        bool show = _battle.IsInBattle && Slots.Count > 0;
        if (_buttonRoot.gameObject.activeSelf != show) SetVisible(show);
        if (!show) return;

        // 턴이 넘어가는 등 사용 가능 여부가 바뀌면 다시 그린다
        int hash = CanAct ? 1 : 0;
        foreach (var slot in Slots) hash = hash * 31 + slot.Progress;
        if (hash != _stateHash) Refresh();

        if (_open && !CanAct) SetOpen(false);
        if (_open && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame
            && !Contains(_list) && !Contains(_buttonRoot))
            SetOpen(false);
    }

    // ───────── 버튼 ─────────

    private RectTransform _buttonRoot;

    private void BuildButton()
    {
        float y = ButtonY + yShift;
        _buttonRoot = K.Group("OrderButton", _root, stretch: false);
        K.TopLeft(_buttonRoot, 90, y, 270, 44);
        _button = K.Pic(_buttonRoot, "Back", "Order_Button", 0, 0, 270, 44, sliced: true);
        _button.raycastTarget = true;
        _diamond = K.Pic(_buttonRoot, "Diamond", "Order_Diamond", 16, 11, 22, 22);
        K.Text(_buttonRoot, "Label", font, 64, 6, 60, 30, 24, K.Paper, TextAlignmentOptions.Left, FontStyles.Bold).text = "지시";
        K.Text(_buttonRoot, "English", font, 126, 16, 60, 18, 12, K.Paper, TextAlignmentOptions.Left).text = "ORDER";
        _badge = K.Pic(_buttonRoot, "Badge", "Order_Badge", 226, 6, 34, 32);
        _badgeText = K.Text(_badge.transform, "Count", font, 0, 0, 34, 32, 22, K.Ink, TextAlignmentOptions.Center, FontStyles.Bold);
        _button.gameObject.AddComponent<Pointer>().Click = () => SetOpen(!_open);
    }

    // ───────── 목록 ─────────

    private void BuildList()
    {
        _list = K.Group("OrderList", _root, stretch: false);
        _panel = K.Pic(_list, "Panel", "List_Panel", 0, 0, 390, 216, sliced: true);
        K.Text(_list, "Eng", font, 26, 6, 64, 20, 14, K.Ink, TextAlignmentOptions.Left, FontStyles.Bold).text = "ORDER";
        K.Text(_list, "Title", font, 90, 6, 176, 20, 14, K.Ink, TextAlignmentOptions.Left).text = "/  동료 지시";
        _countText = K.Text(_list, "Count", font, 344, 6, 28, 20, 14, K.Ink, TextAlignmentOptions.Right);
        K.Pic(_list, "HeaderRule", "List_HeaderRule", 10, 30, 370, 1, sliced: true);
        _rowsRoot = K.Group("Rows", _list, stretch: false);
        K.TopLeft(_rowsRoot, 10, 32, 370, 0);
        _list.gameObject.SetActive(false);
    }

    private void Rebuild()
    {
        var slots = Slots;
        bool same = _rows.Count == slots.Count;
        for (int i = 0; same && i < slots.Count; i++) same = _rows[i].Slot == slots[i];
        if (!same)
        {
            foreach (var r in _rows) Destroy(r.Root.gameObject);
            _rows.Clear();
            for (int i = 0; i < slots.Count; i++) _rows.Add(BuildRow(slots[i], i));

            float height = 30f + 62f * slots.Count;
            float top = ButtonY + yShift - 4f - height;
            K.TopLeft(_list, 86, top, 390, height);
            _panel.rectTransform.sizeDelta = new Vector2(390, height);
            _countText.text = slots.Count.ToString("00");
        }
        Refresh();
    }

    private void Refresh()
    {
        bool act = CanAct;
        int ready = 0;
        foreach (var slot in Slots) if (slot.IsReady) ready++;

        _button.sprite = K.Load(!act ? "Order_Button_Disabled" : (ready > 0 || _open) ? "Order_Button_Active" : "Order_Button");
        _diamond.color = act && ready > 0 ? K.Accent : K.Muted;
        _badge.gameObject.SetActive(act && ready > 0);
        _badgeText.text = ready.ToString();
        foreach (var row in _rows) row.Refresh(act);

        int hash = act ? 1 : 0;
        foreach (var slot in Slots) hash = hash * 31 + slot.Progress;
        _stateHash = hash;
    }

    private void SetOpen(bool open)
    {
        _open = open && CanAct;
        _list.gameObject.SetActive(_open);
        if (!_open) StatusTooltipView.Instance?.Hide();
        Refresh();
    }

    private void SetVisible(bool visible)
    {
        _buttonRoot.gameObject.SetActive(visible);
        if (!visible) SetOpen(false);
    }

    private bool Contains(RectTransform rt)
    {
        var cam = GetComponentInParent<Canvas>().rootCanvas.worldCamera;
        return rt.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(rt, Mouse.current.position.ReadValue(), cam);
    }

    private void Use(Row row)
    {
        var slot = row.Slot;
        if (!_battle.CanUseCompanionSkill(slot)) return;
        SetOpen(false);
        if (!slot.Skill.selectEnemy)
        {
            _battle.TryUseCompanionSkill(slot, null);
            return;
        }
        if (targeting != null) targeting.BeginTargeting(row.Root.position, enemy => _battle.TryUseCompanionSkill(slot, enemy));
    }

    private void ShowTooltip(Row row)
    {
        var slot = row.Slot;
        var skill = slot.Skill;
        string charge = skill.chargeType == CompanionSkillCharge.Turns
            ? $"{slot.Required}턴마다 충전"
            : $"{slot.Required}전투마다 충전 (승리 시)";
        string text = skill.GetDescription(slot.Enhanced) + $"\n<color=#92999F>{charge}</color>";
        if (slot.Enhanced) text += "\n<color=#0DB8F2>인연 강화</color>";
        else if (skill.HasEnhancement)
            text += $"\n<color=#92999F>인연 Lv.MAX 강화: {skill.GetDescription(true)}</color>";
        StatusTooltipView.Instance?.ShowNear(row.Root, skill.icon, skill.skillName, text, placeRight: true);
    }

    // ───────── 줄 ─────────

    private Row BuildRow(CompanionSkillSlot slot, int index)
    {
        var row = new Row { Slot = slot };
        row.Root = K.Group("Row_" + slot.Owner.charID, _rowsRoot, stretch: false);
        K.TopLeft(row.Root, 0, index * RowStep, 370, 60);
        row.Back = K.Pic(row.Root, "Back", "Row_Normal", 0, 0, 370, 60, sliced: true);
        row.Back.raycastTarget = true;
        row.ReadyBar = K.Pic(row.Root, "ReadyBar", "Row_ReadyBar", 0, 0, 5, 60);

        var mask = K.Pic(row.Root, "FaceMask", "Face_Mask", 26, 10, 48, 40);
        mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        var data = slot.Owner.charData;
        var faceSprite = data != null && data.charImage != null ? data.charImage : null;
        if (faceSprite != null)
        {
            var face = new GameObject("Face", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(UICroppedArt));
            face.layer = mask.gameObject.layer;
            var frt = (RectTransform)face.transform;
            frt.SetParent(mask.transform, false);
            K.TopLeft(frt, 0, 0, 48, 40);
            row.Face = face.GetComponent<RawImage>();
            row.Face.raycastTarget = false;
            face.GetComponent<UICroppedArt>().SetSprite(faceSprite, data.faceFocus, data.faceZoom);
        }
        else row.Face = null;
        if (faceSprite == null) K.Pic(mask.transform, "Placeholder", "Face_Placeholder", 0, 0, 48, 40);
        K.Pic(row.Root, "Frame", slot.Enhanced ? "Face_Frame_Enhanced" : "Face_Frame", 20, 4, 60, 52);

        string charName = data != null && !string.IsNullOrWhiteSpace(data.charName) ? data.charName : slot.Owner.charID;
        row.Name = K.Text(row.Root, "Name", font, 104, 5, 120, 21, 12, K.Ink, TextAlignmentOptions.Left);
        row.Name.text = charName;
        if (slot.Enhanced)
        {
            float w = row.Name.GetPreferredValues(charName).x;
            row.Mark = K.Pic(row.Root, "Mark", "Mark_Enhanced", 104 + w + 6, 8, 14, 14);
        }
        row.Skill = K.Text(row.Root, "Skill", font, 104, 26, 120, 28, 20, K.Ink, TextAlignmentOptions.Left, FontStyles.Bold);
        row.Skill.text = slot.Skill.skillName;

        row.ReadyTag = K.Pic(row.Root, "ReadyTag", "Ready_Tag", 276, 16, 86, 30);
        K.Text(row.ReadyTag.transform, "Label", font, 18, 5, 62, 20, 12, K.Paper, TextAlignmentOptions.Left, FontStyles.Bold).text = "READY";

        int cells = Mathf.Clamp(slot.Required, 1, 4);
        for (int i = 0; i < cells; i++)
        {
            float x = 228 + 21 * i;
            row.Cells.Add(K.Pic(row.Root, "Cell" + i, "Charge_Bar_Back", x, 22, 18, 16).gameObject);
            row.Fills.Add(K.Pic(row.Root, "Fill" + i, "Charge_Bar_Fill", x + 2, 24, 14, 12));
        }
        row.Remaining = K.Text(row.Root, "Remaining", font, 322, 20, 42, 22, 14, K.Ink, TextAlignmentOptions.Center);

        var pointer = row.Back.gameObject.AddComponent<Pointer>();
        pointer.Enter = () => { row.Hover = true; row.Refresh(CanAct); ShowTooltip(row); };
        pointer.Exit = () => { row.Hover = false; row.Refresh(CanAct); StatusTooltipView.Instance?.Hide(); };
        pointer.Click = () => Use(row);
        return row;
    }

    private class Row
    {
        public CompanionSkillSlot Slot;
        public RectTransform Root;
        public Image Back, ReadyBar, ReadyTag, Mark;
        public RawImage Face;
        public TextMeshProUGUI Name, Skill, Remaining;
        public readonly List<Image> Fills = new();
        public readonly List<GameObject> Cells = new();
        public bool Hover;

        public void Refresh(bool canAct)
        {
            bool ready = Slot.IsReady;
            bool lit = ready && Hover && canAct; // 먹색 반전은 쓸 수 있는 줄에만
            Back.sprite = K.Load(lit ? "Row_Hover" : "Row_Normal");
            ReadyBar.gameObject.SetActive(ready);
            ReadyTag.gameObject.SetActive(ready);
            foreach (var c in Cells) c.SetActive(!ready);
            for (int i = 0; i < Fills.Count; i++)
                Fills[i].gameObject.SetActive(!ready && i < Mathf.RoundToInt((float)Slot.Progress / Slot.Required * Fills.Count));
            Remaining.gameObject.SetActive(!ready);
            Remaining.text = Slot.IsTurnCharge ? $"{Slot.Required - Slot.Progress}턴" : $"{Slot.Progress}/{Slot.Required}";

            Color text = lit ? K.Paper : K.Ink;
            if (!ready) text.a = 0.55f;
            Name.color = Skill.color = text;
            if (Face != null) Face.color = new Color(1f, 1f, 1f, ready ? 1f : 0.5f);
            if (Mark != null) Mark.sprite = K.Load(lit ? "Mark_Enhanced_White" : "Mark_Enhanced");
        }
    }

    private class Pointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Action Enter, Exit, Click;
        public void OnPointerEnter(PointerEventData e) => Enter?.Invoke();
        public void OnPointerExit(PointerEventData e) => Exit?.Invoke();
        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) Click?.Invoke();
        }
    }
}
