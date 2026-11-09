using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 로비 거점 화면 (예전 베이스캠프 · 모닥불 자리). 페르소나 3 포터블 기숙사를 정면에서 본 구도:
// 구역 탭으로 방을 고르면 합류한 동료가 작게 각자 자리에서 하던 일을 하고 있고, 누르면 말을 건다. 동료는 돌아다니지 않는다.
//   - 구역: BaseZoneData (배경 · 동료 자리 · 시설 표지 · 확장 잠김 · 해금 조건)
//   - 동료 자리: CoopCharData.baseSpots에서 런이 끝날 때마다 하나를 뽑는다 (없으면 빈 자리 아무 데나)
//   - 동료를 누르면: 호감도 이벤트가 있으면('!') 대화 이벤트, 아니면 잡담 한 줄
//   - 시설 표지를 누르면 기존 화면(아카이브 · 훈련장 · 무기고 · 의뢰 · 상점, 탐색 개시는 텐트 시설)을 연다
// UI는 코드로 조립한다. 아트: Resources/BaseCamp (ArtDirection/BaseCampMockup/Extracted, layout.json 1920×1080 왼쪽 위 원점)
// 기획: Docs/기획/거점.md
public class BaseCampView : MonoBehaviour
{
    [Header("구역 (탭 순서)")]
    [SerializeField] private List<BaseZoneData> zones = new();

    [Header("동료 (합류한 적 있는 동료만 나온다)")]
    [SerializeField] private List<CoopCharData> companions = new();

    [Header("시설 화면 (표지를 누르면 켠다)")]
    [SerializeField] private GameObject archiveView;
    [SerializeField] private GameObject trainingView;
    [SerializeField] private GameObject armoryView;
    [SerializeField] private GameObject questView;
    [SerializeField] private GameObject shopView;

    [Header("로비")]
    [Tooltip("거점이 열려 있는 동안 숨기는 로비 메인 화면")]
    [SerializeField] private GameObject mainView;
    [SerializeField] private TextAsset defaultRelationshipDialogue;

    [Header("재화 아이콘 (로비 재화 줄과 같은 그림)")]
    [SerializeField] private Sprite talinIcon;
    [SerializeField] private Sprite partsIcon;
    [SerializeField] private Sprite artifactIcon;

    [Header("글꼴")]
    [SerializeField] private TMP_FontAsset fontBold;
    [SerializeField] private TMP_FontAsset fontRegular;

    [Header("동료")]
    [Tooltip("평소 동료 색 (밤 조명에 맞춰 살짝 어둡게). 마우스를 올리면 흰색")]
    [SerializeField, Range(0f, 1f)] private float contactShadowAlpha = 0.85f;
    [SerializeField, Range(0f, 1f)] private float castShadowAlpha = 0.5f;
    [SerializeField, Min(0.5f)] private float lineHoldSeconds = 3.5f;

    private static readonly Color Ink = new(0.906f, 0.902f, 0.871f, 1f);   // #E7E6DE
    private static readonly Color Dim = new(0.659f, 0.667f, 0.667f, 1f);   // #A8AAAA
    private const float TabX0 = 69f, TabSpan = 1830f, TabY = 962f, TabH = 108f;

    private static int _lastZone;

    private bool _built;
    private RectTransform _frame;       // 위 · 아래 바 위의 글자 · 탭 (1920×1080 가운데)
    private RectTransform _zoneFrame;   // 동료 · 표지 (1920×1080 가운데)
    private RectTransform _topPlate, _tabPlate;
    private Image _background;
    private RectTransform _zoneLayer;
    private RectTransform _fxBackLayer; // 동료 뒤의 움직이는 효과
    private BaseAmbiencePlayer _ambience;
    private CanvasGroup _zoneGroup;
    private GameObject _lockedRoot;
    private TextMeshProUGUI _zoneNumber, _zoneName, _zoneEnglish, _devText, _devValue;
    private TextMeshProUGUI _talinText, _partsText, _artifactText;
    private TextMeshProUGUI _lockedZone, _lockedCondition, _lockedDescription;
    private RectTransform _devTrack, _devFill;
    private RectTransform _tabSelected;
    private readonly List<TabRefs> _tabs = new();
    private int _zone;
    private bool _playingEvent;
    private BaseZoneData _ambienceZone;
    private bool _ambienceCovered;

    // 이번 로비 방문(런 횟수)에 뽑은 자리
    private int _assignedSeed = int.MinValue;
    private readonly Dictionary<string, List<Placement>> _placements = new();

    private class TabRefs
    {
        public RectTransform cell;
        public Image hover;
        public TextMeshProUGUI number, name, english, condition;
        public Image nameLock;
    }

