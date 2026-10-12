using System;
using Coffee.UIEffects;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 덱의 카드를 강화·제거할 때 화면 가운데에 카드를 크게 띄워 보여주는 연출.
// 휴식 노드 강화, 상점 카드 제거, 축복 노드 정화/연마가 모두 이 연출을 쓴다.
//   - 제거: 카드를 띄운 뒤 불타 없어진다 (UIEffect Burn)
//   - 강화: 망치로 두 번 내리친다(튀기 + 흔들림 + 섬광) → 강화된 모습으로 바뀌고 빛이 쓸고 지나간 뒤 DECK으로 날아간다
//   - 변화: 카드가 떨리며 흰 빛으로 차오르고, 완전히 덮인 순간 같은 카드의 모습이 새 카드로 바뀐 뒤 빛이 걷히며 맺힌다 (이벤트 카드 변화)
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
    [SerializeField] private AudioClip transmogrifyClip;      // 카드 변화 순간 (비우면 강화 완료음 사용)

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

    // apply: 실제 변화(덱 교체 후 새 카드 반환). 카드가 빛에 완전히 덮인 순간 실행되고, 돌려준 카드로 모습이 바뀐다.
    public static void Transmogrify(CardData card, Func<CardData> apply, Action onDone = null)
    {
        if (Instance == null || card == null) { apply?.Invoke(); onDone?.Invoke(); return; }
        Instance.PlayTransmogrify(card, apply, onDone);
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

    // 빛 속에서 재구성 — 카드가 떨리며 흰 빛으로 차오르고, 완전히 덮인 순간 같은 카드 오브젝트의 모습을
    // 새 카드로 바꾼 뒤 빛이 걷히며 새 아트가 디졸브로 맺힌다. 사라졌다 새로 생기는 게 아니라
    // '같은 카드가 변하는' 느낌을 주려고 뷰는 처음부터 끝까지 하나만 쓴다.
    // 쓰는 셰이더 변형은 COLOR_FILTER(흰 실루엣)·TRANSITION_DISSOLVE(아트)·TRANSITION_SHINY(빛 스윕)로 모두 등록돼 있다.
    private void PlayTransmogrify(CardData card, Func<CardData> apply, Action onDone)
    {
        const float charge = 0.6f;   // 빛이 차오르는 시간
        const float reveal = 0.6f;   // 새 모습이 맺히는 시간

        var view = Begin(card);
        var rt = (RectTransform)view.transform;
        var frame = view.transform.childCount > 0 ? view.transform.GetChild(0).GetComponent<Graphic>() : null;
        // 프레임 모양 그대로의 흰 실루엣. 아트·글자가 모두 프레임의 자식이라 이것 하나로 카드 전체가 덮인다.
        var glow = CreateFlash(view);

        var seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        seq.Append(rt.DOScale(cardScale, 0.3f).SetEase(Ease.OutBack));

        // 응축: 빛이 차오르고 세로로 살짝 늘어나며, 떨림이 점점 강해진다
        float chargeStart = seq.Duration();
        seq.Append(glow.DOFade(1f, charge).SetEase(Ease.InQuad));
        seq.Join(rt.DOScale(new Vector3(cardScale * 0.94f, cardScale * 1.05f, cardScale), charge).SetEase(Ease.InQuad));
        seq.Insert(chargeStart, rt.DOShakeAnchorPos(charge * 0.5f, 4f, 30, 90f, false, true));
        seq.Insert(chargeStart + charge * 0.5f, rt.DOShakeAnchorPos(charge * 0.5f, 11f, 40, 90f, false, true));

        // 변화의 순간: 완전히 흰 실루엣일 때 데이터만 새 카드로 바꾼다(같은 뷰)
        seq.AppendCallback(() =>
        {
            var newCard = apply?.Invoke();
            if (newCard != null)
            {
                view.SetCard(newCard);
                DisableInteraction(view);
                // 실루엣도 새 카드의 프레임 모양으로 바꾼다 — 프레임 모양이 다른 카드로 바뀌면
                // 옛 모양의 빛이 새 카드 밖으로 삐져나온 채 걷히기 때문(완전히 흰 상태라 바뀌는 순간은 보이지 않는다)
                if (frame is Image frameImage && frameImage.sprite != null)
                {
                    glow.sprite = frameImage.sprite;
                    glow.preserveAspect = frameImage.preserveAspect;
                }
                // 새 아트·글자를 숨겨 두고 빛이 걷히면서 맺히게 한다. 프레임과 빛 자체는 제외.
                UIDissolve.In(view.gameObject, burnNoise, reveal, UIDissolve.DefaultEdge, new[] { frame, glow })
                          .SetUpdate(true);
            }
            Play(transmogrifyClip != null ? transmogrifyClip : upgradeDoneClip);
        });

        // 재구성: 원래 비율로 돌아오며 튕기고, 빛이 걷힌다
        seq.Append(rt.DOScale(cardScale, 0.1f).SetEase(Ease.OutQuad));
        seq.Append(rt.DOPunchScale(Vector3.one * cardScale * 0.08f, 0.3f, 8, 0.6f));
        seq.Join(glow.DOFade(0f, reveal).SetEase(Ease.OutQuad));
        seq.AppendInterval(0.1f);

        // 빛이 한 번 쓸고 지나간 뒤 DECK으로 (강화와 동일)
        seq.AppendCallback(() => Shine(view, 0.55f));
        seq.AppendInterval(0.55f);
        seq.AppendInterval(0.25f);
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
