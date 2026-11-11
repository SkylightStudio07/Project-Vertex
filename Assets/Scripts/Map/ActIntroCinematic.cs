using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ============================================================
// filename   : ActIntroCinematic.cs
// description: 새 막이 시작될 때 전체 화면으로 재생하는 "막 스택" 모션그래픽.
//              막 하나 = 판(두꺼운 슬래브) 하나, 막은 위에서부터 내려온다(1막 = 맨 위). 판이 위부터 딱딱 내려앉고
//              꼭대기에 버텍스 표식이 선다 → 카메라가 축을 따라 이번 막의 판으로 내려가며 지나는 판에 스캔선이 지나간다
//              → 그 판이 막 디오라마로 바뀌며 켜지고 꺾쇠 괄호 · 막 제목이 찍힌다 → 걷힌다.
//              평소에는 판 VisibleActs(3)장. 숨겨진 4막은 그 막에 들어설 때만 맨 아래에 판이 늦게 떠오른다.
//              지나온 막의 판은 그 막의 디오라마로 보인다. 판 사이 딱딱한 그림자 · 괄호 · 라벨 · 판독문 · 해칭은 코드가 그린다.
//              그림: Resources/ActIntro/ (아트 원본 ArtDirection/ActIntroMockup/v2/). 막 디오라마는 ActData.introPlate.
//              1막은 런 시작 때 마키나 축복 전에(GameManager), 2막부터는 새 맵을 열 때(MapUIController) 재생한다.
//              씬 전환 막이 걷힌 뒤에 시작한다. 화면 아무 곳이나 누르면 건너뛴다.
// ============================================================
public class ActIntroCinematic : MonoBehaviour
{
    private const float W = 1920f, H = 1080f;
    public const int VisibleActs = 3;       // 평소 보이는 막 수. 이보다 뒤의 막(숨겨진 막)은 그 막에 들어설 때만 판이 생긴다

    // ── 판 기하 (Resources/ActIntro/Plate_Off 기준, 2배 PNG → 화면 px). 조각을 바꾸면 여기만 고친다 ──
    private const float PlateW = 600f, PlateH = 500f;           // 판 그림 캔버스 (1200×1000)
    private static readonly Vector2 PlatePivot = new(0.5f, 1f - 409.5f / 1000f); // 윗면 마름모 중심 (캔버스 600,409.5)
    private const float TopHalfW = 266f, TopHalfH = 143f;       // 윗면 마름모 반너비 · 반높이 (캔버스 532 · 285.5)
    private const float Thickness = 103f;                       // 판 두께 (캔버스 206)
    private const float MarkerAbove = 240f;                     // 맨 위 판 윗면 중심에서 표식 중심까지 (뒤 꼭짓점 143 + 사이 34 + 표식 반 63)
    private const float MarkerHalf = 63f;

    private static readonly Color Cyan = new(0.118f, 0.627f, 0.902f, 1f);      // #1EA0E6
    private static readonly Color Paper = new(0.92f, 0.93f, 0.94f, 1f);
    private static readonly Color Mist = new(0.30f, 0.37f, 0.46f, 1f);          // 높은 판이 어둠에 묻히는 색
    private static readonly Color LabelCol = new(0.78f, 0.82f, 0.87f, 1f);
    private static readonly Color DimCol = new(0.45f, 0.50f, 0.57f, 1f);
    private static readonly Color Ink = new(0.027f, 0.047f, 0.078f, 1f);      // #070C14

    // 도착 장면: 이번 막 판 윗면 중심의 화면 위치와 배율
    private static readonly Vector2 FocusPos = new(0f, -70f);
    private const float FocusScale = 1.22f;
    private const float BelowPitch = 520f;  // 도착했을 때 이번 막 아래 판과의 간격 (두꺼운 판이라 더 벌린다)
    private const float SpreadPitch = 640f; // 이동하는 동안 이번 막 위의 판들이 벌어져 화면 밖으로 빠져나간다

    private static ActIntroCinematic _current;
    public static bool IsPlaying => _current != null;
    // 마지막으로 연출을 재생한 막 번호. 같은 막의 맵을 처음 열 때 배너 · 패닝을 다시 하지 않게 MapUIController가 본다
    public static int LastPlayedAct { get; private set; }

    private Action _onDone;
    private Sequence _seq;
    private CanvasGroup _group, _stageGroup;
    private RectTransform _stack, _backdrop, _gridLayer, _marker, _axis, _fragments;
    private Image _markerHalo;
    private RectTransform[] _holders;
    private readonly List<Image> _plates = new();
    private readonly List<Image> _glows = new();
    private readonly List<Image> _shadows = new();
    private readonly List<RectTransform> _scans = new();
    private readonly List<TextMeshProUGUI> _labels = new();
    private readonly List<(RectTransform rt, Vector2 home, float phase, float depth)> _frags = new();
    private Image _focusPlate, _focusGlow, _flash, _ring, _pulse;
    private readonly List<RectTransform> _brackets = new();
    private TextMeshProUGUI _actTitle, _actSub, _readout;
    private CanvasGroup _titleGroup;
    private RectTransform _titleHatch;
    private bool _finishing;
    private float _startTime;

