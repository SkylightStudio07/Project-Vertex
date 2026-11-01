using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전투 화면의 명일방주식 띠 연출. 입력은 막지 않는다 (전부 raycastTarget off).
//   - 전투 개시: 먹색 사선 띠 + "작전 개시 / OPERATION START" + 전투 종류·적 수 태그 (보스전은 BossBattleDirector가 따로 연출)
//   - 카드 컷인: 파워 카드·유니크 카드를 쓰면 왼쪽에서 카드 아트 띠가 미끄러져 들어온다
//     (오른쪽 문양은 카테고리 파워 · 스킬 · 공격, 유니크는 이중 테두리)
//   - 적 처치: 왼쪽 위에 "TARGET DOWN 이름 무력화" 기록이 쌓였다 사라진다 (보스는 BOSS 칩)
//   - 승리/패배: 화면 가운데 종이 판 "작전 종료" / 먹색 판 "작전 실패" (승리는 짧은 슬로모션, 패배는 화면이 어두워짐)
// 그림 조각: Resources/BattleFx/<연출>/ (원본 ArtDirection/BattleFxMockup/Extracted — 2배 해상도, PPU 200).
//   승리 · 패배 · 컷인은 v3 조각 Resources/BattleFx/V3/ (원본 ArtDirection/RasterRedo_v2/Direction_Split).
// 좌표는 그 layout.json의 1920×1080 좌상단 기준 값을 그대로 쓴다. UI 요소는 Awake에서 코드로 만든다.
public class BattleOperationFx : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;               // 비우면 TMP 기본 폰트
    [SerializeField] private int sortingOrder = 90;             // 컷인·처치 기록: 덱 목록(99) 아래, 손패 위
    [SerializeField] private int bannerSortingOrder = 210;      // 작전 개시·종료·실패 배너: 적 툴팁(200) 위
    [SerializeField] private Color accent = new(0.05f, 0.72f, 0.95f, 1f);
    [SerializeField] private Color ink = new(0.086f, 0.094f, 0.106f, 1f);
    [SerializeField] private Color paper = new(0.945f, 0.949f, 0.957f, 1f);
    [SerializeField] private Color muted = new(0.62f, 0.64f, 0.67f, 1f);

    [Header("카드 컷인")]
    [SerializeField] private bool cutInPowerCards = true;
    [SerializeField] private CardData.CardRarity cutInMinRarity = CardData.CardRarity.Unique; // 파워 외에는 유니크부터

    [Header("승리 슬로모션")]
    [SerializeField, Range(0.05f, 1f)] private float victorySlowScale = 0.3f;
    [SerializeField] private float victorySlowSeconds = 0.35f; // 실제 시간

    [Header("패배 화면 어둡게")]
    [SerializeField, Range(0f, 1f)] private float defeatDimAlpha = 0.6f;

    private const float RowStep = 58f; // 처치 기록 줄 간격 (layout.json row_repeat_step_screen_px)

    private BattleManager _battle;
    private RectTransform _root;
    private Banner _start;
    private ResultBanner _victory, _defeat;
    private CutIn _cutIn;
    private int _cutInCount; // 이번 전투에서 띄운 컷인 수 — 컷인 번호(01, 02 …)
    private RectTransform _feed;

    private void Awake()
    {
        _root = (RectTransform)transform;
        var canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        if (font == null) font = TMP_Settings.defaultFontAsset;

        _start = BuildStart();
        _victory = BuildResultBanner("Victory", victory: true);
        _defeat = BuildResultBanner("Defeat", victory: false);
        _cutIn = BuildCutIn();
        _feed = Group("KillFeed", _root);
    }

    private void Start()
    {
        _battle = BattleManager.Instance;
        if (_battle == null) return;
        _battle.OnBattleStarted += HandleStarted;
        _battle.OnCardPlayed += HandleCardPlayed;
        _battle.OnBattleVictory += HandleVictory;
        _battle.OnBattleDefeat += HandleDefeat;
        BattleManager.EnemyDefeated += HandleEnemyDefeated;
    }

    private void OnDestroy()
    {
        BattleManager.EnemyDefeated -= HandleEnemyDefeated;
        if (_battle == null) return;
        _battle.OnBattleStarted -= HandleStarted;
        _battle.OnCardPlayed -= HandleCardPlayed;
        _battle.OnBattleVictory -= HandleVictory;
        _battle.OnBattleDefeat -= HandleDefeat;
    }

    // ───────── 이벤트 ─────────

    private void HandleStarted()
    {
        _victory.Hide(); _defeat.Hide();
        _cutInCount = 0;
        for (int i = _feed.childCount - 1; i >= 0; i--) Destroy(_feed.GetChild(i).gameObject);

        var type = _battle.CurrentBattleType;
        if (type == BattleType.Boss && BossBattleDirector.Instance != null) return;

        int count = 0;
        foreach (var e in _battle.Enemies) if (e != null && !e.IsDead) count++;
        bool elite = type == BattleType.Elite;
        _start.SetElite(elite);
        _start.SetSerial(RunData.Instance != null ? RunData.Instance.currentFloor + 1 : 1);
        _start.Play("OPERATION START", "작전 개시", $"{(elite ? "정예 조우" : "일반 조우")} · 적 {count:00}", 0.15f, 0.75f);
    }

    private void HandleCardPlayed(CardData card)
    {
        if (card == null || card.Type == CardData.CardType.Status) return;
        bool power = cutInPowerCards && card.Type == CardData.CardType.Power;
        if (!power && card.Rarity < cutInMinRarity) return;
        // 오른쪽 문양 · 라벨은 카테고리, 이중 테두리는 등급 (따로 본다)
        var (label, ko, emblem) = card.Type switch
        {
            CardData.CardType.Power => ("POWER", "파워", "Power"),
            CardData.CardType.Attack => ("ATTACK", "공격", "Attack"),
            _ => ("SKILL", "스킬", "Skill"),
        };
        _cutInCount++;
        _cutIn.Play(card.CardImage, Resources.Load<Sprite>("BattleFx/V3/CutIn/Emblem_" + emblem),
            card.Rarity == CardData.CardRarity.Unique, _cutInCount, label, ko, card.CardName);
    }

    private void HandleEnemyDefeated(EnemyInstance enemy)
    {
        if (enemy == null || enemy.Data == null) return;
        AddFeed(enemy.Data.enemyName, enemy.Data.rank == EnemyEncounterType.Boss);
    }

    private void HandleVictory(BattleReward _)
    {
        _cutIn.Hide();
        _victory.Play("MISSION ACCOMPLISHED", "작전 종료", "ALL HOSTILES NEUTRALIZED", 0.1f, 0.9f);
        if (victorySlowScale < 0.99f)
        {
            Time.timeScale = victorySlowScale;
            DOVirtual.DelayedCall(victorySlowSeconds, () => Time.timeScale = 1f, ignoreTimeScale: true).SetLink(gameObject);
        }
    }

    private void HandleDefeat()
    {
        _cutIn.Hide();
        _defeat.Play("MISSION FAILED", "작전 실패", "OPERATION TERMINATED", 0.2f, 1.6f);
    }

    // ───────── 작전 개시 · 종료 · 실패 띠 ─────────

    private class Banner
    {
        public RectTransform Root;
        public CanvasGroup Group;
        public RectTransform Band;
        public readonly List<RectTransform> Lines = new();   // 왼쪽부터 그어지는 헤어라인
        public readonly List<Graphic> Decor = new();          // 사선·마름모 등 — 띠가 펼쳐진 뒤 나타남
        public RectTransform Tag;
        public TextMeshProUGUI Eng, Main, Sub;
        public Image Flash, Dim;
        public GameObject EliteMark;
        public TextMeshProUGUI EliteLabel;
        public float DimAlpha;
        public TextMeshProUGUI Serial, SerialCaption, GhostNumber; // 작전 개시 일련번호 (층)
        public CanvasGroup Ghost;                                   // 띠 오른쪽 큰 흐린 번호 — 띠가 펼쳐진 뒤 떠오른다
        public Vector2 GhostHome;
        private Sequence _seq;

        public void SetElite(bool elite)
        {
            if (EliteMark == null) return;
            EliteMark.SetActive(elite);
            EliteLabel.gameObject.SetActive(elite);
        }

        public void SetSerial(int n)
        {
            if (Serial == null) return;
            Serial.text = n.ToString("00");
            SerialCaption.text = $"OPERATION\nSERIAL\n// {n:00}";
            GhostNumber.text = n.ToString("00");
        }

        public void Hide()
        {
            _seq?.Kill();
            Root.gameObject.SetActive(false);
        }

        public void Play(string eng, string main, string sub, float delay, float hold)
        {
            _seq?.Kill();
            Root.gameObject.SetActive(true);
            Eng.text = eng; Main.text = main; Sub.text = sub;
            Group.alpha = 1f;
            Band.localScale = new Vector3(0f, 1f, 1f);
            foreach (var l in Lines) l.localScale = new Vector3(0f, 1f, 1f);
            foreach (var d in Decor) { var c = d.color; c.a = 0f; d.color = c; }
            Main.alpha = 0f; Eng.alpha = 0f;
            Main.characterSpacing = 60f;
            var mainPos = Main.rectTransform.anchoredPosition;
            Main.rectTransform.anchoredPosition = new Vector2(mainPos.x - 40f, mainPos.y);
            Tag.localScale = new Vector3(1f, 0f, 1f);
            if (Flash != null) Flash.color = new Color(1f, 1f, 1f, 0f);
            if (Dim != null) Dim.color = new Color(Dim.color.r, Dim.color.g, Dim.color.b, 0f);

            _seq = DOTween.Sequence().SetLink(Root.gameObject).SetUpdate(true).AppendInterval(delay);
            if (Dim != null) _seq.Insert(delay, Dim.DOFade(DimAlpha, 0.5f));
            for (int i = 0; i < Lines.Count; i++)
                _seq.Insert(delay + i * 0.03f, Lines[i].DOScaleX(1f, 0.24f).SetEase(Ease.OutCubic));
            _seq.Insert(delay + 0.06f, Band.DOScaleX(1f, 0.28f).SetEase(Ease.OutCubic));
            foreach (var d in Decor) _seq.Insert(delay + 0.22f, d.DOFade(1f, 0.2f));
            _seq.Insert(delay + 0.2f, Main.DOFade(1f, 0.2f))
                .Insert(delay + 0.2f, DOTween.To(() => Main.characterSpacing, v => Main.characterSpacing = v, 6f, 0.45f).SetEase(Ease.OutCubic))
                .Insert(delay + 0.2f, Main.rectTransform.DOAnchorPosX(mainPos.x, 0.45f).SetEase(Ease.OutCubic))
                .Insert(delay + 0.28f, Eng.DOFade(1f, 0.2f))
                .Insert(delay + 0.34f, Tag.DOScaleY(1f, 0.18f).SetEase(Ease.OutBack));
            if (Flash != null)
                _seq.Insert(delay + 0.02f, Flash.DOFade(1f, 0.06f))
                    .Insert(delay + 0.08f, Flash.DOFade(0f, 0.4f));
            if (Ghost != null)
            {
                Ghost.alpha = 0f;
                var gr = (RectTransform)Ghost.transform.GetChild(0);
                var gPos = GhostHome;
                gr.anchoredPosition = gPos + new Vector2(60f, 0f);
                _seq.Insert(delay + 0.3f, Ghost.DOFade(1f, 0.4f))
                    .Insert(delay + 0.3f, gr.DOAnchorPos(gPos, 0.7f).SetEase(Ease.OutCubic));
            }
            _seq.AppendInterval(hold)
                .Append(Group.DOFade(0f, 0.3f))
                .Join(Main.rectTransform.DOAnchorPosX(mainPos.x + 60f, 0.3f).SetEase(Ease.InCubic))
                .OnComplete(() =>
                {
                    Main.rectTransform.anchoredPosition = mainPos;
                    Root.gameObject.SetActive(false);
                });
        }
    }

    // 작전 개시 (v7): 먹색 띠 + 왼쪽 흰 일련번호 판(층) + 오른쪽 큰 흐린 번호 + 아래 태그 (일반 · 정예 조우, 적 수)
    // 조각: Resources/BattleFx/V3/Start (원본 ArtDirection/BattleFxMockup/StartEnd_Redesign_v2/Extracted_Start_v7, 좌표는 그 layout.json)
    private Banner BuildStart()
    {
        const string k = "V3/Start/";
        var b = new Banner();
        b.Root = BannerRoot("StartBanner");
        b.Group = b.Root.GetComponent<CanvasGroup>();
        b.Lines.Add(Pic(b.Root, "Line_Top", k + "Line_Top", 99, 150, 1757, 1, true).rectTransform);
        b.Lines.Add(Pic(b.Root, "Band_Shadow", k + "Band_Shadow", 80, 177, 1787, 197).rectTransform);
        b.Band = Pic(b.Root, "Band", k + "Band", 76, 172, 1787, 197).rectTransform;

        // 큰 흐린 번호 — 띠 모양으로 잘라 띠 밖(위 · 아래 · 오른쪽 잘린 모서리)으로 넘치지 않게
        var ghostMask = Pic(b.Root, "GhostMask", k + "Band", 76, 172, 1787, 197);
        ghostMask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        b.Ghost = ghostMask.gameObject.AddComponent<CanvasGroup>();
        b.GhostNumber = Text(ghostMask.rectTransform, "Ghost", 0, 0, 333, 260, 260f, new Color(0.816f, 0.824f, 0.827f, 0.07f), TextAlignmentOptions.Left);
        b.GhostNumber.fontStyle = FontStyles.Bold;
        var gr = b.GhostNumber.rectTransform;
        gr.anchorMin = gr.anchorMax = new Vector2(0f, 1f);
        gr.anchoredPosition = b.GhostHome = new Vector2(1506f - 76f, -(178f - 172f));

        b.Decor.Add(Pic(b.Root, "Serial_Block", k + "Serial_Block", 76, 172, 213, 197));
        b.Decor.Add(Pic(b.Root, "Serial_Ticks", k + "Serial_Ticks", 96, 321, 15, 31));
        b.Decor.Add(Pic(b.Root, "Serial_Divider", k + "Serial_Divider", 314, 191, 1, 110));
        b.Decor.Add(Pic(b.Root, "Ticks_Right", k + "Ticks_Right", 1844, 239, 7, 54));
        b.Lines.Add(Pic(b.Root, "Serial_Rule", k + "Serial_Rule", 164, 273, 27, 1).rectTransform);
        b.Lines.Add(Pic(b.Root, "Title_Rule_L", k + "Title_Rule", 458, 280, 221, 1).rectTransform);
        b.Lines.Add(Pic(b.Root, "Title_Rule_R", k + "Title_Rule", 1236, 280, 221, 1).rectTransform);
        b.Serial = Text(b.Root, "Serial", 162, 195, 84, 68, 58f, Hex("#131718"), TextAlignmentOptions.Left);
        b.SerialCaption = Text(b.Root, "SerialCaption", 164, 298, 97, 50, 10f, Hex("#24272A"), TextAlignmentOptions.Left, 10f);
        b.SerialCaption.fontStyle = FontStyles.Bold;
        b.SerialCaption.lineSpacing = -10f;
        b.Decor.Add(b.Serial); b.Decor.Add(b.SerialCaption);

        b.Tag = Group("Tag", b.Root);
        Pic(b.Tag, "Tag", k + "Tag", 689, 349, 540, 42, true);
        Pic(b.Tag, "Tag_Bar_L", k + "Tag_Bar", 750, 360, 1, 21);
        Pic(b.Tag, "Tag_Bar_R", k + "Tag_Bar", 1170, 360, 1, 21);
        // 정예: 3px 칩 + 청록 ELITE (칩만으로는 잘 안 보인다)
        b.EliteMark = Pic(b.Tag, "Tag_EliteMark", k + "Tag_EliteMark", 773, 368, 3, 3).gameObject;
        b.EliteLabel = Text(b.Tag, "EliteLabel", 781, 361, 44, 22, 12f, accent, TextAlignmentOptions.Left, 8f);
        b.EliteLabel.text = "ELITE";
        b.EliteLabel.fontStyle = FontStyles.Bold;
        b.Sub = Text(b.Tag, "Encounter", 817, 357, 285, 30, 23f, Hex("#F5F5F2"), TextAlignmentOptions.Center, 4f);
        b.Sub.fontStyle = FontStyles.Bold;

        b.Eng = Text(b.Root, "Eng", 775, 198, 371, 25, 16f, Hex("#F1F1EF"), TextAlignmentOptions.Center, 50f);
        b.Main = Text(b.Root, "Main", 743, 230, 438, 99, 86f, Hex("#F8F8F5"), TextAlignmentOptions.Center);
        b.Main.fontStyle = FontStyles.Bold;
        b.Root.gameObject.SetActive(false);
        return b;
    }

    // ───────── 작전 종료 · 실패 판 (v3) ─────────
    // 흰 종이 판(승리) / 먹색 판(패배): 판이 왼쪽에서 밀려 들어오고, 나침반 문양이 늦게 떠오르고, 청록 막대가 왼쪽부터 그어진다.
    // 조각: Resources/BattleFx/V3/Victory · Defeat (원본 ArtDirection/RasterRedo_v2/Direction_Split, 좌표는 그 layout.json)

    private class ResultBanner
    {
        public RectTransform Root, Body, Tag;
        public CanvasGroup Group, BodyGroup;
        public Image Emblem, Bar, Dim;
        public TextMeshProUGUI Eng, Main, Sub;
        public readonly List<TextMeshProUGUI> Minor = new(); // 번호 · 작은 설명 — 판이 선 뒤 나타남
        public float DimAlpha;
        private Sequence _seq;

        public void Hide()
        {
            _seq?.Kill();
            Root.gameObject.SetActive(false);
        }

        public void Play(string eng, string main, string sub, float delay, float hold)
        {
            _seq?.Kill();
            Root.gameObject.SetActive(true);
            Eng.text = eng; Main.text = main; Sub.text = sub;
            Group.alpha = 1f;
            BodyGroup.alpha = 0f;
            Body.anchoredPosition = new Vector2(-140f, 0f);
            Emblem.color = new Color(1f, 1f, 1f, 0f);
            Emblem.rectTransform.localScale = Vector3.one * 1.12f;
            Bar.rectTransform.localScale = new Vector3(0f, 1f, 1f);
            Tag.localScale = new Vector3(1f, 0f, 1f);
            Main.alpha = 0f; Eng.alpha = 0f;
            foreach (var t in Minor) t.alpha = 0f;
            Main.characterSpacing = 40f;
            var mainPos = Main.rectTransform.anchoredPosition;
            Main.rectTransform.anchoredPosition = new Vector2(mainPos.x - 30f, mainPos.y);
            if (Dim != null) Dim.color = new Color(Dim.color.r, Dim.color.g, Dim.color.b, 0f);

            _seq = DOTween.Sequence().SetLink(Root.gameObject).SetUpdate(true).AppendInterval(delay);
            if (Dim != null) _seq.Insert(delay, Dim.DOFade(DimAlpha, 0.5f));
            _seq.Insert(delay, Body.DOAnchorPosX(0f, 0.3f).SetEase(Ease.OutCubic))
                .Insert(delay, BodyGroup.DOFade(1f, 0.16f))
                .Insert(delay + 0.18f, Emblem.DOFade(1f, 0.45f))
                .Insert(delay + 0.18f, Emblem.rectTransform.DOScale(1f, 0.6f).SetEase(Ease.OutCubic))
                .Insert(delay + 0.2f, Bar.rectTransform.DOScaleX(1f, 0.35f).SetEase(Ease.OutCubic))
                .Insert(delay + 0.16f, Main.DOFade(1f, 0.2f))
                .Insert(delay + 0.16f, DOTween.To(() => Main.characterSpacing, v => Main.characterSpacing = v, 0f, 0.45f).SetEase(Ease.OutCubic))
                .Insert(delay + 0.16f, Main.rectTransform.DOAnchorPosX(mainPos.x, 0.45f).SetEase(Ease.OutCubic))
                .Insert(delay + 0.26f, Eng.DOFade(1f, 0.2f))
                .Insert(delay + 0.34f, Tag.DOScaleY(1f, 0.18f).SetEase(Ease.OutBack));
            foreach (var t in Minor) _seq.Insert(delay + 0.3f, t.DOFade(1f, 0.2f));
            _seq.AppendInterval(hold)
                .Append(Group.DOFade(0f, 0.3f))
                .Join(Body.DOAnchorPosX(60f, 0.3f).SetEase(Ease.InCubic))
                .OnComplete(() =>
                {
                    Main.rectTransform.anchoredPosition = mainPos;
                    Root.gameObject.SetActive(false);
                });
        }
    }

    private ResultBanner BuildResultBanner(string folder, bool victory)
    {
        string k = "V3/" + folder + "/";
        var b = new ResultBanner();
        b.Root = BannerRoot(folder + "Banner");
        b.Group = b.Root.GetComponent<CanvasGroup>();
        if (!victory)
        {
            b.Dim = Img(b.Root, "Dim", null, ink);
            Stretch(b.Dim.rectTransform);
            b.DimAlpha = defeatDimAlpha;
        }
        b.Body = Group("Body", b.Root);
        b.BodyGroup = b.Body.gameObject.AddComponent<CanvasGroup>();
        Pic(b.Body, "Panel_Shadow", k + "Panel_Shadow", 426, 386, 1040, 323).color = new Color(1f, 1f, 1f, 0.7f);
        Pic(b.Body, "Panel", k + "Panel", 440, 374, 1040, 323);
        b.Emblem = Pic(b.Body, "Emblem_Compass", k + "Emblem_Compass", 440, 374, 1040, 323);
        PivotAt(b.Emblem.rectTransform, 1271.5f, 539f, 440, 374);
        Pic(b.Body, "Corner_Ink", k + "Corner_Ink", 425, 384, 90, 173);
        b.Bar = Pic(b.Body, "Accent_Bar", k + "Accent_Bar", 442, 678, 533, 8, true);

        b.Tag = Group("Tag", b.Body);
        Pic(b.Tag, "Tag", k + "Tag", 976, 644, 423, 42, true);
        Pic(b.Tag, "Tag_Mark", k + "Tag_Mark", 1043, 663, 30, 1);
        b.Sub = Text(b.Tag, "TagLabel", 1096, 655, 274, 21, 12.6f, Hex("#F3F4F5"), TextAlignmentOptions.Left, 8f);
        b.Sub.fontStyle = FontStyles.Bold;

        Color minor = Hex(victory ? "#60656A" : "#B8BDC2");
        var serial = Text(b.Body, "Serial", 529, 402, 46, 23, 17f, minor, TextAlignmentOptions.Left);
        serial.text = "01";
        b.Main = Text(b.Body, "Main", 531, 458, 560, 108, 98f, Hex(victory ? "#111518" : "#EBEEF0"), TextAlignmentOptions.Left);
        b.Main.fontStyle = FontStyles.Bold;
        b.Eng = Text(b.Body, "Eng", 544, 581, 494, 28, 17f, minor, TextAlignmentOptions.Left, 12f);
        var cap1 = Text(b.Body, "Caption", 544, 632, 390, 16, 10.3f, minor, TextAlignmentOptions.Left);
        cap1.text = victory ? "VERTEX // OPERATION COMPLETE" : "VERTEX // OPERATION TERMINATED";
        var cap2 = Text(b.Body, "Caption_2", 544, 650, 390, 16, 10.3f, minor, TextAlignmentOptions.Left);
        cap2.text = victory ? "ALL HOSTILES NEUTRALIZED." : "EXPEDITION INTERRUPTED.";
        b.Minor.Add(serial); b.Minor.Add(cap1); b.Minor.Add(cap2);
        b.Root.gameObject.SetActive(false);
        return b;
    }

    // ───────── 카드 컷인 (v3) ─────────
    // 공용 먹색 판 하나 + 왼쪽 카드 아트 + 오른쪽 카테고리 문양(파워 · 스킬 · 공격). 유니크는 이중 테두리를 덧얹는다.
    // 조각: Resources/BattleFx/V3/CutIn (layout.json의 art_window 다각형으로 카드 아트를 자른다)

    private class CutIn
    {
        public RectTransform Root, Content, ArtBox;
        public CanvasGroup Group;
        public UICroppedArt Art;
        public Image Emblem;
        public GameObject UniqueBorder;
        public TextMeshProUGUI Index, Label, Name, CategoryKo;
        private Sequence _seq;

        public void Hide()
        {
            _seq?.Kill();
            Root.gameObject.SetActive(false);
        }

        public void Play(Sprite art, Sprite emblem, bool unique, int index, string label, string categoryKo, string name)
        {
            _seq?.Kill();
            Root.gameObject.SetActive(true);
            ArtBox.gameObject.SetActive(art != null);
            if (art != null) Art.SetSprite(art, new Vector2(0.5f, 0.55f));
            Emblem.sprite = emblem;
            UniqueBorder.SetActive(unique);
            Index.text = index.ToString("00");
            Label.text = label;
            CategoryKo.text = categoryKo;
            Name.text = name;

            Group.alpha = 0f;
            Content.anchoredPosition = new Vector2(-260f, 0f);
            ArtBox.anchoredPosition = new Vector2(-80f, 0f);
            Emblem.rectTransform.anchoredPosition = EmblemPos + new Vector2(40f, 0f);
            Name.characterSpacing = 30f;
            _seq = DOTween.Sequence().SetLink(Root.gameObject)
                .Append(Content.DOAnchorPosX(0f, 0.24f).SetEase(Ease.OutCubic))
                .Join(Group.DOFade(1f, 0.12f))
                .Join(ArtBox.DOAnchorPosX(0f, 0.5f).SetEase(Ease.OutCubic))
                .Join(Emblem.rectTransform.DOAnchorPosX(EmblemPos.x, 0.6f).SetEase(Ease.OutCubic))
                .Join(DOTween.To(() => Name.characterSpacing, v => Name.characterSpacing = v, 0f, 0.4f).SetEase(Ease.OutCubic))
                .Append(ArtBox.DOAnchorPosX(18f, 0.8f).SetEase(Ease.Linear))
                .Join(Emblem.rectTransform.DOAnchorPosX(EmblemPos.x - 10f, 0.8f).SetEase(Ease.Linear))
                .Append(Content.DOAnchorPosX(-120f, 0.22f).SetEase(Ease.InCubic))
                .Join(Group.DOFade(0f, 0.22f))
                .OnComplete(() => Root.gameObject.SetActive(false));
        }

        public Vector2 EmblemPos;
    }

    private CutIn BuildCutIn()
    {
        const string k = "V3/CutIn/";
        var c = new CutIn();
        c.Root = Group("CardCutIn", _root);
        c.Group = c.Root.gameObject.AddComponent<CanvasGroup>();
        c.Group.blocksRaycasts = false;
        c.Content = Group("Content", c.Root);

        Pic(c.Content, "Frame_Shadow", k + "Frame_Shadow", 13, 114, 1236, 195).color = new Color(1f, 1f, 1f, 0.65f);

        // 카드 아트: 판 뒤, 사선 다각형 창 모양으로 마스킹 (art_window.polygon_screen_absolute_px)
        const float left = 9.5f, right = 569f, bottomRight = 505.5f, top = 109.5f, bottom = 297.5f;
        var maskRt = Group("ArtMask", c.Content);
        maskRt.anchorMin = maskRt.anchorMax = new Vector2(0.5f, 1f);
        maskRt.pivot = new Vector2(0f, 1f);
        maskRt.anchoredPosition = ScreenPos(left, top);
        maskRt.sizeDelta = new Vector2(right - left, bottom - top);
        maskRt.gameObject.AddComponent<CanvasRenderer>();
        var slice = maskRt.gameObject.AddComponent<SanctuarySliceGraphic>();
        slice.Configure(new Vector4(0f, (bottomRight - left) / (right - left), 0f, 1f), false, false, false);
        maskRt.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        c.ArtBox = Group("ArtBox", maskRt);
        c.ArtBox.offsetMin = new Vector2(-40f, 0f);
        c.ArtBox.offsetMax = new Vector2(40f, 0f);
        c.ArtBox.gameObject.AddComponent<CanvasRenderer>();
        var raw = c.ArtBox.gameObject.AddComponent<RawImage>();
        raw.raycastTarget = false;
        c.Art = c.ArtBox.gameObject.AddComponent<UICroppedArt>();

        Pic(c.Content, "Frame", k + "Frame", 6, 106, 1236, 195);
        Pic(c.Content, "Art_Fade", k + "Art_Fade", 6, 106, 1236, 195);
        c.Emblem = Pic(c.Content, "Emblem", k + "Emblem_Power", 6, 106, 1236, 195);
        c.EmblemPos = c.Emblem.rectTransform.anchoredPosition;
        c.UniqueBorder = Pic(c.Content, "Unique_Border", k + "Unique_Border", 6, 106, 1236, 195).gameObject;
        Pic(c.Content, "Index_Line", k + "Index_Line", 586, 161, 44, 2);
        Pic(c.Content, "Label_Line", k + "Label_Line", 650, 135, 253, 1);
        Pic(c.Content, "Cross", k + "Cross", 1147, 112, 41, 46);
        Pic(c.Content, "Name_Slash", k + "Name_Slash", 520, 238, 21, 46);

        c.Index = Text(c.Content, "Index", 584, 124, 57, 30, 24f, accent, TextAlignmentOptions.Left);
        c.Index.fontStyle = FontStyles.Bold;
        c.Label = Text(c.Content, "Label", 653, 151, 281, 23, 16f, paper, TextAlignmentOptions.Left, 25f);
        c.Label.fontStyle = FontStyles.Bold;
        c.Name = Text(c.Content, "Name", 582, 178, 379, 69, 58f, paper, TextAlignmentOptions.Left);
        c.Name.fontStyle = FontStyles.Bold;
        c.Name.enableAutoSizing = true; c.Name.fontSizeMin = 32f; c.Name.fontSizeMax = 58f;
        c.CategoryKo = Text(c.Content, "CategoryKo", 595, 254, 138, 28, 20.5f, paper, TextAlignmentOptions.Left);
        c.CategoryKo.fontStyle = FontStyles.Bold;
        c.Root.gameObject.SetActive(false);
        return c;
    }

    // 피벗을 화면 좌표 (px, py)로 옮기되 그림 자리는 그대로 (조각 왼쪽 위가 (x, y)일 때)
    private static void PivotAt(RectTransform rt, float px, float py, float x, float y)
    {
        var size = rt.sizeDelta;
        rt.pivot = new Vector2((px - x) / size.x, 1f - (py - y) / size.y);
        rt.anchoredPosition = ScreenPos(px, py);
    }

    private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.white;

    // ───────── 적 처치 기록 ─────────

    // 한 줄: 먹색 줄 + 왼쪽 청록 띠 + "TARGET DOWN" + 구분 사선 + "이름  무력화" (+ 보스는 BOSS 칩)
    private void AddFeed(string enemyName, bool boss)
    {
        const string k = "KillFeed/";
        // 의뢰 추적 패널(QuestTrackerView)이 떠 있으면 그 아래로 내린다 (기본 첫 줄 y 258)
        _feed.anchoredPosition = new Vector2(0f, -Mathf.Max(0f, QuestTrackerView.Bottom + 16f - 258f));
        var row = Group("Feed", _feed);
        // 기존 기록은 아래로 민다
        for (int i = 0; i < _feed.childCount - 1; i++)
        {
            var old = (RectTransform)_feed.GetChild(i);
            old.DOAnchorPosY(old.anchoredPosition.y - RowStep, 0.2f).SetEase(Ease.OutCubic).SetLink(old.gameObject);
        }
        var cg = row.gameObject.AddComponent<CanvasGroup>();
        // 조각 v3 (ArtDirection/RasterRedo_v3/Extracted/KillFeed): 줄 574×53 · 사선 25×38 · 구분 21×38 · BOSS 칩 49×24
        Pic(row, "Row", k + "Row", 64, 257, 574, 53, true);
        Pic(row, "Accent", k + "Row_Accent", 88, 264, 25, 38);
        Pic(row, "Separator", k + "Separator", 268, 264, 21, 38);
        var eng = Text(row, "Eng", 120, 270, 142, 26, 12f, accent, TextAlignmentOptions.Left, 6f);
        eng.text = "TARGET DOWN";
        eng.fontStyle = FontStyles.Bold;
        var name = Text(row, "Name", 310, 268, boss ? 244 : 300, 32, 20f, paper, TextAlignmentOptions.Left);
        name.text = $"{enemyName}  <color=#9AA0A6><size=80%>무력화</size></color>";
        name.overflowMode = TextOverflowModes.Ellipsis;
        if (boss)
        {
            Pic(row, "BossChip", k + "BossChip", 556, 272, 49, 24);
            var chip = Text(row, "BossLabel", 556, 273, 49, 22, 11f, paper, TextAlignmentOptions.Center, 4f); // v3 칩은 테두리만 — 밝은 글자
            chip.text = "BOSS";
            chip.fontStyle = FontStyles.Bold;
        }

        row.anchoredPosition = new Vector2(-60f, 0f);
        cg.alpha = 0f;
        DOTween.Sequence().SetLink(row.gameObject)
            .Append(row.DOAnchorPosX(0f, 0.22f).SetEase(Ease.OutCubic))
            .Join(cg.DOFade(1f, 0.15f))
            .AppendInterval(2.2f)
            .Append(cg.DOFade(0f, 0.35f))
            .OnComplete(() => Destroy(row.gameObject));
    }

    // ───────── 생성 도우미 ─────────

    // 1920×1080 좌상단 좌표 → 화면 위·가운데 기준 anchoredPosition (넓은 화면에서도 가운데 정렬)
    private static Vector2 ScreenPos(float x, float y) => new(x - 960f, -y);

    private RectTransform BannerRoot(string name)
    {
        var root = Group(name, _root);
        var group = root.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        // 가운데 배너는 적 인텐트/상태 툴팁(정렬 200)보다 위 — 전투 시작 때 마우스가 적 위에 있어도 가리지 않게
        var bannerCanvas = root.gameObject.AddComponent<Canvas>();
        bannerCanvas.overrideSorting = true;
        bannerCanvas.sortingOrder = bannerSortingOrder;
        return root;
    }

    private static RectTransform Group(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        Stretch(rt);
        return rt;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    // 코드로 붙이는 Graphic은 CanvasRenderer가 자동으로 따라오지 않을 수 있어 먼저 붙인다
    private static Image Img(Transform parent, string name, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    // 조각 하나를 layout.json 좌표(1920×1080 좌상단, 화면 크기)에 놓는다. 피벗은 왼쪽 위 — 가로로 그어지는 연출은 왼쪽부터 자란다
    private static Image Pic(Transform parent, string name, string sprite, float x, float y, float w, float h, bool sliced = false)
    {
        var s = Resources.Load<Sprite>("BattleFx/" + sprite);
        if (s == null) Debug.LogWarning($"[BattleOperationFx] 조각 없음: Resources/BattleFx/{sprite}");
        var img = Img(parent, name, s, Color.white);
        img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = ScreenPos(x, y);
        rt.sizeDelta = new Vector2(w, Mathf.Max(h, 1f));
        return img;
    }

    private TextMeshProUGUI Text(Transform parent, string name, float x, float y, float w, float h, float size, Color color,
        TextAlignmentOptions align, float spacing = 0f)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.characterSpacing = spacing;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = ScreenPos(x, y);
        rt.sizeDelta = new Vector2(w, h);
        return t;
    }
}
