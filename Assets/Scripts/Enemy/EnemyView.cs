// ============================================================
// filename   : EnemyView.cs
// description   : EnemyInstance를 적 프리팹 UI에 바인딩하는 뷰 컴포넌트.
//             적 프리팹 루트에 부착하고 Inspector에서 각 UI 슬롯을 연결하시오.
// ============================================================

using System;
using System.Collections.Generic;
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
    [SerializeField] private TextMeshProUGUI intentValueText; // 공격 수치 등. DamageEffect 없는 행동이면 숨김
    [SerializeField] private List<IntentSprite> intentSprites;

    public EnemyInstance Instance { get; private set; }

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

        instance.OnDamaged      += HandleDamaged;
        instance.OnDied         += HandleDied;
        instance.OnIntentChanged += RefreshIntent;

        RefreshHP();
        RefreshIntent();
    }

    private void OnDestroy() => Unbind();

    private void Unbind()
    {
        if (Instance == null) return;
        Instance.OnDamaged      -= HandleDamaged;
        Instance.OnDied         -= HandleDied;
        Instance.OnIntentChanged -= RefreshIntent;
        Instance = null;
    }

    private void HandleDamaged(int _) => RefreshHP();
    private void HandleDied()         => Destroy(gameObject);

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