    private class Placement
    {
        public CoopCharData character;
        public BaseZoneData.Spot spot;
        public Sprite pose;
        public float heightScale = 1f;
        public float hover;
        public string activity;
    }

    // 로비 메인 '02 거점' 카드가 부른다
    public void Open() => gameObject.SetActive(true);
    public void Close() => gameObject.SetActive(false);

    private void OnEnable()
    {
        Build();
        if (mainView != null) mainView.SetActive(false);
        MetaProgress.OnChanged += RefreshTop;
        RefreshTop();
        AssignCompanions();
        RefreshTabs();
        ShowZone(Mathf.Clamp(_lastZone, 0, Mathf.Max(0, zones.Count - 1)), instant: true);
    }

    private void OnDisable()
    {
        MetaProgress.OnChanged -= RefreshTop;
        if (_ambience != null) _ambience.Play(null, 0f, 22000f);
        if (mainView != null) mainView.SetActive(true);
    }

    // 시설 화면이 위를 덮으면 환경음을 줄인다
    private void Update()
    {
        bool covered = IsOn(archiveView) || IsOn(trainingView) || IsOn(armoryView) || IsOn(questView) || IsOn(shopView);
        if (covered != _ambienceCovered)
        {
            _ambienceCovered = covered;
            RefreshAmbience();
        }
    }

    private static bool IsOn(GameObject view) => view != null && view.activeInHierarchy;

    private void RefreshAmbience()
    {
        if (_ambience == null)
        {
            var go = new GameObject("BaseAmbience", typeof(AudioSource), typeof(AudioLowPassFilter), typeof(BaseAmbiencePlayer));
            go.transform.SetParent(transform.parent, false); // 화면이 꺼져도 페이드가 끝나게 화면 밖에
            _ambience = go.GetComponent<BaseAmbiencePlayer>();
        }
        var z = _ambienceZone;
        if (z == null || z.ambience == null || _ambienceCovered) _ambience.Play(null, 0f, 22000f);
        else _ambience.Play(z.ambience, z.ambienceVolume, z.ambienceCutoff);
    }

    // ── 조립 ──
    private void Build()
    {
        if (_built) return;
        _built = true;
        var root = (RectTransform)transform;

        _background = NewImage(root, "Background", null);
        Stretch(_background.rectTransform);
        _background.color = new Color(0.55f, 0.55f, 0.58f, 1f);
        _background.preserveAspect = false;
        _background.raycastTarget = true; // 뒤 로비를 누르지 못하게

        // 그리는 순서: 배경 → 구역(동료 · 표지) → 잠김 덮개 → 위 · 아래 바 판 → 바 위 글자 · 탭
        _zoneFrame = NewFrame(root, "ZoneFrame");
        var fxGo = new GameObject("FxBack", typeof(RectTransform));
        fxGo.layer = gameObject.layer;
        _fxBackLayer = (RectTransform)fxGo.transform;
        _fxBackLayer.SetParent(_zoneFrame, false);
        Stretch(_fxBackLayer);
        var layerGo = new GameObject("Zone", typeof(RectTransform), typeof(CanvasGroup));
        layerGo.layer = gameObject.layer;
        _zoneLayer = (RectTransform)layerGo.transform;
        _zoneLayer.SetParent(_zoneFrame, false);
        Stretch(_zoneLayer);
        _zoneGroup = layerGo.GetComponent<CanvasGroup>();

        BuildLocked(root);
        BuildPlates(root);
        _frame = NewFrame(root, "Frame");
        BuildTop();
        BuildTabs();
    }

    private RectTransform NewFrame(RectTransform root, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(root, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1920f, 1080f);
        return rt;
    }

    // 위 · 아래 바 판은 화면 폭 전체로 늘린다 (넓은 화면에서도 끝까지)
    private void BuildPlates(RectTransform root)
    {
        var top = NewImage(root, "TopBar", Sprite("TopBar"), sliced: true);
        _topPlate = top.rectTransform;
        _topPlate.anchorMin = new Vector2(0f, 1f); _topPlate.anchorMax = new Vector2(1f, 1f); _topPlate.pivot = new Vector2(0.5f, 1f);
        _topPlate.sizeDelta = new Vector2(0f, 110f); _topPlate.anchoredPosition = Vector2.zero;
        top.raycastTarget = true;

        var tab = NewImage(root, "TabBar", Sprite("TabBar"), sliced: true);
        _tabPlate = tab.rectTransform;
        _tabPlate.anchorMin = new Vector2(0f, 0f); _tabPlate.anchorMax = new Vector2(1f, 0f); _tabPlate.pivot = new Vector2(0.5f, 0f);
        _tabPlate.offsetMin = new Vector2(5f, 10f); _tabPlate.offsetMax = new Vector2(-1f, 118f);
        tab.raycastTarget = true;
    }

