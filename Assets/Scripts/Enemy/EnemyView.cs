// ============================================================
// filename   : EnemyView.cs
// description   : EnemyInstance를 적 프리팹 UI에 바인딩하는 뷰 컴포넌트.
//             적 프리팹 루트에 부착하고 Inspector에서 각 UI 슬롯을 연결하시오.
// ============================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyView : MonoBehaviour
{
    [Header("스프라이트")]
    [SerializeField] private Image enemyImage;

    [Header("HP 바")]
    [SerializeField] private Image  hpFill;     // Image Type: Filled (Horizontal)
    [SerializeField] private TextMeshProUGUI hpText;

    public EnemyInstance Instance { get; private set; }

    public void Bind(EnemyInstance instance)
    {
        Unbind();

        Instance = instance;

        enemyImage.sprite  = instance.EnemySprite;
        enemyImage.enabled = instance.EnemySprite != null;

        instance.OnDamaged += HandleDamaged;
        instance.OnDied    += HandleDied;

        RefreshHP();
    }

    private void OnDestroy() => Unbind();

    private void Unbind()
    {
        if (Instance == null) return;
        Instance.OnDamaged -= HandleDamaged;
        Instance.OnDied    -= HandleDied;
        Instance = null;
    }

    private void HandleDamaged(int _) => RefreshHP();
    private void HandleDied()         => Destroy(gameObject);

    private void RefreshHP()
    {
        float ratio = Instance.MaxHP > 0 ? (float)Instance.HP / Instance.MaxHP : 0f;
        if (hpFill != null) hpFill.fillAmount = ratio;
        if (hpText != null) hpText.text = $"{Instance.HP} / {Instance.MaxHP}";
    }
}
