// ============================================================
// filename   : EnemyView.cs
// description   : EnemyInstance를 적 프리팹 UI에 바인딩하는 뷰 컴포넌트.
//             적 프리팹 루트에 부착하고 Inspector에서 각 UI 슬롯을 연결하시오.
// ============================================================

using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyView : MonoBehaviour
{
    // IntentType별 표시 아이콘. EnemyAction.intentType 값과 매칭됨.
    [Serializable]
    public struct IntentSprite
    {
        public IntentType intentType;
        public Sprite sprite;
    }

    [Header("스프라이트")]
    [SerializeField] private Image enemyImage;

    [Header("HP 바")]
    [SerializeField] private Image  hpFill;     // Image Type: Filled (Horizontal)
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("현재 방어도")]
    [SerializeField] private GameObject blockBadge;
    [SerializeField] private TextMeshProUGUI blockText;

    [Header("인텐트")]
    [SerializeField] private Image intentIcon;
    [SerializeField] private GameObject intentField; // 단순 blob 용도
    [SerializeField] private TextMeshProUGUI intentValueText; // 공격 수치 등. DamageEffect 없는 행동이면 숨김
    [SerializeField] private List<IntentSprite> intentSprites;

    [Header("인텐트 부유 애니메이션")]
    [SerializeField] private float bobAmplitude = 6f; // 위아래로 움직이는 폭(px)
    [SerializeField] private float bobSpeed     = 2f; // 초당 진동 속도
    [Tooltip("적 그림 위 끝과 인텐트 아이콘 아래 끝 사이 간격 (이 뷰의 로컬 단위)")]
    [SerializeField] private float intentGap    = 4f;

    [Header("공격 모션 (전진→후퇴)")]
    [SerializeField] private float lungeDistance    = 40f; // 전진 거리(px). 좌우 방향은 Inspector에서 부호로 조정
    [SerializeField] private float lungeOutDuration = 0.2f;
    [SerializeField] private float lungeBackDuration = 0.2f;

    [Header("버프/디버프")]
    [SerializeField] private StatusListView statusList;

    [Header("피격 이펙트")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("방어도 획득 이펙트")]
    // 플레이어/협력자 쪽(PartyView.HandleBlockGained)과 같은 프리팹을 그대로 재사용한다 —
    // "적이 방어도를 올릴 때도 동일한 연출"이 요구사항이라 별도로 만들지 않았다.
    [SerializeField] private GameObject blockGainShieldPrefab;
    [SerializeField] private GameObject blockGainParticlePrefab;

    [Header("사망 연출")]
    // 스프라이트는 디졸브로 부서지고, HP바·인텐트 등 부속 UI는 먼저 페이드아웃된다.
    // 결과창 지연(RewardsView.openDelay, 0.75초) 안에 끝나도록 짧게 잡는다.
    [SerializeField] private Texture deathDissolveNoise;
    [SerializeField, Min(0f)] private float deathDissolveDuration = 0.6f;

    [Header("등장 연출")]
    // 전투 시작 시 오른쪽에서 미끄러져 들어오며 나타난다. EnemyZoneView가 순서대로 지연을 준다.
    [SerializeField] private float enterSlideDistance = 120f;
    [SerializeField, Min(0f)] private float enterDuration = 0.4f;

    private RectTransform _intentIconRect;
    private Vector2 _initialIntentIconPos;
    private Vector2 _intentIconBasePos;

    private RectTransform _intentValueRect;
    private Vector2 _initialIntentValuePos;
    private Vector2 _intentValueBasePos;

    private RectTransform _intentFieldRect;
    private Vector2 _initialIntentFieldPos;
    private Vector2 _intentFieldBasePos;

    // 자동 매핑 기기 사용 시 현재 인텐트 오른쪽에 표시되는 향후 행동 아이콘.
    // 프리팹 직렬화 참조를 늘리지 않도록 현재 인텐트 Image의 시각 속성을 복제해 런타임 생성한다.
    private readonly List<UnityEngine.UI.Image> _lookaheadIcons = new();
    private readonly List<RectTransform> _lookaheadRects = new();
    private readonly List<Vector2> _lookaheadBasePositions = new();

    private Vector3 _enemyImageBaseScale = Vector3.one;
    private RectTransform _rect;

    public EnemyInstance Instance { get; private set; }

    private void Awake()
    {
        if (enemyImage != null)
        {
            _enemyImageBaseScale = enemyImage.rectTransform.localScale;
            if (_enemyImageBaseScale == Vector3.zero) _enemyImageBaseScale = Vector3.one;
        }

        if (intentIcon != null)
        {
            _intentIconRect = intentIcon.rectTransform;
            _initialIntentIconPos = _intentIconRect.anchoredPosition;
            _intentIconBasePos = _initialIntentIconPos;
        }

        if (intentValueText != null)
        {
            _intentValueRect = intentValueText.rectTransform;
            _initialIntentValuePos = _intentValueRect.anchoredPosition;
            _intentValueBasePos = _initialIntentValuePos;
        }

        if (intentField != null)
        {
            _intentFieldRect = intentField.transform as RectTransform;
            if (_intentFieldRect != null)
            {
                _initialIntentFieldPos = _intentFieldRect.anchoredPosition;
                _intentFieldBasePos = _initialIntentFieldPos;
            }
        }

        _rect = transform as RectTransform;
    }

    private void Update()
    {
        if (_intentIconRect == null || !intentIcon.enabled) return;

        // Sin(시간 * 속도) → -1~+1을 반복하는 값. 여기에 진폭을 곱하면 -amplitude~+amplitude(px) 범위의 오프셋이 된다.
        // 예) bobAmplitude=6, bobSpeed=2 → 초당 약 0.3회 진동, 최대 ±6px 위아래로 움직임.
        float offsetY = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;

        // 기준 위치에 오프셋을 더한다. 기준점을 고정해야 오프셋이 누적되지 않는다.
        _intentIconRect.anchoredPosition = _intentIconBasePos + new Vector2(0f, offsetY);
        if (_intentValueRect != null)
            _intentValueRect.anchoredPosition = _intentValueBasePos + new Vector2(0f, offsetY);
        if (_intentFieldRect != null)
            _intentFieldRect.anchoredPosition = _intentFieldBasePos + new Vector2(0f, offsetY);
        for (int i = 0; i < _lookaheadRects.Count && i < _lookaheadBasePositions.Count; i++)
            if (_lookaheadRects[i] != null)
                _lookaheadRects[i].anchoredPosition = _lookaheadBasePositions[i] + new Vector2(0f, offsetY);
    }

    public void Bind(EnemyInstance instance)
    {
        Unbind();

        if (instance == null)
        {
            Debug.LogWarning("[EnemyView] Bind에 null EnemyInstance가 전달됨.");
            return;
        }

        Instance = instance;

        ApplySpriteAndScale(instance);

        instance.OnDamaged       += HandleDamaged;
        instance.OnDied          += HandleDied;
        instance.OnIntentChanged += RefreshIntent;
        instance.OnActionStarted += PlayLungeMotion;
        instance.OnBlockGained   += HandleBlockGained;
        instance.OnBlockChanged  += RefreshBlock;

        if (statusList != null) statusList.Bind(instance.Statuses);

        RefreshHP();
        RefreshBlock(instance.Block);
        RefreshIntent();
    }

    private void ApplySpriteAndScale(EnemyInstance instance)
    {
        float scale = instance.SpriteScale;
        float extraOffsetY = instance.IntentOffsetY;

        if (enemyImage != null)
        {
            var sheetAnim = enemyImage.GetComponent<UISpriteSheetAnimator>();
            if (instance.IdleFrames != null && instance.IdleFrames.Length > 0)
            {
                if (sheetAnim == null) sheetAnim = enemyImage.gameObject.AddComponent<UISpriteSheetAnimator>();
                sheetAnim.Configure(instance.IdleFrames, instance.IdleFrameRate, true);
                enemyImage.enabled = true;
            }
            else
            {
                if (sheetAnim != null) sheetAnim.Stop();
                enemyImage.sprite  = instance.EnemySprite;
                enemyImage.enabled = instance.EnemySprite != null;
            }

            enemyImage.rectTransform.localScale = new Vector3(_enemyImageBaseScale.x * scale, _enemyImageBaseScale.y * scale, 1f);

            if (instance.EnemySprite == null && (instance.IdleFrames == null || instance.IdleFrames.Length == 0))
            {
                Debug.LogWarning($"[EnemyView] '{instance.Data?.enemyName}'의 Sprite가 null입니다! 텍스처 임포트 설정(Sprite Mode: Multiple)이나 EnemyData 에셋을 확인하세요.");
            }
        }

        // 인텐트를 적 그림의 실제 머리 위에 붙인다.
        // 스프라이트 박스(Image rect) 위 끝을 기준으로 하면, 시트 프레임 위쪽의 투명 여백만큼 붕 떠서
        // 화면 위쪽(DECK 버튼 근처)에 뜨게 된다. 스프라이트의 Tight 메시에서 불투명 영역 위 끝을 구해 쓴다.
        float deltaY = extraOffsetY;
        if (_intentIconRect != null && enemyImage != null && TryGetVisibleTopLocal(out float visibleTop))
        {
            // 아이콘 아래 끝이 그림 위 끝 + intentGap 에 오도록, 초기 배치 기준의 이동량을 구한다
            // 아이콘에 localScale(현재 0.2)이 걸려 있어 부모 좌표 기준 높이는 rect × scale 이다
            float iconBottomAtInitial = _initialIntentIconPos.y
                                        - _intentIconRect.rect.height * _intentIconRect.pivot.y * _intentIconRect.localScale.y;
            deltaY += visibleTop + intentGap - iconBottomAtInitial;
        }
        else
        {
            // 메시 정보를 못 얻으면 예전 방식(박스 위 끝 기준)
            float spriteHeight = enemyImage != null ? enemyImage.rectTransform.rect.height : 100f;
            float pivotTopFactor = enemyImage != null ? (1f - enemyImage.rectTransform.pivot.y) : 0.5f;
            deltaY += spriteHeight * pivotTopFactor * (scale - 1.0f);
        }

        _intentIconBasePos = _initialIntentIconPos + new Vector2(0f, deltaY);
        _intentValueBasePos = _initialIntentValuePos + new Vector2(0f, deltaY);
        if (_intentFieldRect != null)
            _intentFieldBasePos = _initialIntentFieldPos + new Vector2(0f, deltaY);

        if (_intentIconRect != null)
            _intentIconRect.anchoredPosition = _intentIconBasePos;
        if (_intentValueRect != null)
            _intentValueRect.anchoredPosition = _intentValueBasePos;
        if (_intentFieldRect != null)
            _intentFieldRect.anchoredPosition = _intentFieldBasePos;
        LayoutLookaheadIcons();
    }

    // 적 그림(현재 스프라이트)의 불투명 영역 위 끝을, 인텐트 아이콘과 같은 좌표계(이 뷰의 로컬)에서 구한다.
    // Image는 preserveAspect로 rect 안에 맞춰 그리므로 그 배치까지 반영한다.
    private bool TryGetVisibleTopLocal(out float top)
    {
        top = 0f;
        var sprite = enemyImage.sprite;
        if (sprite == null) return false;
        var verts = sprite.vertices;
        if (verts == null || verts.Length == 0) return false;

        float maxY = float.MinValue;
        foreach (var v in verts) maxY = Mathf.Max(maxY, v.y);
        Rect sr = sprite.rect;
        float topPx = sprite.pivot.y + maxY * sprite.pixelsPerUnit;     // 스프라이트 아래 끝 기준 픽셀
        float normalized = Mathf.Clamp01(topPx / sr.height);

        Rect r = enemyImage.rectTransform.rect;
        float drawnH = r.height;
        if (enemyImage.preserveAspect && sr.width > 0f)
            drawnH = Mathf.Min(r.height, r.width * sr.height / sr.width);
        float localTop = r.center.y - drawnH * 0.5f + normalized * drawnH; // Image 로컬

        Vector3 world = enemyImage.rectTransform.TransformPoint(new Vector3(r.center.x, localTop, 0f));
        var parent = _intentIconRect.parent as RectTransform;
        top = parent != null ? parent.InverseTransformPoint(world).y : world.y;
        // anchoredPosition 기준으로 맞추기 위해 앵커 오프셋을 뺀다 (앵커가 부모 중앙이 아닐 때 대비)
        if (parent != null)
        {
            Vector2 anchorRef = Vector2.Lerp(_intentIconRect.anchorMin, _intentIconRect.anchorMax, 0.5f);
            top -= parent.rect.yMin + parent.rect.height * anchorRef.y;
        }
        return true;
    }

    private void OnDestroy() => Unbind();

    private void Unbind()
    {
        if (Instance == null) return;
        Unsubscribe();
        Instance = null;
        RefreshBlock(0);

        if (statusList != null) statusList.Unbind();

        if (enemyImage != null)
        {
            var sheetAnim = enemyImage.GetComponent<UISpriteSheetAnimator>();
            if (sheetAnim != null) sheetAnim.Stop();
            enemyImage.rectTransform.localScale = _enemyImageBaseScale;
        }

        _intentIconBasePos = _initialIntentIconPos;
        _intentValueBasePos = _initialIntentValuePos;
        if (_intentFieldRect != null)
            _intentFieldBasePos = _initialIntentFieldPos;

        if (_intentIconRect != null)
            _intentIconRect.anchoredPosition = _initialIntentIconPos;
        if (_intentValueRect != null)
            _intentValueRect.anchoredPosition = _initialIntentValuePos;
        if (_intentFieldRect != null)
            _intentFieldRect.anchoredPosition = _initialIntentFieldPos;
        HideLookaheadIcons();
    }

    private void Unsubscribe()
    {
        Instance.OnDamaged       -= HandleDamaged;
        Instance.OnDied          -= HandleDied;
        Instance.OnIntentChanged -= RefreshIntent;
        Instance.OnActionStarted -= PlayLungeMotion;
        Instance.OnBlockGained   -= HandleBlockGained;
        Instance.OnBlockChanged  -= RefreshBlock;
    }

    private void HandleDamaged(int amount)
    {
        RefreshHP();
        SpawnHitEffect();
        if (amount > 0) HitFlash.Play(enemyImage); // 피격 시 붉게 물들었다 돌아옴
    }

    private void HandleDied()
    {
        // 구독만 끊는다 — 사라지는 동안 인텐트 갱신·피격 이펙트 등이 더 들어오지 않도록.
        // Unbind()는 스프라이트 크기·인텐트 위치를 기본값으로 되돌려 죽는 순간 적이 작아져 보이므로 쓰지 않는다.
        // (Instance.IsDead가 true라 EnemyTargeting도 이 뷰를 대상으로 잡지 않는다)
        if (Instance != null) Unsubscribe();
        if (TryGetComponent<EnemyHoverInfo>(out var hover)) hover.Hide(); // 호버 이름·툴팁 정리
        enabled = false; // 인텐트 부유(Update) 정지

        var keepSprite = enemyImage != null ? new[] { (UnityEngine.UI.Graphic)enemyImage } : null;
        var seq = UIDissolve.Out(gameObject, null, deathDissolveDuration * 0.5f, UIDissolve.DefaultEdge, keepSprite);
        if (enemyImage != null)
        {
            enemyImage.raycastTarget = false;
            var fx = UIDissolve.Prepare(enemyImage, deathDissolveNoise, UIDissolve.DefaultEdge);
            if (fx != null)
            {
                fx.transitionRate = 0f;
                seq.Join(DOTween.To(() => fx.transitionRate, r => fx.transitionRate = r, 1f, deathDissolveDuration)
                                .SetEase(Ease.InQuad));
            }
            else seq.Join(enemyImage.DOFade(0f, deathDissolveDuration));
        }
        seq.OnComplete(() => Destroy(gameObject));
    }

    // EnemyZoneView가 생성 직후 호출. delay만큼 기다렸다가 오른쪽에서 슬라이드 + 페이드로 등장.
    public void PlayEnter(float delay)
    {
        if (_rect == null) return;
        if (!TryGetComponent<CanvasGroup>(out var group)) group = gameObject.AddComponent<CanvasGroup>();

        Vector2 target = _rect.anchoredPosition;
        _rect.anchoredPosition = target + new Vector2(enterSlideDistance, 0f);
        group.alpha = 0f;

        DOTween.Sequence().SetLink(gameObject)
               .Insert(delay, _rect.DOAnchorPos(target, enterDuration).SetEase(Ease.OutCubic))
               .Insert(delay, group.DOFade(1f, enterDuration * 0.8f));
    }

    // 공격 행동 직전 — 앞으로 살짝 전진했다가 원위치로 후퇴.
    // 복귀 기준 위치는 Awake 시점 캐싱값이 아니라 호출 시점의 실제 anchoredPosition을 사용한다.
    // enemyContainer에 Horizontal Layout Group이 붙어있어서, Awake 시점엔 레이아웃이
    // 아직 자리를 재배치하기 전이라 캐싱한 값이 실제 위치와 달라지는 문제가 있었음.
    private void PlayLungeMotion()
    {
        if (_rect == null) return;

        Vector2 originalPos = _rect.anchoredPosition;
        Vector2 forward = originalPos + new Vector2(-lungeDistance, 0f);
        _rect.DOAnchorPos(forward, lungeOutDuration)
            .OnComplete(() => _rect.DOAnchorPos(originalPos, lungeBackDuration));
    }

    private void SpawnHitEffect()
    {
        // 루트(transform) 위치가 아니라 실제 스프라이트가 그려지는 enemyImage 위치를 기준으로 한다.
        // 루트 RectTransform의 피벗/레이아웃 위치가 enemyImage와 다를 수 있어서 어긋나는 문제가 있었음.
        Transform anchor = enemyImage != null ? enemyImage.transform : transform;
        HitEffectSpawner.Spawn(hitEffectPrefab, anchor);
    }

    private void HandleBlockGained(int amount)
    {
        Transform anchor = enemyImage != null ? enemyImage.transform : transform;
        HitEffectSpawner.Spawn(blockGainShieldPrefab, anchor);
        HitEffectSpawner.Spawn(blockGainParticlePrefab, anchor);
    }

    private void RefreshBlock(int amount)
    {
        if (blockText != null) blockText.text = amount.ToString();
        if (blockBadge != null) blockBadge.SetActive(amount > 0);
    }

    private void RefreshHP()
    {
        if (Instance == null) return;

        float ratio = Instance.MaxHP > 0 ? (float)Instance.HP / Instance.MaxHP : 0f;
        if (hpFill != null) hpFill.fillAmount = ratio;
        if (hpText != null) hpText.text = $"{Instance.HP} / {Instance.MaxHP}";
        if (_previewLoss > 0) ApplyDamagePreview(); // 미리보기 중에 HP가 바뀌면 다시 그린다
    }

    // ---- 예상 피해 미리보기 (카드로 겨냥 중일 때) ----
    // 깎일 구간을 붉게 깜빡이는 막대로 보여주고, HP 숫자는 남을 체력을 붉게 표시한다.
    // 막대는 hpFill을 복제해 바로 뒤에 깐다: hpFill은 남을 체력까지만, 복제본은 현재 체력까지 채운다.
    private static readonly Color PreviewColor = new(0.9f, 0.2f, 0.18f, 1f);
    private Image _previewFill;
    private int _previewLoss;

    public void ShowDamagePreview(int hpLoss)
    {
        if (Instance == null || hpFill == null) return;
        _previewLoss = Mathf.Max(0, hpLoss);
        if (_previewLoss == 0) { ClearDamagePreview(); return; }
        ApplyDamagePreview();
    }

    public void ClearDamagePreview()
    {
        if (_previewLoss == 0 && (_previewFill == null || !_previewFill.gameObject.activeSelf)) return;
        _previewLoss = 0;
        if (_previewFill != null)
        {
            _previewFill.DOKill();
            _previewFill.gameObject.SetActive(false);
        }
        RefreshHP();
    }

    private void ApplyDamagePreview()
    {
        if (_previewFill == null)
        {
            _previewFill = Instantiate(hpFill, hpFill.transform.parent);
            _previewFill.name = "DamagePreview";
            _previewFill.raycastTarget = false;
            _previewFill.transform.SetSiblingIndex(hpFill.transform.GetSiblingIndex()); // hpFill 바로 뒤
        }
        // hpFill 앵커는 Slider가 바꿀 수 있어 매번 맞춘다
        var src = hpFill.rectTransform; var dst = _previewFill.rectTransform;
        dst.anchorMin = src.anchorMin; dst.anchorMax = src.anchorMax;
        dst.offsetMin = src.offsetMin; dst.offsetMax = src.offsetMax; dst.pivot = src.pivot;

        int max = Mathf.Max(1, Instance.MaxHP);
        int after = Mathf.Max(0, Instance.HP - _previewLoss);
        hpFill.fillAmount = (float)after / max;
        _previewFill.fillAmount = (float)Instance.HP / max;
        _previewFill.gameObject.SetActive(true);

        if (!DOTween.IsTweening(_previewFill))
        {
            _previewFill.color = PreviewColor;
            _previewFill.DOFade(0.35f, 0.45f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        }

        if (hpText != null)
        {
            string hex = ColorUtility.ToHtmlStringRGB(PreviewColor);
            hpText.text = $"<color=#{hex}>{after}</color> / {Instance.MaxHP}";
        }
    }

    private void RefreshIntent()
    {
        if (Instance == null) return;

        EnemyAction action = Instance.GetCurrentAction();

        // 인텐트 아이콘 호버 툴팁 — 대상 적/내용 갱신 (프리팹에 IntentTooltipTrigger가 붙어 있을 때만)
        if (intentIcon != null && intentIcon.TryGetComponent<IntentTooltipTrigger>(out var tooltip))
            tooltip.SetOwner(Instance);

        if (intentIcon != null)
        {
            Sprite sprite = action == null ? null : GetIntentSprite(action.intentType);
            intentIcon.sprite  = sprite;
            intentIcon.enabled = sprite != null;
        }

        if (intentValueText != null)
        {
            int? amount = Instance.GetIntentDamageAmount();
            intentValueText.gameObject.SetActive(amount.HasValue);
            if (amount.HasValue) intentValueText.text = amount.Value.ToString();
        }

        RefreshLookaheadIcons();
    }

    private void RefreshLookaheadIcons()
    {
        int count = BattleManager.Instance?.State?.EnemyIntentLookahead ?? 0;
        if (Instance == null || intentIcon == null || count <= 0)
        {
            HideLookaheadIcons();
            return;
        }

        IReadOnlyList<EnemyAction> upcoming = Instance.GetUpcomingActions(count);
        EnsureLookaheadIconCount(upcoming.Count);
        for (int i = 0; i < _lookaheadIcons.Count; i++)
        {
            bool visible = i < upcoming.Count;
            var image = _lookaheadIcons[i];
            if (image == null) continue;
            image.gameObject.SetActive(visible);
            if (!visible) continue;

            image.sprite = GetIntentSprite(upcoming[i].intentType);
            image.enabled = image.sprite != null;
        }
        LayoutLookaheadIcons();
    }

    private void EnsureLookaheadIconCount(int count)
    {
        while (_lookaheadIcons.Count < count)
        {
            var go = new GameObject(
                $"NextIntent{_lookaheadIcons.Count + 1}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(UnityEngine.UI.Image));
            go.layer = intentIcon.gameObject.layer;

            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(intentIcon.transform.parent, false);
            rect.anchorMin = _intentIconRect.anchorMin;
            rect.anchorMax = _intentIconRect.anchorMax;
            rect.pivot = _intentIconRect.pivot;
            rect.sizeDelta = _intentIconRect.sizeDelta;
            rect.localScale = _intentIconRect.localScale * 0.62f;
            rect.SetSiblingIndex(intentIcon.transform.GetSiblingIndex() + 1 + _lookaheadIcons.Count);

            var image = go.GetComponent<UnityEngine.UI.Image>();
            image.preserveAspect = intentIcon.preserveAspect;
            image.material = intentIcon.material;
            image.color = new Color(intentIcon.color.r, intentIcon.color.g, intentIcon.color.b, intentIcon.color.a * 0.72f);
            image.raycastTarget = false;

            _lookaheadRects.Add(rect);
            _lookaheadIcons.Add(image);
        }
    }

    private void LayoutLookaheadIcons()
    {
        if (_intentIconRect == null) return;
        _lookaheadBasePositions.Clear();
        float currentWidth = Mathf.Abs(_intentIconRect.rect.width * _intentIconRect.localScale.x);
        float spacing = Mathf.Max(24f, currentWidth * 0.72f);
        for (int i = 0; i < _lookaheadRects.Count; i++)
        {
            Vector2 basePosition = _intentIconBasePos + new Vector2(spacing * (i + 1), 0f);
            _lookaheadBasePositions.Add(basePosition);
            if (_lookaheadRects[i] != null)
                _lookaheadRects[i].anchoredPosition = basePosition;
        }
    }

    private void HideLookaheadIcons()
    {
        foreach (var image in _lookaheadIcons)
            if (image != null) image.gameObject.SetActive(false);
    }

    private Sprite GetIntentSprite(IntentType type)
    {
        if (intentSprites == null) return null;
        foreach (var entry in intentSprites)
            if (entry.intentType == type) return entry.sprite;
        return null;
    }
}
