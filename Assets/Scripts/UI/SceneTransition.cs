using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 씬 전환 막 (로비 종이 문법). SceneTransition.Load("씬 이름")만 부르면 된다. 귀환: Load(씬, "귀환", "기지로 복귀합니다", "RETURN  //  BASE")
//   닫힘: 위 막(오른쪽에서)·아래 막(왼쪽에서)이 들어와 맞물리고, 이음새 선이 그어진 뒤 제목 틀·엠블럼·글자·로딩 칩이 나타난다
//         막의 앞장서는 끝(위 막 왼쪽 / 아래 막 오른쪽)은 먹색 사선 띠라 들어오는 동안 < 모양으로 보인다
//   로딩: 이음새 청록 막대가 진행률만큼 차오르고(선두 표식이 따라감), 엠블럼이 천천히 돌고, 로딩 칩 점이 차례로 깜빡인다
//   열림: 새 씬이 켜진 뒤 글자가 빠지고 막이 반대 방향으로 빠지며 흰 섬광
// 조각: Resources/SceneTransition/*.png (원본 ArtDirection/SceneTransitionMockup/Extracted, 좌표는 그 layout.json — 1920×1080 왼쪽 위 기준).
//   2배 해상도(PPU 200) 조각이다. 막 한 장(2640×548)은 텍스처 한도 때문에 Lead / Body / Tail 세 조각을 한 부모 아래 붙여 움직인다.
// 조각이 없으면 단색 도형으로 대신 그린다. 씬을 넘어가야 하므로 스스로 만든 DontDestroyOnLoad 오버레이 캔버스에 그리고 끝나면 사라진다.
public class SceneTransition : MonoBehaviour
{
    private const float W = 1920f, H = 1080f;
    private const float CurtainW = 2640f, CurtainH = 548f;
    private const float LeadW = 920f, BodyW = 1120f, TailW = 600f;
    private const float UpperClosedX = -520f, LowerClosedX = -200f; // 닫혔을 때 앞장서는 사선 띠(390)는 화면 밖에 숨는다
    private static readonly Color Ink = new(0.086f, 0.094f, 0.106f, 1f);     // #16181B
    private static readonly Color Paper = new(0.933f, 0.941f, 0.949f, 1f);   // #EEF0F2
    private static readonly Color Muted = new(0.447f, 0.475f, 0.498f, 1f);   // #72797F
    private static readonly Color Faint = new(0.576f, 0.6f, 0.624f, 1f);     // #93999F
    private static readonly Color Accent = new(0.05f, 0.72f, 0.95f, 1f);

    public static bool IsTransitioning { get; private set; }

    private RectTransform _root, _top, _bottom, _seam, _seamFill, _seamHead, _emblem;
    private CanvasGroup _textCg, _chipCg, _seamCg;
    private TextMeshProUGUI _title;
    private Image[] _dots;
    private Image _flash;
    private const float MinHold = 0.9f;

    // tag: 제목 틀 왼쪽 위 작은 먹색 칸 글자 (훈련은 "SIM"). 비우면 칸을 숨긴다
    public static void Load(string sceneName, string title = "출정", string sub = "작전 지역으로 이동합니다", string eng = "OPERATION  //  DEPLOY", string tag = null)
    {
        if (IsTransitioning || string.IsNullOrWhiteSpace(sceneName)) return;
        var go = new GameObject("[SceneTransition]");
        DontDestroyOnLoad(go);
        var t = go.AddComponent<SceneTransition>();
        t.Build(title, sub, eng, tag);
        t.StartCoroutine(t.Run(sceneName));
    }

