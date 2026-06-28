using TMPro;
using UnityEngine;

// 플레이어 HP·블록·에너지·탄약 표시.
// 값 변경 지점이 GameManager(HP)/BattleManager(에너지,탄약)/PlayerCombatant(블록)로 흩어져 있고
// 전부에 이벤트가 있지는 않아서(PlayerCombatant는 OnBlockChanged 없음), 매 프레임 폴링으로 갱신한다.
// 나중에 필요하면 이벤트 기반으로 바꿀 것.
//
// 단, 매 프레임 무조건 문자열 보간/ToString()을 하면 값이 안 바뀌어도 매번 GC Alloc이 발생하고
// (턴제 게임이라 대부분의 프레임에서 값이 그대로인데도) blockText.SetActive도 매번 호출돼서 낭비가 된다.
// 그래서 이전 값을 캐싱해 실제로 바뀐 프레임에만 텍스트/SetActive를 갱신한다.
//
// SerializeField들은 null 체크를 유지한다 (의도적). 하나라도 Inspector 연결이 빠지면
// 거기서 예외가 터져 같은 Update() 안의 나머지 필드 갱신까지 같이 멈추는 걸 막기 위함 —
// 이 프로젝트는 Inspector 와이어링 누락으로 인한 버그를 여러 번 겪었어서(GraphicRaycaster, EventView 등)
// "하나 빠지면 그것만 안 보이고 나머지는 정상 동작"하는 쪽을 일관되게 택하고 있다.
public class PlayerHUDView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI blockText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI ammoText;

    // 아직 한 번도 갱신 안 된 상태를 나타내는 값. HP/블록/에너지/탄약은 음수가 될 수 없으므로 안전한 sentinel.
    private const int Unset = int.MinValue;

    private int _lastHp = Unset;
    private int _lastMaxHp = Unset;
    private int _lastBlock = Unset;
    private int _lastEnergy = Unset;
    private int _lastMaxEnergy = Unset;
    private int _lastAmmo = Unset;

    private void Update()
    {
        if (GameManager.Instance != null && hpText != null)
        {
            int hp = GameManager.Instance.PlayerHP;
            int maxHp = GameManager.Instance.MaxPlayerHP;
            if (hp != _lastHp || maxHp != _lastMaxHp)
            {
                _lastHp = hp;
                _lastMaxHp = maxHp;
                hpText.text = $"{hp} / {maxHp}";
            }
        }

        if (BattleManager.Instance == null) return;

        if (blockText != null)
        {
            int block = BattleManager.Instance.PlayerBlock;
            if (block != _lastBlock)
            {
                _lastBlock = block;
                bool hasBlock = block > 0;
                blockText.gameObject.SetActive(hasBlock);
                if (hasBlock) blockText.text = block.ToString();
            }
        }

        if (energyText != null)
        {
            int energy = BattleManager.Instance.Energy;
            int maxEnergy = BattleManager.Instance.MaxEnergy;
            if (energy != _lastEnergy || maxEnergy != _lastMaxEnergy)
            {
                _lastEnergy = energy;
                _lastMaxEnergy = maxEnergy;
                energyText.text = $"{energy} / {maxEnergy}";
            }
        }

        if (ammoText != null)
        {
            int ammo = BattleManager.Instance.Ammo;
            if (ammo != _lastAmmo)
            {
                _lastAmmo = ammo;
                ammoText.text = ammo.ToString();
            }
        }
    }
}