    private int _count;     // 판 수
    private int _focus;     // 이번 막 판의 높이 번호 (0 = 맨 아래)
    private float _pitch;   // 처음 판 사이 간격
    private float _above, _below; // 지금 판 사이 간격 (이번 막 위 · 아래)

    // 연출을 시작한다. 그림이 없으면 false를 돌려주고 아무것도 하지 않는다(호출부가 예전 방식으로 대신한다).
    public static bool Play(ActData act, Action onDone = null)
    {
        if (act == null || Load("Plate_Off") == null) return false;
        if (_current != null) _current.Finish(true);
        var go = new GameObject("ActIntroCinematic", typeof(RectTransform));
        var c = go.AddComponent<ActIntroCinematic>();
        _current = c;
        LastPlayedAct = act.actNumber;
        c._onDone = onDone;
        c._count = Mathf.Max(VisibleActs, act.actNumber);
        c._focus = c.LevelOf(Mathf.Clamp(act.actNumber, 1, c._count));
        // 목업처럼 위 판 바닥이 아래 판 뒤쪽을 조금 가리게 겹쳐 쌓는다 (전체 높이 389 중 140 겹침)
        c._pitch = 2f * TopHalfH + Thickness - 140f - (c._count - VisibleActs) * 30f;
        c._above = c._below = c._pitch;
        c.Build(act);
        c.StartCoroutine(c.WaitThenRun());
        return true;
    }

    private void OnDestroy()
    {
        _seq?.Kill();
        if (_current == this) _current = null;
    }

    // 씬 전환 막이 걷힐 때까지는 바탕만 깔아 두고(뒤의 전투 화면이 비치지 않게) 기다린다
    private IEnumerator WaitThenRun()
    {
        _stageGroup.alpha = 0f;
        while (SceneTransition.IsTransitioning) yield return null;
        _stageGroup.alpha = 1f;
        Run();
    }

    // 판은 아래(0)부터 쌓는 높이 번호로 다룬다. 막은 위에서부터 내려온다: 1막 = 맨 위, 마지막 막 = 맨 아래
    private int LevelOf(int actNumber) => _count - actNumber;
    private int ActOf(int level) => _count - level;
    private bool IsHiddenReveal => _count > VisibleActs && _focus == 0; // 숨겨진 막(맨 아래)에 처음 들어서는 순간

    // ───────── 진행 ─────────

    private void Run()
    {
        _startTime = Time.unscaledTime;
        // 처음 장면: 표식까지 탑 전체가 화면 가운데에 오게
        float bottomY = -10f - ((_count - 1) * _pitch + MarkerAbove + MarkerHalf - (TopHalfH + Thickness)) * 0.5f;
        _stack.anchoredPosition = new Vector2(0f, bottomY + _focus * _pitch);
        _stack.localScale = Vector3.one;

        _seq = DOTween.Sequence().SetLink(gameObject).SetUpdate(true);
        _seq.AppendInterval(0.1f);

        // 1. 판이 위(1막)부터 딱딱 내려앉는다. 숨겨진 막의 판은 한 박자 늦게 아래 어둠에서 떠오른다
        float t = 0.1f;
        for (int i = _count - 1; i >= 0; i--)
        {
            bool late = IsHiddenReveal && i == 0;
            var rt = _plates[i].rectTransform;
            var cg = rt.GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            float at = late ? 0.1f + VisibleActs * 0.1f + 0.5f : t;
            _seq.Insert(at, rt.DOAnchorPos(Vector2.zero, late ? 0.6f : 0.32f).From(new Vector2(0f, late ? -220f : 56f)).SetEase(late ? Ease.OutQuart : Ease.OutExpo));
            _seq.InsertCallback(at, () => cg.alpha = 1f);
            Snap(_labels[i], at + 0.08f, 0.6f);
            if (late)
            {
                _seq.Insert(at + 0.5f, _glows[i].DOFade(1f, 0.05f));
                _seq.Insert(at + 0.55f, _glows[i].DOFade(0f, 0.7f));
            }
            else t += 0.1f;
        }
        float stacked = IsHiddenReveal ? 0.1f + VisibleActs * 0.1f + 1.2f : t + 0.25f;

        // 2. 꼭대기 표식: 세로로 솟으며 서고, 가로로 뒤집히며 천천히 돈다(결정이 대칭이라 회전처럼 보인다)
        var markerCg = _marker.GetComponent<CanvasGroup>();
        markerCg.alpha = 0f;
        _marker.localScale = new Vector3(1f, 0.3f, 1f);
        _seq.InsertCallback(stacked, () => markerCg.alpha = 1f);
        _seq.Insert(stacked, _marker.DOScaleY(1f, 0.35f).SetEase(Ease.OutBack));
        _seq.Insert(stacked, _markerHalo.DOFade(0.8f, 0.2f));
        _seq.Insert(stacked + 0.2f, _markerHalo.DOFade(0.4f, 0.6f));
        DOTween.To(() => 0f, v => _marker.localScale = new Vector3(Mathf.Cos(v), _marker.localScale.y, 1f), Mathf.PI * 2f, 2.4f)
            .SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear).SetLink(gameObject).SetUpdate(true);