    private IEnumerator Run(string sceneName)
    {
        IsTransitioning = true;
        Time.timeScale = 1f;

        // 1) 닫힘
        _top.anchoredPosition = new Vector2(W + 100f, 0f);
        _bottom.anchoredPosition = new Vector2(-CurtainW - 100f, -532f);
        _seam.localScale = new Vector3(0f, 1f, 1f);
        _seamCg.alpha = 1f;
        SetFill(0f);
        _textCg.alpha = 0f;
        _chipCg.alpha = 0f;
        _title.characterSpacing = 60f;
        _emblem.localScale = Vector3.one * 1.4f;
        var close = DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
            .Append(_top.DOAnchorPosX(UpperClosedX, 0.42f).SetEase(Ease.OutCubic))
            .Insert(0.06f, _bottom.DOAnchorPosX(LowerClosedX, 0.42f).SetEase(Ease.OutCubic))
            .Insert(0.3f, _seam.DOScaleX(1f, 0.3f).SetEase(Ease.OutCubic))
            .Insert(0.38f, _textCg.DOFade(1f, 0.22f))
            .Insert(0.38f, _emblem.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
            .Insert(0.38f, DOTween.To(() => _title.characterSpacing, v => _title.characterSpacing = v, 4f, 0.5f).SetEase(Ease.OutCubic))
            .Insert(0.5f, _chipCg.DOFade(1f, 0.2f));
        yield return close.WaitForCompletion();

        // 로딩 중 반복 연출: 엠블럼 회전, 로딩 칩 점 차례로 깜빡임
        _emblem.DORotate(new Vector3(0f, 0f, -360f), 6f, RotateMode.FastBeyond360)
               .SetEase(Ease.Linear).SetLoops(-1).SetUpdate(true).SetLink(gameObject);
        for (int i = 0; i < _dots.Length; i++)
            _dots[i].DOFade(1f, 0.25f).SetDelay(i * 0.18f).SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true).SetLink(_dots[i].gameObject);

        // 2) 로딩 — 막이 완전히 닫힌 뒤에 불러 이전 씬이 사라지는 순간을 가린다
        float shownAt = Time.unscaledTime;
        var op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null) { yield return Open(); yield break; }
        op.allowSceneActivation = false;
        float shown = 0f;
        while (op.progress < 0.9f || Time.unscaledTime - shownAt < MinHold)
        {
            float target = Mathf.Min(op.progress / 0.9f, (Time.unscaledTime - shownAt) / MinHold);
            shown = Mathf.MoveTowards(shown, target, Time.unscaledDeltaTime * 2.5f);
            SetFill(shown);
            yield return null;
        }
        SetFill(1f);
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        // 새 씬의 Start(런 초기화·첫 화면 열기)가 한 번 돈 뒤에 연다
        yield return null;
        yield return new WaitForSecondsRealtime(0.15f);
        yield return Open();
    }

    private IEnumerator Open()
    {
        var textRt = (RectTransform)_textCg.transform;
        var open = DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
            .Append(_textCg.DOFade(0f, 0.18f))
            .Join(textRt.DOAnchorPosX(textRt.anchoredPosition.x + 80f, 0.25f).SetEase(Ease.InCubic))
            .Join(_chipCg.DOFade(0f, 0.15f))
            .Insert(0.08f, _seamCg.DOFade(0f, 0.2f))
            .Insert(0.08f, _seamFill.GetComponent<Image>().DOFade(0f, 0.2f))
            .Insert(0.1f, _top.DOAnchorPosX(-CurtainW - 100f, 0.5f).SetEase(Ease.InCubic))
            .Insert(0.14f, _bottom.DOAnchorPosX(W + 100f, 0.5f).SetEase(Ease.InCubic))
            .Insert(0.34f, _flash.DOFade(0.35f, 0.06f))
            .Insert(0.4f, _flash.DOFade(0f, 0.45f));
        if (_seamHead != null) _seamHead.gameObject.SetActive(false);
        yield return open.WaitForCompletion();
        IsTransitioning = false;
        Destroy(gameObject);
    }

    private void SetFill(float t)
    {
        _seamFill.localScale = new Vector3(t, 1f, 1f);
        _seamFill.gameObject.SetActive(t > 0.001f);
        if (_seamHead != null)
        {
            _seamHead.anchoredPosition = new Vector2(W * t, -540f);
            _seamHead.gameObject.SetActive(t > 0.01f && t < 0.999f);
        }
    }

    // ───────── 생성 ─────────

    private void Build(string title, string sub, string eng, string tag)
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(W, H);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        // 1920×1080 판을 화면 가운데에 두고 그 위에 왼쪽 위 기준으로 배치한다
        _root = NewRect("Stage", transform);
        _root.anchorMin = _root.anchorMax = _root.pivot = new Vector2(0.5f, 0.5f);
        _root.sizeDelta = new Vector2(W, H);

        var blocker = NewGraphic<Image>("Blocker", transform); // 입력 차단 (화면 전체, 투명)
        var brt = blocker.rectTransform;
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one; brt.offsetMin = brt.offsetMax = Vector2.zero;
        blocker.color = new Color(0f, 0f, 0f, 0f);
        blocker.raycastTarget = true;
        blocker.transform.SetAsFirstSibling();

        // 막: 위는 Lead(왼쪽 사선)-Body-Tail, 아래는 Tail-Body-Lead(오른쪽 사선). 막에 붙은 작은 글자는 막과 같이 움직인다
        _top = Curtain("CurtainU", UpperClosedX, 0f, false, out var uLead, out var uTail);
        NewText("Brand", uLead, "VERTEX  //  TRANSIT", 15f, Ink, 616f, 40f, 304f, 24f).characterSpacing = 30f;
        NewText("Motto", uLead, "EXPEDITION\nPEOPLE\nA MORE DISTANT TOMORROW", 10f, Faint, 616f, 82f, 302f, 46f).characterSpacing = 20f;
        NewText("No", uTail, "01", 15f, Faint, 286f, 40f, 34f, 22f).alignment = TextAlignmentOptions.Center;

        _bottom = Curtain("CurtainL", LowerClosedX, 532f, true, out var lLead, out var lTail);
        NewText("No", lLead, "02", 15f, Faint, 292f, 352f, 34f, 22f).alignment = TextAlignmentOptions.Center;
        NewText("Motto", lTail, "SHELTER\nPEOPLE\nTOWARD A CLEANER TOMORROW", 10f, Faint, 296f, 424f, 324f, 40f).characterSpacing = 20f;

        // 이음새 (y=540): 레일 → 채움(왼쪽 기준 가로 스케일) → 선두 표식
        _seam = Place("Seam", _root, 0f, 528f, W, 24f);
        _seam.pivot = new Vector2(0.5f, 0.5f);
        _seam.anchoredPosition = new Vector2(W * 0.5f, -540f);
        _seamCg = _seam.gameObject.AddComponent<CanvasGroup>();
        SpriteOr("Seam_Base", _seam, 0f, 0f, W, 24f, Ink);
        var fill = SpriteOr("Seam_Fill", _root, 0f, 537f, W, 6f, Accent);
        _seamFill = fill.rectTransform;
        _seamFill.pivot = new Vector2(0f, 1f);
        _seamFill.anchoredPosition = new Vector2(0f, -537f);
        if (LoadSprite("Seam_Head") != null)
        {
            _seamHead = SpriteOr("Seam_Head", _root, 0f, 524f, 32f, 32f, Accent).rectTransform;
            _seamHead.pivot = new Vector2(0.5f, 0.5f);
        }

        // 가운데 글자 무리 (제목 틀·엠블럼·글자). 페이드·밀림을 한 번에
        var text = Place("TitleGroup", _root, 0f, 0f, W, H);
        _textCg = text.gameObject.AddComponent<CanvasGroup>();
        SpriteOr("TitleFrame", text, 468f, 306f, 984f, 364f, Color.clear);
        var emblem = SpriteOr("Emblem", text, 604f, 340f, 180f, 180f, Color.clear);
        _emblem = emblem.rectTransform;
        _emblem.pivot = new Vector2(0.5f, 0.5f);
        _emblem.anchoredPosition = new Vector2(694f, -430f);
        var engText = NewText("Eng", text, eng, 20f, Accent, 764f, 328f, 500f, 26f);
        engText.characterSpacing = 30f;
        engText.alignment = TextAlignmentOptions.Center;
        _title = NewText("Title", text, title, 124f, Ink, 745f, 368f, 500f, 136f);
        _title.fontStyle = FontStyles.Bold;
        _title.alignment = TextAlignmentOptions.Center;
        var subText = NewText("Sub", text, sub, 30f, Muted, 697f, 574f, 600f, 42f);
        subText.characterSpacing = 4f;
        subText.alignment = TextAlignmentOptions.Center;
        if (!string.IsNullOrEmpty(tag))
        {
            var tagImg = SpriteOr("Tag_Sim", text, 474f, 274f, 150f, 34f, Ink);
            NewText("Label", tagImg.transform, tag, 20f, Color.white, 8f, 3f, 116f, 26f).alignment = TextAlignmentOptions.Center;
        }

        // 우하단 로딩 칩 + 깜빡이는 점 3개
        var chip = Place("LoadingChip", _root, 1432f, 946f, 364f, 48f);
        _chipCg = chip.gameObject.AddComponent<CanvasGroup>();
        SpriteOr("LoadingChip", chip, 0f, 0f, 364f, 48f, Ink);
        _dots = new Image[3];
        var dotSprite = LoadSprite("Chip_Dot");
        float[] dotX = { 254f, 276f, 298f };
        for (int i = 0; i < 3; i++)
        {
            var d = NewGraphic<Image>("Dot" + i, chip);
            Place(d.rectTransform, dotX[i] - 4f, 20f, 8f, 8f);
            d.sprite = dotSprite != null ? dotSprite : Circle;
            d.color = new Color(1f, 1f, 1f, 0f);
            _dots[i] = d;
        }
        NewText("Label", chip, "NOW LOADING", 16f, Color.white, 68f, 13f, 170f, 22f).characterSpacing = 20f;

        _flash = NewGraphic<Image>("Flash", transform);
        var frt = _flash.rectTransform;
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one; frt.offsetMin = frt.offsetMax = Vector2.zero;
        _flash.color = new Color(1f, 1f, 1f, 0f);
    }

    // 막 부모(2640×548) + 조각 세 개. 그림이 없으면 사선 종이색 도형 하나로 대신한다
    private RectTransform Curtain(string name, float x, float y, bool leadOnRight, out Transform lead, out Transform tail)
    {
        var rt = Place(name, _root, x, y, CurtainW, CurtainH);
        if (LoadSprite(name + "_Lead") == null)
        {
            var q = NewGraphic<UISkewQuad>(name + "_Fallback", rt);
            q.color = Paper;
            q.Skew = 70f;
            Place(q.rectTransform, 0f, 0f, CurtainW, CurtainH);
            lead = Place(name + "_Lead", rt, leadOnRight ? TailW + BodyW : 0f, 0f, LeadW, CurtainH);
            tail = Place(name + "_Tail", rt, leadOnRight ? 0f : LeadW + BodyW, 0f, TailW, CurtainH);
            return rt;
        }
        float leadX = leadOnRight ? TailW + BodyW : 0f;
        float tailX = leadOnRight ? 0f : LeadW + BodyW;
        float bodyX = leadOnRight ? TailW : LeadW;
        lead = SpriteOr(name + "_Lead", rt, leadX, 0f, LeadW, CurtainH, Paper).transform;
        SpriteOr(name + "_Body", rt, bodyX, 0f, BodyW, CurtainH, Paper);
        tail = SpriteOr(name + "_Tail", rt, tailX, 0f, TailW, CurtainH, Paper).transform;
        return rt;
    }

    private static Sprite LoadSprite(string name) => Resources.Load<Sprite>("SceneTransition/" + name);

    private static Sprite _circle;
    private static Sprite Circle
    {
        get
        {
            if (_circle != null) return _circle;
            const int n = 32;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n * 0.5f, n * 0.5f));
                px[y * n + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(n * 0.5f - d) * 255));
            }
            tex.SetPixels32(px);
            tex.Apply();
            _circle = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
            return _circle;
        }
    }

    // 그림 조각을 놓는다. 그림이 없으면 fallback 색의 사각형(투명이면 안 보임)
    private Image SpriteOr(string name, Transform parent, float x, float y, float w, float h, Color fallback)
    {
        var img = NewGraphic<Image>(name, parent);
        Place(img.rectTransform, x, y, w, h);
        var sprite = LoadSprite(name);
        img.sprite = sprite;
        img.color = sprite != null ? Color.white : fallback;
        return img;
    }

    private static RectTransform Place(string name, Transform parent, float x, float y, float w, float h)
    {
        var rt = NewRect(name, parent);
        Place(rt, x, y, w, h);
        return rt;
    }

    private static void Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    private static T NewGraphic<T>(string name, Transform parent) where T : Graphic
    {
        var rt = NewRect(name, parent);
        rt.gameObject.AddComponent<CanvasRenderer>();
        var g = rt.gameObject.AddComponent<T>();
        g.raycastTarget = false;
        return g;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Color color,
                                           float x, float y, float w, float h)
    {
        var t = NewGraphic<TextMeshProUGUI>(name, parent);
        if (TMP_Settings.defaultFontAsset != null) t.font = TMP_Settings.defaultFontAsset;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.MidlineLeft;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        Place(t.rectTransform, x, y, w, h);
        return t;
    }
}