    private void BuildTop()
    {
        Pic(_frame, "Emblem", "Emblem_Star", R(39, 12, 73, 76));
        Label(_frame, "Title", R(134, 13, 110, 49), 42, true).text = "거점";
        Label(_frame, "TitleEn", R(136, 69, 100, 20), 17, false, spacing: 7).text = "BASE";
        Pic(_frame, "Slash", "TopBar_Slash", R(249, 11, 61, 82));
        _zoneNumber = Label(_frame, "ZoneNumber", R(340, 20, 48, 42), 29, true);
        _zoneName = Label(_frame, "ZoneName", R(396, 13, 360, 48), 38, true);
        _zoneEnglish = Label(_frame, "ZoneEn", R(341, 66, 470, 24), 17, false, spacing: 6);

        Pic(_frame, "Divider", "TopBar_Divider", R(1186, 18, 1, 62));
        _devText = Label(_frame, "Dev", R(1217, 17, 180, 26), 21, true);
        _devTrack = Pic(_frame, "DevTrack", "DevBar_Track", R(1386, 27, 220, 8), sliced: true).rectTransform;
        _devFill = Pic(_frame, "DevFill", "DevBar_Fill", R(1386, 27, 44, 8), sliced: true).rectTransform;
        _devValue = Label(_frame, "DevValue", R(1617, 17, 78, 25), 18, true);
        Pic(_frame, "ResourceRule", "TopBar_ResourceRule", R(1217, 46, 474, 1));
        _talinText = Currency("Talin", talinIcon, 1348f, 1382f);
        _partsText = Currency("Parts", partsIcon, 1483f, 1517f);
        _artifactText = Currency("Artifact", artifactIcon, 1573f, 1609f);

        var back = Pic(_frame, "Back", "Button_Back", R(1729, 19, 168, 63));
        back.raycastTarget = true;
        Pic(back.rectTransform, "Arrow", "Glyph_ArrowLeft", Local(1729, 19, 1768, 36, 27, 26));
        Label(back.rectTransform, "Label", Local(1729, 19, 1802, 31, 78, 38), 28, true).text = "로비";
        var normal = back.sprite; var hover = Sprite("Button_Back_Hover");
        Pointer(back.gameObject,
            enter: () => back.sprite = hover,
            exit: () => back.sprite = normal,
            click: Close);
    }

    private TextMeshProUGUI Currency(string name, Sprite icon, float iconX, float textX)
    {
        if (icon != null)
        {
            var image = NewImage(_frame, name + "Icon", icon);
            Place(image.rectTransform, R(iconX, 56, 28, 28));
            image.preserveAspect = true;
        }
        return Label(_frame, name, R(textX, 54, 90, 32), 26, true);
    }

    private void BuildTabs()
    {
        int n = Mathf.Max(1, zones.Count);
        float w = TabSpan / n;
        for (int i = 0; i < zones.Count; i++)
        {
            float x = TabX0 + i * w;
            var t = new TabRefs();
            t.hover = Pic(_frame, $"Tab{i}Hover", "Tab_Hover", R(x - 32, TabY, w + 35, TabH));
            t.hover.enabled = false;
            var hit = NewImage(_frame, $"Tab{i}", null);
            hit.color = Color.clear;
            hit.raycastTarget = true;
            t.cell = hit.rectTransform;
            Place(t.cell, R(x, TabY, w, TabH));
            if (i < zones.Count - 1) Pic(_frame, $"Tab{i}Divider", "Tab_Divider", R(x + w - 32, 977, 32, 78));
            t.number = Label(_frame, $"Tab{i}Number", R(x + 54, 983, 34, 27), 18, false);
            Pic(_frame, $"Tab{i}NumberLine", "Tab_NumberLine", R(x + 54, 1014, 22, 1));
            t.name = Label(_frame, $"Tab{i}Name", R(x + 116, 978, w - 120, 41), 34, true);
            t.english = Label(_frame, $"Tab{i}En", R(x + 116, 1027, w - 120, 23), 16, false, spacing: 4);
            t.condition = Label(_frame, $"Tab{i}Condition", R(x + 95, 1027, w - 98, 25), 17, false);
            t.condition.color = Dim;
            t.condition.enableAutoSizing = true; t.condition.fontSizeMin = 12f; t.condition.fontSizeMax = 17f;
            t.nameLock = Pic(_frame, $"Tab{i}Lock", "Glyph_Lock_Small", R(x + 116, 980, 22, 30));
            int index = i;
            Pointer(hit.gameObject,
                enter: () => t.hover.enabled = true,
                exit: () => t.hover.enabled = false,
                click: () => ShowZone(index));
            _tabs.Add(t);
        }
        _tabSelected = Pic(_frame, "TabSelected", "Tab_Selected", R(TabX0 + 19, 1059, w - 77, 5)).rectTransform;
    }

