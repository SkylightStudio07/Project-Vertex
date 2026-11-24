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
using UnityEngine.Pool;
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
    public RectTransform SpriteRect => enemyImage != null ? enemyImage.rectTransform : (RectTransform)transform; // 피해 숫자 위치 기준 (BattleImpactFx)

    [Header("HP 바")]
    [SerializeField] private Image  hpFill;     // Image Type: Filled (Horizontal)
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("현재 방어도")]
    [SerializeField] private GameObject blockBadge;
    [SerializeField] private TextMeshProUGUI blockText;

    [Header("인텐트")]
    [SerializeField] private RectTransform intentRow;
    [SerializeField] private IntentIconView intentIconPrefab;
    [SerializeField] private IntentTooltipTrigger intentTooltip;
    [SerializeField] private GameObject intentField; // 단순 blob 용도
    [SerializeField] private List<IntentSprite> intentSprites;
    [Header("강한 공격 인텐트 (HeavyAttack 기준: 강함 15+ / 치명 30+)")]
    [SerializeField] private float heavyIntentScale = 1.15f;
    [SerializeField] private float lethalIntentScale = 1.3f;
    [SerializeField] private Color lethalIntentColor = new(0.88f, 0.27f, 0.24f, 1f);

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

    [Header("행동 이름 (적 턴)")]
    [Tooltip("적이 행동할 때 인텐트 자리에 잠깐 띄우는 행동 이름 (슬더스 방식)")]
    [SerializeField] private TextMeshProUGUI actionNameText;
    [SerializeField, Min(0f)] private float actionNameHold = 1.0f;

    [Header("페이즈 전환")]
    [Tooltip("페이즈 전환 대사를 띄울 텍스트 (인텐트 아이콘 위에 잠깐 떴다 사라진다)")]
    [SerializeField] private TextMeshProUGUI phaseLineText;
    [SerializeField, Min(0f)] private float phaseLineHold = 2.2f;

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

    private Vector2 _initialIntentRowPos;
    private Vector2 _intentRowBasePos;

    private RectTransform _intentFieldRect;
    private Vector2 _initialIntentFieldPos;
    private Vector2 _intentFieldBasePos;

    private ObjectPool<IntentIconView> _intentPool;
    private readonly List<IntentIconView> _intentItems = new();
    private readonly List<IntentIconView> _lookaheadItems = new();
    private readonly List<IntentType> _intentTypes = new();
    private float _intentAlpha = 1f;
    private RectTransform IntentAnchor => _intentItems.Count > 0
        ? (RectTransform)_intentItems[0].transform : intentRow;

    private Vector3 _enemyImageBaseScale = Vector3.one;
    private RectTransform _rect;

    public EnemyInstance Instance { get; private set; }

    private void Awake()
    {
        EnsureIntentPool();
        if (enemyImage != null)
        {
            _enemyImageBaseScale = enemyImage.rectTransform.localScale;
            if (_enemyImageBaseScale == Vector3.zero) _enemyImageBaseScale = Vector3.one;
        }

        if (intentRow != null)
        {
            _initialIntentRowPos = intentRow.anchoredPosition;
            _intentRowBasePos = _initialIntentRowPos;
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
        if (intentRow == null || _intentItems.Count == 0) return;

        // Sin(시간 * 속도) → -1~+1을 반복하는 값. 여기에 진폭을 곱하면 -amplitude~+amplitude(px) 범위의 오프셋이 된다.
        // 예) bobAmplitude=6, bobSpeed=2 → 초당 약 0.3회 진동, 최대 ±6px 위아래로 움직임.
        float offsetY = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;

        // 기준 위치에 오프셋을 더한다. 기준점을 고정해야 오프셋이 누적되지 않는다.
        if (intentRow != null)
            intentRow.anchoredPosition = _intentRowBasePos + new Vector2(0f, offsetY);
        if (_intentFieldRect != null)
            _intentFieldRect.anchoredPosition = _intentFieldBasePos + new Vector2(0f, offsetY);
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
        instance.OnPhaseChanged  += HandlePhaseChanged;

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
            var twitch = enemyImage.GetComponent<EnemyIdleTwitch>();
            bool wantsTwitch = instance.Data != null && instance.Data.idleTwitch;
            if (wantsTwitch && twitch == null) twitch = enemyImage.gameObject.AddComponent<EnemyIdleTwitch>();
            if (twitch != null) twitch.enabled = wantsTwitch;
            BattleUnitLook.Apply(enemyImage, BattleUnitLook.Side.Enemy); // 외곽선 · 림라이트 · 색 보정 · 발밑 그림자 (아군과 같은 처리)
            AlignHpBarToBody(instance);
            RefreshTraitTag(instance);

            if (instance.EnemySprite == null && (instance.IdleFrames == null || instance.IdleFrames.Length == 0))
            {
                Debug.LogWarning($"[EnemyView] '{instance.Data?.enemyName}'의 Sprite가 null입니다! 텍스처 임포트 설정(Sprite Mode: Multiple)이나 EnemyData 에셋을 확인하세요.");
            }
        }

        // 인텐트를 적 그림의 실제 머리 위에 붙인다.
        // 스프라이트 박스(Image rect) 위 끝을 기준으로 하면, 시트 프레임 위쪽의 투명 여백만큼 붕 떠서
        // 화면 위쪽(DECK 버튼 근처)에 뜨게 된다. 스프라이트의 Tight 메시에서 불투명 영역 위 끝을 구해 쓴다.
        float deltaY = extraOffsetY;
        if (intentRow != null && enemyImage != null && TryGetVisibleTopLocal(out float visibleTop))
        {
            // 항목 간격은 LayoutGroup이 담당하고, 묶음 전체만 적의 실제 머리 위에 맞춘다.
            float rowBottomAtInitial = _initialIntentRowPos.y - intentRow.rect.height * intentRow.pivot.y;
            deltaY += visibleTop + intentGap - rowBottomAtInitial;
        }
        else
        {
            // 메시 정보를 못 얻으면 예전 방식(박스 위 끝 기준)
            float spriteHeight = enemyImage != null ? enemyImage.rectTransform.rect.height : 100f;
            float pivotTopFactor = enemyImage != null ? (1f - enemyImage.rectTransform.pivot.y) : 0.5f;
            deltaY += spriteHeight * pivotTopFactor * (scale - 1.0f);
        }

        _intentRowBasePos = _initialIntentRowPos + new Vector2(0f, deltaY);
        if (_intentFieldRect != null)
            _intentFieldBasePos = _initialIntentFieldPos + new Vector2(0f, deltaY);

        if (intentRow != null)
            intentRow.anchoredPosition = _intentRowBasePos;
        if (_intentFieldRect != null)
            _intentFieldRect.anchoredPosition = _intentFieldBasePos;
    }

    // 시트 안에서 캐릭터가 한쪽으로 치우쳐 그려져도(노상강도) HP 바 · 상태 줄이 보이는 몸 가운데 아래에 오게 가로만 맞춘다.
    // 처음 위치를 기억해 두고 거기서부터 옮겨, 같은 뷰를 다시 쓸 때 밀림이 쌓이지 않게 한다
    private bool _hpHomeSet;
    private Vector2 _hpHome, _statusHome;
    private void AlignHpBarToBody(EnemyInstance instance)
    {
        if (instance?.Data == null || instance.Data.useScreenRect || hpFill == null) return;
        var hpBar = hpFill.transform.parent as RectTransform;
        var parent = hpBar != null ? hpBar.parent as RectTransform : null;
        if (parent == null) return;
        var status = statusList != null ? statusList.transform as RectTransform : null;
        if (!_hpHomeSet) { _hpHome = hpBar.anchoredPosition; _statusHome = status != null ? status.anchoredPosition : Vector2.zero; _hpHomeSet = true; }
        hpBar.anchoredPosition = _hpHome;
        if (status != null) status.anchoredPosition = _statusHome;
        float dx = 0f;
        if (BattleUnitLook.TryVisibleFeetWorld(enemyImage, out Vector3 feet))
            dx = parent.InverseTransformPoint(feet).x - parent.InverseTransformPoint(hpBar.position).x;
        // 몸 가운데로 맞춘 뒤 hpBarOffset(수동 보정)을 더한다
        Vector2 move = new Vector2(dx, 0f) + instance.Data.hpBarOffset;
        hpBar.anchoredPosition += move;
        if (status != null) status.anchoredPosition += move + instance.Data.statusRowOffset;
    }

    // HP 바 오른쪽 위 속성 칩 (왼쪽 위는 상태 줄). 기록 2편 전이면 '?' 칩 하나, 공개되면 속성마다 아이콘 칩. 속성이 없는 적은 공개 후 숨긴다.
    // HP 바 자식이라 바를 옮기면 같이 따라간다. 배치: ArtDirection/EnemyTraitMockup/Extracted/layout.json (battle)
    private RectTransform _traitRow;
    private readonly List<TraitChip> _traitChips = new();
    private static readonly Vector2 TraitRightChipOffset = new(-36f, 46f); // HP 바 오른쪽 위 → 맨 오른쪽 칩 왼쪽 위
    private const float TraitChipStep = 49f; // 칩 46 + 간격 3

    private void RefreshTraitTag(EnemyInstance instance)
    {
        var hpBar = hpFill != null ? hpFill.transform.parent as RectTransform : null;
        var data = instance?.Data;
        if (hpBar == null || data == null) return;

        if (_traitRow == null)
        {
            var go = new GameObject("TraitChips", typeof(RectTransform));
            go.layer = hpBar.gameObject.layer;
            _traitRow = (RectTransform)go.transform;
            _traitRow.SetParent(hpBar, false);
            _traitRow.anchorMin = _traitRow.anchorMax = new Vector2(1f, 1f);
            _traitRow.pivot = new Vector2(1f, 0f);
            _traitRow.sizeDelta = Vector2.zero;
            _traitRow.anchoredPosition = Vector2.zero;
        }
        foreach (var chip in _traitChips) if (chip != null) Destroy(chip.gameObject);
        _traitChips.Clear();

        var traits = new List<EnemyTrait>();
        if (!MaterialArchive.TraitsRevealed(data)) traits.Add(EnemyTrait.None); // 미상 '?'
        else traits.AddRange(EnemyTraits.Each(data.traits));

        // 선언 순서대로 왼쪽부터, 마지막 칩이 맨 오른쪽
        for (int i = 0; i < traits.Count; i++)
        {
            var chip = TraitChip.Create(_traitRow, traits[i]);
            var rt = (RectTransform)chip.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = TraitRightChipOffset - new Vector2((traits.Count - 1 - i) * TraitChipStep, 0f);
            _traitChips.Add(chip);
        }
    }

    // 특효 카드로 이 적을 겨냥하는 동안 맞는 칩만 켠다 (DamagePreview가 부름). None이면 모두 끈다
    public void HighlightTraits(EnemyTrait traits)
    {
        foreach (var chip in _traitChips)
            if (chip != null) chip.SetActive((chip.Trait & traits) != 0);
    }

    // 화면 고정 배치(EnemyData.useScreenRect) — 화면 끝에 붙는 거대 보스용.
    // EnemyZoneView가 슬롯 위치를 잡은 뒤(등장 연출 전) 호출한다. 그림을 화면 좌표에 놓고,
    // INTENT·HP 바를 그림 안 기준점(intentAnchor, hpBarAnchor)으로 옮긴다.
    public void ApplyScreenLayout()
    {
        EnemyData data = Instance?.Data;
        if (data == null || !data.useScreenRect || enemyImage == null) return;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        var root = canvas.rootCanvas.transform as RectTransform;
        Rect cr = root.rect;

        // 1920×1080 좌상단 원점 → 루트 캔버스 로컬 (비율 유지라 오른쪽 끝은 항상 화면 끝)
        Rect s = data.screenRect;
        Vector2 Design(float x, float y) => new(cr.xMin + x / 1920f * cr.width, cr.yMax - y / 1080f * cr.height);
        Vector3 center = root.TransformPoint(Design(s.x + s.width * 0.5f, s.y + s.height * 0.5f));
        Vector2 size = new(s.width / 1920f * cr.width, s.height / 1080f * cr.height);

        RectTransform img = enemyImage.rectTransform;
        img.localScale = _enemyImageBaseScale;
        float toLocal = root.lossyScale.x / Mathf.Max(1e-6f, img.lossyScale.x);
        img.sizeDelta = size * toLocal;
        img.position = center;

        // 그림 안 기준점(0~1, 좌하단 원점) → 월드
        Vector3 SpritePoint(Vector2 n)
        {
            Rect r = img.rect;
            return img.TransformPoint(new Vector3(r.xMin + n.x * r.width, r.yMin + n.y * r.height, 0f));
        }

        // HP 바 — 상태 아이콘 줄은 HP 바와의 간격을 그대로 따라간다
        RectTransform hpBar = hpFill != null ? hpFill.transform.parent as RectTransform : null;
        if (hpBar != null)
        {
            RectTransform status = statusList != null ? statusList.transform as RectTransform : null;
            Vector2 statusOffset = status != null ? status.anchoredPosition - hpBar.anchoredPosition : Vector2.zero;
            hpBar.position = SpritePoint(data.hpBarAnchor);
            if (status != null) status.anchoredPosition = hpBar.anchoredPosition + statusOffset;
        }

        // 의도 묶음 아래 끝을 보스 그림의 기준점에 맞춘다. 내부 정렬은 LayoutGroup이 담당한다.
        if (intentRow != null && intentRow.parent is RectTransform parent)
        {
            Vector2 local = parent.InverseTransformPoint(SpritePoint(data.intentAnchor));
            Vector2 anchorRef = Vector2.Lerp(intentRow.anchorMin, intentRow.anchorMax, 0.5f);
            local -= new Vector2(parent.rect.xMin + parent.rect.width * anchorRef.x, parent.rect.yMin + parent.rect.height * anchorRef.y);
            float halfBelow = intentRow.rect.height * intentRow.pivot.y;
            Vector2 rowPos = local + new Vector2(0f, intentGap + halfBelow);

            Vector2 shift = rowPos - _initialIntentRowPos;
            _intentRowBasePos = rowPos;
            if (_intentFieldRect != null) _intentFieldBasePos = _initialIntentFieldPos + shift;
            intentRow.anchoredPosition = _intentRowBasePos;
            if (_intentFieldRect != null) _intentFieldRect.anchoredPosition = _intentFieldBasePos;
        }
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
        var parent = intentRow.parent as RectTransform;
        top = parent != null ? parent.InverseTransformPoint(world).y : world.y;
        // anchoredPosition 기준으로 맞추기 위해 앵커 오프셋을 뺀다 (앵커가 부모 중앙이 아닐 때 대비)
        if (parent != null)
        {
            Vector2 anchorRef = Vector2.Lerp(intentRow.anchorMin, intentRow.anchorMax, 0.5f);
            top -= parent.rect.yMin + parent.rect.height * anchorRef.y;
        }
        return true;
    }

    private void OnDestroy()
    {
        Unbind();
        ClearIntentViews();
        _intentPool?.Dispose();
    }

    // 적 턴에 행동을 시작하면 인텐트를 잠깐 가리고 그 자리에 행동 이름을 띄운다.
    private void ShowActionName()
    {
        var action = Instance?.GetCurrentAction();
        if (actionNameText == null || action == null || string.IsNullOrWhiteSpace(action.DisplayName)) return;

        actionNameText.text = action.DisplayName;
        if (IntentAnchor != null) actionNameText.rectTransform.position = IntentAnchor.position;

        SetIntentAlpha(0f);
        actionNameText.DOKill();
        actionNameText.gameObject.SetActive(true);
        actionNameText.alpha = 0f;
        DOTween.Sequence().SetLink(actionNameText.gameObject).SetTarget(actionNameText)
            .Append(actionNameText.DOFade(1f, 0.12f))
            .AppendInterval(actionNameHold)
            .Append(actionNameText.DOFade(0f, 0.25f))
            .OnComplete(() =>
            {
                actionNameText.gameObject.SetActive(false);
                SetIntentAlpha(1f); // 그 사이 다음 행동으로 갱신된 인텐트를 다시 보인다
            });
    }

    private void SetIntentAlpha(float alpha)
    {
        _intentAlpha = alpha;
        foreach (var item in _intentItems) item.SetAlpha(alpha);
        foreach (var item in _lookaheadItems) item.SetAlpha(alpha);
    }

    // 페이즈 전환: 적이 움찔 커졌다 돌아오며 붉게 번쩍이고, 전환 대사가 인텐트 위에 잠깐 떠오른다.
    private void HandlePhaseChanged(EnemyPhase phase)
    {
        if (enemyImage != null)
        {
            var rect = enemyImage.rectTransform;
            rect.DOComplete();
            rect.DOPunchScale(Vector3.one * 0.12f, 0.45f, 6, 0.6f).SetLink(gameObject);
            HitFlash.Play(enemyImage);
        }

        if (phaseLineText == null || phase == null || string.IsNullOrWhiteSpace(phase.transitionLine)) return;
        phaseLineText.text = phase.transitionLine;
        var lineRect = phaseLineText.rectTransform;
        if (IntentAnchor != null)
        {
            lineRect.position = IntentAnchor.position;
            lineRect.anchoredPosition += new Vector2(0f, 13f); // 루트가 3배라 화면상 약 40px 위
        }
        phaseLineText.DOKill();
        phaseLineText.gameObject.SetActive(true);
        phaseLineText.alpha = 0f;
        DOTween.Sequence().SetLink(phaseLineText.gameObject).SetTarget(phaseLineText)
            .Append(phaseLineText.DOFade(1f, 0.25f))
            .AppendInterval(phaseLineHold)
            .Append(phaseLineText.DOFade(0f, 0.5f))
            .OnComplete(() => phaseLineText.gameObject.SetActive(false));
    }

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

        _intentRowBasePos = _initialIntentRowPos;
        if (_intentFieldRect != null)
            _intentFieldBasePos = _initialIntentFieldPos;

        if (intentRow != null)
            intentRow.anchoredPosition = _initialIntentRowPos;
        if (_intentFieldRect != null)
            _intentFieldRect.anchoredPosition = _initialIntentFieldPos;
        ClearIntentViews();
    }

    private void Unsubscribe()
    {
        Instance.OnDamaged       -= HandleDamaged;
        Instance.OnDied          -= HandleDied;
        Instance.OnIntentChanged -= RefreshIntent;
        Instance.OnActionStarted -= PlayLungeMotion;
        Instance.OnBlockGained   -= HandleBlockGained;
        Instance.OnBlockChanged  -= RefreshBlock;
        Instance.OnPhaseChanged  -= HandlePhaseChanged;
    }

    private void HandleDamaged(int amount)
    {
        RefreshHP();
        if (!BattleImpactFx.DrawsImpacts) SpawnHitEffect(); // 탄착 그림이 있으면 BattleImpactFx가 속성별로 그린다
        if (amount > 0)
        {
            HitFlash.Play(enemyImage, hitWhitePeak); // 맞은 순간 희게 번쩍이고, 붉게 물들었다 돌아옴
            PlayRecoil(amount);
        }
    }

    [Header("맞은 순간 경직")]
    [Tooltip("피해 1일 때 밀리는 거리 (px)")]
    [SerializeField] private float recoilMin = 8f;
    [Tooltip("피해가 recoilFullDamage 이상일 때 밀리는 거리 (px)")]
    [SerializeField] private float recoilMax = 24f;
    [SerializeField] private float recoilFullDamage = 20f;
    [SerializeField] private float recoilOut = 0.05f;
    [SerializeField] private float recoilBack = 0.22f;
    [Tooltip("맞은 순간 흰 번쩍임 세기 (0이면 끔)")]
    [SerializeField, Range(0f, 1f)] private float hitWhitePeak = 0.75f;
    private float _recoilOffset;
    private Tween _recoilTween;

    // 뒤로 밀렸다 돌아온다. 돌진 · 등장 연출과 겹쳐도 되도록 위치에 차이만 더한다. 히트스톱 중에도 움직인다
    private void PlayRecoil(int amount)
    {
        if (enemyImage == null) return;
        _recoilTween?.Kill();
        SetRecoil(0f);
        float weight = RecoilWeight();
        if (weight <= 0f) return;
        float distance = Mathf.Lerp(recoilMin, recoilMax, Mathf.InverseLerp(1f, recoilFullDamage, amount)) * weight;
        _recoilTween = DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
            .Append(DOTween.To(() => _recoilOffset, SetRecoil, distance, recoilOut).SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => _recoilOffset, SetRecoil, 0f, recoilBack).SetEase(Ease.OutCubic));
    }

    private void SetRecoil(float value)
    {
        enemyImage.rectTransform.anchoredPosition += new Vector2(value - _recoilOffset, 0f);
        _recoilOffset = value;
    }

    // 큰 적일수록 덜 밀린다. 화면 고정 거대 보스는 거의 안 움직인다
    private float RecoilWeight()
    {
        var data = Instance?.Data;
        if (data == null) return 1f;
        if (data.useScreenRect) return 0.2f;
        return data.rank == EnemyEncounterType.Boss ? 0.6f : data.rank == EnemyEncounterType.Elite ? 0.85f : 1f;
    }

    private void HandleDied()
    {
        ClearIntentViews();
        // 격퇴 후 대화하는 적: 사라지지 않고 서 있는다 (HP · 인텐트 · 상태만 걷어낸다). 클릭은 DefeatedTalkSession이 맡는다
        if (Instance?.Data != null && Instance.Data.defeatedTalk != null && enemyImage != null)
        {
            var data = Instance.Data.defeatedTalk;
            Unsubscribe();
            if (TryGetComponent<EnemyHoverInfo>(out var hoverInfo)) hoverInfo.Hide();
            enabled = false;
            if (hpFill != null) hpFill.transform.parent.gameObject.SetActive(false);
            if (statusList != null) statusList.gameObject.SetActive(false);
            if (blockBadge != null) blockBadge.SetActive(false);
            if (intentField != null) intentField.SetActive(false);
            if (actionNameText != null) actionNameText.gameObject.SetActive(false);
            enemyImage.raycastTarget = false;
            enemyImage.DOColor(new Color(0.78f, 0.8f, 0.84f, 1f), 0.4f).SetLink(gameObject); // 숨을 고르는 듯 살짝 가라앉힌다
            DefeatedTalkSession.Register(this, enemyImage.rectTransform, data);
            return;
        }

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
        ShowActionName();
        if (_rect == null) return;
        if (Instance != null && !Instance.PlaysAttackMotionNow) return;

        // 키프레임 공격(포즈 몇 장 + 반동 · 돌진): 제자리 돌진 · 움찔 없이 키프레임이 움직임을 다 맡는다
        if (Instance?.Data != null && Instance.Data.HasAttackKeyframes && enemyImage != null)
        {
            var kp = enemyImage.GetComponent<PoseSequencePlayer>();
            if (kp == null) kp = enemyImage.gameObject.AddComponent<PoseSequencePlayer>();
            kp.Play(Instance.Data.attackKeyframes.ToArray());
            return;
        }

        // 공격 스프라이트 애니메이션이 등록되어 있으면 재생 (대기 애니메이션 자동 일시정지 및 복귀)
        if (Instance != null && Instance.AttackFrames != null && Instance.AttackFrames.Length > 0 && enemyImage != null)
        {
            var seqPlayer = enemyImage.GetComponent<PoseSequencePlayer>();
            if (seqPlayer == null) seqPlayer = enemyImage.gameObject.AddComponent<PoseSequencePlayer>();
            var swingOverlay = enemyImage.GetComponent<BatSwingOverlay>();
            if (swingOverlay != null) swingOverlay.EnsurePlayerBinding();
            seqPlayer.Play(Instance.AttackFrames, Instance.AttackFrameRate,
                           Instance.Data != null ? Instance.Data.AttackScaleMultiplier : 1f); // 시트 여백 차이 보정
            ScheduleGroundFx();
        }

        // 몸놀림(도약 · 반동 · 공중제비)이 있으면 그림만 그 단계대로 움직인다 — HP 바는 제자리
        if (Instance?.Data != null && Instance.Data.attackMotion != null && Instance.Data.attackMotion.Count > 0 && enemyImage != null)
        {
            PlayAttackMotion(Instance.Data.attackMotion);
            return;
        }

        // 화면 끝에 붙은 보스는 앞으로 나오지 않고 제자리에서 움찔한다
        if (Instance?.Data != null && Instance.Data.noLunge)
        {
            if (enemyImage != null)
            {
                var img = enemyImage.rectTransform;
                img.DOComplete();
                img.DOPunchScale(Vector3.one * 0.05f, lungeOutDuration + lungeBackDuration, 5, 0.5f).SetLink(gameObject);
            }
            return;
        }

        Vector2 originalPos = _rect.anchoredPosition;
        Vector2 forward = originalPos + new Vector2(-lungeDistance, 0f);
        _rect.DOAnchorPos(forward, lungeOutDuration)
            .OnComplete(() => _rect.DOAnchorPos(originalPos, lungeBackDuration));
    }

    // 공격 몸놀림 (EnemyData.attackMotion). 그림 rect만 단계별로 옮기고 돌린다.
    // 피벗이 발밑이어도 그림 가운데를 축으로 돌도록 위치를 보정한다 (공중제비가 발을 축으로 넘어가지 않게)
    private Sequence _motion;
    private Vector2 _motionBase;
    private void PlayAttackMotion(List<EnemyData.AttackMotionStep> steps)
    {
        var img = enemyImage.rectTransform;
        if (_motion != null && _motion.IsActive()) { _motion.Kill(); ApplyMotion(img, Vector2.zero, 0f); }
        _motionBase = img.anchoredPosition;

        _motion = DOTween.Sequence().SetLink(gameObject);
        Vector2 prevOff = Vector2.zero;
        float prevRot = 0f;
        foreach (var s in steps)
        {
            if (s == null) continue;
            Vector2 from = prevOff, to = s.offset;
            float r0 = prevRot, r1 = s.rotation, jump = s.jumpHeight;
            var ease = s.ease;
            _motion.Append(DOVirtual.Float(0f, 1f, Mathf.Max(0.01f, s.duration), t =>
            {
                float e = DOVirtual.EasedValue(0f, 1f, t, ease);
                Vector2 off = Vector2.LerpUnclamped(from, to, e);
                if (jump > 0f) off.y += jump * 4f * t * (1f - t); // 포물선은 시간에 고르게
                ApplyMotion(img, off, Mathf.LerpUnclamped(r0, r1, e));
            }).SetEase(Ease.Linear));
            if (s.hold > 0f) _motion.AppendInterval(s.hold);
            prevOff = to;
            prevRot = r1;
        }
        _motion.OnComplete(() => ApplyMotion(img, Vector2.zero, 0f));
    }

    private void ApplyMotion(RectTransform img, Vector2 offset, float angle)
    {
        Vector2 c = img.rect.center;
        c = new Vector2(c.x * img.localScale.x, c.y * img.localScale.y);
        Vector2 rotated = (Vector2)(Quaternion.Euler(0f, 0f, angle) * c);
        img.anchoredPosition = _motionBase + offset + (c - rotated);
        img.localEulerAngles = new Vector3(0f, 0f, angle % 360f);
    }

    // 공격 프레임 번호마다 땅이 파이는 충격을 터뜨린다 (EnemyData.groundFxShots)
    private void ScheduleGroundFx()
    {
        var d = Instance?.Data;
        if (d == null || d.groundFxShots == null || d.groundFxShots.Count == 0 || enemyImage == null) return;
        float fps = Mathf.Max(1f, Instance.AttackFrameRate);
        // 총구마다 자기 탄착점에서 터진다
        foreach (var shot in d.groundFxShots)
        {
            Vector2 point = shot.point;
            DOVirtual.DelayedCall(shot.frame / fps, () => SpawnGroundFx(point)).SetLink(gameObject);
        }
    }

    private void SpawnGroundFx(Vector2 point)
    {
        var d = Instance?.Data;
        if (d == null || enemyImage == null) return;
        // 탄착점은 "그려진 그림" 기준이다. Image는 preserveAspect로 상자 안에 맞춰 그리므로 상자 전체가 아니라 맞춰 그려진 영역을 쓴다
        Rect r = enemyImage.rectTransform.rect;
        Vector2 drawn = r.size;
        var sprite = enemyImage.sprite;
        if (sprite != null && enemyImage.preserveAspect)
        {
            float k = Mathf.Min(r.width / sprite.rect.width, r.height / sprite.rect.height);
            drawn = sprite.rect.size * k;
        }
        Vector3 world = enemyImage.rectTransform.TransformPoint(new Vector3(r.center.x + (point.x - 0.5f) * drawn.x, r.center.y + (point.y - 0.5f) * drawn.y, 0f));
        GroundImpactFx.Play(transform as RectTransform, world, d.groundFxScale, GroundImpactFx.StageDustColor());
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
        if (hpText != null)
        {
            hpText.text = $"{Instance.HP} / {Instance.MaxHP}";
            // 숫자는 바 가운데: 연회색 채움이 가운데를 덮으면 먹색, 절반 아래로 내려가 어두운 트랙 위면 흰색 (전투 HUD v3)
            hpText.color = ratio >= 0.55f ? new Color(0.086f, 0.094f, 0.106f, 1f) : new Color(0.95f, 0.95f, 0.96f, 1f);
        }
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
        var action = Instance.GetCurrentAction();
        EnsureIntentPool();
        if (action == null || _intentPool == null)
        {
            ClearIntentViews();
            return;
        }

        int? damage = Instance.GetIntentDamageAmount(out int? damagePerHit, out int hitCount);
        _intentTypes.Clear();
        AddIntent(action.intentType);
        if (action.additionalIntents != null)
            foreach (var type in action.additionalIntents) AddIntent(type);

        int lookahead = BattleManager.Instance?.State?.EnemyIntentLookahead ?? 0;
        var upcoming = Instance.GetUpcomingActions(lookahead);
        // 줄어든 항목부터 반납하여 다른 목록에서 필요한 항목에도 바로 재사용한다.
        ReturnUnusedItems(_intentItems, _intentTypes.Count);
        ReturnUnusedItems(_lookaheadItems, upcoming.Count);
        EnsureItemCount(_intentItems, _intentTypes.Count);
        EnsureItemCount(_lookaheadItems, upcoming.Count);

        for (int i = 0; i < _intentTypes.Count; i++)
        {
            var item = _intentItems[i];
            var type = _intentTypes[i];
            item.Bind(GetIntentSprite(type), type == IntentType.Attack ? damage : null,
                hitCount: hitCount, damagePerHit: damagePerHit);
            if (type == IntentType.Attack)
                item.ApplyAttackTier(damage, heavyIntentScale, lethalIntentScale, lethalIntentColor);
            item.SetAlpha(_intentAlpha);
            item.transform.SetAsLastSibling();
        }
        for (int i = 0; i < upcoming.Count; i++)
        {
            var item = _lookaheadItems[i];
            item.Bind(GetIntentSprite(upcoming[i].intentType), null, lookahead: true);
            item.SetAlpha(_intentAlpha);
            item.transform.SetAsLastSibling();
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(intentRow);
        intentTooltip?.SetOwner(Instance, damage, _intentTypes, IntentAnchor, hitCount, damagePerHit);
    }

    private void AddIntent(IntentType type)
    {
        if (!_intentTypes.Contains(type)) _intentTypes.Add(type);
    }

    // 적 뷰 하나가 소유하는 작은 풀. 반환한 항목은 비활성화되므로 LayoutGroup에서 제외된다.
    private void EnsureIntentPool()
    {
        if (_intentPool != null || intentIconPrefab == null || intentRow == null) return;
        _intentPool = new ObjectPool<IntentIconView>(
            createFunc: () => Instantiate(intentIconPrefab, intentRow),
            actionOnGet: item => item.gameObject.SetActive(true),
            actionOnRelease: item => item.ResetForPool(),
            actionOnDestroy: DestroyIntentItem,
            collectionCheck: true,
            defaultCapacity: 4,
            maxSize: 16);
    }

    private static void DestroyIntentItem(IntentIconView item)
    {
        if (item == null) return;
        if (Application.isPlaying) Destroy(item.gameObject);
        else DestroyImmediate(item.gameObject);
    }

    private void EnsureItemCount(List<IntentIconView> items, int count)
    {
        while (items.Count < count) items.Add(_intentPool.Get());
    }

    private void ReturnUnusedItems(List<IntentIconView> items, int count)
    {
        for (int i = items.Count - 1; i >= count; i--)
        {
            _intentPool.Release(items[i]);
            items.RemoveAt(i);
        }
    }

    private void ClearIntentViews()
    {
        if (_intentPool != null)
        {
            ReturnUnusedItems(_intentItems, 0);
            ReturnUnusedItems(_lookaheadItems, 0);
        }
        _intentTypes.Clear();
        _intentAlpha = 1f;
        intentTooltip?.SetOwner(null, null, _intentTypes, intentRow);
    }
    public Sprite GetIntentSprite(IntentType type)
    {
        if (intentSprites == null) return null;
        foreach (var entry in intentSprites)
            if (entry.intentType == type) return entry.sprite;
        return null;
    }
}
