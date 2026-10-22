using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 씬 전환 로딩 화면. SceneTransition.Load("씬 이름")만 부르면 된다.
//   들어감: 로딩 화면이 지금 화면 위로 떠오른다 (회백 다각 면 바탕 · 가운데 VERTEX 엠블럼 · 안내 한 줄 · 청록 진행선 · 왼쪽 아래 FIELD GUIDE 팁)
//   로딩: 엠블럼의 별만 주기적으로 세로축으로 뒤집히고(틀은 고정), 진행선이 차오르며 끝 마름모와 % 숫자가 따라간다
//   나감: 로딩 화면이 덮은 채로 새 씬을 켜고, 로딩 화면이 걷히며 새 화면
// 그림: Resources/SceneTransition (Loading_BG · Emblem_Star · Emblem_Frame · Line_Track · Line_Fill · Line_Head, 2배 해상도).
//   원본 ArtDirection/LoadingMockup/Extracted_Draft, 좌표는 그 layout.json (1920×1080 왼쪽 위). 그림이 없으면 단색으로 대신 그린다.
// 씬을 넘어가야 하므로 스스로 만든 DontDestroyOnLoad 오버레이 캔버스에 그리고 끝나면 사라진다.
public class SceneTransition : MonoBehaviour
{
    private const float W = 1920f, H = 1080f;
    private const float EmblemX = 960f, EmblemY = 380f, EmblemSize = 512f;
    private const float TrackX = 159f, TrackY = 738f, TrackW = 1602f, TrackH = 6f;
    private const float MinHold = 1.1f;
    private static readonly Color Ink = new(0.086f, 0.094f, 0.106f, 1f);       // #16181B
    private static readonly Color Cyan = new(0.051f, 0.722f, 0.949f, 1f);      // #0DB8F2
    private static readonly Color StatusCol = new(0.376f, 0.404f, 0.427f, 1f); // #60676D
    private static readonly Color PercentCol = new(0.271f, 0.298f, 0.325f, 1f);// #454C53
    private static readonly Color LabelCol = new(0.49f, 0.514f, 0.541f, 1f);   // #7D838A
    private static readonly Color TipCol = new(0.333f, 0.365f, 0.396f, 1f);    // #555D65

    // FIELD GUIDE — 로딩마다 하나를 무작위로
    private static readonly string[] Tips =
    {
        "동료의 호감도가 오르면 새로운 기록이 열립니다.",
        "특효 카드는 맞는 속성의 적에게 피해가 1.2배로 들어갑니다.",
        "런이 끝나면 남은 크레딧의 1/10이 탈린으로 바뀝니다.",
        "탈린은 로비 상점에서 파츠와 선물로 바꿀 수 있습니다.",
        "상점의 파츠 재고는 스토리가 진행될 때마다 다시 채워집니다.",
        "의뢰는 한 번에 두 개까지 수주할 수 있습니다.",
        "탄창의 첫 탄과 마지막 탄에는 추가 효과가 붙는 카드가 있습니다.",
        "적의 의도 아이콘이 크게 흔들리면 강한 공격이 옵니다.",
        "막 보스를 처음 쓰러뜨리면 아티팩트와 거점 개발도를 얻습니다.",
        "적을 여러 번 쓰러뜨릴수록 아카이브의 관측 기록이 열립니다.",
        "훈련장에서는 기록에 남지 않는 모의 전투를 할 수 있습니다.",
    };

    public static bool IsTransitioning { get; private set; }

    private CanvasGroup _stage;
    private RectTransform _star, _frame, _head;
    private Image _fill;
    private TextMeshProUGUI _status, _percent;
    private string _statusText;
    private Sequence _flipLoop;

