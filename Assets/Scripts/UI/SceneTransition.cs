using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 씬 전환 막 (명일방주식). SceneTransition.Load("씬 이름")만 부르면 된다. 귀환: Load(씬, "귀환", "기지로 복귀합니다", "RETURN  //  BASE")
//   닫힘: 위 막(오른쪽에서)·아래 막(왼쪽에서)이 들어와 맞물리고, 이음새 선이 그어진 뒤 제목 틀·엠블럼·글자·로딩 칩이 나타난다
//   로딩: 이음새 청록 막대가 진행률만큼 차오르고(선두 표식이 따라감), 엠블럼이 천천히 돌고, 로딩 칩 점이 차례로 깜빡인다
//   열림: 새 씬이 켜진 뒤 글자가 빠지고 막이 반대 방향으로 빠지며 흰 섬광
// 조각: Resources/SceneTransition/*.png (원본 ArtDirection/SceneTransition/Extracted, 좌표는 그 layout.json — 1920×1080 왼쪽 위 기준).
// 조각이 없으면 단색 도형으로 대신 그린다. 씬을 넘어가야 하므로 스스로 만든 DontDestroyOnLoad 오버레이 캔버스에 그리고 끝나면 사라진다.
public class SceneTransition : MonoBehaviour
{
    private const float W = 1920f, H = 1080f;
    private const float CurtainW = 2320f, CurtainH = 548f;
    private static readonly Color Ink = new(0.035f, 0.04f, 0.05f, 1f);
    private static readonly Color Accent = new(0.05f, 0.72f, 0.95f, 1f);

    public static bool IsTransitioning { get; private set; }

    private RectTransform _root, _top, _bottom, _seam, _seamFill, _seamHead, _emblem;
    private CanvasGroup _textCg, _chipCg, _seamCg;
    private TextMeshProUGUI _title;
    private Image[] _dots;
    private Image _flash;
    private const float MinHold = 0.9f;

    public static void Load(string sceneName, string title = "출정", string sub = "작전 지역으로 이동합니다", string eng = "OPERATION  //  DEPLOY")
    {
        if (IsTransitioning || string.IsNullOrWhiteSpace(sceneName)) return;
        var go = new GameObject("[SceneTransition]");
        DontDestroyOnLoad(go);
        var t = go.AddComponent<SceneTransition>();
        t.Build(title, sub, eng);
        t.StartCoroutine(t.Run(sceneName));
    }

