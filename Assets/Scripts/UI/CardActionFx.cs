using System;
using Coffee.UIEffects;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 덱의 카드를 강화·제거할 때 화면 가운데에 카드를 크게 띄워 보여주는 연출.
// 휴식 노드 강화, 상점 카드 제거, 축복 노드 정화/연마가 모두 이 연출을 쓴다.
//   - 제거: 카드를 띄운 뒤 불타 없어진다 (UIEffect Burn)
//   - 강화: 망치로 두 번 내리친다(튀기 + 흔들림 + 섬광) → 강화된 모습으로 바뀌고 빛이 쓸고 지나간 뒤 DECK으로 날아간다
// 연출 중에는 화면 전체 입력을 막는다. 씬에 이 컴포넌트가 없으면 Upgrade()/Remove()는 연출 없이 즉시 처리한다.
public class CardActionFx : MonoBehaviour
{
    public static CardActionFx Instance { get; private set; }

    [SerializeField] private CardView cardPrefab;            // 크게 띄울 카드 (목록용 카드 프리팹)
    [SerializeField] private float cardScale = 1.8f;
    [SerializeField] private Texture burnNoise;              // 불타기 노이즈 (비우면 페이드)
    [SerializeField] private Color burnEdge = new(1f, 0.45f, 0.12f, 1f);
    [SerializeField] private RectTransform deckTarget;       // 강화된 카드가 날아갈 곳 (DECK 버튼)
    [SerializeField] private int sortingOrder = 150;         // 덱 목록(99)·카드 상세(100) 위

