using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ============================================================
// filename   : OperationStatusView.cs
// description: 작전 상황 (OPERATION STATUS) — 런 출격 전 집결 화면. 로비 메뉴 01 「탐색 개시」와 04 「작전 상황」이 연다.
//              왼쪽에 작전 담당이 서고, 오른쪽 판은 두 쪽으로 나눈다 (탭 · Q / E로 넘김, 열 때는 늘 01부터).
//                01 작전: 작전 현황(수주한 의뢰, 가로로 나란히) + 출격
//                02 장비 · 덱: 장비 현황(무기) + 덱 현황(통계 · 많이 든 카드)
//              화면은 코드로 만든다(씬에는 이 컴포넌트와 참조만). 좌표는 목업 1920×1080 기준
//              (ArtDirection/OperationStatusMockup/OpStatus_Mockup.png, 조각은 Resources/OperationStatus/).
//              반응형: 왼쪽 판은 화면 왼쪽, 오른쪽 판은 오른쪽 가장자리에 붙고, 가운데(작전 담당 · 대사창)는 가운데 아래에 붙는다.
//              출격 · 닫기는 TentInteractionHandler(출정 시설)가 맡는다: 출격 버튼 = runStartButton, 로비 버튼 = closeButton.
// ============================================================
public class OperationStatusView : MonoBehaviour
{
    private const float W = 1920f, H = 1080f;

    [Header("데이터")]
    [SerializeField] private WeaponData weapon;
    [SerializeField] private Sprite weaponArt;
    [SerializeField] private ActData firstAct;
    [SerializeField] private LobbyKeeperData keeper;
    [SerializeField] private Sprite keeperArt;
    [SerializeField] private Sprite background;

    [Header("연결")]
    [Tooltip("작전 게시판 (QuestBoardView 화면 루트). 열 때 이 화면 위로 올린다")]
    [SerializeField] private GameObject questBoard;
    [Tooltip("로비 메인의 무기고 버튼 (무기고 링크가 닫은 뒤 대신 누른다)")]
    [SerializeField] private Button armoryMenuButton;
    [Tooltip("출정 시설: 출격 = StartRun, 로비 = CloseInteraction (전환 연출 · 런 시작을 그대로 쓴다)")]
    [SerializeField] private TentInteractionHandler tent;
    [SerializeField] private TMP_FontAsset fontBold, fontRegular;

    [Header("작전 담당 대사")]
    [SerializeField] private string lineDeployHover = "벌써 나가시게요? 신호만 주세요. 게이트는 제가 열어 드릴게요.";
    [SerializeField] private string lineNoOperation = "빈손으로 나가시게요? 게시판에서 하나쯤 챙겨 가요. 아깝잖아요.";

    public Button DeployButton { get; private set; }
    public Button BackButton { get; private set; }

    // 색
    private static readonly Color Ink = new(0.043f, 0.067f, 0.102f, 1f);       // #0B111A
    private static readonly Color Paper = new(0.93f, 0.935f, 0.94f, 1f);
    private static readonly Color InkText = new(0.086f, 0.094f, 0.106f, 1f);
    private static readonly Color Soft = new(0.72f, 0.76f, 0.81f, 1f);
    private static readonly Color Dim = new(0.48f, 0.53f, 0.60f, 1f);
    private static readonly Color Cyan = new(0.118f, 0.627f, 0.902f, 1f);
    private const float PanelAlpha = 0.88f;

    private bool _built;
    private RectTransform _root, _left, _right, _center, _bg, _keeper, _snowLayer, _scan;
    private LobbyKeeperTalk _talk;
    private Sequence _intro;
    private readonly List<(RectTransform rt, Vector2 home, float from)> _slideIns = new();
    private readonly List<(RectTransform rt, Vector2 vel, float phase)> _snow = new();
    private TextMeshProUGUI _readout;
    private float _openedAt;

    // 동적 글자 · 칸
    private TextMeshProUGUI _weaponName, _weaponSub, _ammo, _shot, _upgrade, _deckTotal, _sector, _activeText;
    private Image _weaponImage, _activeFill;
    private readonly TextMeshProUGUI[] _typeCount = new TextMeshProUGUI[3];
    private readonly Image[] _typeFill = new Image[3];
    private readonly Image[] _costBar = new Image[4];
    private readonly TextMeshProUGUI[] _costCount = new TextMeshProUGUI[4];
    private readonly List<GameObject> _cardRows = new();
    private RectTransform _cardRowRoot, _opRoot;
    private readonly List<GameObject> _opItems = new();
    private GameObject _deckOverlay;
    private RectTransform _deckOverlayList;
    private TextMeshProUGUI _deploySummary;

    // 쪽 (01 작전 / 02 장비 · 덱)
    private readonly RectTransform[] _pages = new RectTransform[2];
    private readonly Image[] _tabs = new Image[2];
    private readonly TextMeshProUGUI[] _tabLabels = new TextMeshProUGUI[2];
    private int _page;
    private const float HistHeight = 190f;
    private const int CardRowCount = 6;
    private const float CardRowWidth = 460f;

    private void Awake() => Build();