    private void BuildLocked(RectTransform root)
    {
        var go = new GameObject("Locked", typeof(RectTransform));
        go.layer = gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(root, false);
        Stretch(rt);
        _lockedRoot = go;
        var dim = NewImage(rt, "Dim", Sprite("LockedDim"));
        Stretch(dim.rectTransform);

        var panel = new GameObject("Panel", typeof(RectTransform));
        panel.layer = go.layer;
        var pr = (RectTransform)panel.transform;
        pr.SetParent(rt, false);
        pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(1920f, 1080f);
        Pic(pr, "Plate", "LockedPanel", R(648, 306, 624, 432));
        Pic(pr, "Lock", "Glyph_Lock_Large", R(939, 336, 43, 53));
        Label(pr, "Title", R(704, 403, 512, 62), 48, true, TextAlignmentOptions.Center).text = "구역 잠김";
        _lockedZone = Label(pr, "Zone", R(704, 472, 512, 32), 23, false, TextAlignmentOptions.Center, spacing: 2);
        _lockedCondition = Label(pr, "Condition", R(698, 548, 524, 43), 29, true, TextAlignmentOptions.Center);
        _lockedDescription = Label(pr, "Description", R(704, 602, 512, 33), 21, false, TextAlignmentOptions.Center);
        Pic(pr, "StatusBand", "LockedPanel_Status", R(785, 658, 350, 48));
        Label(pr, "Status", R(785, 665, 350, 33), 25, true, TextAlignmentOptions.Center).text = "미완료";
        go.SetActive(false);
    }

    // ── 상단 ──
    private void RefreshTop()
    {
        if (!_built) return;
        int level = MetaProgress.DevelopmentLevel;
        var (current, total) = MetaProgress.LevelProgress();
        float ratio = total > 0 ? Mathf.Clamp01((float)current / total) : 1f;
        _devText.text = $"BASE DEV Lv.{level}";
        _devValue.text = total > 0 ? $"{current} / {total}" : "MAX";
        _devFill.sizeDelta = new Vector2(Mathf.Max(8f, _devTrack.sizeDelta.x * ratio), _devFill.sizeDelta.y);
        _talinText.text = MetaProgress.Talin.ToString("N0");
        _partsText.text = MetaProgress.Parts.ToString("N0");
        _artifactText.text = MetaProgress.Artifacts.ToString("N0");
    }

    // ── 탭 ──
    private void RefreshTabs()
    {
        for (int i = 0; i < _tabs.Count; i++)
        {
            var z = zones[i];
            var t = _tabs[i];
            bool open = z.IsUnlocked;
            t.number.text = (i + 1).ToString("00");
            t.name.text = z.displayName;
            t.name.fontSize = open ? 34f : 32f;
            t.name.color = open ? Ink : Dim;
            t.english.text = z.englishName;
            t.english.gameObject.SetActive(open);
            t.condition.text = z.unlockLabel;
            t.condition.gameObject.SetActive(!open);
            t.nameLock.gameObject.SetActive(!open);
            if (!open)
            {
                var lr = t.nameLock.rectTransform;
                lr.anchoredPosition = new Vector2(t.name.rectTransform.anchoredPosition.x + t.name.GetPreferredValues(z.displayName).x + 10f, lr.anchoredPosition.y);
            }
        }
    }

    private void ShowZone(int index, bool instant = false)
    {
        if (zones.Count == 0) return;
        index = Mathf.Clamp(index, 0, zones.Count - 1);
        _zone = _lastZone = index;
        var z = zones[index];
        bool open = z.IsUnlocked;

        _zoneNumber.text = (index + 1).ToString("00");
        _zoneName.text = z.displayName;
        _zoneEnglish.text = z.englishName;
        _background.sprite = z.background;
        _background.color = z.background != null ? Color.white : new Color(0.08f, 0.09f, 0.1f, 1f);

        // 선택 막대는 미끄러져 옮겨 간다
        float w = TabSpan / Mathf.Max(1, zones.Count);
        var target = new Vector2(TabX0 + index * w + 19f, -1059f);
        _tabSelected.DOKill();
        if (instant) _tabSelected.anchoredPosition = target;
        else _tabSelected.DOAnchorPos(target, 0.22f).SetEase(Ease.OutCubic).SetLink(gameObject);

        _lockedRoot.SetActive(!open);
        if (!open)
        {
            _lockedZone.text = $"{index + 1:00} {z.displayName} / {z.englishName}";
            _lockedCondition.text = z.unlockLabel;
            _lockedDescription.text = z.unlockDescription;
        }

        BuildZoneContent(z, open);
        if (!instant)
        {
            _zoneGroup.alpha = 0f;
            _zoneGroup.DOKill();
            _zoneGroup.DOFade(1f, 0.25f).SetLink(gameObject);
        }
    }

