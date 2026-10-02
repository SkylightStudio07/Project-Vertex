using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전투 화면의 명일방주식 띠 연출. 입력은 막지 않는다 (전부 raycastTarget off).
//   - 전투 개시: 먹색 사선 띠 + "작전 개시 / OPERATION START" + 전투 종류·적 수 태그 (보스전은 BossBattleDirector가 따로 연출)
//   - 카드 컷인: 파워 카드·유니크 카드를 쓰면 왼쪽에서 카드 아트 띠가 미끄러져 들어온다 (유니크는 이중 테두리·눈금)
//   - 적 처치: 왼쪽 위에 "TARGET DOWN 이름 무력화" 기록이 쌓였다 사라진다 (보스는 BOSS 칩)
//   - 승리/패배: 화면 가운데 종이 띠 "작전 종료" / 먹색 띠 "작전 실패" (승리는 흰 섬광 + 짧은 슬로모션, 패배는 화면이 어두워짐)
// 그림 조각: Resources/BattleFx/<연출>/ (원본 ArtDirection/BattleFxMockup/Extracted — 2배 해상도, PPU 200).
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
    private Banner _start, _victory, _defeat;
    private CutIn _cutInPower, _cutInUnique;
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
        _cutInPower = BuildCutIn(unique: false);
        _cutInUnique = BuildCutIn(unique: true);
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
        for (int i = _feed.childCount - 1; i >= 0; i--) Destroy(_feed.GetChild(i).gameObject);

        var type = _battle.CurrentBattleType;
        if (type == BattleType.Boss && BossBattleDirector.Instance != null) return;

        int count = 0;
        foreach (var e in _battle.Enemies) if (e != null && !e.IsDead) count++;
        bool elite = type == BattleType.Elite;
        _start.SetElite(elite);
        _start.Play("OPERATION START", "작전 개시", $"{(elite ? "정예 조우" : "일반 조우")}  ·  적 {count:00}", 0.15f, 0.75f);
    }

    private void HandleCardPlayed(CardData card)
    {
        if (card == null || card.Type == CardData.CardType.Status) return;
        bool power = cutInPowerCards && card.Type == CardData.CardType.Power;
        if (!power && card.Rarity < cutInMinRarity) return;
        string label = card.Type switch
        {
            CardData.CardType.Power => "POWER ACTIVATED",
            CardData.CardType.Attack => "ATTACK",
            _ => "SKILL ACTIVATED",
        };
        bool unique = card.Rarity == CardData.CardRarity.Unique;
        (unique ? _cutInPower : _cutInUnique).Hide();
        (unique ? _cutInUnique : _cutInPower).Play(card.CardImage, label, card.CardName, unique ? "UNIQUE" : "POWER");
    }

    private void HandleEnemyDefeated(EnemyInstance enemy)
    {
        if (enemy == null || enemy.Data == null) return;
        AddFeed(enemy.Data.enemyName, enemy.Data.rank == EnemyEncounterType.Boss);
    }

    private void HandleVictory(BattleReward _)
    {
        _cutInPower.Hide(); _cutInUnique.Hide();
        _victory.Play("MISSION ACCOMPLISHED", "작전 종료", "ALL HOSTILES NEUTRALIZED", 0.1f, 0.9f);
        if (victorySlowScale < 0.99f)
        {
            Time.timeScale = victorySlowScale;
            DOVirtual.DelayedCall(victorySlowSeconds, () => Time.timeScale = 1f, ignoreTimeScale: true).SetLink(gameObject);
        }
    }

    private void HandleDefeat()
    {
        _cutInPower.Hide(); _cutInUnique.Hide();
        _defeat.Play("MISSION FAILED", "작전 실패", "OPERATOR DOWN", 0.2f, 1.6f);
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
        public Vector2 SubFullPos, SubFullSize, SubElitePos, SubEliteSize;
        public float DimAlpha;
        private Sequence _seq;

        public void SetElite(bool elite)
        {
            if (EliteMark == null) return;
            EliteMark.SetActive(elite);
            EliteLabel.gameObject.SetActive(elite);
            Sub.rectTransform.anchoredPosition = elite ? SubElitePos : SubFullPos;
            Sub.rectTransform.sizeDelta = elite ? SubEliteSize : SubFullSize;
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
                .Insert(delay + 0.2f, DOTween.To(() => Main.characterSpacing, v => Main.characterSpacing = v, 14f, 0.45f).SetEase(Ease.OutCubic))
                .Insert(delay + 0.2f, Main.rectTransform.DOAnchorPosX(mainPos.x, 0.45f).SetEase(Ease.OutCubic))
                .Insert(delay + 0.28f, Eng.DOFade(1f, 0.2f))
                .Insert(delay + 0.34f, Tag.DOScaleY(1f, 0.18f).SetEase(Ease.OutBack));
            if (Flash != null)
                _seq.Insert(delay + 0.02f, Flash.DOFade(1f, 0.06f))
                    .Insert(delay + 0.08f, Flash.DOFade(0f, 0.4f));
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

    private Banner BuildStart()
    {
        const string k = "Start/";
        var b = new Banner();
        b.Root = BannerRoot("StartBanner");
        b.Group = b.Root.GetComponent<CanvasGroup>();
        b.Lines.Add(Pic(b.Root, "Line_Top", k + "Line_Top", 96, 154, 1764, 1, true).rectTransform);
        b.Band = Pic(b.Root, "Band", k + "Band", 2, 174, 1880, 194, true).rectTransform;
        b.Lines.Add(Pic(b.Root, "Line_Bottom", k + "Line_Bottom", 66, 368, 1668, 1, true).rectTransform);
        b.Lines.Add(Pic(b.Root, "Label_Line_L", k + "Label_Line", 558, 210, 170, 1, true).rectTransform);
        b.Lines.Add(Pic(b.Root, "Label_Line_R", k + "Label_Line", 1192, 210, 170, 1, true).rectTransform);
        b.Decor.Add(Pic(b.Root, "Slash_Accent", k + "Slash_Accent", 1680, 250, 106, 100));
        b.Decor.Add(Pic(b.Root, "Title_Slash", k + "Title_Slash", 1228, 228, 82, 118));

        b.Tag = Group("Tag", b.Root);
        Pic(b.Tag, "Tag", k + "Tag", 718, 356, 488, 46, true);
        b.EliteMark = Pic(b.Tag, "Tag_EliteMark", k + "Tag_EliteMark", 742, 360, 126, 38, true).gameObject;
        Pic(b.Tag, "Tick", k + "Tick", 1134, 374, 14, 14);
        b.EliteLabel = Text(b.Tag, "EliteLabel", 768, 366, 80, 28, 17f, accent, TextAlignmentOptions.Center, 4f);
        b.EliteLabel.text = "ELITE";
        b.EliteLabel.fontStyle = FontStyles.Bold;
        b.Sub = Text(b.Tag, "Encounter", 916, 365, 205, 29, 18f, paper, TextAlignmentOptions.Center, 2f);
        b.SubElitePos = b.Sub.rectTransform.anchoredPosition; b.SubEliteSize = b.Sub.rectTransform.sizeDelta;
        b.SubFullPos = ScreenPos(742, 365); b.SubFullSize = new Vector2(380f, 29f);

        b.Eng = Text(b.Root, "Eng", 754, 194, 430, 28, 19f, accent, TextAlignmentOptions.Center, 26f);
        b.Main = Text(b.Root, "Main", 706, 224, 518, 122, 82f, paper, TextAlignmentOptions.Center);
        b.Main.fontStyle = FontStyles.Bold;
        b.Root.gameObject.SetActive(false);
        return b;
    }

    // 작전 종료(종이 띠) / 작전 실패(먹색 띠) — 같은 배치, 다른 조각
    private Banner BuildResultBanner(string folder, bool victory)
    {
        string k = folder + "/";
        var b = new Banner();
        b.Root = BannerRoot(folder + "Banner");
        b.Group = b.Root.GetComponent<CanvasGroup>();
        if (!victory)
        {
            b.Dim = Img(b.Root, "Dim", null, ink);
            Stretch(b.Dim.rectTransform);
            b.DimAlpha = defeatDimAlpha;
        }
        b.Band = Pic(b.Root, "Band", k + "Band", 0, 500, 1920, 220, true).rectTransform;
        if (victory) b.Flash = Pic(b.Root, "Flash", k + "Flash", 0, 500, 1920, 220);
        b.Lines.Add(Pic(b.Root, "Line_Top", k + "Line_Top", 96, 504, 1762, 1, true).rectTransform);
        b.Lines.Add(Pic(b.Root, "Line_Bottom_L", k + "Line_Bottom", 44, 714, 696, 1, true).rectTransform);
        b.Lines.Add(Pic(b.Root, "Line_Bottom_R", k + "Line_Bottom", 1194, 714, 656, 1, true).rectTransform);
        b.Lines.Add(Pic(b.Root, "Label_Line_L", k + "Label_Line", 350, 548, 268, 1, true).rectTransform);
        b.Lines.Add(Pic(b.Root, "Label_Line_R", k + "Label_Line", 1300, 548, 268, 1, true).rectTransform);
        b.Lines.Add(Pic(b.Root, "Title_Line_L", k + "Title_Line", 412, 630, 206, 1, true).rectTransform);
        b.Lines.Add(Pic(b.Root, "Title_Line_R", k + "Title_Line", 1300, 630, 206, 1, true).rectTransform);
        b.Decor.Add(Pic(b.Root, "Slash", k + "Slash", 814, 480, 214, 284));
        b.Decor.Add(Pic(b.Root, "Slash_Edge_L", k + "Slash_Edge", 12, 502, 82, 110));
        b.Decor.Add(Pic(b.Root, "Slash_Edge_R", k + "Slash_Edge", 1838, 608, 82, 110));
        b.Decor.Add(Pic(b.Root, "Slash_Short_R", k + "Slash_Short", 1840, 502, 42, 56));
        b.Decor.Add(Pic(b.Root, "Slash_Short_L", k + "Slash_Short", 32, 676, 42, 56));

        b.Tag = Group("Tag", b.Root);
        Pic(b.Tag, "Tag", k + "Tag", 760, 688, 392, 28, true);
        Pic(b.Tag, "Diamond", k + "Diamond", 1154, 694, 18, 18);
        b.Sub = Text(b.Tag, "TagLabel", 782, 690, 340, 22, 14f, victory ? paper : muted, TextAlignmentOptions.Center, 10f);
        b.Sub.fontStyle = FontStyles.Bold;

        b.Eng = Text(b.Root, "Eng", 652, 532, 600, 32, 20f, victory ? ink : muted, TextAlignmentOptions.Center, 34f);
        b.Main = Text(b.Root, "Main", 670, 568, 590, 106, 80f, victory ? ink : paper, TextAlignmentOptions.Center);
        b.Main.fontStyle = FontStyles.Bold;
        b.Root.gameObject.SetActive(false);
        return b;
    }

    // ───────── 카드 컷인 ─────────

    private class CutIn
    {
        public RectTransform Root, Content, ArtBox;
        public CanvasGroup Group;
        public UICroppedArt Art;
        public TextMeshProUGUI Label, Name, Chip;
        private Sequence _seq;

        public void Hide()
        {
            _seq?.Kill();
            Root.gameObject.SetActive(false);
        }

        public void Play(Sprite art, string label, string name, string chip)
        {
            _seq?.Kill();
            Root.gameObject.SetActive(true);
            ArtBox.gameObject.SetActive(art != null);
            if (art != null) Art.SetSprite(art, new Vector2(0.5f, 0.55f));
            Label.text = label;
            Name.text = name;
            Chip.text = chip;

            Group.alpha = 0f;
            Content.anchoredPosition = new Vector2(-260f, 0f);
            ArtBox.anchoredPosition = new Vector2(-80f, 0f);
            Name.characterSpacing = 30f;
            _seq = DOTween.Sequence().SetLink(Root.gameObject)
                .Append(Content.DOAnchorPosX(0f, 0.24f).SetEase(Ease.OutCubic))
                .Join(Group.DOFade(1f, 0.12f))
                .Join(ArtBox.DOAnchorPosX(0f, 0.5f).SetEase(Ease.OutCubic))
                .Join(DOTween.To(() => Name.characterSpacing, v => Name.characterSpacing = v, 2f, 0.4f).SetEase(Ease.OutCubic))
                .Append(ArtBox.DOAnchorPosX(18f, 0.8f).SetEase(Ease.Linear))
                .Append(Content.DOAnchorPosX(-120f, 0.22f).SetEase(Ease.InCubic))
                .Join(Group.DOFade(0f, 0.22f))
                .OnComplete(() => Root.gameObject.SetActive(false));
        }
    }

    // 목업에서 파워는 y 102, 유니크는 y 310에 그렸다. 둘 다 파워 자리(y 102)에 띄운다.
    private CutIn BuildCutIn(bool unique)
    {
        const string k = "CutIn/";
        string v = unique ? "Unique" : "Power";
        float dy = unique ? 310f - 102f : 0f; // 유니크 좌표를 파워 자리로 옮기는 양
        var c = new CutIn();
        c.Root = Group("CardCutIn_" + v, _root);
        c.Group = c.Root.gameObject.AddComponent<CanvasGroup>();
        c.Group.blocksRaycasts = false;
        c.Content = Group("Content", c.Root);

        // 카드 아트: 프레임 뒤, 사선 다각형 창 모양으로 마스킹 (layout.json art_window.polygon_screen)
        float right = unique ? 658f : 614f, bottomRight = unique ? 566f : 504f;
        var maskRt = Group("ArtMask", c.Content);
        maskRt.anchorMin = maskRt.anchorMax = new Vector2(0.5f, 1f);
        maskRt.pivot = new Vector2(0f, 1f);
        maskRt.anchoredPosition = ScreenPos(8f, 110f);
        maskRt.sizeDelta = new Vector2(right - 8f, 178f);
        maskRt.gameObject.AddComponent<CanvasRenderer>();
        var slice = maskRt.gameObject.AddComponent<SanctuarySliceGraphic>();
        slice.Configure(new Vector4(0f, (bottomRight - 8f) / (right - 8f), 0f, 1f), false, false, false);
        maskRt.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        c.ArtBox = Group("ArtBox", maskRt);
        c.ArtBox.offsetMin = new Vector2(-40f, 0f);
        c.ArtBox.offsetMax = new Vector2(40f, 0f);
        c.ArtBox.gameObject.AddComponent<CanvasRenderer>();
        var raw = c.ArtBox.gameObject.AddComponent<RawImage>();
        raw.raycastTarget = false;
        c.Art = c.ArtBox.gameObject.AddComponent<UICroppedArt>();

        Pic(c.Content, "Frame", k + "Frame_" + v, 0, 102, 1140, 200);
        Pic(c.Content, "Label_Plate", k + "Label_Plate_" + v, unique ? 686 : 630, 336 - (unique ? dy : 208), 424, 32);
        Pic(c.Content, "Line", k + "Line", unique ? 686 : 630, unique ? 366 - dy : 158, unique ? 318 : 414, 1, true);
        Pic(c.Content, "Diamond", k + "Diamond", unique ? 998 : 1040, unique ? 359 - dy : 151, 14, 14);
        Pic(c.Content, "Chip", k + "Chip_" + v, 694, 250, 136, 24);
        if (unique) Pic(c.Content, "Ticks", k + "Ticks_Unique", 712, 496 - dy, 250, 8);

        // 유니크 프레임은 아트 창이 넓어(위 오른쪽 끝 x 658) 사선이 라벨 줄에 걸린다 — 라벨을 아래 청록 선 시작점에 맞춘다
        c.Label = Text(c.Content, "Label", unique ? 694 : 648, 124, unique ? 352 : 392, 28, 17f, accent, TextAlignmentOptions.Left, 22f);
        c.Label.fontStyle = FontStyles.Bold;
        c.Name = Text(c.Content, "Name", 646, 168, 392, 74, 52f, paper, TextAlignmentOptions.Left);
        c.Name.fontStyle = FontStyles.Bold;
        c.Name.enableAutoSizing = true; c.Name.fontSizeMin = 30f; c.Name.fontSizeMax = 52f;
        c.Chip = Text(c.Content, "ChipLabel", 714, 250, 100, 24, 13f, unique ? ink : accent, TextAlignmentOptions.Center, 8f);
        c.Chip.fontStyle = FontStyles.Bold;
        c.Root.gameObject.SetActive(false);
        return c;
    }

    // ───────── 적 처치 기록 ─────────

    // 한 줄: 먹색 줄 + 왼쪽 청록 띠 + "TARGET DOWN" + 구분 사선 + "이름  무력화" (+ 보스는 BOSS 칩)
    private void AddFeed(string enemyName, bool boss)
    {
        const string k = "KillFeed/";
        var row = Group("Feed", _feed);
        // 기존 기록은 아래로 민다
        for (int i = 0; i < _feed.childCount - 1; i++)
        {
            var old = (RectTransform)_feed.GetChild(i);
            old.DOAnchorPosY(old.anchoredPosition.y - RowStep, 0.2f).SetEase(Ease.OutCubic).SetLink(old.gameObject);
        }
        var cg = row.gameObject.AddComponent<CanvasGroup>();
        Pic(row, "Row", k + "Row", 64, 258, 590, 50, true);
        Pic(row, "Accent", k + "Row_Accent", 88, 266, 22, 34);
        Pic(row, "Separator", k + "Separator", 268, 266, 18, 34);
        var eng = Text(row, "Eng", 120, 270, 142, 26, 12f, accent, TextAlignmentOptions.Left, 6f);
        eng.text = "TARGET DOWN";
        eng.fontStyle = FontStyles.Bold;
        var name = Text(row, "Name", 310, 268, boss ? 244 : 300, 32, 20f, paper, TextAlignmentOptions.Left);
        name.text = $"{enemyName}  <color=#9AA0A6><size=80%>무력화</size></color>";
        name.overflowMode = TextOverflowModes.Ellipsis;
        if (boss)
        {
            Pic(row, "BossChip", k + "BossChip", 562, 274, 60, 22, true);
            var chip = Text(row, "BossLabel", 566, 274, 52, 22, 12f, ink, TextAlignmentOptions.Center, 4f);
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