        // 3. 카메라가 축을 따라 이번 막 판으로 내려간다. 지나온 막 판은 벌어지며 빠져나가고, 지나갈 때 스캔선이 훑는다
        float moveAt = stacked + 0.6f, move = 1.4f;
        _seq.Insert(moveAt, _stack.DOAnchorPos(FocusPos, move).SetEase(Ease.InOutCubic));
        _seq.Insert(moveAt, _stack.DOScale(FocusScale, move).SetEase(Ease.InOutCubic));
        _seq.Insert(moveAt, DOTween.To(() => _above, v => _above = v, SpreadPitch, move).SetEase(Ease.InOutQuad));
        _seq.Insert(moveAt, DOTween.To(() => _below, v => _below = v, BelowPitch, move).SetEase(Ease.InOutQuad));
        _seq.Insert(moveAt, _backdrop.DOAnchorPosY(-10f + (_count - 1 - _focus) * 50f, move).SetEase(Ease.InOutCubic));
        _seq.Insert(moveAt, _gridLayer.DOAnchorPosY((_count - 1 - _focus) * 90f, move).SetEase(Ease.InOutCubic));
        for (int i = _count - 1; i > _focus; i--)
        {
            float k = (_count - 1 - i) / (float)Mathf.Max(1, _count - 1 - _focus);
            float at = moveAt + 0.1f + k * move * 0.5f;
            var label = _labels[i];
            _seq.Insert(at, _glows[i].DOFade(0.9f, 0.04f));
            _seq.Insert(at + 0.05f, _glows[i].DOFade(0f, 0.5f).SetEase(Ease.InQuad));
            _seq.InsertCallback(at, () => { label.color = Cyan; label.alpha = 1f; });
            _seq.InsertCallback(at + 0.4f, () => { label.color = DimCol; label.alpha = 0.7f; });
            var scan = _scans[i];
            _seq.InsertCallback(at - 0.02f, () => scan.gameObject.SetActive(true));
            _seq.Insert(at, scan.DOAnchorPosY(-(TopHalfH + Thickness + 10f), 0.28f).From(new Vector2(0f, TopHalfH + 10f)).SetEase(Ease.Linear));
            _seq.InsertCallback(at + 0.3f, () => scan.gameObject.SetActive(false));
        }
        _pulse.color = new Color(Cyan.r, Cyan.g, Cyan.b, 0f);
        _seq.Insert(moveAt, _pulse.DOFade(1f, 0.2f));
        _seq.Insert(moveAt, _pulse.rectTransform.DOAnchorPosY(0f, move).From(new Vector2(0f, Mathf.Max(1, _count - 1 - _focus) * _pitch + 140f)).SetEase(Ease.InOutCubic));
        _seq.Insert(moveAt + move - 0.15f, _pulse.DOFade(0f, 0.25f));

