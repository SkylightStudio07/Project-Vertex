using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 런 종료 화면. 두 경우에 뜬다.
//   - 런 클리어: 마지막 막(ActData.isFinalAct) 보스를 격파하고 보상을 닫으면 (GameManager.CompleteAct → Open)
//   - 패배: 플레이어가 쓰러지면 "작전 실패" 배너(BattleOperationFx)가 끝난 뒤 (OpenDefeat)
//   - 훈련장: 이기든 지든 (HandleVictory / HandleDefeat — 훈련 중이면 보상 화면 대신 여기)
// 버튼은 "기지로 귀환" — 씬 전환 막(귀환 문구)을 거쳐 로비 씬으로 돌아간다. 로비 씬이 빌드에 없으면 예전처럼 현재 씬을 다시 연다.
// 판: 종이 "작전 보고서" (조각 Resources/BattleFx/Result, 원본 ArtDirection/BattleFxMockup/Extracted/Result).
//   기록 칸 4개 — 실전: 도달 층 / 격퇴 / 획득 카드 / 진행 턴 (RunStats), 훈련: 진행 턴 / 격퇴 / 남은 HP / 덱
//   클리어·훈련 완료는 청록, 실패·훈련 종료는 회색 강조선.
// 평소엔 켜 둔 채 투명·입력 차단 없음으로 두고, 열 때만 페이드 인한다(비활성 오브젝트는 Awake가 안 돌아 Instance가 비기 때문).
[RequireComponent(typeof(CanvasGroup))]
public class RunClearView : MonoBehaviour
{
    public static RunClearView Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextMeshProUGUI englishText;
    [SerializeField] private List<TextMeshProUGUI> statLabels = new();
    [SerializeField] private List<TextMeshProUGUI> statValues = new();
    [Header("상태 강조선 (클리어 = 청록 / 실패 = 회색)")]
    [SerializeField] private Image titleAccent;
    [SerializeField] private List<Image> statAccents = new();
    [SerializeField] private Image cornerAccent;
    [SerializeField] private Sprite accentClear;
    [SerializeField] private Sprite accentDefeat;
    [SerializeField] private Sprite statAccentClear;
    [SerializeField] private Sprite statAccentDefeat;
    [SerializeField] private Sprite cornerClear;
    [SerializeField] private Sprite cornerDefeat;
    [SerializeField] private Button restartButton;
    [SerializeField, Min(0f)] private float fadeDuration = 0.8f;
    [Tooltip("귀환할 로비 씬 이름 (Build Settings에 있어야 한다)")]
    [SerializeField] private string lobbySceneName = "Lobby";
    [Tooltip("패배 후 이 화면이 뜨기까지 기다리는 시간 (작전 실패 배너가 끝나는 시점)")]
    [SerializeField, Min(0f)] private float defeatDelay = 2.2f;
    [Tooltip("v3 보고서 판(Resources/BattleFx/V3/Result)을 코드로 만들어 씬에 깔린 예전 판 대신 쓴다")]
    [SerializeField] private bool useV3 = true;

    private CanvasGroup _group;
    private BattleManager _battle;
    private bool _open;
    private ReportV3 _v3;