    private IEnumerator Run(string sceneName)
    {
        IsTransitioning = true;
        Time.timeScale = 1f;

        // 1) 닫힘
        _top.anchoredPosition = new Vector2(-200f + W + 300f, 0f);
        _bottom.anchoredPosition = new Vector2(-200f - W - 300f, -532f);
        _seam.localScale = new Vector3(0f, 1f, 1f);
        _seamCg.alpha = 1f;
        SetFill(0f);
        _textCg.alpha = 0f;
        _chipCg.alpha = 0f;
        _title.characterSpacing = 60f;
        _emblem.localScale = Vector3.one * 1.4f;
        var close = DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
            .Append(_top.DOAnchorPosX(-200f, 0.42f).SetEase(Ease.OutCubic))
            .Insert(0.06f, _bottom.DOAnchorPosX(-200f, 0.42f).SetEase(Ease.OutCubic))
            .Insert(0.3f, _seam.DOScaleX(1f, 0.3f).SetEase(Ease.OutCubic))
            .Insert(0.38f, _textCg.DOFade(1f, 0.22f))
            .Insert(0.38f, _emblem.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
            .Insert(0.38f, DOTween.To(() => _title.characterSpacing, v => _title.characterSpacing = v, 12f, 0.5f).SetEase(Ease.OutCubic))
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
            .Insert(0.1f, _top.DOAnchorPosX(-200f - W - 300f, 0.5f).SetEase(Ease.InCubic))
            .Insert(0.14f, _bottom.DOAnchorPosX(-200f + W + 300f, 0.5f).SetEase(Ease.InCubic))
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
        if (_seamHead != null)
        {
            _seamHead.anchoredPosition = new Vector2(-200f + CurtainW * t - 24f, -528f);
            _seamHead.gameObject.SetActive(t > 0.01f && t < 0.999f);
        }
    }

    // ───────── 생성 ─────────

    private void Build(string title, string sub, string eng)
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

        _top = Curtain("Curtain_Upper", 0f);
        _bottom = Curtain("Curtain_Lower", 532f);

        // 이음새: 바탕 → 채움(왼쪽 기준 가로 스케일) → 선두 표식
        _seam = Place("Seam", _root, -200f, 528f, CurtainW, 24f);
        _seam.pivot = new Vector2(0.5f, 0.5f);
        _seam.anchoredPosition = new Vector2(-200f + CurtainW * 0.5f, -540f);
        _seamCg = _seam.gameObject.AddComponent<CanvasGroup>();
        SpriteOr("Seam_Base", _seam, 0f, 0f, CurtainW, 24f, new Color(Accent.r * 0.3f, Accent.g * 0.3f, Accent.b * 0.3f, 1f));
        var fill = SpriteOr("Seam_Fill", _root, -200f, 537f, CurtainW, 6f, Accent);
        _seamFill = fill.rectTransform;
        _seamFill.pivot = new Vector2(0f, 1f);
        _seamFill.anchoredPosition = new Vector2(-200f, -537f);
        var head = LoadSprite("Seam_Head");
        if (head != null) _seamHead = SpriteOr("Seam_Head", _root, 0f, 528f, 48f, 24f, Accent).rectTransform;

        // 가운데 글자 무리 (제목 틀·엠블럼·글자). 페이드·밀림을 한 번에
        var text = Place("TitleGroup", _root, 0f, 0f, W, H);
        _textCg = text.gameObject.AddComponent<CanvasGroup>();
        SpriteOr("TitleFrame", text, 360f, 390f, 1200f, 300f, Color.clear);
        var emblem = SpriteOr("Emblem", text, 480f, 370f, 160f, 160f, Color.clear);
        _emblem = emblem.rectTransform;
        _emblem.pivot = new Vector2(0.5f, 0.5f);
        _emblem.anchoredPosition = new Vector2(560f, -450f);
        NewText("Eng", text, eng, 20f, Accent, 724f, 394f, 700f, 30f).characterSpacing = 18f;
        _title = NewText("Title", text, title, 88f, Color.white, 720f, 430f, 800f, 100f);
        NewText("Sub", text, sub, 22f, new Color(0.72f, 0.75f, 0.78f, 1f), 726f, 574f, 800f, 32f).characterSpacing = 6f;

        // 우하단 로딩 칩 + 깜빡이는 점 3개 (칩 그림의 점 위에 흰 점을 겹친다)
        var chip = Place("LoadingChip", _root, 1600f, 1000f, 280f, 44f);
        _chipCg = chip.gameObject.AddComponent<CanvasGroup>();
        SpriteOr("LoadingChip", chip, 0f, 0f, 280f, 44f, Ink);
        _dots = new Image[3];
        float[] dotX = { 23f, 42.5f, 61.5f };
        for (int i = 0; i < 3; i++)
        {
            var d = NewGraphic<Image>("Dot" + i, chip);
            Place(d.rectTransform, dotX[i] - 4f, 18f, 8f, 8f);
            d.sprite = Circle;
            d.color = new Color(1f, 1f, 1f, 0f);
            _dots[i] = d;
        }
        NewText("Label", chip, "NOW LOADING", 14f, Color.white, 82f, 0f, 190f, 44f).characterSpacing = 8f;

        _flash = NewGraphic<Image>("Flash", transform);
        var frt = _flash.rectTransform;
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one; frt.offsetMin = frt.offsetMax = Vector2.zero;
        _flash.color = new Color(1f, 1f, 1f, 0f);
    }

    // 막: 그림이 있으면 그림, 없으면 사선 단색 도형 (위 막 왼쪽 끝 / 아래 막 오른쪽 끝이 사선)
    private RectTransform Curtain(string name, float y)
    {
        var sprite = LoadSprite(name);
        if (sprite != null) return SpriteOr(name, _root, -200f, y, CurtainW, CurtainH, Ink).rectTransform;
        var q = NewGraphic<UISkewQuad>(name, _root);
        q.color = Ink;
        q.Skew = 70f;
        Place(q.rectTransform, -200f, y, CurtainW, CurtainH);
        return q.rectTransform;
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