    private void OnEnable()
    {
        Build();
        _openedAt = Time.unscaledTime;
        Refresh();
        if (DeployButton != null) DeployButton.interactable = true;
        if (QuestManager.Instance != null) QuestManager.Instance.OnStateChanged += Refresh;
        _page = -1;
        SetPage(0, false);   // 열 때는 늘 01 작전부터
        PlayIntro();
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null) QuestManager.Instance.OnStateChanged -= Refresh;
        _intro?.Kill();
        if (_deckOverlay != null) _deckOverlay.SetActive(false);
    }

    // ───────── 갱신 ─────────

    public void Refresh()
    {
        if (!_built) return;
        RefreshWeapon();
        RefreshDeck();
        RefreshOperations();
        if (_sector != null) _sector.text = firstAct != null ? $"{firstAct.actNumber}막 · {firstAct.actName}" : "1막";
    }

    private void RefreshWeapon()
    {
        _weaponImage.sprite = weaponArt;
        _weaponImage.enabled = weaponArt != null;
        if (weapon == null) return;
        int level = WeaponUpgrades.GetLevel(weapon);
        _weaponName.text = weapon.WeaponName;
        _weaponSub.text = "기본 시작 무기";
        _ammo.text = (weapon.MaxAmmo + weapon.MaxAmmoBonusAt(level)).ToString();
        int shot = weapon.ShotDamageBonusAt(level);
        _shot.text = $"+{Mathf.Max(0, shot)}";
        _upgrade.text = weapon.MaxUpgradeLevel > 0 ? $"{level} / {weapon.MaxUpgradeLevel}" : "—";
    }

    private List<(CardData card, int count)> DeckGroups()
    {
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
        return order.Select(k => groups[k]).ToList();
    }

    private void RefreshDeck()
    {
        var deck = DeckManager.Instance != null ? DeckManager.Instance.PlayerDeck : new List<CardData>();
        int total = deck.Count(c => c != null);
        _deckTotal.text = $"{total}<size=60%>장</size>";

        // 종류: 공격 · 스킬 · 파워
        var types = new[] { CardData.CardType.Attack, CardData.CardType.Skill, CardData.CardType.Power };
        for (int i = 0; i < 3; i++)
        {
            int n = deck.Count(c => c != null && c.Type == types[i]);
            _typeCount[i].text = n.ToString();
            _typeFill[i].fillAmount = total > 0 ? (float)n / total : 0f;
        }

        // 비용 분포 0 · 1 · 2 · 3+
        var cost = new int[4];
        foreach (var c in deck) if (c != null) cost[Mathf.Clamp(c.EnergyCost, 0, 3)]++;
        int max = Mathf.Max(1, cost.Max());
        for (int i = 0; i < 4; i++)
        {
            _costCount[i].text = cost[i].ToString();
            float h = 4f + (HistHeight - 34f) * cost[i] / max;
            _costBar[i].rectTransform.sizeDelta = new Vector2(_costBar[i].rectTransform.sizeDelta.x, h);
            _costCount[i].rectTransform.anchoredPosition = new Vector2(_costCount[i].rectTransform.anchoredPosition.x, h + 2f);
        }

        // 카드 줄: 많은 순 CardRowCount줄
        foreach (var r in _cardRows) if (r != null) Destroy(r);
        _cardRows.Clear();
        var top = DeckGroups().OrderByDescending(g => g.count).Take(CardRowCount).ToList();
        for (int i = 0; i < top.Count; i++)
            _cardRows.Add(CardRow(_cardRowRoot, 0f, i * 49f, CardRowWidth, top[i].card, top[i].count, false));
    }

    private void RefreshOperations()
    {
        foreach (var o in _opItems) if (o != null) Destroy(o);
        _opItems.Clear();
        var q = QuestManager.Instance;
        var accepted = q != null ? q.Accepted.ToList() : new List<QuestData>();
        int max = q != null ? q.MaxActive : 2;
        _activeText.text = $"진행 중  <b>{accepted.Count} / {max}</b>";
        _activeFill.fillAmount = max > 0 ? (float)accepted.Count / max : 0f;

        // 작전 칸은 가로로 나란히 (칸이 늘어도 너비를 나눠 쓴다)
        const float gap = 29f, h = 246f;
        int slots = Mathf.Max(1, max);
        float w = (_opRoot.rect.width - gap * (slots - 1)) / slots;
        for (int i = 0; i < slots; i++)
        {
            float x = i * (w + gap);
            if (i < accepted.Count) _opItems.Add(OpCard(_opRoot, x, 0f, w, h, accepted[i], false));
            else _opItems.Add(OpEmpty(_opRoot, x, 0f, w, h));
        }

        if (_deploySummary != null)
        {
            int exp = accepted.Sum(a => a.rewardExp);
            _deploySummary.text = accepted.Count > 0
                ? $"수주한 작전 {accepted.Count}건  ·  보수 합계 <color=#EDEFF0>{exp} EXP</color>"
                : "수주한 작전 없음  ·  출격 전에 작전 게시판을 확인하세요";
        }
    }

    // ───────── 생성 ─────────

    private void Build()
    {
        if (_built) return;
        _built = true;
        _root = NewRect("OperationStatus", transform);
        Stretch(_root);

        // 바탕 (가로가 넓거나 좁아도 덮는다) + 눈 · 스캔선
        var bgFill = NewImage("Fill", _root, null, Ink);
        Stretch(bgFill.rectTransform);
        var bg = NewImage("Background", _root, background, Color.white);
        _bg = bg.rectTransform;
        _bg.anchorMin = _bg.anchorMax = new Vector2(0.5f, 0.5f);
        _bg.sizeDelta = new Vector2(W, H) * 1.06f;
        var fitter = bg.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = W / H;
        var ring = NewImage("RingGlow", _root, LoadAct("Halo"), new Color(0.75f, 0.88f, 1f, 0.12f));
        ring.rectTransform.anchorMin = ring.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        ring.rectTransform.sizeDelta = new Vector2(620f, 260f);
        ring.rectTransform.anchoredPosition = new Vector2(80f, 375f);
        var breath = ring.gameObject.AddComponent<UIGlowBreath>();
        _snowLayer = NewRect("Snow", _root);
        Stretch(_snowLayer);
        BuildSnow();
        var scan = NewImage("Scan", _root, Load("Scan") ?? LoadAct("Scan"), new Color(0.7f, 0.85f, 1f, 0.10f));
        _scan = scan.rectTransform;
        _scan.anchorMin = new Vector2(0f, 1f); _scan.anchorMax = new Vector2(1f, 1f);
        _scan.sizeDelta = new Vector2(0f, 90f);

        // 세 묶음: 왼쪽 · 가운데 · 오른쪽 (각각 1920×1080 좌표계, 화면 가장자리에 붙는다)
        _left = Group("Left", new Vector2(0f, 0.5f));
        _center = Group("Keeper", new Vector2(0f, 0.5f)); // 캐릭터는 왼쪽에 크게 (총력전 UI처럼)
        _right = Group("Right", new Vector2(1f, 0.5f));
        _center.SetSiblingIndex(_left.GetSiblingIndex()); // 작전 담당은 제목 판 뒤

        BuildCenter();
        BuildLeft();
        BuildRight();
        BuildDeckOverlay();
        if (tent != null)
        {
            DeployButton.onClick.AddListener(Deploy);
            BackButton.onClick.AddListener(tent.CloseInteraction);
        }
    }

    // 출격: 버튼이 한 번 번쩍이고 출정 시설이 런을 시작한다(효과음 · 씬 전환은 시설이 낸다)
    private void Deploy()
    {
        DeployButton.interactable = false;
        var rt = (RectTransform)DeployButton.transform;
        rt.DOPunchScale(Vector3.one * 0.04f, 0.25f, 8, 0.6f).SetUpdate(true).SetLink(gameObject);
        tent.StartRun();
    }

    private RectTransform Group(string name, Vector2 anchor)
    {
        var g = NewRect(name, _root);
        g.anchorMin = g.anchorMax = g.pivot = anchor;
        g.sizeDelta = new Vector2(W, H);
        return g;
    }

    private void BuildCenter()
    {
        // 작전 담당 (발이 화면 아래로 조금 나간다)
        var k = NewImage("Keeper", _center, keeperArt, Color.white);
        k.preserveAspect = true;
        _keeper = k.rectTransform;
        Place(_keeper, -10f, 96f, 860f, 1290f);
        _keeper.pivot = new Vector2(0.5f, 0f);
        _keeper.anchoredPosition = new Vector2(-10f + 860f / 2f, -(96f + 1290f));
        k.gameObject.AddComponent<UIIdleBreath>();
        Bracket(_center, 40f, 880f, 18f, 1f, 1f, Paper);

        var tag = NewImage("NameTag", _center, Load("Name_Tag"), Paper);
        Place(tag.rectTransform, 66f, 883f, 208f, 31f);
        string keeperName = keeper != null && !string.IsNullOrEmpty(keeper.displayName) ? keeper.displayName : "작전 담당";
        var tagText = Text(tag.rectTransform, keeperName + "  <size=70%><color=#5A6270>/ OPERATIONS</color></size>", 15f, InkText, true);
        Place(tagText.rectTransform, 14f, 0f, 190f, 31f);
        // 이름이 길면(편재의 유리아) 이름표를 글자에 맞춰 늘린다
        tagText.ForceMeshUpdate();
        float tagW = Mathf.Max(208f, tagText.preferredWidth + 32f);
        Sliced(tag, Paper);
        tag.rectTransform.sizeDelta = new Vector2(tagW, 31f);
        tagText.rectTransform.sizeDelta = new Vector2(tagW - 18f, 31f);

        var dlg = NewImage("Dialogue", _center, Load("Dialogue_Panel"), Color.white);
        Sliced(dlg, Ink);
        Place(dlg.rectTransform, 60f, 931f, 760f, 101f);
        var tick = NewImage("Tick", dlg.rectTransform, null, Cyan);
        Place(tick.rectTransform, 25f, 16f, 4f, 20f);
        var dName = Text(dlg.rectTransform, keeperName, 21f, Paper, true);
        Place(dName.rectTransform, 46f, 10f, 400f, 32f);
        var dLine = Text(dlg.rectTransform, "", 21f, Paper, false);
        Place(dLine.rectTransform, 48f, 46f, 680f, 46f);
        dLine.textWrappingMode = TextWrappingModes.Normal;

        _talk = gameObject.GetComponent<LobbyKeeperTalk>();
        if (_talk == null)
        {
            // 붙는 순간의 OnEnable은 data가 비어 그냥 지나간다. 다음 열기부터는 인사 · 방문 호감도가 정상으로 돈다
            _talk = gameObject.AddComponent<LobbyKeeperTalk>();
            _talk.Bind(keeper, dLine, dName, new Graphic[] { k, dlg });
            _talk.Greet();
        }
        _slideIns.Add((_keeper, _keeper.anchoredPosition, -40f));
        _slideIns.Add((dlg.rectTransform, dlg.rectTransform.anchoredPosition, -30f));
    }

    private void BuildLeft()
    {
        // 제목
        var title = NewImage("Title", _left, Load("Title_Plate"), Paper);
        Place(title.rectTransform, 60f, 37f, 584f, 108f);
        var no = Text(title.rectTransform, "04", 30f, InkText, true);
        Place(no.rectTransform, 44f, 32f, 60f, 44f);
        var bar = NewImage("Bar", title.rectTransform, null, InkText);
        Place(bar.rectTransform, 116f, 22f, 2f, 66f);
        var t = Text(title.rectTransform, "작전 상황", 52f, InkText, true);
        Place(t.rectTransform, 146f, 6f, 420f, 70f);
        var te = Text(title.rectTransform, "OPERATION STATUS  /  FORWARD STAGING", 14f, InkText, false);
        Place(te.rectTransform, 149f, 68f, 420f, 22f);
        te.characterSpacing = 18f;
        var sec = Text(_left, "SECTOR 01  ·  FLOOR 01  ·  STANDBY", 13f, Soft, false);
        Place(sec.rectTransform, 110f, 152f, 420f, 20f);
        sec.characterSpacing = 30f;
        Cross(_left, 48f, 38f); Cross(_left, 75f, 160f); Cross(_left, 48f, 1040f);
        _slideIns.Add((title.rectTransform, title.rectTransform.anchoredPosition, -60f));
    }

    private void BuildRight()
    {
        // 로비 버튼 · 판독문
        var back = NewImage("Back", _right, Load("Button_Back"), Paper);
        Place(back.rectTransform, 1620f, 42f, 234f, 63f);
        BackButton = MakeButton(back, Load("Button_Back_Hover"), Load("Button_Back_Pressed"));
        var bt = Text(back.rectTransform, "←  로비", 30f, InkText, true);
        Stretch(bt.rectTransform); bt.alignment = TextAlignmentOptions.Center;
        var readBack = NewImage("ReadoutBack", _right, null, new Color(0.02f, 0.035f, 0.06f, 0.72f)); // 배경 줄무늬 위에서도 읽히게
        Place(readBack.rectTransform, 1440f, 40f, 150f, 66f);
        _readout = Text(_right, "", 14f, Soft, false);
        Place(_readout.rectTransform, 1450f, 42f, 136f, 64f);
        _readout.characterSpacing = 12f; _readout.lineSpacing = 4f;
        Line(_right, 1594f, 44f, 1f, 62f);
        Cross(_right, 1880f, 38f); Cross(_right, 1880f, 1040f);
        _slideIns.Add((back.rectTransform, back.rectTransform.anchoredPosition, 60f));

        // 쪽 두 장: 01 작전(작전 현황 · 출격) / 02 장비 · 덱. 둘 다 같은 자리(x 860~1873, y 193~1014)를 쓴다
        for (int i = 0; i < _pages.Length; i++)
        {
            _pages[i] = NewRect(i == 0 ? "Page_Operations" : "Page_Loadout", _right);
            Stretch(_pages[i]);
            _pages[i].gameObject.AddComponent<CanvasGroup>();
        }
        BuildTabs();
        BuildOperationsPage(_pages[0]);
        BuildLoadoutPage(_pages[1]);
    }

    // 판 위 탭: 고른 탭은 종이색 머리판, 나머지는 어두운 판
    private void BuildTabs()
    {
        string[] labels = { "01   작전  <size=60%><color=#7A8492>OPERATIONS</color></size>", "02   장비 · 덱  <size=60%><color=#7A8492>LOADOUT</color></size>" };
        for (int i = 0; i < _tabs.Length; i++)
        {
            int page = i;
            var tab = NewImage("Tab" + (i + 1), _right, Load("Panel_Header"), Paper);
            Sliced(tab, Paper);
            Place(tab.rectTransform, 860f + i * 318f, 128f, 306f, 48f);
            tab.raycastTarget = true;
            var label = Text(tab.rectTransform, labels[i], 22f, InkText, true);
            Place(label.rectTransform, 24f, 0f, 280f, 48f);
            var btn = tab.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => SetPage(page, true));
            tab.gameObject.AddComponent<UIButtonSfx>();
            tab.gameObject.AddComponent<OpHoverReact>().Setup(tab.rectTransform, null, Vector2.zero, null);
            _tabs[i] = tab;
            _tabLabels[i] = label;
            _slideIns.Add((tab.rectTransform, tab.rectTransform.anchoredPosition, 60f));
        }
        var hint = Text(_right, "Q / E  쪽 넘기기", 13f, Soft, false);
        Place(hint.rectTransform, 1680f, 140f, 190f, 24f);
        hint.alignment = TextAlignmentOptions.MidlineRight;
        hint.characterSpacing = 12f;
    }

    private void SetPage(int index, bool animate)
    {
        if (_pages[0] == null) return;
        index = Mathf.Clamp(index, 0, _pages.Length - 1);
        bool changed = index != _page;
        _page = index;
        for (int i = 0; i < _pages.Length; i++)
        {
            bool on = i == index;
            _pages[i].gameObject.SetActive(on);
            _tabs[i].color = on ? Paper : new Color(Ink.r, Ink.g, Ink.b, 0.82f);
            _tabLabels[i].color = on ? InkText : Soft;
        }
        if (!animate || !changed) return;
        var p = _pages[index];
        var cg = p.GetComponent<CanvasGroup>();
        p.DOKill(); cg.DOKill();
        p.anchoredPosition = new Vector2(index == 1 ? 40f : -40f, 0f);
        cg.alpha = 0f;
        p.DOAnchorPos(Vector2.zero, 0.3f).SetEase(Ease.OutExpo).SetUpdate(true).SetLink(gameObject);
        cg.DOFade(1f, 0.2f).SetUpdate(true).SetLink(gameObject);
        UISfx.Play("ui_click");
    }

    // 01 작전: 위에 작전 현황(작전 카드 나란히), 아래에 출격
    private void BuildOperationsPage(RectTransform p)
    {
        var c = Panel(p, "A / 01", "작전 현황", "OPERATIONS", 860f, 193f, 1013f, 460f, 63f, 60f);
        _activeText = Text(c, "", 20f, Paper, false); Place(_activeText.rectTransform, 37f, 74f, 180f, 28f);
        var track = NewImage("Track", c, null, new Color(1f, 1f, 1f, 0.15f)); Place(track.rectTransform, 217f, 86f, 765f, 7f);
        _activeFill = NewImage("Fill", track.rectTransform, null, Cyan);
        Stretch(_activeFill.rectTransform);
        _activeFill.type = Image.Type.Filled; _activeFill.fillMethod = Image.FillMethod.Horizontal; _activeFill.sprite = WhiteSprite();
        _opRoot = NewRect("Ops", c); Place(_opRoot, 31f, 120f, 951f, 246f);
        var board = NewImage("BoardButton", c, Load("Button_Board"), Color.white);
        Sliced(board, Ink);
        Place(board.rectTransform, 26f, 390f, 961f, 44f);
        var boardBtn = MakeButton(board, Load("Button_Board_Hover"), Load("Button_Board_Pressed"));
        boardBtn.onClick.AddListener(OpenQuestBoard);
        var bbt = Text(board.rectTransform, "작전 게시판  →", 20f, Paper, true);
        Stretch(bbt.rectTransform); bbt.alignment = TextAlignmentOptions.Center;

        var d = Panel(p, "B / 02", null, "DEPLOY", 860f, 670f, 1013f, 344f, 57f, 60f);
        var ns = Text(d, "NEXT SECTOR", 13f, Soft, false); Place(ns.rectTransform, 36f, 96f, 300f, 20f); ns.characterSpacing = 30f;
        _sector = Text(d, "", 30f, Paper, true); Place(_sector.rectTransform, 36f, 118f, 450f, 42f);
        Line(d, 36f, 176f, 420f, 1f);
        _deploySummary = Text(d, "", 18f, Soft, false); Place(_deploySummary.rectTransform, 36f, 190f, 440f, 56f);
        _deploySummary.textWrappingMode = TextWrappingModes.Normal;
        var dep = NewImage("Deploy", d, Load("Button_Deploy"), Color.white);
        if (dep.sprite == null) dep.color = Ink;
        Place(dep.rectTransform, 504f, 130f, 479f, 141f);
        DeployButton = MakeButton(dep, Load("Button_Deploy_Hover"), Load("Button_Deploy_Pressed"));
        var dt = Text(dep.rectTransform, "출격", 60f, Paper, true); Place(dt.rectTransform, 32f, 26f, 200f, 86f);
        var arrow = Text(dep.rectTransform, "→", 46f, Paper, true); Place(arrow.rectTransform, 158f, 36f, 80f, 66f);
        dep.gameObject.AddComponent<UIPixelBubbles>();
        var react = dep.gameObject.AddComponent<OpHoverReact>();
        react.Setup(dep.rectTransform, arrow.rectTransform, new Vector2(16f, 0f), () =>
        {
            if (_talk == null) return;
            bool none = QuestManager.Instance == null || !QuestManager.Instance.Accepted.Any();
            _talk.Say(none ? lineNoOperation : lineDeployHover);
        });
    }

    // 02 장비 · 덱: 위에 장비 현황(무기 그림 크게), 아래에 덱 현황(왼쪽 통계 · 오른쪽 많이 든 카드)
    private void BuildLoadoutPage(RectTransform p)
    {
        var a = Panel(p, "C / 03", "장비 현황", "EQUIPMENT", 860f, 193f, 1013f, 326f, 59f, 60f);
        _weaponName = Text(a, "", 40f, Paper, true); Place(_weaponName.rectTransform, 40f, 94f, 300f, 52f);
        _weaponSub = Text(a, "", 19f, Soft, false); Place(_weaponSub.rectTransform, 40f, 148f, 300f, 28f);
        var win = NewRect("WeaponWindow", a); Place(win, 380f, 74f, 580f, 140f);
        _weaponImage = NewImage("Weapon", win, null, Color.white); Stretch(_weaponImage.rectTransform); _weaponImage.preserveAspect = true;
        foreach (var (bx, by, sx, sy) in new[] { (0f, 0f, 1f, 1f), (580f, 0f, -1f, 1f), (0f, 140f, 1f, -1f), (580f, 140f, -1f, -1f) })
            Bracket(win, bx, by, 14f, sx, sy, Soft);
        Line(a, 24f, 225f, 965f, 1f);
        string[] labels = { "탄창", "사격 추가 피해", "강화" };
        float[] xs = { 40f, 300f, 560f };
        var vals = new TextMeshProUGUI[3];
        for (int i = 0; i < 3; i++)
        {
            var l = Text(a, labels[i], 15f, Soft, false); Place(l.rectTransform, xs[i], 233f, 200f, 22f);
            vals[i] = Text(a, "", 26f, Paper, true); Place(vals[i].rectTransform, xs[i], 253f, 200f, 34f);
            if (i > 0) Line(a, xs[i] - 28f, 236f, 1f, 50f);
        }
        _ammo = vals[0]; _shot = vals[1]; _upgrade = vals[2];
        LinkText(a, "무기고  →", 820f, 284f, 162f, 26f, OpenArmory);

        var b = Panel(p, "D / 04", "덱 현황", "DECK", 860f, 537f, 1013f, 477f, 61f, 60f);
        _deckTotal = Text(b, "", 54f, Paper, true); Place(_deckTotal.rectTransform, 38f, 76f, 150f, 70f);
        Line(b, 176f, 80f, 1f, 68f);
        string[] tl = { "공격", "스킬", "파워" };
        for (int i = 0; i < 3; i++)
        {
            float y = 79f + i * 26f;
            var l = Text(b, tl[i], 16f, Soft, false); Place(l.rectTransform, 206f, y - 2f, 50f, 22f);
            var track = NewImage("Track", b, null, new Color(1f, 1f, 1f, 0.12f)); Place(track.rectTransform, 260f, y + 6f, 160f, 8f);
            _typeFill[i] = NewImage("Fill", track.rectTransform, null, Paper);
            Stretch(_typeFill[i].rectTransform);
            _typeFill[i].type = Image.Type.Filled; _typeFill[i].fillMethod = Image.FillMethod.Horizontal;
            _typeFill[i].sprite = WhiteSprite();
            _typeCount[i] = Text(b, "", 17f, Paper, true); Place(_typeCount[i].rectTransform, 430f, y - 3f, 30f, 22f);
            _typeCount[i].alignment = TextAlignmentOptions.MidlineRight;
        }
        Line(b, 37f, 164f, 429f, 1f);
        var cl = Text(b, "비용 분포", 16f, Soft, false); Place(cl.rectTransform, 39f, 172f, 120f, 24f);
        var hist = NewRect("Histogram", b); Place(hist, 84f, 206f, 360f, HistHeight);
        Line(hist, 0f, HistHeight - 3f, 360f, 1f);
        string[] cn = { "0", "1", "2", "3+" };
        for (int i = 0; i < 4; i++)
        {
            float cx = 51f + i * 88f;
            var col = NewRect("Col" + i, hist);
            col.anchorMin = col.anchorMax = new Vector2(0f, 0f); col.pivot = new Vector2(0.5f, 0f);
            col.anchoredPosition = new Vector2(cx, 3f); col.sizeDelta = new Vector2(64f, 0f);
            _costBar[i] = NewImage("Bar", col, null, new Color(0.86f, 0.88f, 0.9f, 0.9f));
            _costBar[i].rectTransform.anchorMin = _costBar[i].rectTransform.anchorMax = new Vector2(0.5f, 0f);
            _costBar[i].rectTransform.pivot = new Vector2(0.5f, 0f);
            _costBar[i].rectTransform.sizeDelta = new Vector2(64f, 4f);
            _costCount[i] = Text(col, "0", 15f, Paper, true);
            _costCount[i].rectTransform.anchorMin = _costCount[i].rectTransform.anchorMax = new Vector2(0.5f, 0f);
            _costCount[i].rectTransform.pivot = new Vector2(0.5f, 0f);
            _costCount[i].rectTransform.sizeDelta = new Vector2(60f, 20f);
            _costCount[i].alignment = TextAlignmentOptions.Bottom;
            var lab = Text(hist, cn[i], 14f, Soft, false);
            lab.rectTransform.anchorMin = lab.rectTransform.anchorMax = new Vector2(0f, 0f);
            lab.rectTransform.pivot = new Vector2(0.5f, 1f);
            lab.rectTransform.anchoredPosition = new Vector2(cx, -2f);
            lab.rectTransform.sizeDelta = new Vector2(60f, 20f);
            lab.alignment = TextAlignmentOptions.Top;
        }
        Line(b, 496f, 80f, 1f, 340f);
        var rl = Text(b, "많이 든 카드", 16f, Soft, false); Place(rl.rectTransform, 522f, 72f, 200f, 24f);
        _cardRowRoot = NewRect("Rows", b); Place(_cardRowRoot, 522f, 102f, CardRowWidth, CardRowCount * 49f);
        LinkText(b, "덱 전체 보기  →", 780f, 432f, 202f, 26f, () => ShowDeckOverlay(true));
    }

    private void BuildDeckOverlay()
    {
        var ov = NewRect("DeckOverlay", _root);
        Stretch(ov);
        _deckOverlay = ov.gameObject;
        var dim = NewImage("Dim", ov, null, new Color(0.01f, 0.02f, 0.04f, 0.72f));
        Stretch(dim.rectTransform);
        dim.raycastTarget = true;
        dim.gameObject.AddComponent<Button>().onClick.AddListener(() => ShowDeckOverlay(false));
        var p = NewImage("Panel", ov, Load("Panel_Body"), Color.white);
        Sliced(p, Ink);
        p.raycastTarget = true;
        p.rectTransform.anchorMin = p.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        p.rectTransform.sizeDelta = new Vector2(760f, 760f);
        var head = NewImage("Header", p.rectTransform, Load("Panel_Header"), Paper);
        Sliced(head, Paper);
        Place(head.rectTransform, 0f, 0f, 760f, 60f);
        var ht = Text(head.rectTransform, "덱 전체  <size=55%><color=#5A6270>DECK LIST</color></size>", 30f, InkText, true);
        Place(ht.rectTransform, 28f, 6f, 600f, 48f);
        var close = Text(head.rectTransform, "닫기  ×", 20f, InkText, true);
        Place(close.rectTransform, 620f, 12f, 120f, 36f);
        close.alignment = TextAlignmentOptions.MidlineRight;
        close.raycastTarget = true;
        close.gameObject.AddComponent<Button>().onClick.AddListener(() => ShowDeckOverlay(false));
        var scroll = NewRect("Scroll", p.rectTransform); Place(scroll, 30f, 80f, 700f, 660f);
        scroll.gameObject.AddComponent<RectMask2D>();
        var sr = scroll.gameObject.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.scrollSensitivity = 30f;
        _deckOverlayList = NewRect("List", scroll);
        _deckOverlayList.anchorMin = new Vector2(0f, 1f); _deckOverlayList.anchorMax = new Vector2(1f, 1f); _deckOverlayList.pivot = new Vector2(0.5f, 1f);
        sr.content = _deckOverlayList;
        _deckOverlay.SetActive(false);
    }

    private void ShowDeckOverlay(bool on)
    {
        if (_deckOverlay == null) return;
        if (!on) { _deckOverlay.SetActive(false); return; }
        for (int i = _deckOverlayList.childCount - 1; i >= 0; i--) Destroy(_deckOverlayList.GetChild(i).gameObject);
        var groups = DeckGroups();
        for (int i = 0; i < groups.Count; i++) CardRow(_deckOverlayList, 0f, i * 64f, 700f, groups[i].card, groups[i].count, true);
        _deckOverlayList.sizeDelta = new Vector2(0f, groups.Count * 64f);
        // 창 높이는 줄 수에 맞춘다 (최대 760)
        var panel = (RectTransform)_deckOverlay.transform.Find("Panel");
        float ph = Mathf.Clamp(groups.Count * 64f + 110f, 260f, 760f);
        panel.sizeDelta = new Vector2(760f, ph);
        ((RectTransform)panel.Find("Scroll")).sizeDelta = new Vector2(700f, ph - 100f);
        _deckOverlay.SetActive(true);
        var p = (RectTransform)_deckOverlay.transform.Find("Panel");
        p.localScale = Vector3.one * 0.96f;
        p.DOScale(1f, 0.18f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(_deckOverlay);
        UISfx.Play("ui_click");
    }

    // ───────── 조각 ─────────

    // 어두운 판 + 종이 머리판(번호 · 한글 · 영문). 머리판에 마우스를 올리면 시안 띠, 판이 살짝 떠오른다. 판 전체 RectTransform을 돌려준다
    private RectTransform Panel(RectTransform group, string no, string title, string eng, float x, float y, float w, float h, float headH, float slideFrom)
    {
        var root = NewRect("Panel_" + no, group);
        Place(root, x, y, w, h);
        var body = NewImage("Body", root, Load("Panel_Body"), Color.white);
        Sliced(body, Ink);
        body.color = new Color(body.color.r, body.color.g, body.color.b, PanelAlpha);
        body.raycastTarget = true;
        Place(body.rectTransform, 0f, headH - 4f, w, h - headH + 4f);
        var head = NewImage("Header", root, Load("Panel_Header"), Color.white);
        Sliced(head, Paper);
        Place(head.rectTransform, 0f, 0f, w, headH);
        // 호버 머리판은 머리판 안쪽 맨 아래 층에 겹친다 (글자 밑)
        var headHover = NewImage("HeaderHover", head.rectTransform, Load("Panel_Header_Hover"), new Color(1f, 1f, 1f, 0f));
        Sliced(headHover, Paper);
        if (headHover.sprite == null) { Place(headHover.rectTransform, 0f, headH - 3f, w, 3f); headHover.color = new Color(Cyan.r, Cyan.g, Cyan.b, 0f); }
        else Stretch(headHover.rectTransform);
        headHover.transform.SetAsFirstSibling();
        var n = Text(head.rectTransform, no, 15f, InkText, true); Place(n.rectTransform, 26f, 0f, 80f, headH);
        var bar = NewImage("Bar", head.rectTransform, null, InkText); Place(bar.rectTransform, 100f, headH * 0.25f, 2f, headH * 0.5f);
        float tx = 118f;
        if (!string.IsNullOrEmpty(title))
        {
            var t = Text(head.rectTransform, title, 32f, InkText, true); Place(t.rectTransform, tx, 0f, 200f, headH);
            t.ForceMeshUpdate(); tx += t.preferredWidth + 22f;
        }
        var e = Text(head.rectTransform, eng, 13f, InkText, false); Place(e.rectTransform, tx, 0f, 220f, headH); e.characterSpacing = 30f;
        var react = root.gameObject.AddComponent<OpHoverReact>();
        react.Setup(root, null, Vector2.zero, null, headHover);
        _slideIns.Add((root, root.anchoredPosition, slideFrom));
        return root; // 판 안 내용은 판 왼쪽 위(머리판 포함) 기준 좌표로 놓는다
    }

    private GameObject CardRow(RectTransform parent, float x, float y, float w, CardData card, int count, bool withDesc)
    {
        float h = withDesc ? 60f : 47f;
        var row = NewImage("Row", parent, Load("Row_Card"), Color.white);
        Sliced(row, new Color(0.08f, 0.11f, 0.15f, 1f));
        Place(row.rectTransform, x, y, w, h);
        var icon = NewImage("Icon", row.rectTransform, IconOf(card), Paper);
        icon.preserveAspect = true;
        Place(icon.rectTransform, 22f, h / 2f - 15f, 30f, 30f);
        string name = card.CardName + (card.isUpgraded ? " <color=#1EA0E6>+</color>" : "");
        var n = Text(row.rectTransform, name, withDesc ? 21f : 21f, Paper, true);
        Place(n.rectTransform, 76f, withDesc ? 4f : 0f, w - 160f, withDesc ? 30f : h);
        if (withDesc)
        {
            var dsc = Text(row.rectTransform, card.GetDescription(card.isUpgraded).Replace("\n", " "), 15f, Soft, false);
            Place(dsc.rectTransform, 78f, 32f, w - 170f, 24f);
            dsc.overflowMode = TextOverflowModes.Ellipsis;
        }
        var cnt = Text(row.rectTransform, $"×{count}", 21f, Paper, true);
        Place(cnt.rectTransform, w - 80f, 0f, 64f, h);
        cnt.alignment = TextAlignmentOptions.MidlineRight;
        return row.gameObject;
    }

    private Sprite IconOf(CardData c)
    {
        if (c.BaseCardName.Contains("재장전")) return Load("Icon_Card_Reload");
        return c.Type == CardData.CardType.Attack ? Load("Icon_Card_Attack") : Load("Icon_Card_Defense");
    }

    private GameObject OpCard(RectTransform parent, float x, float y, float w, float h, QuestData q, bool compact)
    {
        var card = NewImage("OpCard", parent, Load("Op_Card"), Color.white);
        Sliced(card, Ink);
        card.raycastTarget = true;
        Place(card.rectTransform, x, y, w, h);
        var hover = NewImage("Hover", card.rectTransform, Load("Op_Card_Hover"), new Color(1f, 1f, 1f, 0f));
        Sliced(hover, Ink);
        Stretch(hover.rectTransform);
        var icon = NewImage("Icon", card.rectTransform, Load("Icon_Op_" + q.IconKind), Paper);
        icon.preserveAspect = true;
        Place(icon.rectTransform, 32f, 32f, 46f, 46f);
        Line(card.rectTransform, 96f, 24f, 2f, 60f);
        var type = Text(card.rectTransform, q.TypeName, 15f, Soft, false); Place(type.rectTransform, 116f, 18f, 200f, 22f);
        var title = Text(card.rectTransform, q.title, compact ? 26f : 32f, Paper, true); Place(title.rectTransform, 116f, 36f, w - 140f, 44f);
        title.overflowMode = TextOverflowModes.Ellipsis;
        var client = Text(card.rectTransform, q.client, 17f, Soft, false); Place(client.rectTransform, 118f, compact ? 76f : 84f, w - 140f, 24f);
        var objs = q.Objectives();
        int shown = compact ? Mathf.Min(1, objs.Count) : Mathf.Min(2, objs.Count);
        float oy = compact ? 106f : 120f;
        for (int i = 0; i < shown; i++)
        {
            var chk = NewImage("Check", card.rectTransform, Load("Check_Off"), Soft);
            Place(chk.rectTransform, 38f, oy + i * 33f, 22f, 22f);
            var ot = Text(card.rectTransform, objs[i], 18f, Paper, false); Place(ot.rectTransform, 78f, oy - 3f + i * 33f, w - 100f, 28f);
        }
        if (!compact)
        {
            Line(card.rectTransform, 22f, 190f, w - 44f, 1f);
            var rl = Text(card.rectTransform, "보상", 16f, Soft, false); Place(rl.rectTransform, 52f, 200f, 60f, 32f);
            var rv = Text(card.rectTransform, $"{q.rewardExp} EXP", 26f, Paper, true); Place(rv.rectTransform, 112f, 196f, 200f, 36f);
        }
        else
        {
            var rv = Text(card.rectTransform, $"{q.rewardExp} EXP", 20f, Paper, true); Place(rv.rectTransform, w - 150f, 18f, 130f, 26f);
            rv.alignment = TextAlignmentOptions.MidlineRight;
        }
        var react = card.gameObject.AddComponent<OpHoverReact>();
        react.Setup(card.rectTransform, null, Vector2.zero, null, hover);
        var btn = card.gameObject.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(OpenQuestBoard);
        card.gameObject.AddComponent<UIButtonSfx>();
        return card.gameObject;
    }

    private GameObject OpEmpty(RectTransform parent, float x, float y, float w, float h)
    {
        var slot = NewImage("OpEmpty", parent, Load("Op_Slot_Empty"), Color.white);
        Sliced(slot, new Color(1f, 1f, 1f, 0.06f));
        slot.raycastTarget = true;
        Place(slot.rectTransform, x, y, w, h);
        var t = Text(slot.rectTransform, "비어 있음  ·  작전 게시판에서 수주", 17f, Soft, false);
        Stretch(t.rectTransform); t.alignment = TextAlignmentOptions.Center;
        var btn = slot.gameObject.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(OpenQuestBoard);
        slot.gameObject.AddComponent<UIButtonSfx>();
        slot.gameObject.AddComponent<OpHoverReact>().Setup(slot.rectTransform, null, Vector2.zero, null);
        return slot.gameObject;
    }

    // ───────── 동작 ─────────

    private void OpenQuestBoard()
    {
        if (questBoard == null) return;
        var me = transform; while (me.parent != null && me.parent.GetComponent<Canvas>() == null) me = me.parent; // 이 화면의 캔버스 바로 아래 조상
        if (questBoard.transform.parent == me.parent) questBoard.transform.SetSiblingIndex(me.GetSiblingIndex() + 1);
        questBoard.SetActive(true);
        UISfx.Play("ui_click");
    }

    private void OpenArmory()
    {
        if (BackButton != null) BackButton.onClick.Invoke();
        if (armoryMenuButton != null && armoryMenuButton.interactable) armoryMenuButton.onClick.Invoke();
    }

    // ───────── 연출 ─────────

    private void PlayIntro()
    {
        _intro?.Kill();
        _intro = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        for (int i = 0; i < _slideIns.Count; i++)
        {
            var (rt, home, from) = _slideIns[i];
            if (rt == null) continue;
            bool vertical = rt == _keeper || rt.name == "Dialogue";
            var start = home + (vertical ? new Vector2(0f, from) : new Vector2(from, 0f));
            rt.anchoredPosition = start;
            var cg = rt.GetComponent<CanvasGroup>();
            if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>(); // ?? 는 Unity의 가짜 null을 못 거른다
            cg.alpha = 0f;
            float at = 0.04f * i;
            _intro.Insert(at, rt.DOAnchorPos(home, 0.42f).SetEase(Ease.OutExpo));
            _intro.Insert(at, cg.DOFade(1f, 0.25f));
        }
        if (_bg != null) { _bg.localScale = Vector3.one * 1.04f; _intro.Insert(0f, _bg.DOScale(1f, 1.2f).SetEase(Ease.OutCubic)); }
    }

    private void Update()
    {
        if (!_built) return;
        float t = Time.unscaledTime;

        // Q / E 쪽 넘기기 (덱 전체 창이 떠 있으면 무시)
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && (_deckOverlay == null || !_deckOverlay.activeSelf))
        {
            if (kb.qKey.wasPressedThisFrame) SetPage(_page - 1, true);
            else if (kb.eKey.wasPressedThisFrame) SetPage(_page + 1, true);
        }

        // 마우스 시차: 바탕은 멀리, 작전 담당은 가까이
        Vector2 m = UnityEngine.InputSystem.Mouse.current != null ? UnityEngine.InputSystem.Mouse.current.position.ReadValue() : new Vector2(Screen.width, Screen.height) * 0.5f;
        Vector2 n = new(Mathf.Clamp01(m.x / Mathf.Max(1, Screen.width)) - 0.5f, Mathf.Clamp01(m.y / Mathf.Max(1, Screen.height)) - 0.5f);
        if (_bg != null) _bg.anchoredPosition = Vector2.Lerp(_bg.anchoredPosition, -n * 24f, Time.unscaledDeltaTime * 4f);
        if (_center != null) _center.anchoredPosition = Vector2.Lerp(_center.anchoredPosition, -n * 10f, Time.unscaledDeltaTime * 4f);

        // 눈발
        var size = _snowLayer.rect.size;
        foreach (var s in _snow)
        {
            var p = s.rt.anchoredPosition + (s.vel + new Vector2(Mathf.Sin(t * 0.7f + s.phase) * 8f, 0f)) * Time.unscaledDeltaTime;
            if (p.y < -size.y * 0.5f - 10f) p = new Vector2(Random.Range(-size.x * 0.5f, size.x * 0.5f), size.y * 0.5f + 10f);
            if (p.x < -size.x * 0.5f - 10f) p.x += size.x + 20f;
            s.rt.anchoredPosition = p;
        }

        // 스캔선: 6초마다 위에서 아래로
        if (_scan != null)
        {
            float k = Mathf.Repeat(t, 6f) / 2.2f;
            _scan.anchoredPosition = new Vector2(0f, -Mathf.Clamp01(k) * (_root.rect.height + 90f));
        }

        if (_readout != null)
        {
            float el = t - _openedAt;
            _readout.text = $"ALT  0280\nSEC  01-01\nT+   {Mathf.FloorToInt(el / 60f):00}:{el % 60f:00}";
        }
    }

    private void BuildSnow()
    {
        var dot = WhiteSprite();
        for (int i = 0; i < 46; i++)
        {
            float depth = Random.value;
            var img = NewImage("Flake", _snowLayer, dot, new Color(0.9f, 0.95f, 1f, Mathf.Lerp(0.12f, 0.55f, depth)));
            float sz = Mathf.Lerp(2f, 5f, depth);
            img.rectTransform.sizeDelta = new Vector2(sz, sz);
            img.rectTransform.anchoredPosition = new Vector2(Random.Range(-W * 0.6f, W * 0.6f), Random.Range(-H * 0.5f, H * 0.5f));
            _snow.Add((img.rectTransform, new Vector2(Mathf.Lerp(-10f, -35f, depth), -Mathf.Lerp(20f, 70f, depth)), Random.value * 6.28f));
        }
    }

    // ───────── 도우미 ─────────

    private static Sprite Load(string name) => Resources.Load<Sprite>("OperationStatus/" + name);
    private static Sprite LoadAct(string name) => Resources.Load<Sprite>("ActIntro/" + name);

    private static Sprite s_white;
    private static Sprite WhiteSprite()
    {
        if (s_white != null) return s_white;
        var tex = Texture2D.whiteTexture;
        s_white = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        return s_white;
    }

    // 조각이 있으면 9-slice로, 없으면 단색 판으로
    private static void Sliced(Image img, Color fallback)
    {
        if (img.sprite == null) { img.sprite = null; img.color = new Color(fallback.r, fallback.g, fallback.b, img.color.a > 0f ? fallback.a : 0f); return; }
        if (img.sprite.border != Vector4.zero) { img.type = Image.Type.Sliced; img.pixelsPerUnitMultiplier = 1f; }
    }

    private Button MakeButton(Image img, Sprite hover, Sprite pressed)
    {
        img.raycastTarget = true;
        var b = img.gameObject.AddComponent<Button>();
        if (hover != null || pressed != null)
        {
            b.transition = Selectable.Transition.SpriteSwap;
            var ss = b.spriteState; ss.highlightedSprite = hover; ss.selectedSprite = null; ss.pressedSprite = pressed ?? hover; b.spriteState = ss;
        }
        else b.transition = Selectable.Transition.ColorTint;
        img.canvasRenderer.SetColor(Color.white);
        img.gameObject.AddComponent<UIButtonSfx>();
        return b;
    }

    private void LinkText(RectTransform parent, string text, float x, float y, float w, float h, UnityEngine.Events.UnityAction onClick)
    {
        var t = Text(parent, text, 16f, Paper, false);
        Place(t.rectTransform, x, y, w, h);
        t.alignment = TextAlignmentOptions.MidlineRight;
        t.raycastTarget = true;
        var b = t.gameObject.AddComponent<Button>();
        b.transition = Selectable.Transition.ColorTint;
        var cb = b.colors; cb.highlightedColor = new Color(0.55f, 0.85f, 1f, 1f); cb.pressedColor = Cyan; b.colors = cb;
        b.onClick.AddListener(onClick);
        t.gameObject.AddComponent<UIButtonSfx>();
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite, Color color)
    {
        var rt = NewRect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private TextMeshProUGUI Text(Transform parent, string text, float size, Color color, bool bold)
    {
        var rt = NewRect("Txt", parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        var f = bold ? fontBold : fontRegular;
        if (f == null) f = fontBold != null ? fontBold : TMP_Settings.defaultFontAsset;
        t.font = f;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.MidlineLeft;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.raycastTarget = false;
        return t;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // 부모 왼쪽 위 기준 [x, y, w, h]
    private static void Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    private static void Line(RectTransform parent, float x, float y, float w, float h)
        => Place(NewImage("Line", parent, null, new Color(1f, 1f, 1f, 0.22f)).rectTransform, x, y, w, h);

    private static void Cross(RectTransform parent, float cx, float cy)
    {
        var c = new Color(1f, 1f, 1f, 0.7f);
        Place(NewImage("CrossH", parent, null, c).rectTransform, cx - 11f, cy - 0.75f, 22f, 1.5f);
        Place(NewImage("CrossV", parent, null, c).rectTransform, cx - 0.75f, cy - 11f, 1.5f, 22f);
    }

    // 꺾쇠 한 귀퉁이 (x, y는 꺾이는 점, sx · sy는 선이 뻗는 방향)
    private static void Bracket(RectTransform parent, float x, float y, float len, float sx, float sy, Color c)
    {
        var h = NewImage("BrH", parent, null, c); Place(h.rectTransform, sx > 0 ? x : x - len, y - 1f, len, 2f);
        var v = NewImage("BrV", parent, null, c); Place(v.rectTransform, x - 1f, sy > 0 ? y : y - len, 2f, len);
    }
}

// 판 · 카드 · 버튼의 호버 반응: 살짝 떠오르고, 강조 그림(머리판 시안 띠 · 밝은 카드)이 나타나고, 화살표가 밀린다
public class OpHoverReact : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform _target, _nudge;
    private Vector2 _nudgeBy, _home, _nudgeHome;
    private Image _highlight;
    private System.Action _onEnter;
    private Tween _t1, _t2, _t3;
    private bool _inited;

    public void Setup(RectTransform target, RectTransform nudge, Vector2 nudgeBy, System.Action onEnter, Image highlight = null)
    {
        _target = target; _nudge = nudge; _nudgeBy = nudgeBy; _onEnter = onEnter; _highlight = highlight;
        // 원래 자리는 만들 때의 배치 위치로 고정한다. 마우스를 처음 올린 순간에 재면
        // 등장 연출로 미끄러져 들어오는 중간 위치가 기억되어 그 자리에 멈춰 버린다
        if (_target != null) { _home = _target.anchoredPosition; _inited = true; }
        if (_nudge != null) _nudgeHome = _nudge.anchoredPosition;
    }

    private void Init() { }

    public void OnPointerEnter(PointerEventData e)
    {
        if (!_inited) return;
        Kill();
        _t1 = _target.DOAnchorPos(_home + new Vector2(0f, 4f), 0.16f).SetEase(Ease.OutCubic).SetUpdate(true);
        if (_highlight != null) _t2 = _highlight.DOFade(1f, 0.14f).SetUpdate(true);
        if (_nudge != null) _t3 = _nudge.DOAnchorPos(_nudgeHome + _nudgeBy, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
        _onEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (!_inited) return;
        Kill();
        _t1 = _target.DOAnchorPos(_home, 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);
        if (_highlight != null) _t2 = _highlight.DOFade(0f, 0.2f).SetUpdate(true);
        if (_nudge != null) _t3 = _nudge.DOAnchorPos(_nudgeHome, 0.2f).SetUpdate(true);
        _target.localScale = Vector3.one;
    }

    public void OnPointerDown(PointerEventData e) { if (_target != null) _target.DOScale(0.98f, 0.06f).SetUpdate(true); }
    public void OnPointerUp(PointerEventData e) { if (_target != null) _target.DOScale(1f, 0.12f).SetEase(Ease.OutBack).SetUpdate(true); }

    private void OnDisable()
    {
        Kill();
        if (_inited && _target != null) { _target.anchoredPosition = _home; _target.localScale = Vector3.one; }
        if (_highlight != null) { var c = _highlight.color; c.a = 0f; _highlight.color = c; }
        if (_nudge != null && _inited) _nudge.anchoredPosition = _nudgeHome;
    }

    private void Kill() { _t1?.Kill(); _t2?.Kill(); _t3?.Kill(); }
}