    // ── 구역 내용: 시설 표지 · 확장 잠김 · 동료 ──
    private void BuildZoneContent(BaseZoneData z, bool open)
    {
        for (int i = _zoneLayer.childCount - 1; i >= 0; i--) Destroy(_zoneLayer.GetChild(i).gameObject);
        for (int i = _fxBackLayer.childCount - 1; i >= 0; i--) Destroy(_fxBackLayer.GetChild(i).gameObject);
        _ambienceZone = open ? z : null;
        RefreshAmbience();
        if (!open) return;

        BaseZoneFx.BuildBack(_fxBackLayer, z);

        // 동료 · 소품을 뒷줄(위쪽 발밑)부터 그려 앞줄이 앞에 오게
        var drawOrder = new List<(float y, System.Action build)>();
        if (_placements.TryGetValue(z.id, out var list))
            foreach (var p in list) drawOrder.Add((p.spot.feet.y, () => BuildCompanion(p, z)));
        foreach (var prop in z.props)
            if (prop.sprite != null) drawOrder.Add((prop.feet.y, () => BuildProp(prop, z)));
        foreach (var item in drawOrder.OrderBy(d => d.y)) item.build();
        BaseZoneFx.BuildFront(_zoneLayer, z);

        foreach (var e in z.expansions)
        {
            if (MetaProgress.DevelopmentLevel >= e.devLevel) continue;
            var plate = Pic(_zoneLayer, "Expansion", "Sign_Locked", e.rect, sliced: true);
            Label(plate.rectTransform, "Label", new Rect(20f, 10f, e.rect.width - 40f, 27f), 18, false, TextAlignmentOptions.Center)
                .text = $"BASE DEV Lv.{e.devLevel}에서 개방";
        }

        foreach (var f in z.facilities) BuildSign(f);
    }

    private void BuildSign(BaseZoneData.FacilityPoint f)
    {
        var plate = Pic(_zoneLayer, "Sign_" + f.link, "Sign", f.rect, sliced: true);
        plate.raycastTarget = true;
        var pr = plate.rectTransform;
        float w = f.rect.width;
        float top = (f.rect.height - 70f) * 0.5f; // 기본 판 높이 70 기준으로 글자를 가운데에
        Label(pr, "Name", new Rect(15f, top + 6f, w - 30f, 30f), 24, true, TextAlignmentOptions.Center).text = f.label;
        Label(pr, "En", new Rect(15f, top + 43f, w - 30f, 20f), 13, false, TextAlignmentOptions.Center, spacing: 4).text = f.english;
        var accent = Pic(pr, "Accent", "Sign_Hover_Accent", new Rect(10f, f.rect.height - 7f, 34f, 3f));
        accent.enabled = false;
        var normal = plate.sprite; var hover = Sprite("Sign_Hover");
        Pointer(plate.gameObject,
            enter: () => { plate.sprite = hover; accent.enabled = true; },
            exit: () => { plate.sprite = normal; accent.enabled = false; },
            click: () => OpenFacility(f.link));
    }

    private void OpenFacility(BaseZoneData.FacilityLink link)
    {
        switch (link)
        {
            case BaseZoneData.FacilityLink.Deploy:
                // 출격 준비는 텐트 시설(LobbyUIManager 전환)로 넘긴다. 거점을 닫아 로비 메인을 돌려놓은 뒤 연다
                Close();
                var fm = LobbyManager.Instance != null ? LobbyManager.Instance.FacilityManager : null;
                fm?.TryBeginInteraction(FacilityType.Tent);
                return;
            case BaseZoneData.FacilityLink.Quest: SetOn(questView); return;
            case BaseZoneData.FacilityLink.Shop: SetOn(shopView); return;
            case BaseZoneData.FacilityLink.Archive: SetOn(archiveView); return;
            case BaseZoneData.FacilityLink.Training: SetOn(trainingView); return;
            case BaseZoneData.FacilityLink.Armory: SetOn(armoryView); return;
        }
    }

    private static void SetOn(GameObject view)
    {
        if (view != null) view.SetActive(true);
    }

    // ── 동료 ──
    private static int CoopLevel(CoopCharData data)
    {
        int level = CooperationManager.Instance != null ? CooperationManager.Instance.GetCoopLevel(data.charID) : 0;
        if (PlayerRecord.TryGetCoop(data.charID, out int saved, out _)) level = Mathf.Max(level, saved);
        return level;
    }