        // 4. 이번 막 판 점등: 디오라마로 바뀌고 테두리 · 꺾쇠 괄호 · 섬광 · 물결
        float lit = moveAt + move - 0.1f;
        _focusPlate.color = new Color(1f, 1f, 1f, 0f);
        _seq.Insert(lit, _focusPlate.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
        _seq.Insert(lit, _focusGlow.DOFade(1f, 0.15f));
        _seq.Insert(lit + 0.2f, _focusGlow.DOFade(0.3f, 1.0f).SetEase(Ease.OutQuad)); // 켜지는 순간만 밝고 곧 옅게 가라앉는다
        _seq.Insert(lit, _flash.DOFade(1f, 0.05f));
        _seq.Insert(lit, _flash.rectTransform.DOScale(1.25f, 0.5f).From(Vector3.one * 0.4f).SetEase(Ease.OutCubic));
        _seq.Insert(lit + 0.1f, _flash.DOFade(0f, 0.45f));
        _seq.Insert(lit, _ring.DOFade(0.5f, 0.05f));
        _seq.Insert(lit, _ring.rectTransform.DOScale(1.6f, 0.8f).From(Vector3.one * 0.55f).SetEase(Ease.OutCubic));
        _seq.Insert(lit + 0.2f, _ring.DOFade(0f, 0.6f));
        var focusLabel = _labels[_focus];
        _seq.InsertCallback(lit, () => { focusLabel.color = Cyan; focusLabel.alpha = 1f; });
        for (int b = 0; b < _brackets.Count; b++)
        {
            var br = _brackets[b];
            var home = br.anchoredPosition;
            var from = home + new Vector2(Mathf.Sign(home.x) * 40f, Mathf.Sign(home.y) * 24f);
            br.gameObject.SetActive(false);
            _seq.InsertCallback(lit + 0.05f, () => br.gameObject.SetActive(true));
            _seq.Insert(lit + 0.05f, br.DOAnchorPos(home, 0.25f).From(from).SetEase(Ease.OutExpo));
        }

        // 5. 막 제목: 해칭 띠가 세로로 자라고 글자가 타이핑된다
        float title = lit + 0.15f;
        _titleGroup.alpha = 0f;
        _seq.InsertCallback(title, () => _titleGroup.alpha = 1f);
        _seq.Insert(title, _titleHatch.DOSizeDelta(new Vector2(_titleHatch.sizeDelta.x, 128f), 0.3f).From(new Vector2(_titleHatch.sizeDelta.x, 0f)).SetEase(Ease.OutExpo));
        _seq.Insert(title, ((RectTransform)_titleGroup.transform).DOAnchorPosX(0f, 0.4f).From(new Vector2(-24f, 0f)).SetEase(Ease.OutExpo));
        Type(_actTitle, title + 0.1f, 0.45f);
        Type(_actSub, title + 0.3f, 0.5f);

        // 6. 잠깐 머문 뒤 걷힌다. 다음 화면(축복 도입의 암전 등)은 걷히기 시작할 때 연다
        _seq.AppendInterval(1.1f);
        _seq.AppendCallback(InvokeDone);
        _seq.Append(_group.DOFade(0f, 0.45f).SetEase(Ease.InQuad));
        _seq.OnComplete(() => Finish(false));
    }

    // 글자가 두 번 깜빡이며 찍힌다 (모션그래픽식 하드 컷)
    private void Snap(TMP_Text text, float at, float alpha)
    {
        _seq.InsertCallback(at, () => text.alpha = alpha);
        _seq.InsertCallback(at + 0.05f, () => text.alpha = 0f);
        _seq.InsertCallback(at + 0.09f, () => text.alpha = alpha);
    }

    private void Type(TextMeshProUGUI text, float at, float duration)
    {
        text.ForceMeshUpdate();
        int n = text.textInfo.characterCount;
        text.maxVisibleCharacters = 0;
        _seq.Insert(at, DOTween.To(() => 0, v => text.maxVisibleCharacters = v, n, duration).SetEase(Ease.Linear));
    }