    private void Awake()
    {
        Instance = this;
        _group = GetComponent<CanvasGroup>();
        if (useV3) _v3 = BuildV3();
        SetVisible(false);
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(ReturnToLobby);
        }
    }

    private void Start()
    {
        _battle = BattleManager.Instance;
        if (_battle != null)
        {
            _battle.OnBattleDefeat += HandleDefeat;
            _battle.OnBattleVictory += HandleVictory;
        }
    }

    private void OnDestroy()
    {
        if (_battle != null)
        {
            _battle.OnBattleDefeat -= HandleDefeat;
            _battle.OnBattleVictory -= HandleVictory;
        }
        if (Instance == this) Instance = null;
    }

    public void Open(ActData clearedAct)
    {
        QuestManager.Instance.SettleRun(); // 런 클리어 → 의뢰 정산 (보상은 로비에서 수령)
        int talin = SettleTalin();
        RecordRun(cleared: true, talin);
        Show("런 클리어",
             (clearedAct != null && !string.IsNullOrWhiteSpace(clearedAct.actName)
                 ? $"{clearedAct.actNumber}막 · {clearedAct.actName} 돌파"
                 : "모든 작전을 완료했습니다") + TalinSuffix(talin), success: true);
    }

    // 실전 승리는 보상 화면(RewardsView)이 받는다. 훈련장만 여기서 결과를 띄운다.
    private void HandleVictory(BattleReward reward)
    {
        if (!TrainingSession.IsActive) return;
        DOVirtual.DelayedCall(defeatDelay, () => Show("훈련 완료", $"{TrainingTargetName()} 격파", success: true), ignoreTimeScale: true)
                 .SetLink(gameObject);
    }

    private static string TrainingTargetName()
    {
        var encounter = TrainingSession.Encounter;
        if (encounter == null) return "훈련 상대";
        if (!string.IsNullOrWhiteSpace(encounter.bossTitle)) return encounter.bossTitle;
        var first = encounter.enemies != null && encounter.enemies.Count > 0 ? encounter.enemies[0] : null;
        return first != null ? first.enemyName : encounter.name;
    }

    private void HandleDefeat()
    {
        if (TrainingSession.IsActive)
        {
            DOVirtual.DelayedCall(defeatDelay, () => Show("훈련 종료", $"{TrainingTargetName()}에게 패배했습니다", success: false), ignoreTimeScale: true)
                     .SetLink(gameObject);
            return;
        }
        QuestManager.Instance.SettleRun(); // 패배도 런 종료 — 이미 정산했으면 무시된다
        int talin = SettleTalin();
        int floor = RunData.Instance != null ? RunData.Instance.currentFloor + 1 : 0;
        int chapter = GameManager.Instance != null ? GameManager.Instance.Chapter : 1;
        RecordRun(cleared: false, talin);
        DOVirtual.DelayedCall(defeatDelay, () => Show("작전 실패", $"{chapter}막 {floor}층에서 쓰러졌습니다" + TalinSuffix(talin), success: false), ignoreTimeScale: true)
                 .SetLink(gameObject);
    }

    // 남은 크레딧의 1/10을 탈린으로 (런마다 한 번, 패배 포함)
    private static int SettleTalin()
        => MetaProgress.SettleRun(GameManager.Instance != null ? GameManager.Instance.PlayerGold : 0);

    // 아카이브 > 플레이 기록에 이번 런을 남긴다 (훈련장은 이 경로로 오지 않는다)
    private static bool s_recorded; // 한 런에 한 번 (MetaProgress.BeginRun과 같은 때 풀린다)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRecorded() => s_recorded = false;

    public static void BeginRun() => s_recorded = false;

    private static void RecordRun(bool cleared, int talin)
    {
        if (s_recorded) return;
        s_recorded = true;
        PlayerRecord.AddRun(new PlayerRecord.RunEntry
        {
            endedTicks = System.DateTime.Now.Ticks,
            cleared = cleared,
            act = GameManager.Instance != null ? GameManager.Instance.Chapter : 1,
            floor = RunData.Instance != null ? RunData.Instance.currentFloor + 1 : 0,
            kills = RunStats.EnemiesDefeated,
            turns = RunStats.Turns,
            cards = RunStats.CardsObtained,
            talin = talin,
        });
    }

    private static string TalinSuffix(int talin) => talin > 0 ? $"  ·  <color=#0DB8F2>탈린 +{talin}</color>" : "";

    private void Show(string title, string subtitle, bool success)
    {
        if (_open) return;
        _open = true;
        if (titleText != null) titleText.text = title;
        if (subtitleText != null) subtitleText.text = subtitle;
        if (englishText != null)
            englishText.text = _v3 != null
                ? (TrainingSession.IsActive ? "TRAINING REPORT" : "OPERATION REPORT")
                : (TrainingSession.IsActive ? "RESULT  /  TRAINING REPORT" : "RESULT  /  OPERATION REPORT");
        FillStats();
        ApplyAccent(success);
        _v3?.Apply(success);

        SetVisible(true);
        _group.alpha = 0f;
        _group.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(gameObject);
        _v3?.PlayEntrance(fadeDuration);
    }

    private void FillStats()
    {
        (string label, string value)[] stats;
        if (TrainingSession.IsActive)
        {
            int hp = GameManager.Instance != null ? GameManager.Instance.PlayerHP : 0;
            int deck = DeckManager.Instance != null ? DeckManager.Instance.PlayerDeck.Count : 0;
            stats = new[] { ("진행 턴", Two(RunStats.Turns)), ("격퇴", Two(RunStats.EnemiesDefeated)), ("남은 HP", hp.ToString()), ("덱", Two(deck)) };
        }
        else
        {
            int floor = RunData.Instance != null ? RunData.Instance.currentFloor + 1 : 0;
            stats = new[] { ("도달 층", Two(floor)), ("격퇴", Two(RunStats.EnemiesDefeated)), ("획득 카드", Two(RunStats.CardsObtained)), ("진행 턴", Two(RunStats.Turns)) };
        }
        for (int i = 0; i < stats.Length; i++)
        {
            if (i < statLabels.Count && statLabels[i] != null) statLabels[i].text = stats[i].label;
            if (i < statValues.Count && statValues[i] != null) statValues[i].text = stats[i].value;
        }
    }

    private static string Two(int n) => n.ToString("00");

    private void ApplyAccent(bool success)
    {
        if (titleAccent != null) titleAccent.sprite = success ? accentClear : accentDefeat;
        foreach (var a in statAccents) if (a != null) a.sprite = success ? statAccentClear : statAccentDefeat;
        if (cornerAccent != null) cornerAccent.sprite = success ? cornerClear : cornerDefeat;
    }

    private void SetVisible(bool visible)
    {
        _group.alpha = visible ? _group.alpha : 0f;
        _group.blocksRaycasts = visible;
        _group.interactable = visible;
    }

    // ───────── v3 보고서 판 ─────────
    // 흰 종이 판(클리어) / 먹색 판(실패) + 왼쪽 사진 띠(이번 전투 배경을 띠 모양으로 잘라 넣음) + 나침반 문양 + 기록 4칸.
    // 조각 Resources/BattleFx/V3/Result (원본 ArtDirection/RasterRedo_v2/Direction_Split/Result, 좌표는 그 layout.json의 1920×1080 좌상단 값)
    private class ReportV3
    {
        public RectTransform Root;
        public Image Panel, Emblem, StripOverlay, StripPhoto, TitleAccent, Button;
        public readonly List<Image> StatAccents = new();
        public Button ReturnButton;
        public readonly List<TextMeshProUGUI> InkTexts = new(); // 판 위 글자 — 클리어는 먹색, 실패는 밝은 회색
        public TextMeshProUGUI Caption, StripCaption;

        private static Sprite S(string name) => Resources.Load<Sprite>("BattleFx/V3/Result/" + name);

        public void Apply(bool success)
        {
            string d = success ? "" : "_Defeat";
            Panel.sprite = S("Panel" + d);
            Emblem.sprite = S("Emblem_Compass" + d);
            StripOverlay.sprite = S("Strip_Overlay" + d);
            TitleAccent.sprite = S("Title_Accent" + d);
            foreach (var a in StatAccents) a.sprite = S("Stat_Accent" + d);
            Button.sprite = S("Button_Return_Normal" + d);
            ReturnButton.spriteState = new SpriteState
            {
                highlightedSprite = S("Button_Return_Hover" + d),
                selectedSprite = S("Button_Return_Hover" + d),
                pressedSprite = S("Button_Return_Pressed" + d),
                disabledSprite = S("Button_Return_Pressed" + d),
            };
            var ink = success ? Hex("#16181B") : Hex("#E3E6E9");
            foreach (var t in InkTexts) t.color = ink;
            Caption.text = success ? "EXPEDITION COMPLETE.\nANOTHER LAYER AWAITS." : "EXPEDITION FAILED.\nANOTHER LAYER AWAITS.";

            int floor = RunData.Instance != null ? RunData.Instance.currentFloor + 1 : 0;
            StripCaption.text = $"FIELD RECORD\n/ {floor:00}";
            var stage = BattleStageView.Active;
            StripPhoto.sprite = stage != null ? stage.background : null;
            StripPhoto.enabled = StripPhoto.sprite != null;
        }

        public void PlayEntrance(float duration)
        {
            Root.DOKill();
            Root.anchoredPosition = new Vector2(60f, 0f);
            Root.DOAnchorPosX(0f, duration * 0.7f).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(Root.gameObject);
            Emblem.DOKill();
            Emblem.color = new Color(1f, 1f, 1f, 0f);
            Emblem.DOFade(1f, duration).SetDelay(duration * 0.4f).SetUpdate(true).SetLink(Emblem.gameObject);
        }
    }

    private ReportV3 BuildV3()
    {
        var font = titleText != null ? titleText.font : TMP_Settings.defaultFontAsset;
        // 씬에 깔린 예전 판은 끈다 (배경 어둡게 하는 Dim만 남긴다)
        foreach (Transform child in transform)
            if (child.name != "Dim") child.gameObject.SetActive(false);

        var r = new ReportV3();
        var go = new GameObject("ReportV3", typeof(RectTransform));
        go.layer = gameObject.layer;
        r.Root = (RectTransform)go.transform;
        r.Root.SetParent(transform, false);
        r.Root.anchorMin = Vector2.zero; r.Root.anchorMax = Vector2.one;
        r.Root.offsetMin = r.Root.offsetMax = Vector2.zero;

        var shadow = V3Pic(r.Root, "Panel_Shadow", "Panel_Shadow", 527, 185, 1342, 793);
        shadow.color = new Color(1f, 1f, 1f, 0.72f);
        r.Panel = V3Pic(r.Root, "Panel", "Panel", 520, 174, 1342, 793);

        // 사진 띠: 띠 모양 마스크 안에 이번 전투 배경을 화면 전체 크기로 깔아, 판 너머로 전장이 비쳐 보이게 한다
        var mask = V3Pic(r.Root, "Strip_Mask", "Strip_Mask", 520, 174, 269, 793);
        mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        r.StripPhoto = NewImage(mask.rectTransform, "Photo", null);
        var prt = r.StripPhoto.rectTransform;
        prt.anchorMin = prt.anchorMax = new Vector2(0f, 1f);
        prt.pivot = new Vector2(0f, 1f);
        prt.anchoredPosition = new Vector2(-520f, 174f);
        prt.sizeDelta = new Vector2(1920f, 1080f);
        r.StripOverlay = V3Pic(r.Root, "Strip_Overlay", "Strip_Overlay", 520, 174, 269, 793);
        r.Emblem = V3Pic(r.Root, "Emblem_Compass", "Emblem_Compass", 520, 174, 1342, 793);
        V3Pic(r.Root, "Strip_Frame", "Strip_Frame", 520, 174, 269, 793);

        r.TitleAccent = V3Pic(r.Root, "Title_Accent", "Title_Accent", 804, 441, 53, 8);
        float[] ruleX = { 804, 1053, 1303, 1571 };
        float[] accentX = { 829, 1078, 1329, 1596 };
        float[] labelX = { 829, 1078, 1329, 1596 };
        float[] valueX = { 824, 1074, 1324, 1592 };
        statLabels.Clear(); statValues.Clear(); statAccents.Clear();
        for (int i = 0; i < 4; i++)
        {
            V3Pic(r.Root, "Stat_Rule_V" + i, "Stat_Rule_V", ruleX[i], 636, 1, 180);
            V3Pic(r.Root, "Stat_Rule_H" + i, "Stat_Rule_H", ruleX[i], 813, 170, 5);
            r.StatAccents.Add(V3Pic(r.Root, "Stat_Accent" + i, "Stat_Accent", accentX[i], 678, 21, 3));
            var label = V3Text(r.Root, font, "StatLabel" + i, labelX[i], 638, 178, 34, 23f, FontStyles.Bold);
            var value = V3Text(r.Root, font, "StatValue" + i, valueX[i], 692, 220, 110, 103f, FontStyles.Bold);
            statLabels.Add(label); statValues.Add(value);
            r.InkTexts.Add(label); r.InkTexts.Add(value);
        }

        var serial = V3Text(r.Root, font, "Serial", 807, 217, 80, 28, 18.4f, FontStyles.Bold);
        serial.text = "01";
        englishText = V3Text(r.Root, font, "Eyebrow", 807, 262, 459, 29, 14.9f, FontStyles.Normal, 13f);
        titleText = V3Text(r.Root, font, "Title", 796, 302, 760, 130, 118f, FontStyles.Bold);
        titleText.enableAutoSizing = true; titleText.fontSizeMin = 72f; titleText.fontSizeMax = 118f;
        subtitleText = V3Text(r.Root, font, "Subtitle", 804, 469, 900, 48, 35.6f, FontStyles.Bold);
        subtitleText.enableAutoSizing = true; subtitleText.fontSizeMin = 22f; subtitleText.fontSizeMax = 35.6f;
        r.Caption = V3Text(r.Root, font, "Caption", 804, 537, 448, 40, 11.5f, FontStyles.Normal);
        var emblemCaption = V3Text(r.Root, font, "EmblemCaption", 1751, 482, 103, 53, 10.3f, FontStyles.Normal);
        emblemCaption.text = "VERTEX\nEXPEDITION\nRECORD";
        r.InkTexts.AddRange(new[] { serial, englishText, titleText, subtitleText, r.Caption, emblemCaption });
        r.StripCaption = V3Text(r.Root, font, "StripCaption", 556, 864, 163, 36, 11.5f, FontStyles.Bold);
        r.StripCaption.color = Hex("#E5E8EB");

        r.Button = V3Pic(r.Root, "ReturnButton", "Button_Return_Normal", 1369, 876, 477, 92);
        r.Button.raycastTarget = true;
        r.ReturnButton = r.Button.gameObject.AddComponent<Button>();
        r.ReturnButton.targetGraphic = r.Button;
        r.ReturnButton.transition = Selectable.Transition.SpriteSwap;
        // 붙는 순간 기본 색조 전환(비활성 = 반투명 회색)이 이미 칠해져 있다 — 원래 색으로 되돌린다
        r.Button.canvasRenderer.SetColor(Color.white);
        var btnLabel = V3Text(r.Root, font, "Label", 1453, 896, 299, 45, 33.3f, FontStyles.Bold);
        btnLabel.text = "기지로 귀환";
        btnLabel.color = Hex("#E5E8EB");
        btnLabel.rectTransform.SetParent(r.Button.rectTransform, true);
        var arrow = V3Text(r.Root, font, "Arrow", 1732, 896, 50, 45, 33.3f, FontStyles.Bold);
        arrow.text = "→";
        arrow.color = Hex("#E5E8EB");
        arrow.rectTransform.SetParent(r.Button.rectTransform, true);
        restartButton = r.ReturnButton;

        // 예전 강조선 교체 로직(ApplyAccent)은 쓰지 않는다 — 실패 / 클리어 조각은 ReportV3.Apply가 바꾼다
        titleAccent = null; cornerAccent = null;
        return r;
    }

    // 조각 하나를 1920×1080 좌상단 좌표에 놓는다 (화면 위 · 가운데 기준 — 넓은 화면에서도 가운데)
    private static Image V3Pic(RectTransform parent, string name, string sprite, float x, float y, float w, float h)
    {
        var img = NewImage(parent, name, Resources.Load<Sprite>("BattleFx/V3/Result/" + sprite));
        Place(img.rectTransform, x, y, w, h);
        return img;
    }

    private static Image NewImage(RectTransform parent, string name, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI V3Text(RectTransform parent, TMP_FontAsset font, string name, float x, float y, float w, float h,
        float size, FontStyles style, float spacing = 0f)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.characterSpacing = spacing;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        t.raycastTarget = false;
        Place(t.rectTransform, x, y, w, h);
        return t;
    }

    private static void Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x - 960f, -y);
        rt.sizeDelta = new Vector2(w, Mathf.Max(h, 1f));
    }

    private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.white;

    private void ReturnToLobby()
    {
        if (restartButton != null) restartButton.interactable = false;
        TrainingSession.End(); // 훈련장 출격 정보는 귀환과 함께 비운다
        if (!string.IsNullOrWhiteSpace(lobbySceneName) && Application.CanStreamedLevelBeLoaded(lobbySceneName))
        {
            SceneTransition.Load(lobbySceneName, "귀환", "기지로 복귀합니다", "RETURN  //  BASE");
            return;
        }

        // 로비가 빌드에 없으면 현재 씬을 다시 열어 새 런을 시작한다
        var scene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(scene.buildIndex);
#endif
    }
}