    [Header("효과음 (선택)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hammerClip;
    [SerializeField] private AudioClip upgradeDoneClip;
    [SerializeField] private AudioClip burnClip;

    private RectTransform _stage;
    private CanvasGroup _dimmer;
    private bool _playing;

    public bool IsPlaying => _playing;

    private void Awake()
    {
        Instance = this;
        BuildStage();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---- 호출부용 정적 진입점: 연출기가 없으면 즉시 처리 ----

    // apply: 실제 강화(카드 데이터 변경). 두 번째 망치질 순간 실행된다.
    public static void Upgrade(CardData card, Action apply, Action onDone = null)
    {
        if (Instance == null || card == null) { apply?.Invoke(); onDone?.Invoke(); return; }
        Instance.PlayUpgrade(card, apply, onDone);
    }

    // apply: 실제 제거(덱에서 빼고 파괴). 카드 모습을 먼저 떠 둔 직후 실행된다.
    public static void Remove(CardData card, Action apply, Action onDone = null)
    {
        if (Instance == null || card == null) { apply?.Invoke(); onDone?.Invoke(); return; }
        Instance.PlayRemove(card, apply, onDone);
    }

    // ---- 연출 ----

    private void PlayRemove(CardData card, Action apply, Action onDone)
    {
        var view = Begin(card);
        apply?.Invoke(); // 카드 인스턴스가 파괴돼도 이미 그려 둔 모습은 남는다

        var rt = (RectTransform)view.transform;
        var seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        seq.Append(rt.DOScale(cardScale, 0.3f).SetEase(Ease.OutBack));
        seq.AppendInterval(0.4f);
        seq.AppendCallback(() => Play(burnClip));
        seq.Append(UIDissolve.Out(view.gameObject, burnNoise, 0.9f, burnEdge, null, TransitionFilter.Burn));
        seq.Join(rt.DOAnchorPosY(rt.anchoredPosition.y + 40f, 0.9f).SetEase(Ease.InQuad));
        seq.OnComplete(() => End(view, onDone));
    }

    private void PlayUpgrade(CardData card, Action apply, Action onDone)
    {
        var view = Begin(card);
        var rt = (RectTransform)view.transform;
        var flash = CreateFlash(view);

        var seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        seq.Append(rt.DOScale(cardScale, 0.3f).SetEase(Ease.OutBack));
        seq.AppendInterval(0.25f);
        AppendHammer(seq, rt, flash, 0.5f);
        seq.AppendInterval(0.2f);
        AppendHammer(seq, rt, flash, 0.9f);
        // 두 번째 망치질에 강화가 적용되고 강화된 모습으로 바뀐다
        seq.InsertCallback(seq.Duration() - 0.3f, () =>
        {
            apply?.Invoke();
            view.SetCard(card);
            DisableInteraction(view);
            Play(upgradeDoneClip);
        });
        // 빛 효과는 쓸 차례에 붙인다 — 시퀀스를 만들 때 붙이면 연출 시작부터 프레임에 효과가 걸려 색이 틀어진다
        seq.AppendCallback(() => Shine(view, 0.55f));
        seq.AppendInterval(0.55f);
        seq.AppendInterval(0.35f);
        if (deckTarget != null)
        {
            seq.Append(rt.DOMove(deckTarget.position, 0.45f).SetEase(Ease.InCubic));
            seq.Join(rt.DOScale(cardScale * 0.12f, 0.45f).SetEase(Ease.InCubic));
            seq.AppendCallback(() => deckTarget.DOPunchScale(Vector3.one * 0.15f, 0.25f, 6, 0.6f).SetUpdate(true));
        }
        else
        {
            seq.Append(rt.DOScale(0f, 0.3f).SetEase(Ease.InBack));
        }
        seq.OnComplete(() => End(view, onDone));
    }

    // 망치 한 번: 튀기 + 흔들림 + 흰 섬광 + 소리
    private void AppendHammer(Sequence seq, RectTransform rt, Image flash, float flashAlpha)
    {
        seq.AppendCallback(() => Play(hammerClip));
        seq.Append(rt.DOPunchScale(Vector3.one * cardScale * 0.08f, 0.3f, 8, 0.6f));
        seq.Join(rt.DOShakeAnchorPos(0.25f, 14f, 30, 90f, false, true));
        seq.Join(DOTween.Sequence()
            .Append(flash.DOFade(flashAlpha, 0.04f))
            .Append(flash.DOFade(0f, 0.3f)));
    }

    // 강화 완료 후 카드 프레임 위로 빛이 한 번 쓸고 지나간다 (UIEffect Shiny)
    private void Shine(CardView view, float duration)
    {
        if (view == null) return;
        var frame = view.transform.childCount > 0 ? view.transform.GetChild(0).GetComponent<Graphic>() : null;
        if (frame == null) return;
        if (!frame.TryGetComponent<UIEffect>(out var fx)) fx = frame.gameObject.AddComponent<UIEffect>();
        fx.transitionFilter = TransitionFilter.Shiny;
        fx.transitionWidth = 0.18f;
        fx.transitionSoftness = 0.6f;
        fx.transitionColorFilter = ColorFilter.Additive;
        fx.transitionColor = new Color(1f, 1f, 1f, 0.9f);
        fx.transitionRate = 0f;
        DOTween.To(() => fx.transitionRate, r => fx.transitionRate = r, 1f, duration)
               .SetEase(Ease.InOutSine).SetUpdate(true).SetLink(frame.gameObject)
               .OnComplete(() => { if (fx != null) Destroy(fx); }); // 다 쓸고 지나가면 효과를 뗀다
    }

    // ---- 무대 ----

    private CardView Begin(CardData card)
    {
        _playing = true;
        gameObject.SetActive(true);
        _dimmer.blocksRaycasts = true;
        _dimmer.DOKill();
        _dimmer.DOFade(1f, 0.2f).SetUpdate(true);

        var view = Instantiate(cardPrefab, _stage);
        view.SetCard(card);
        DisableInteraction(view);
        var rt = (RectTransform)view.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.zero;
        return view;
    }

    private void End(CardView view, Action onDone)
    {
        if (view != null) Destroy(view.gameObject);
        _dimmer.DOKill();
        _dimmer.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() =>
        {
            _dimmer.blocksRaycasts = false;
            _playing = false;
        });
        onDone?.Invoke();
    }

    // 연출용 카드는 호버·우클릭 상세 등 입력에 반응하지 않게 한다
    private static void DisableInteraction(CardView view)
    {
        foreach (var g in view.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;
        foreach (var b in view.GetComponents<MonoBehaviour>())
            if (b != view) b.enabled = false;
    }

    private Image CreateFlash(CardView view)
    {
        var bg = view.transform.childCount > 0 ? view.transform.GetChild(0) as RectTransform : (RectTransform)view.transform;
        var go = new GameObject("HammerFlash", typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(bg, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        // 카드 프레임 모양 그대로의 흰 실루엣으로 번쩍이게: 같은 스프라이트를 쓰고 색을 흰색으로 치환한다.
        // (그냥 흰 Image면 이미지 여백까지 사각형으로 번쩍인다)
        if (bg.TryGetComponent<Image>(out var frame) && frame.sprite != null)
        {
            img.sprite = frame.sprite;
            img.preserveAspect = frame.preserveAspect;
            var fx = go.AddComponent<UIEffect>();
            fx.colorFilter = ColorFilter.Replace;
            fx.color = Color.white;
        }
        img.color = new Color(1f, 1f, 1f, 0f);
        return img;
    }

    // 자체 Canvas(덱 목록·카드 상세 위) + 화면 전체 어둡게 하는 판(입력 차단) + 카드 무대
    private void BuildStage()
    {
        if (!TryGetComponent<Canvas>(out var canvas)) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        if (!TryGetComponent<GraphicRaycaster>(out _)) gameObject.AddComponent<GraphicRaycaster>();

        var dimGo = new GameObject("Dimmer", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        var dimRt = (RectTransform)dimGo.transform;
        dimRt.SetParent(transform, false);
        dimRt.anchorMin = Vector2.zero; dimRt.anchorMax = Vector2.one; dimRt.offsetMin = dimRt.offsetMax = Vector2.zero;
        dimGo.GetComponent<Image>().color = new Color(0.03f, 0.035f, 0.04f, 0.65f);
        _dimmer = dimGo.GetComponent<CanvasGroup>();
        _dimmer.alpha = 0f;
        _dimmer.blocksRaycasts = false;

        var stageGo = new GameObject("Stage", typeof(RectTransform));
        _stage = (RectTransform)stageGo.transform;
        _stage.SetParent(transform, false);
        _stage.anchorMin = _stage.anchorMax = new Vector2(0.5f, 0.5f);
        _stage.sizeDelta = Vector2.zero;
    }

    private void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }
}