    private void Update()
    {
        if (_holders == null) return;
        for (int i = 0; i < _count; i++)
        {
            int d = i - _focus;
            _holders[i].anchoredPosition = new Vector2(0f, d * (d > 0 ? _above : _below));
        }
        float topY = _holders[_count - 1].anchoredPosition.y;
        _marker.anchoredPosition = new Vector2(0f, topY + MarkerAbove + Mathf.Sin(Time.unscaledTime * 2.4f) * 5f);
        _markerHalo.rectTransform.anchoredPosition = _marker.anchoredPosition;
        float bottomY = _holders[0].anchoredPosition.y;
        _axis.anchoredPosition = new Vector2(0f, bottomY - 520f);
        _axis.sizeDelta = new Vector2(1.5f, topY - bottomY + MarkerAbove - MarkerHalf + 520f); // 축은 표식 바로 아래서 끝난다

        float scale = _stack.localScale.x;
        for (int i = 0; i < _count; i++)
        {
            // 판 높이에 따라 어둠에 묻힌다 (화면 위쪽일수록 어둡고, 화면 밖으로 빠져나갈 때는 옅어진다)
            float y = _stack.anchoredPosition.y + (_holders[i].anchoredPosition.y + _plates[i].rectTransform.anchoredPosition.y) * scale;
            float fog = Mathf.Clamp01((y + 120f) / 560f);
            var c = Color.Lerp(Paper, Mist, fog * 0.85f);
            if (i > _focus) c = Color.Lerp(c, Mist, 0.3f); // 지나온 막(위쪽)은 조금 가라앉힌다
            c.a = i == _focus ? 1f : 1f - Mathf.Max(Mathf.Clamp01((y - 330f) / 260f), Mathf.Clamp01((-y - 380f) / 220f)) * 0.85f;
            _plates[i].color = c;

            // 위 판이 떨어뜨리는 딱딱한 그림자: 판이 멀어질수록 옅어지고 작아진다
            if (i < _count - 1)
            {
                float gap = _holders[i + 1].anchoredPosition.y - _holders[i].anchoredPosition.y;
                float near = Mathf.Clamp01(_pitch / Mathf.Max(gap, 1f));
                float upperAlpha = _plates[i + 1].GetComponent<CanvasGroup>().alpha * _plates[i + 1].color.a;
                var sc = _shadows[i].color;
                float lowerAlpha = _plates[i].GetComponent<CanvasGroup>().alpha; // 아래 판이 아직 없으면 그림자도 없다
                sc.a = 0.38f * near * near * upperAlpha * lowerAlpha * c.a;
                _shadows[i].color = sc;
                _shadows[i].rectTransform.localScale = Vector3.one * Mathf.Lerp(0.7f, 0.94f, near);
            }
        }

        // 파편 슬래브가 느리게 떠다닌다 (깊이가 얕을수록 크게 움직인다)
        float tt = Time.unscaledTime;
        foreach (var f in _frags)
            f.rt.anchoredPosition = f.home + new Vector2(Mathf.Sin(tt * 0.5f + f.phase) * 6f, Mathf.Sin(tt * 0.8f + f.phase * 1.7f) * 9f) * f.depth;

        // 판독문: 고도 · 구역 · 경과 시간이 돌아간다
        if (_readout != null)
        {
            float alt = (_stack.anchoredPosition.y - FocusPos.y) * 0.5f + ActOf(_focus) * 100f;
            float el = Time.unscaledTime - _startTime;
            _readout.text = $"ALT  {Mathf.Abs(alt):0000}\nSEC  {ActOf(_focus):00} / {_count:00}\nT+   {el:00.00}";
        }
    }

    private void Skip() => Finish(true);

    private void Finish(bool fast)
    {
        if (_finishing) return;
        _finishing = true;
        _seq?.Kill();
        if (fast && _group != null)
        {
            _group.blocksRaycasts = false;
            _group.DOFade(0f, 0.25f).SetUpdate(true).SetLink(gameObject).OnComplete(() => Destroy(gameObject));
        }
        else Destroy(gameObject);
        InvokeDone();
    }

    private void InvokeDone()
    {
        var done = _onDone;
        _onDone = null;
        if (_group != null) _group.blocksRaycasts = false; // 걷히는 동안 다음 화면을 누를 수 있게
        done?.Invoke();
    }

    // ───────── 생성 ─────────

    private void Build(ActData act)
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900; // 씬 전환 막(1000) 아래, 맵 · HUD · 축복 위
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(W, H);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        _group = gameObject.AddComponent<CanvasGroup>();

        // 바탕 (누르면 건너뛴다). 넓은 화면에서도 덮도록 늘이고, 이동 때 조금만 움직여 깊이를 만든다
        var fill = NewImage("Fill", transform, null, Ink);
        Stretch(fill.rectTransform);
        fill.raycastTarget = true;
        var skip = fill.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener(_ => Skip());
        skip.triggers.Add(entry);

        var back = NewImage("Backdrop", transform, Load("Backdrop"), Color.white);
        _backdrop = back.rectTransform;
        _backdrop.anchorMin = new Vector2(0f, 0.5f);
        _backdrop.anchorMax = new Vector2(1f, 0.5f);
        _backdrop.sizeDelta = new Vector2(0f, H + 240f);
        _backdrop.anchoredPosition = new Vector2(0f, -40f);
        if (back.sprite == null) back.enabled = false;

        // 희미한 아이소메트릭 격자 (바탕보다 조금 더 움직여 깊이를 만든다)
        var grid = NewImage("Grid", transform, Load("Grid"), new Color(1f, 1f, 1f, 0.03f)); // NAI 바탕에도 옅은 격자가 있어 겹치지 않게 약하게
        grid.type = Image.Type.Tiled;
        _gridLayer = grid.rectTransform;
        _gridLayer.anchorMin = new Vector2(0f, 0.5f);
        _gridLayer.anchorMax = new Vector2(1f, 0.5f);
        _gridLayer.sizeDelta = new Vector2(0f, H + 400f);
        if (grid.sprite == null) grid.enabled = false;

        // 판 · 글자 묶음 (씬 전환 막이 걷히기 전엔 숨긴다)
        var stage = NewRect("Stage", transform);
        Stretch(stage);
        _stageGroup = stage.gameObject.AddComponent<CanvasGroup>();

        // 화면 왼쪽 가장자리의 사선 해칭 띠
        HatchBand(stage, 44f, 0f, 22f, H, 0.16f);