    // title·eng는 예전 막 연출의 큰 제목용이라 지금은 쓰지 않는다. 안내 한 줄은 sub, tag("SIM")는 앞에 붙인다
    public static void Load(string sceneName, string title = "출정", string sub = "작전 지역으로 이동합니다", string eng = "OPERATION  //  DEPLOY", string tag = null)
    {
        if (IsTransitioning || string.IsNullOrWhiteSpace(sceneName)) return;
        var go = new GameObject("[SceneTransition]");
        DontDestroyOnLoad(go);
        var t = go.AddComponent<SceneTransition>();
        t.Build(string.IsNullOrEmpty(tag) ? sub : $"{tag}  ·  {sub}");
        t.StartCoroutine(t.Run(sceneName));
    }

    private IEnumerator Run(string sceneName)
    {
        IsTransitioning = true;
        Time.timeScale = 1f;

        // 1) 로딩 화면이 지금 화면 위로 떠오른다
        _stage.alpha = 0f;
        SetFill(0f);
        _star.localScale = Vector3.one * 0.7f;
        _frame.localScale = Vector3.one * 1.08f;
        var enter = DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
            .Append(_stage.DOFade(1f, 0.3f).SetEase(Ease.OutQuad))
            .Insert(0.06f, _frame.DOScale(1f, 0.45f).SetEase(Ease.OutCubic))
            .Insert(0.1f, _star.DOScale(1f, 0.45f).SetEase(Ease.OutBack));
        StartFlipLoop();
        StartCoroutine(Dots());
        yield return enter.WaitForCompletion(); // 로딩 화면이 다 덮은 뒤에 이전 씬을 내린다

        // 2) 로딩
        float shownAt = Time.unscaledTime;
        var op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null) { yield return Leave(); yield break; }
        op.allowSceneActivation = false;
        float shown = 0f;
        while (op.progress < 0.9f || Time.unscaledTime - shownAt < MinHold)
        {
            float target = Mathf.Min(op.progress / 0.9f, (Time.unscaledTime - shownAt) / MinHold);
            shown = Mathf.MoveTowards(shown, target, Time.unscaledDeltaTime * 2f);
            SetFill(shown);
            yield return null;
        }
        while (shown < 1f) { shown = Mathf.MoveTowards(shown, 1f, Time.unscaledDeltaTime * 3f); SetFill(shown); yield return null; }
        yield return new WaitForSecondsRealtime(0.12f);

