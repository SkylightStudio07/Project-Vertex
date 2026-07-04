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

    [Header("인텐트")]
    [SerializeField] private Image intentIcon;
    [SerializeField] private GameObject intentField; // 단순 blob 용도
    [SerializeField] private TextMeshProUGUI intentValueText; // 공격 수치 등. DamageEffect 없는 행동이면 숨김
    [SerializeField] private List<IntentSprite> intentSprites;

    [Header("인텐트 부유 애니메이션")]
    [SerializeField] private float bobAmplitude = 6f; // 위아래로 움직이는 폭(px)
    [SerializeField] private float bobSpeed     = 2f; // 초당 진동 속도

    [Header("공격 모션 (전진→후퇴)")]
    [SerializeField] private float lungeDistance    = 40f; // 전진 거리(px). 좌우 방향은 Inspector에서 부호로 조정
    [SerializeField] private float lungeOutDuration = 0.2f;
    [SerializeField] private float lungeBackDuration = 0.2f;

    [Header("피격 이펙트")]
    [SerializeField] private GameObject hitEffectPrefab;

    private RectTransform _intentIconRect;
    private Vector2 _intentIconBasePos;
    private RectTransform _intentValueRect;
    private Vector2 _intentValueBasePos;

    private RectTransform _rect;

    public EnemyInstance Instance { get; private set; }

    private void Awake()
    {
        if (intentIcon != null)
        {
            _intentIconRect = intentIcon.rectTransform;
            // 부유 애니메이션의 기준점. 매 프레임 여기에 오프셋을 더해서 위아래로 움직인다.
            _intentIconBasePos = _intentIconRect.anchoredPosition;
        }

        if (intentValueText != null)
        {
            _intentValueRect = intentValueText.rectTransform;
            _intentValueBasePos = _intentValueRect.anchoredPosition;
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

        enemyImage.sprite  = instance.EnemySprite;
        enemyImage.enabled = instance.EnemySprite != null;

        instance.OnDamaged       += HandleDamaged;
        instance.OnDied          += HandleDied;
        instance.OnIntentChanged += RefreshIntent;
        instance.OnActionStarted += PlayLungeMotion;

        RefreshHP();
        RefreshIntent();
    }

    private void OnDestroy() => Unbind();

    private void Unbind()
    {
        if (Instance == null) return;
        Instance.OnDamaged       -= HandleDamaged;
        Instance.OnDied          -= HandleDied;
        Instance.OnIntentChanged -= RefreshIntent;
        Instance.OnActionStarted -= PlayLungeMotion;
        Instance = null;
    }

    private void HandleDamaged(int _)
    {
        RefreshHP();
        SpawnHitEffect();
    }

    private void HandleDied() => Destroy(gameObject);

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

    private void RefreshHP()
    {
        if (Instance == null) return;

        float ratio = Instance.MaxHP > 0 ? (float)Instance.HP / Instance.MaxHP : 0f;
        if (hpFill != null) hpFill.fillAmount = ratio;
        if (hpText != null) hpText.text = $"{Instance.HP} / {Instance.MaxHP}";
    }

    private void RefreshIntent()
    {
        if (Instance == null) return;

        EnemyAction action = Instance.GetCurrentAction();

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
    }

    private Sprite GetIntentSprite(IntentType type)
    {
        if (intentSprites == null) return null;
        foreach (var entry in intentSprites)
            if (entry.intentType == type) return entry.sprite;
        return null;
    }
}