        // 파편 슬래브 (조각이 있으면): 탑 주위에 흩어져 뒤에서 느리게 떠다닌다
        _fragments = NewRect("Fragments", stage);
        _fragments.anchorMin = _fragments.anchorMax = new Vector2(0.5f, 0.5f);
        _fragments.sizeDelta = Vector2.zero;
        var fragSprites = new List<Sprite>();
        foreach (var n in new[] { "Fragment_A", "Fragment_B", "Fragment_C" }) { var s = Load(n); if (s != null) fragSprites.Add(s); }
        if (fragSprites.Count > 0)
        {
            var rng = new System.Random(act.actNumber * 7919);
            for (int k = 0; k < 7; k++)
            {
                var s = fragSprites[k % fragSprites.Count];
                float depth = Mathf.Lerp(0.35f, 1f, (float)rng.NextDouble());
                var img = NewImage($"Frag_{k}", _fragments, s, Color.Lerp(Mist, Paper, depth * 0.6f));
                float sz = Mathf.Lerp(60f, 170f, depth);
                img.rectTransform.sizeDelta = new Vector2(sz, sz * s.rect.height / s.rect.width);
                var home = new Vector2((k % 2 == 0 ? -1f : 1f) * Mathf.Lerp(380f, 760f, (float)rng.NextDouble()), Mathf.Lerp(-420f, 420f, (float)rng.NextDouble()));
                img.rectTransform.anchoredPosition = home;
                img.color = new Color(img.color.r, img.color.g, img.color.b, Mathf.Lerp(0.25f, 0.9f, depth));
                _frags.Add((img.rectTransform, home, (float)rng.NextDouble() * 6.28f, depth));
            }
        }

        // 스택 (원점 = 이번 막 판 윗면 중심)
        _stack = NewRect("Stack", stage);
        _stack.anchorMin = _stack.anchorMax = new Vector2(0.5f, 0.5f);
        _stack.sizeDelta = Vector2.zero;

        // 축: 판 뒤를 지나는 가는 흰 선 + 축을 타고 내려가는 빛
        var axis = NewImage("Axis", _stack, null, new Color(1f, 1f, 1f, 0.35f));
        _axis = axis.rectTransform;
        _axis.pivot = new Vector2(0.5f, 0f);
        _pulse = NewImage("Pulse", _stack, Load("Flash"), Cyan);
        _pulse.rectTransform.sizeDelta = new Vector2(70f, 70f);

        // 판: 아래 막을 먼저 만들어 먼저 그린다(위 판일수록 카메라에 가까워 아래 판의 뒤쪽을 가린다)
        var off = Load("Plate_Off");
        var summit = Load("Plate_Boss") ?? off;
        var glow = Load("Plate_Glow");
        var shadow = Load("Shadow");
        var scanSprite = Load("Scan");
        _holders = new RectTransform[_count];
        for (int i = 0; i < _count; i++)
        {
            int actNo = ActOf(i);
            var holder = NewRect($"Act_{actNo}", _stack);
            holder.anchorMin = holder.anchorMax = new Vector2(0.5f, 0.5f);
            holder.sizeDelta = Vector2.zero;
            _holders[i] = holder;

            // 지나온 막(위쪽)은 그 막의 디오라마, 맨 아래(마지막 막) 판은 조준선 판, 나머지는 빈 판
            Sprite sprite = i == 0 ? summit : off;
            if (i > _focus)
            {
                var cleared = GameManager.Instance != null ? GameManager.Instance.GetAct(actNo) : null;
                if (cleared != null && cleared.introPlate != null) sprite = cleared.introPlate;
            }
            var img = NewImage("Plate", holder, sprite, Paper);
            PlaceAtPivot(img.rectTransform, PlateW, PlateH, PlatePivot);
            img.gameObject.AddComponent<CanvasGroup>();
            _plates.Add(img);

            var g = NewImage("Glow", img.rectTransform, glow, new Color(1f, 1f, 1f, 0f));
            Stretch(g.rectTransform);
            if (glow == null) g.enabled = false;
            _glows.Add(g);

            // 지나갈 때 판을 훑는 가로 스캔선
            var scan = NewImage("Scan", img.rectTransform, scanSprite, new Color(0.75f, 0.9f, 1f, 0.9f));
            scan.rectTransform.anchorMin = scan.rectTransform.anchorMax = PlatePivot;
            scan.rectTransform.sizeDelta = new Vector2(TopHalfW * 2.2f, 28f);
            scan.gameObject.SetActive(false);
            _scans.Add(scan.rectTransform);

            // 위 판이 윗면에 떨어뜨리는 딱딱한 마름모 그림자 (판 위, 다음 판 아래에 그려진다)
            var sh = NewImage("Shadow", holder, shadow, new Color(0f, 0f, 0f, 0f));
            sh.rectTransform.anchorMin = sh.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            sh.rectTransform.sizeDelta = new Vector2(TopHalfW * 2f, TopHalfH * 2f);
            sh.rectTransform.anchoredPosition = new Vector2(-10f, 6f); // 빛이 왼쪽 위에서 오므로 살짝 왼쪽 위로
            if (shadow == null) sh.enabled = false;
            _shadows.Add(sh);

            var label = NewText($"Label_{actNo}", img.rectTransform, $"[ ACT {actNo:00} ]", 15f, DimCol);
            label.characterSpacing = 16f;
            label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(1f, PlatePivot.y);
            label.rectTransform.pivot = new Vector2(0f, 0.5f);
            label.rectTransform.sizeDelta = new Vector2(200f, 24f);
            label.rectTransform.anchoredPosition = new Vector2(34f, 0f);
            label.alpha = 0f;
            _labels.Add(label);
        }

