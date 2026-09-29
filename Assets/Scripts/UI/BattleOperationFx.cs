using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전투 화면의 명일방주식 띠 연출. 입력은 막지 않는다 (전부 raycastTarget off).
//   - 전투 개시: 검은 사선 띠 + "작전 개시 / OPERATION START" + 적 수·전투 종류 태그 (보스전은 BossBattleDirector가 따로 연출)
//   - 카드 컷인: 파워 카드·유니크 카드를 쓰면 왼쪽에서 카드 아트 띠가 미끄러져 들어온다
//   - 적 처치: 왼쪽 위에 "▸ 이름  무력화" 기록이 쌓였다 사라진다
//   - 승리/패배: 화면 가운데 흰 띠 "작전 종료" / 검은 띠 "작전 실패" (승리는 짧은 슬로모션 포함)
// UI 요소는 Awake에서 코드로 만든다. 이 오브젝트는 Canvas 아래 화면 전체를 덮는 RectTransform이어야 한다.
public class BattleOperationFx : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;               // 비우면 TMP 기본 폰트
    [SerializeField] private int sortingOrder = 90;             // 컷인·처치 기록: 덱 목록(99) 아래, 손패 위
    [SerializeField] private int bannerSortingOrder = 210;      // 작전 개시·종료·실패 배너: 적 툴팁(200) 위
    [SerializeField] private Color accent = new(0.05f, 0.72f, 0.95f, 1f);
    [SerializeField] private Color ink = new(0.04f, 0.05f, 0.06f, 1f); // 반투명 어둠은 Linear 색 공간에서 뜨므로 불투명
    [SerializeField] private Color paper = new(0.96f, 0.96f, 0.95f, 0.97f);

    [Header("카드 컷인")]
    [SerializeField] private bool cutInPowerCards = true;
    [SerializeField] private CardData.CardRarity cutInMinRarity = CardData.CardRarity.Unique; // 파워 외에는 유니크부터

    [Header("승리 슬로모션")]
    [SerializeField, Range(0.05f, 1f)] private float victorySlowScale = 0.3f;
    [SerializeField] private float victorySlowSeconds = 0.35f; // 실제 시간

    private BattleManager _battle;
    private RectTransform _root;
    private Banner _start, _victory, _defeat;
    private CutIn _cutIn;
    private RectTransform _feed;

    private void Awake()
    {
        _root = (RectTransform)transform;
        var canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        if (font == null) font = TMP_Settings.defaultFontAsset;

        _start = BuildBanner("Start", 140f, 150f, ink, Color.white, accent, 40f);
        _victory = BuildBanner("Victory", 30f, 190f, paper, ink, accent, -48f);
        _defeat = BuildBanner("Defeat", 30f, 190f, ink, new Color(0.72f, 0.74f, 0.76f, 1f), new Color(0.45f, 0.47f, 0.5f, 1f), -48f);
        _cutIn = BuildCutIn();
        _feed = NewRect("KillFeed", _root);
        _feed.anchorMin = _feed.anchorMax = new Vector2(0f, 1f);
        _feed.pivot = new Vector2(0f, 1f);
        _feed.anchoredPosition = new Vector2(34f, -250f);
        _feed.sizeDelta = new Vector2(420f, 300f);
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
        string kind = type == BattleType.Elite ? "<color=#0DB8F2>ELITE</color> · 정예 조우" : "HOSTILE · 일반 조우";
        _start.Play("OPERATION START", "작전 개시", $"{kind}   적 {count:00}", 0.15f, 0.75f);
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
        _cutIn.Play(card.CardImage, label, card.CardName, card.Rarity == CardData.CardRarity.Unique);
    }

    private void HandleEnemyDefeated(EnemyInstance enemy)
    {
        if (enemy == null || enemy.Data == null) return;
        AddFeed(enemy.Data.enemyName);
    }

    private void HandleVictory(BattleReward _)
    {
        _cutIn.Hide();
        _victory.Play("MISSION ACCOMPLISHED", "작전 종료", "ALL HOSTILES NEUTRALIZED", 0.1f, 0.9f, flash: true);
        if (victorySlowScale < 0.99f)
        {
            Time.timeScale = victorySlowScale;
            DOVirtual.DelayedCall(victorySlowSeconds, () => Time.timeScale = 1f, ignoreTimeScale: true).SetLink(gameObject);
        }
    }

    private void HandleDefeat()
    {
        _cutIn.Hide();
        _defeat.Play("MISSION FAILED", "작전 실패", "OPERATOR DOWN", 0.2f, 1.6f);
    }

    // ───────── 적 처치 기록 ─────────

    private void AddFeed(string enemyName)
    {
        var row = NewRect("Feed", _feed);
        row.anchorMin = row.anchorMax = row.pivot = new Vector2(0f, 1f);
        row.sizeDelta = new Vector2(360f, 40f);
        // 기존 기록은 아래로 민다
        for (int i = 0; i < _feed.childCount - 1; i++)
        {
            var old = (RectTransform)_feed.GetChild(i);
            old.DOAnchorPosY(old.anchoredPosition.y - 46f, 0.2f).SetEase(Ease.OutCubic).SetLink(old.gameObject);
        }
        var cg = row.gameObject.AddComponent<CanvasGroup>();
        var bg = NewSkew("Bg", row, ink, 14f);
        Stretch(bg.rectTransform);
        var bar = NewImage("Bar", row, accent);
        bar.rectTransform.anchorMin = new Vector2(0f, 0f); bar.rectTransform.anchorMax = new Vector2(0f, 1f);
        bar.rectTransform.pivot = new Vector2(0f, 0.5f);
        bar.rectTransform.sizeDelta = new Vector2(5f, -10f);
        bar.rectTransform.anchoredPosition = new Vector2(8f, 0f);
        var text = NewText("Text", row, 22f, Color.white, TextAlignmentOptions.MidlineLeft);
        Stretch(text.rectTransform, 24f, 12f);
        text.text = $"<color=#0DB8F2><size=70%>TARGET DOWN</size></color>   {enemyName}  <color=#9AA0A6>무력화</color>";

        row.anchoredPosition = new Vector2(-60f, 0f);
        cg.alpha = 0f;
        DOTween.Sequence().SetLink(row.gameObject)
            .Append(row.DOAnchorPosX(0f, 0.22f).SetEase(Ease.OutCubic))
            .Join(cg.DOFade(1f, 0.15f))
            .AppendInterval(2.2f)
            .Append(cg.DOFade(0f, 0.35f))
            .OnComplete(() => Destroy(row.gameObject));
    }

    // ───────── 가운데 띠 배너 ─────────

    private class Banner
    {
        public RectTransform Root;
        public CanvasGroup Group;
        public RectTransform Band, Line, Slash;
        public TextMeshProUGUI Eng, Main, Sub;
        public RectTransform SubTag;
        public Image Flash;
        public float BandWidth;
        private Sequence _seq;

        public void Hide()
        {
            _seq?.Kill();
            Root.gameObject.SetActive(false);
        }

        public void Play(string eng, string main, string sub, float delay, float hold, bool flash = false)
        {
            _seq?.Kill();
            Root.gameObject.SetActive(true);
            Eng.text = eng; Main.text = main; Sub.text = sub;
            Group.alpha = 1f;
            Band.localScale = new Vector3(0f, 1f, 1f);
            Line.localScale = new Vector3(0f, 1f, 1f);
            Slash.anchoredPosition = new Vector2(-BandWidth * 0.5f - 200f, 0f);
            Main.alpha = 0f; Eng.alpha = 0f;
            Main.characterSpacing = 60f;
            Main.rectTransform.anchoredPosition = new Vector2(-40f, Main.rectTransform.anchoredPosition.y);
            SubTag.localScale = new Vector3(1f, 0f, 1f);
            if (Flash != null) Flash.color = new Color(1f, 1f, 1f, 0f);

            _seq = DOTween.Sequence().SetLink(Root.gameObject).SetUpdate(true)
                .AppendInterval(delay)
                .Append(Line.DOScaleX(1f, 0.22f).SetEase(Ease.OutCubic))
                .Insert(delay + 0.06f, Band.DOScaleX(1f, 0.26f).SetEase(Ease.OutCubic))
                .Insert(delay + 0.18f, Slash.DOAnchorPosX(BandWidth * 0.5f + 200f, 0.45f).SetEase(Ease.InOutSine))
                .Insert(delay + 0.2f, Main.DOFade(1f, 0.2f))
                .Insert(delay + 0.2f, DOTween.To(() => Main.characterSpacing, v => Main.characterSpacing = v, 14f, 0.45f).SetEase(Ease.OutCubic))
                .Insert(delay + 0.2f, Main.rectTransform.DOAnchorPosX(0f, 0.45f).SetEase(Ease.OutCubic))
                .Insert(delay + 0.28f, Eng.DOFade(1f, 0.2f))
                .Insert(delay + 0.34f, SubTag.DOScaleY(1f, 0.18f).SetEase(Ease.OutBack));
            if (Flash != null)
                _seq.Insert(delay + 0.02f, Flash.DOFade(0.45f, 0.06f))
                    .Insert(delay + 0.08f, Flash.DOFade(0f, 0.4f));
            _seq.AppendInterval(hold)
                .Append(Group.DOFade(0f, 0.3f))
                .Join(Main.rectTransform.DOAnchorPosX(60f, 0.3f).SetEase(Ease.InCubic))
                .OnComplete(() => Root.gameObject.SetActive(false));
        }
    }

    private Banner BuildBanner(string name, float y, float height, Color bandColor, Color textColor, Color lineColor, float skew)
    {
        var b = new Banner { BandWidth = 2300f };
        b.Root = NewRect(name + "Banner", _root);
        Stretch(b.Root);
        b.Group = b.Root.gameObject.AddComponent<CanvasGroup>();
        b.Group.blocksRaycasts = false;
        // 가운데 배너는 적 인텐트/상태 툴팁(정렬 200)보다 위 — 전투 시작 때 마우스가 적 위에 있어도 가리지 않게
        var bannerCanvas = b.Root.gameObject.AddComponent<Canvas>();
        bannerCanvas.overrideSorting = true;
        bannerCanvas.sortingOrder = bannerSortingOrder;

        if (name == "Victory")
        {
            b.Flash = NewImage("Flash", b.Root, new Color(1f, 1f, 1f, 0f));
            Stretch(b.Flash.rectTransform);
        }

        var band = NewSkew("Band", b.Root, bandColor, skew);
        b.Band = band.rectTransform;
        b.Band.anchorMin = b.Band.anchorMax = new Vector2(0.5f, 0.5f);
        b.Band.pivot = new Vector2(0f, 0.5f);
        b.Band.sizeDelta = new Vector2(b.BandWidth, height);
        b.Band.anchoredPosition = new Vector2(-b.BandWidth * 0.5f, y);

        // 띠 안을 한 번 훑고 지나가는 흰 사선
        var slashHolder = NewRect("SlashMask", b.Root);
        slashHolder.gameObject.AddComponent<RectMask2D>();
        slashHolder.anchorMin = slashHolder.anchorMax = new Vector2(0.5f, 0.5f);
        slashHolder.sizeDelta = new Vector2(b.BandWidth, height);
        slashHolder.anchoredPosition = new Vector2(0f, y);
        var slash = NewSkew("Slash", slashHolder, new Color(1f, 1f, 1f, bandColor.grayscale > 0.5f ? 0.9f : 0.18f), skew * 3f);
        if (bandColor.grayscale > 0.5f) slash.color = new Color(lineColor.r, lineColor.g, lineColor.b, 0.35f);
        b.Slash = slash.rectTransform;
        b.Slash.sizeDelta = new Vector2(90f, height);

        var line = NewImage("Line", b.Root, lineColor);
        b.Line = line.rectTransform;
        b.Line.anchorMin = b.Line.anchorMax = new Vector2(0.5f, 0.5f);
        b.Line.pivot = new Vector2(0f, 0.5f);
        b.Line.sizeDelta = new Vector2(b.BandWidth, 4f);
        b.Line.anchoredPosition = new Vector2(-b.BandWidth * 0.5f, y - height * 0.5f - 6f);

        b.Eng = NewText("Eng", b.Root, 22f, lineColor, TextAlignmentOptions.Center);
        b.Eng.characterSpacing = 38f;
        b.Eng.rectTransform.sizeDelta = new Vector2(1400f, 34f);
        b.Eng.rectTransform.anchoredPosition = new Vector2(0f, y + height * 0.5f - 30f);

        b.Main = NewText("Main", b.Root, height * 0.5f, textColor, TextAlignmentOptions.Center);
        b.Main.rectTransform.sizeDelta = new Vector2(1400f, height * 0.6f);
        b.Main.rectTransform.anchoredPosition = new Vector2(0f, y - 10f);

        // 띠 아래 작은 검은 태그
        b.SubTag = NewRect("SubTag", b.Root);
        b.SubTag.anchorMin = b.SubTag.anchorMax = new Vector2(0.5f, 0.5f);
        b.SubTag.pivot = new Vector2(0.5f, 1f);
        b.SubTag.sizeDelta = new Vector2(460f, 38f);
        b.SubTag.anchoredPosition = new Vector2(0f, y - height * 0.5f - 14f);
        var tagBg = NewSkew("Bg", b.SubTag, bandColor.grayscale > 0.5f ? ink : new Color(0.96f, 0.96f, 0.95f, 0.95f), 16f);
        Stretch(tagBg.rectTransform);
        b.Sub = NewText("Text", b.SubTag, 19f, bandColor.grayscale > 0.5f ? Color.white : ink, TextAlignmentOptions.Center);
        b.Sub.characterSpacing = 6f;
        Stretch(b.Sub.rectTransform);

        b.Root.gameObject.SetActive(false);
        return b;
    }

    // ───────── 카드 컷인 ─────────

    private class CutIn
    {
        public RectTransform Root;
        public CanvasGroup Group;
        public RectTransform Band, ArtBox;
        public UICroppedArt Art;
        public TextMeshProUGUI Label, Name;
        public Image Line;
        public Color Accent, Gold;
        private Sequence _seq;

        public void Hide()
        {
            _seq?.Kill();
            Root.gameObject.SetActive(false);
        }

        public void Play(Sprite art, string label, string name, bool unique)
        {
            _seq?.Kill();
            Root.gameObject.SetActive(true);
            ArtBox.gameObject.SetActive(art != null);
            if (art != null) Art.SetSprite(art, new Vector2(0.5f, 0.55f));
            Label.text = label;
            Name.text = name;
            Line.color = unique ? Gold : Accent;
            Label.color = Line.color;

            Group.alpha = 0f;
            Root.anchoredPosition = new Vector2(-260f, Root.anchoredPosition.y);
            ArtBox.anchoredPosition = new Vector2(-80f, 0f);
            Name.characterSpacing = 30f;
            _seq = DOTween.Sequence().SetLink(Root.gameObject)
                .Append(Root.DOAnchorPosX(0f, 0.24f).SetEase(Ease.OutCubic))
                .Join(Group.DOFade(1f, 0.12f))
                .Join(ArtBox.DOAnchorPosX(0f, 0.5f).SetEase(Ease.OutCubic))
                .Join(DOTween.To(() => Name.characterSpacing, v => Name.characterSpacing = v, 2f, 0.4f).SetEase(Ease.OutCubic))
                .Append(ArtBox.DOAnchorPosX(18f, 0.8f).SetEase(Ease.Linear))
                .Append(Root.DOAnchorPosX(-120f, 0.22f).SetEase(Ease.InCubic))
                .Join(Group.DOFade(0f, 0.22f))
                .OnComplete(() => Root.gameObject.SetActive(false));
        }
    }

    private CutIn BuildCutIn()
    {
        var c = new CutIn { Accent = accent, Gold = new Color(0.98f, 0.82f, 0.35f, 1f) };
        c.Root = NewRect("CardCutIn", _root);
        c.Root.anchorMin = c.Root.anchorMax = c.Root.pivot = new Vector2(0f, 0.5f);
        c.Root.sizeDelta = new Vector2(720f, 128f);
        c.Root.anchoredPosition = new Vector2(0f, 170f);
        c.Group = c.Root.gameObject.AddComponent<CanvasGroup>();
        c.Group.blocksRaycasts = false;

        var band = NewSkew("Band", c.Root, ink, 44f);
        c.Band = band.rectTransform;
        Stretch(c.Band, -40f, 0f);

        // 아트: 띠 오른쪽 절반, 사각 마스크로 자른다
        var artMask = NewRect("ArtMask", c.Root);
        artMask.gameObject.AddComponent<RectMask2D>();
        artMask.anchorMin = new Vector2(0.4f, 0f); artMask.anchorMax = new Vector2(1f, 1f);
        artMask.offsetMin = new Vector2(0f, 6f); artMask.offsetMax = new Vector2(-30f, -6f);
        c.ArtBox = NewRect("ArtBox", artMask);
        Stretch(c.ArtBox, -30f, 0f);
        c.ArtBox.gameObject.AddComponent<CanvasRenderer>();
        var raw = c.ArtBox.gameObject.AddComponent<RawImage>();
        raw.raycastTarget = false;
        c.Art = c.ArtBox.gameObject.AddComponent<UICroppedArt>();
        // 아트 왼쪽을 띠 색으로 점점 흐리게 덮어 긴 카드 이름이 아트 위로 넘어가도 읽히게 (띠 4장 계단 그라데이션)
        for (int i = 0; i < 4; i++)
        {
            var fade = NewImage("Fade" + i, artMask, new Color(ink.r, ink.g, ink.b, 0.9f - i * 0.2f));
            fade.rectTransform.anchorMin = new Vector2(0f, 0f); fade.rectTransform.anchorMax = new Vector2(0f, 1f);
            fade.rectTransform.pivot = new Vector2(0f, 0.5f);
            fade.rectTransform.sizeDelta = new Vector2(40f, 0f);
            fade.rectTransform.anchoredPosition = new Vector2(i * 40f, 0f);
        }

        c.Line = NewImage("Line", c.Root, accent);
        c.Line.rectTransform.anchorMin = new Vector2(0f, 0f); c.Line.rectTransform.anchorMax = new Vector2(1f, 0f);
        c.Line.rectTransform.pivot = new Vector2(0.5f, 1f);
        c.Line.rectTransform.sizeDelta = new Vector2(-20f, 4f);
        c.Line.rectTransform.anchoredPosition = new Vector2(-10f, -4f);

        c.Label = NewText("Label", c.Root, 18f, accent, TextAlignmentOptions.MidlineLeft);
        c.Label.characterSpacing = 24f;
        c.Label.rectTransform.anchorMin = c.Label.rectTransform.anchorMax = c.Label.rectTransform.pivot = new Vector2(0f, 0.5f);
        c.Label.rectTransform.sizeDelta = new Vector2(360f, 28f);
        c.Label.rectTransform.anchoredPosition = new Vector2(40f, 28f);

        c.Name = NewText("Name", c.Root, 44f, Color.white, TextAlignmentOptions.MidlineLeft);
        c.Name.rectTransform.anchorMin = c.Name.rectTransform.anchorMax = c.Name.rectTransform.pivot = new Vector2(0f, 0.5f);
        c.Name.rectTransform.sizeDelta = new Vector2(480f, 60f);
        c.Name.enableAutoSizing = true;
        c.Name.fontSizeMin = 28f;
        c.Name.fontSizeMax = 44f;
        c.Name.rectTransform.anchoredPosition = new Vector2(38f, -14f);

        c.Root.gameObject.SetActive(false);
        return c;
    }

    // ───────── 생성 도우미 ─────────

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    // 코드로 붙이는 Graphic은 CanvasRenderer가 자동으로 따라오지 않을 수 있어 먼저 붙인다
    private static GameObject NewGraphicRect(string name, Transform parent)
    {
        var go = NewRect(name, parent).gameObject;
        go.AddComponent<CanvasRenderer>();
        return go;
    }

    private static void Stretch(RectTransform rt, float padX = 0f, float padY = 0f)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padX, padY); rt.offsetMax = new Vector2(-padX, -padY);
    }

    private static Image NewImage(string name, Transform parent, Color color)
    {
        var img = NewGraphicRect(name, parent).AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static UISkewQuad NewSkew(string name, Transform parent, Color color, float skew)
    {
        var q = NewGraphicRect(name, parent).AddComponent<UISkewQuad>();
        q.color = color;
        q.Skew = skew;
        q.raycastTarget = false;
        return q;
    }

    private TextMeshProUGUI NewText(string name, Transform parent, float size, Color color, TextAlignmentOptions align)
    {
        var t = NewGraphicRect(name, parent).AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        return t;
    }
}