    // 런 횟수가 바뀔 때만 다시 뽑는다 (같은 로비 방문 동안은 자리가 고정)
    private void AssignCompanions()
    {
        int seed = LobbyManager.Instance != null ? LobbyManager.Instance.CompletedRunCount : 0;
        if (seed == _assignedSeed && _placements.Count > 0) return;
        _assignedSeed = seed;
        _placements.Clear();

        var rng = new System.Random(seed * 7919 + 17);
        var free = new List<(BaseZoneData zone, BaseZoneData.Spot spot)>();
        foreach (var z in zones)
            if (z != null && z.IsUnlocked)
                foreach (var s in z.spots) free.Add((z, s));

        IEnumerable<CoopCharData> all = companions.Count > 0 ? companions
            : CooperationManager.Instance != null ? CooperationManager.Instance.AllCharData() : Enumerable.Empty<CoopCharData>();
        var met = all.Where(c => c != null && CoopLevel(c) > 0).OrderBy(c => c.charID).ToList();
        // 순서도 섞어서 같은 동료가 늘 먼저 좋은 자리를 가져가지 않게
        met = met.OrderBy(_ => rng.Next()).ToList();

        foreach (var c in met)
        {
            var wanted = c.baseSpots
                .Select(b => (b, slot: free.FindIndex(f => f.zone.id == b.zoneId && f.spot.id == b.spotId)))
                .Where(x => x.slot >= 0).ToList();
            int pick; BaseSpotPose pose = null;
            if (wanted.Count > 0)
            {
                var w = wanted[rng.Next(wanted.Count)];
                pick = w.slot; pose = w.b;
            }
            else if (free.Count > 0) pick = rng.Next(free.Count);
            else break; // 자리가 모자라면 남은 동료는 이번엔 나오지 않는다

            var (zone, spot) = free[pick];
            free.RemoveAt(pick);
            var sprite = pose != null && pose.pose != null ? pose.pose
                : c.campSprite != null ? c.campSprite
                : c.standingSprite != null ? c.standingSprite : c.charImage;
            if (sprite == null) continue;
            if (!_placements.TryGetValue(zone.id, out var list)) _placements[zone.id] = list = new List<Placement>();
            list.Add(new Placement
            {
                character = c, spot = spot, pose = sprite,
                heightScale = pose != null && pose.heightScale > 0f ? pose.heightScale : 1f,
                hover = pose != null ? pose.hover : 0f,
                activity = pose != null && !string.IsNullOrEmpty(pose.activity) ? pose.activity : c.campActivity,
            });
        }
    }

    // 바닥 그림자: 닿는 곳의 진한 타원 + 광원 반대쪽으로 늘어진 옅은 타원. 숨쉬기에 흔들리지 않게 몸 밖에 둔다
    private void BuildShadow(RectTransform holder, BaseZoneData z, float feetX, float width, float strength)
    {
        var sprite = Sprite("Shadow_Soft");
        if (sprite == null) return;
        float dx = feetX - z.lightX;
        float far = Mathf.Clamp01(Mathf.Abs(dx) / 700f);
        float side = dx >= 0f ? 1f : -1f;

        void Add(string name, Vector2 pos, Vector2 size, float alpha)
        {
            var image = NewImage(holder, name, sprite);
            var rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            image.color = new Color(0f, 0f, 0f, alpha * strength);
        }

        Add("ShadowCast", new Vector2(side * width * (0.16f + 0.34f * far), -2f),
            new Vector2(width * (1.15f + 0.6f * far), width * 0.3f), castShadowAlpha);
        Add("ShadowContact", new Vector2(0f, 0f), new Vector2(width * 1.0f, width * 0.26f), contactShadowAlpha);
    }

    private void BuildProp(BaseZoneData.Prop prop, BaseZoneData z)
    {
        float h = prop.height;
        float w = h * prop.sprite.rect.width / Mathf.Max(1f, prop.sprite.rect.height);
        var holder = new GameObject("Prop_" + prop.sprite.name, typeof(RectTransform));
        holder.layer = gameObject.layer;
        var hr = (RectTransform)holder.transform;
        hr.SetParent(_zoneLayer, false);
        hr.anchorMin = hr.anchorMax = new Vector2(0f, 1f);
        hr.pivot = new Vector2(0.5f, 0f);
        hr.anchoredPosition = new Vector2(prop.feet.x, -prop.feet.y);
        hr.sizeDelta = new Vector2(w, h);
        BuildShadow(hr, z, prop.feet.x, w, 1f);
        var body = NewImage(hr, "Body", prop.sprite);
        Stretch(body.rectTransform);
        body.preserveAspect = true;
        body.color = z.propTint;
        if (prop.mirror) body.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
    }