        // 이번 막 판: 디오라마 · 점등 효과 · 꺾쇠 괄호를 겹친다
        var focusRt = _plates[_focus].rectTransform;
        _focusGlow = NewImage("FocusGlow", focusRt, glow, new Color(1f, 1f, 1f, 0f));
        Stretch(_focusGlow.rectTransform);
        if (glow == null) _focusGlow.enabled = false;
        _ring = NewImage("Ring", focusRt, Load("Ring"), new Color(Cyan.r, Cyan.g, Cyan.b, 0f));
        _ring.rectTransform.anchorMin = _ring.rectTransform.anchorMax = PlatePivot;
        _ring.rectTransform.sizeDelta = new Vector2(TopHalfW * 2f, TopHalfH * 2f);
        _focusPlate = NewImage("Diorama", focusRt, act.introPlate != null ? act.introPlate : Load("Plate_Floor01"), Color.white);
        Stretch(_focusPlate.rectTransform);
        _focusPlate.rectTransform.pivot = PlatePivot;
        if (_focusPlate.sprite == null) _focusPlate.enabled = false;
        _flash = NewImage("Flash", focusRt, Load("Flash"), new Color(1f, 1f, 1f, 0f));
        _flash.rectTransform.anchorMin = _flash.rectTransform.anchorMax = PlatePivot;
        _flash.rectTransform.sizeDelta = new Vector2(360f, 360f);
        foreach (var sx in new[] { -1f, 1f })
            foreach (var sy in new[] { -1f, 1f })
                _brackets.Add(Bracket(focusRt, new Vector2(sx * (TopHalfW + 22f), sy * (TopHalfH + 14f)), sx, sy));
        _labels[_focus].transform.SetAsLastSibling();

        // 꼭대기 표식
        _marker = NewRect("Marker", _stack);
        _marker.anchorMin = _marker.anchorMax = new Vector2(0.5f, 0.5f);
        _marker.sizeDelta = new Vector2(84f, 126f);
        _marker.gameObject.AddComponent<CanvasGroup>();
        _markerHalo = NewImage("Halo", _stack, Load("Halo"), new Color(Cyan.r, Cyan.g, Cyan.b, 0f));
        _markerHalo.rectTransform.sizeDelta = new Vector2(260f, 260f);
        _markerHalo.rectTransform.SetSiblingIndex(_marker.GetSiblingIndex());
        var crystal = NewImage("Crystal", _marker, Load("Marker_Vertex"), Color.white);
        Stretch(crystal.rectTransform);
        Update(); // 첫 프레임부터 판 · 표식 · 축 자리를 맞춘다

        // 글자: 왼쪽 위 시스템 라벨, 오른쪽 위 판독문, 왼쪽 아래 막 제목
        var corner = NewText("System", stage, "VERTEX  /  ASCENT", 17f, LabelCol);
        Place(corner.rectTransform, 108f, 34f, 500f, 26f);
        corner.characterSpacing = 40f;
        Cross(stage, 57f, 57f);
        Ruler(stage, 108f, 74f, 295f);
        _readout = NewText("Readout", stage, "", 13f, DimCol);
        Place(_readout.rectTransform, 1480f, 34f, 360f, 70f);
        _readout.alignment = TextAlignmentOptions.TopRight;
        _readout.characterSpacing = 20f;
        _readout.lineSpacing = 8f;
        Cross(stage, 1863f, 57f);
        Ruler(stage, 1560f, 118f, 280f);

