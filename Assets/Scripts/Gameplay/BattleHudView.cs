using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전투 HUD v3에서 새로 생긴 표시들. (HP·에너지·탄약 수치는 PlayerHUDView가 그대로 맡는다)
//   - 상단 작전 바: FLOOR(현재 층) · TURN(턴 수) · 막 이름
//   - 오른쪽 자원 모듈: 무기 이름, 에너지 0이면 번개 기호를 꺼진 색으로
//   - END TURN: 플레이어 턴이 아니면(적 턴·턴 종료 처리 중) 비활성 — 버튼 틀은 Button의 Disabled 스프라이트, 화살표·글자는 여기서
// 값이 바뀐 프레임에만 갱신한다. 조각: Assets/Art/UI/BattleHUD (원본 ArtDirection/BattleHUD/Extracted)
public class BattleHudView : MonoBehaviour
{
    [Header("상단 작전 바")]
    [SerializeField] private TextMeshProUGUI floorText;
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI actNameText;

    [Header("자원 모듈")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Image energyIcon;
    [SerializeField] private Sprite energyIconNormal, energyIconEmpty;
    [SerializeField] private TextMeshProUGUI energyText;

    [Header("END TURN")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TextMeshProUGUI endTurnLabel;
    [SerializeField] private Image endTurnArrow;
    [SerializeField] private Sprite arrowNormal, arrowDisabled;

    private static readonly Color TextOn = new(0.95f, 0.95f, 0.96f, 1f);
    private static readonly Color TextOff = new(0.35f, 0.38f, 0.41f, 1f);

    private int _floor = int.MinValue, _turn = int.MinValue, _energy = int.MinValue;
    private string _act, _weapon;
    private int _canEnd = -1;

    private void Update()
    {
        var run = RunData.Instance;
        int floor = run != null ? run.currentFloor + 1 : 0;
        if (floor != _floor && floorText != null) { _floor = floor; floorText.text = floor.ToString("00"); }

        var gm = GameManager.Instance;
        string act = gm != null && gm.CurrentAct != null ? gm.CurrentAct.actName : "";
        if (act != _act && actNameText != null) { _act = act; actNameText.text = act; }

        var bm = BattleManager.Instance;
        var state = bm != null ? bm.State : null;
        if (state == null) return;

        int turn = Mathf.Max(1, state.TurnNumber);
        if (turn != _turn && turnText != null) { _turn = turn; turnText.text = turn.ToString(); }

        string weapon = bm.CurrentWeapon != null ? bm.CurrentWeapon.WeaponName : "";
        if (weapon != _weapon && weaponNameText != null) { _weapon = weapon; weaponNameText.text = weapon; }

        int energy = state.Energy;
        if (energy != _energy)
        {
            _energy = energy;
            if (energyIcon != null && energyIconNormal != null)
                energyIcon.sprite = energy > 0 || energyIconEmpty == null ? energyIconNormal : energyIconEmpty;
            if (energyText != null) energyText.color = energy > 0 ? TextOn : TextOff;
        }

        int canEnd = state.Phase == BattlePhase.PlayerTurn && !bm.IsPlayerTurnEnding && bm.IsInBattle ? 1 : 0;
        if (canEnd != _canEnd)
        {
            _canEnd = canEnd;
            bool on = canEnd == 1;
            if (endTurnButton != null) endTurnButton.interactable = on;
            if (endTurnLabel != null) endTurnLabel.color = on ? TextOn : TextOff;
            if (endTurnArrow != null && arrowNormal != null)
                endTurnArrow.sprite = on || arrowDisabled == null ? arrowNormal : arrowDisabled;
        }
    }
}