        // 3) 로딩 화면이 덮은 채로 새 씬을 켠다 → 새 씬의 Start가 한 번 돈 뒤에 로딩 화면이 걷힌다
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;
        yield return null;
        yield return new WaitForSecondsRealtime(0.1f);
        yield return Leave();
    }

    private IEnumerator Leave()
    {
        yield return _stage.DOFade(0f, 0.4f).SetEase(Ease.InQuad).SetUpdate(true).SetLink(gameObject).WaitForCompletion();
        _flipLoop?.Kill();
        IsTransitioning = false;
        Destroy(gameObject);
    }

    // 별: 잠깐 멈춤 → 세로축으로 뒤집혀 납작해짐 → 다시 펼쳐짐 (틀은 고정)
    private void StartFlipLoop()
    {
        _flipLoop = DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
            .AppendInterval(0.7f)
            .Append(_star.DOScaleX(0f, 0.18f).SetEase(Ease.InQuad))
            .Append(_star.DOScaleX(1f, 0.26f).SetEase(Ease.OutBack))
            .SetLoops(-1);
    }

    // "기지로 복귀합니다" 뒤에 점 . .. ...
    private IEnumerator Dots()
    {
        int n = 0;
        while (true)
        {
            _status.text = _statusText + new string('.', n % 4);
            n++;
            yield return new WaitForSecondsRealtime(0.35f);
        }
    }

    private void SetFill(float t)
    {
        _fill.rectTransform.sizeDelta = new Vector2(TrackW * t, TrackH);
        _fill.enabled = t > 0.002f;
        _head.anchoredPosition = new Vector2(TrackX + TrackW * t, -(TrackY + TrackH * 0.5f));
        _head.gameObject.SetActive(t > 0.002f);
        _percent.text = Mathf.RoundToInt(t * 100f) + "%";
    }

    // ───────── 생성 ─────────

    private void Build(string status)
    {
        _statusText = status;
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(W, H);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        var blocker = NewGraphic<Image>("Blocker", transform); // 입력 차단 (화면 전체, 투명)
        Stretch(blocker.rectTransform);
        blocker.color = new Color(0f, 0f, 0f, 0f);
        blocker.raycastTarget = true;

        var stageRt = NewRect("Stage", transform);
        Stretch(stageRt);
        _stage = stageRt.gameObject.AddComponent<CanvasGroup>();
        var bg = NewGraphic<Image>("Background", stageRt);
        Stretch(bg.rectTransform);
        bg.sprite = LoadSprite("Loading_BG");
        bg.color = bg.sprite != null ? Color.white : new Color(0.933f, 0.941f, 0.949f, 1f);

        // 1920×1080 판을 가운데에 두고 왼쪽 위 기준으로 배치
        var root = NewRect("Board", stageRt);
        root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(W, H);

        // 진행선: 회색 바탕 → 청록 채움 → 끝 마름모
        var track = Sprite("Track", root, "Line_Track", TrackX, TrackY, TrackW, TrackH, new Color(0.75f, 0.76f, 0.78f, 1f));
        Sliced(track);
        _fill = Sprite("Fill", root, "Line_Fill", TrackX, TrackY, 0f, TrackH, Cyan);
        Sliced(_fill);
        var head = Sprite("Head", root, "Line_Head", 0f, 0f, 26f, 26f, Cyan);
        _head = head.rectTransform;
        _head.pivot = new Vector2(0.5f, 0.5f);

        // 엠블럼: 틀(고정) + 별(뒤집힘), 같은 512 칸·같은 중심
        _frame = Centered(Sprite("Frame", root, "Emblem_Frame", 0f, 0f, EmblemSize, EmblemSize, new Color(0, 0, 0, 0)).rectTransform);
        _star = Centered(Sprite("Star", root, "Emblem_Star", 0f, 0f, EmblemSize, EmblemSize, Ink).rectTransform);

        // 글자
        var word = NewText("Wordmark", root, "VERTEX", 30f, Ink, 760f, 540f, 400f, 39f);
        word.alignment = TextAlignmentOptions.Center;
        word.characterSpacing = 73f; // 디자인 자간 22px ÷ 30px
        _status = NewText("Status", root, status, 26f, StatusCol, 560f, 601f, 800f, 38f);
        _status.alignment = TextAlignmentOptions.Center;
        _percent = NewText("Percent", root, "0%", 28f, PercentCol, 1565f, 759f, 200f, 32f);
        _percent.alignment = TextAlignmentOptions.MidlineRight;
        var label = NewText("TipLabel", root, "FIELD GUIDE", 13f, LabelCol, 129f, 949f, 200f, 20f);
        label.characterSpacing = 23f;
        NewText("Tip", root, Tips[Random.Range(0, Tips.Length)], 21f, TipCol, 129f, 978f, 900f, 29f);
        var footer = NewText("Footer", root, "PROJECT VERTEX  /  ARCHIVE", 12f, LabelCol, 1363f, 980f, 400f, 20f);
        footer.alignment = TextAlignmentOptions.MidlineRight;
        footer.characterSpacing = 25f;
    }

    private RectTransform Centered(RectTransform rt)
    {
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(EmblemX, -EmblemY);
        return rt;
    }

    // 2배 해상도 9-slice (가로로 늘림, 양 끝 유지)
    private static void Sliced(Image img)
    {
        if (img.sprite == null) return;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 2f;
    }

    private static Sprite LoadSprite(string name) => Resources.Load<Sprite>("SceneTransition/" + name);

    private static Image Sprite(string name, Transform parent, string sprite, float x, float y, float w, float h, Color fallback)
    {
        var img = NewGraphic<Image>(name, parent);
        Place(img.rectTransform, x, y, w, h);
        img.sprite = LoadSprite(sprite);
        img.color = img.sprite != null ? Color.white : fallback;
        return img;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
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