        var titleRoot = NewRect("Title", stage);
        titleRoot.anchorMin = titleRoot.anchorMax = new Vector2(0f, 1f);
        titleRoot.pivot = new Vector2(0f, 1f);
        titleRoot.sizeDelta = new Vector2(W, H);
        _titleGroup = titleRoot.gameObject.AddComponent<CanvasGroup>();
        _titleHatch = HatchBand(titleRoot, 64f, 814f, 14f, 128f, 0.35f);
        var actLabel = NewText("ActLabel", titleRoot, $"ACT {act.actNumber:00}", 20f, Color.white);
        Place(actLabel.rectTransform, 96f, 818f, 300f, 30f);
        actLabel.characterSpacing = 30f;
        var actNumber = NewText("ActNumber", titleRoot, $"{act.actNumber}막", 76f, Color.white);
        Place(actNumber.rectTransform, 92f, 845f, 220f, 86f);
        actNumber.ForceMeshUpdate();
        float numW = actNumber.preferredWidth;
        var slash = NewText("Slash", titleRoot, "/", 46f, new Color(1f, 1f, 1f, 0.6f));
        Place(slash.rectTransform, 92f + numW + 22f, 852f, 40f, 66f);
        float tx = 92f + numW + 76f;
        _actTitle = NewText("ActTitle", titleRoot, act.actName, 34f, Color.white);
        Place(_actTitle.rectTransform, tx, 856f, 900f, 46f);
        _actSub = NewText("ActSub", titleRoot, "FLOOR 01  —  OPERATION START", 15f, LabelCol);
        Place(_actSub.rectTransform, tx, 908f, 700f, 24f);
        _actSub.characterSpacing = 46f;
        Cross(titleRoot, 107f, 947f);
        Ruler(titleRoot, 150f, 946f, Mathf.Max(420f, tx - 150f + 400f));
    }

    // ───────── 도우미 ─────────

    private static Sprite Load(string name) => Resources.Load<Sprite>("ActIntro/" + name);

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
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

    private static TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Color color)
    {
        var rt = NewRect(name, parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) t.font = TMP_Settings.defaultFontAsset;
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

    // 1920×1080 기준 왼쪽 위 좌표 [x, y, w, h]
    private static void Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    private static void PlaceAtPivot(RectTransform rt, float w, float h, Vector2 pivot)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = pivot;
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = Vector2.zero;
    }

    private static void Cross(Transform parent, float cx, float cy)
    {
        var c = new Color(1f, 1f, 1f, 0.75f);
        Place(NewImage("CrossH", parent, null, c).rectTransform, cx - 11f, cy - 0.75f, 22f, 1.5f);
        Place(NewImage("CrossV", parent, null, c).rectTransform, cx - 0.75f, cy - 11f, 1.5f, 22f);
    }

    // 얇은 눈금자: 가로선 + 작은 눈금, 다섯 칸마다 긴 눈금
    private static void Ruler(Transform parent, float x, float y, float w)
    {
        var c = new Color(1f, 1f, 1f, 0.35f);
        Place(NewImage("Ruler", parent, null, c).rectTransform, x, y, w, 1f);
        int n = Mathf.FloorToInt(w / 15f);
        for (int i = 0; i <= n; i++)
        {
            float h = i % 5 == 0 ? 9f : 5f;
            Place(NewImage("Tick", parent, null, c).rectTransform, x + i * 15f, y - h + 1f, 1f, h);
        }
    }

    // 사선 해칭 띠 (왼쪽 위 기준 좌표, 위에서 아래로 자란다)
    private static RectTransform HatchBand(Transform parent, float x, float y, float w, float h, float alpha)
    {
        var img = NewImage("Hatch", parent, Load("Hatch"), new Color(1f, 1f, 1f, alpha));
        img.type = Image.Type.Tiled;
        Place(img.rectTransform, x, y, w, h);
        if (img.sprite == null) img.enabled = false;
        return img.rectTransform;
    }

    // 꺾쇠 괄호 한 귀퉁이: 판 윗면 중심 기준 위치, sx · sy는 어느 쪽으로 열린 괄호인지
    private static RectTransform Bracket(Transform parent, Vector2 pos, float sx, float sy)
    {
        var root = NewRect("Bracket", parent);
        root.anchorMin = root.anchorMax = PlatePivot;
        root.sizeDelta = Vector2.zero;
        root.anchoredPosition = pos;
        var h = NewImage("H", root, null, Cyan);
        h.rectTransform.pivot = new Vector2(sx > 0 ? 1f : 0f, sy > 0 ? 1f : 0f);
        h.rectTransform.anchorMin = h.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        h.rectTransform.sizeDelta = new Vector2(16f, 2f);
        var v = NewImage("V", root, null, Cyan);
        v.rectTransform.pivot = h.rectTransform.pivot;
        v.rectTransform.anchorMin = v.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        v.rectTransform.sizeDelta = new Vector2(2f, 12f);
        return root;
    }
}
