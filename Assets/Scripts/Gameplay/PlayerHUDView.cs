using TMPro;
using UnityEngine;

// 플레이어 HP·블록·에너지·탄약 표시.
// 값 변경 지점이 GameManager(HP)/BattleManager(에너지,탄약)/PlayerCombatant(블록)로 흩어져 있고
// 전부에 이벤트가 있지는 않아서(PlayerCombatant는 OnBlockChanged 없음), 매 프레임 폴링으로 갱신한다.
// 수치 몇 개 비교라 비용은 무시할 만함. 나중에 필요하면 이벤트 기반으로 바꿀 것.
public class PlayerHUDView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI blockText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI ammoText;

    private void Update()
    {
        if (GameManager.Instance != null && hpText != null)
            hpText.text = $"{GameManager.Instance.PlayerHP} / {GameManager.Instance.MaxPlayerHP}";

        if (BattleManager.Instance == null) return;

        if (blockText != null)
        {
            int block = BattleManager.Instance.PlayerBlock;
            blockText.gameObject.SetActive(block > 0);
            if (block > 0) blockText.text = block.ToString();
        }

        if (energyText != null)
            energyText.text = $"{BattleManager.Instance.Energy} / {BattleManager.Instance.MaxEnergy}";

        if (ammoText != null)
            ammoText.text = BattleManager.Instance.Ammo.ToString();
    }
}