    private void BuildCompanion(Placement p, BaseZoneData z)
    {
        var c = p.character;
        float h = p.spot.height * p.heightScale;
        float aspect = p.pose.rect.width / Mathf.Max(1f, p.pose.rect.height);
        float w = h * aspect;

        var holder = new GameObject("Companion_" + c.charID, typeof(RectTransform));
        holder.layer = gameObject.layer;
        var hr = (RectTransform)holder.transform;
        hr.SetParent(_zoneLayer, false);
        hr.anchorMin = hr.anchorMax = new Vector2(0f, 1f);
        hr.pivot = new Vector2(0.5f, 0f);
        hr.anchoredPosition = new Vector2(p.spot.feet.x, -p.spot.feet.y);
        hr.sizeDelta = new Vector2(w, h);

        BuildShadow(hr, z, p.spot.feet.x, Mathf.Min(w, h * 0.6f), p.hover > 0f ? 0.55f : 1f);

        var body = NewImage(hr, "Body", p.pose);
        Stretch(body.rectTransform);
        body.rectTransform.pivot = new Vector2(0.5f, 0f);
        body.rectTransform.anchoredPosition += new Vector2(0f, p.hover);
        body.preserveAspect = true;
        body.raycastTarget = true;
        body.color = z.characterTint;
        if (p.spot.mirror) body.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
        // 가만히 숨쉬기 (위로만 아주 살짝)
        float phase = UnityEngine.Random.value * 2f;
        body.rectTransform.DOScaleY(1.012f, 1.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetDelay(phase).SetLink(holder);

        bool hasEvent = CooperationManager.Instance != null && CooperationManager.Instance.IsCoopLevelUP(c.charID);
        if (hasEvent)
        {
            var marker = Pic(hr, "Event", "Marker_Event", new Rect(0f, 0f, 39f, 39f));
            var mr = marker.rectTransform;
            mr.anchorMin = mr.anchorMax = new Vector2(0.5f, 1f);
            mr.pivot = new Vector2(0.5f, 0f);
            mr.anchoredPosition = new Vector2(0f, 14f);
            var glyph = Pic(mr, "Glyph", "Glyph_Event", new Rect(16f, 10.5f, 7f, 18f));
            mr.DOAnchorPosY(22f, 0.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(holder);
        }

        var tag = BuildNameTag(hr, c.charName, p.activity, w, h, p.spot.feet.x > 1920f * 0.62f);
        tag.root.SetActive(false);

        float lineUntil = 0f;
        Pointer(body.gameObject,
            enter: () => { body.color = Color.white; tag.root.SetActive(true); },
            exit: () =>
            {
                body.color = z.characterTint;
                if (Time.unscaledTime >= lineUntil) { tag.root.SetActive(false); tag.SetLine(p.activity); }
            },
            click: () =>
            {
                if (hasEvent) { PlayRelationshipEvent(c); return; }
                string line = c.campLines != null && c.campLines.Count > 0 ? c.campLines[UnityEngine.Random.Range(0, c.campLines.Count)] : null;
                if (string.IsNullOrEmpty(line)) return;
                tag.root.SetActive(true);
                tag.SetLine(line);
                lineUntil = Time.unscaledTime + lineHoldSeconds;
                DOVirtual.DelayedCall(lineHoldSeconds, () =>
                {
                    if (Time.unscaledTime + 0.01f < lineUntil) return;
                    tag.SetLine(p.activity);
                    if (body.color != Color.white) tag.root.SetActive(false);
                }).SetLink(holder);
            });
    }

    private class NameTag
    {
        public GameObject root;
        public RectTransform plate;
        public TextMeshProUGUI name, line;
        public float minWidth;
        public bool flip;

        public void SetLine(string text)
        {
            line.text = text ?? string.Empty;
            float textW = Mathf.Max(name.GetPreferredValues(name.text).x, line.GetPreferredValues(line.text).x);
            float w = Mathf.Max(minWidth, Mathf.Ceil(textW) + 42f);
            plate.sizeDelta = new Vector2(w, plate.sizeDelta.y);
        }
    }

    // 이름표: 지시선 끝점을 머리 옆에 두고, 판은 그 위 오른쪽(화면 오른쪽 끝이면 왼쪽)에. layout.json name_tag_anchor (80%)
    private NameTag BuildNameTag(RectTransform holder, string charName, string activity, float w, float h, bool flip)
    {
        var tag = new NameTag { minWidth = 218f, flip = flip };
        var go = new GameObject("NameTag", typeof(RectTransform));
        go.layer = holder.gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(holder, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = new Vector2((flip ? -1f : 1f) * w * 0.18f, h * 0.86f); // 끝점 = 머리 옆
        tag.root = go;

        var leader = Pic(rt, "Leader", "NameTag_Leader", new Rect(0f, 0f, 40f, 64f), sliced: true).rectTransform;
        leader.anchorMin = leader.anchorMax = Vector2.zero;
        leader.pivot = Vector2.zero;
        leader.anchoredPosition = Vector2.zero;
        if (flip) leader.localScale = new Vector3(-1f, 1f, 1f);

        var plate = Pic(rt, "Plate", "NameTag", new Rect(0f, 0f, 218f, 70f), sliced: true).rectTransform;
        plate.anchorMin = plate.anchorMax = Vector2.zero;
        plate.pivot = new Vector2(flip ? 1f : 0f, 0f);
        plate.anchoredPosition = new Vector2(flip ? -2f : 2f, 62f);
        tag.plate = plate;

        var divider = Pic(plate, "Divider", "NameTag_Divider", new Rect(21f, 35f, 174f, 1f)).rectTransform;
        divider.anchorMax = new Vector2(1f, 1f);
        divider.offsetMax = new Vector2(-23f, divider.offsetMax.y);
        tag.name = Label(plate, "Name", new Rect(21f, 8f, 178f, 23f), 20, true);
        tag.name.rectTransform.anchorMax = new Vector2(1f, 1f);
        tag.name.rectTransform.offsetMax = new Vector2(-19f, tag.name.rectTransform.offsetMax.y);
        tag.name.text = charName;
        tag.line = Label(plate, "Line", new Rect(21f, 44f, 178f, 19f), 14, false);
        tag.line.rectTransform.anchorMax = new Vector2(1f, 1f);
        tag.line.rectTransform.offsetMax = new Vector2(-19f, tag.line.rectTransform.offsetMax.y);
        tag.SetLine(activity);
        return tag;
    }

    // 호감도 이벤트 ('!') — 예전 모닥불(BonfireInteractionHandler)과 같은 규칙
    private void PlayRelationshipEvent(CoopCharData character)
    {
        if (_playingEvent) return;
        var cm = CooperationManager.Instance;
        if (cm == null || !cm.IsCoopLevelUP(character.charID)) return;
        var view = FindFirstObjectByType<DialogueView>(FindObjectsInactive.Include);
        if (view == null) { Debug.LogWarning("[Base] DialogueView is missing."); return; }

        int level = cm.GetCoopLevel(character.charID);
        var json = cm.GetCoopDialogue(character.charID, level + 1) ?? cm.GetCoopDialogue(character.charID, level) ?? defaultRelationshipDialogue;
        if (json == null) { Debug.LogWarning($"[Base] {character.charID} 호감도 대화가 없습니다."); return; }

        _playingEvent = true;
        view.Play(json, () =>
        {
            cm.SettlePoint(character.charID);
            _playingEvent = false;
            if (isActiveAndEnabled) ShowZone(_zone, instant: true);
        });
    }

    // ── 조립 도우미 (좌표는 1920×1080 왼쪽 위 원점) ──
    private static Rect R(float x, float y, float w, float h) => new(x, y, w, h);

    // 부모 판 기준 상대 좌표 (부모의 화면 위치를 빼서)
    private static Rect Local(float px, float py, float x, float y, float w, float h) => new(x - px, y - py, w, h);

    private static Sprite Sprite(string name) => Resources.Load<Sprite>("BaseCamp/" + name);

    private Image Pic(RectTransform parent, string name, string sprite, Rect r, bool sliced = false)
    {
        var image = NewImage(parent, name, Sprite(sprite), sliced);
        Place(image.rectTransform, r);
        return image;
    }

    private Image NewImage(RectTransform parent, string name, Sprite sprite, bool sliced = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = gameObject.layer;
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.raycastTarget = false;
        return image;
    }

    private static void Place(RectTransform rt, Rect r)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(r.x, -r.y);
        rt.sizeDelta = new Vector2(r.width, r.height);
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private TextMeshProUGUI Label(RectTransform parent, string name, Rect r, float size, bool bold,
        TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft, float spacing = 0f)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        Place(rt, r);
        var t = go.AddComponent<TextMeshProUGUI>();
        var font = bold ? fontBold : fontRegular;
        if (font == null) font = fontBold != null ? fontBold : TMP_Settings.defaultFontAsset;
        t.font = font;
        t.fontSize = size;
        t.color = Ink;
        t.alignment = align;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        t.characterSpacing = spacing / size * 100f;
        t.raycastTarget = false;
        return t;
    }

    private static void Pointer(GameObject go, Action enter, Action exit, Action click)
    {
        var p = go.AddComponent<BaseCampPointer>();
        p.Enter = enter; p.Exit = exit; p.Click = click;
        go.AddComponent<UIButtonSfx>(); // 호버 · 클릭 효과음 (로비 버튼과 같은 소리)
    }
}

// 거점 화면 조각의 마우스 올림 · 누름
public class BaseCampPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Action Enter, Exit, Click;
    public void OnPointerEnter(PointerEventData e) => Enter?.Invoke();
    public void OnPointerExit(PointerEventData e) => Exit?.Invoke();
    public void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left) Click?.Invoke();
    }
}
