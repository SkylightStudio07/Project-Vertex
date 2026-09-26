using UnityEngine;
using UnityEngine.UI;

// 적 인텐트 아이콘 옆에 "이 적이 무엇을 하려는지" 툴팁을 띄운다.
// 툴팁 패널은 상태 칩과 같은 StatusTooltipView를 공유하고, 인텐트 아이콘 왼쪽에 뜬다.
// 마우스 판정은 적 전체를 받는 EnemyHoverInfo(적 루트)가 하고, 여기서는 표시만 맡는다 —
// 아이콘과 적 스프라이트를 오갈 때 따로 Enter/Exit가 오면 툴팁이 깜빡이기 때문.
// EnemyView가 인텐트 갱신 때 SetOwner로 대상 적을 넘겨준다.
[RequireComponent(typeof(Graphic))]
public class IntentTooltipTrigger : MonoBehaviour
{
    private EnemyInstance _enemy;
    private Image _icon;
    private bool _shown;

    private void Awake()
    {
        _icon = GetComponent<Image>();
        GetComponent<Graphic>().raycastTarget = true; // 아이콘 위에서도 적 호버로 잡히게
    }

    public void SetOwner(EnemyInstance enemy)
    {
        _enemy = enemy;
        if (_shown) Show(); // 떠 있는 중에 인텐트가 바뀌면 내용 갱신
    }

    public void Show()
    {
        var action = _enemy != null && !_enemy.IsDead ? _enemy.GetCurrentAction() : null;
        if (action == null || _icon == null || !_icon.enabled) { Hide(); return; }
        _shown = true;
        Describe(action.intentType, _enemy.GetIntentDamageAmount(), out string title, out string desc);
        StatusTooltipView.Instance?.ShowNear((RectTransform)transform, _icon.sprite, title, desc);
    }

    public void Hide()
    {
        if (!_shown) return;
        _shown = false;
        StatusTooltipView.Instance?.Hide();
    }

    private void OnDisable() => Hide();

    private static void Describe(IntentType type, int? damage, out string title, out string desc)
    {
        switch (type)
        {
            case IntentType.Attack:
                title = "공격";
                desc = damage.HasValue
                    ? $"이 적은 플레이어를 공격하려 하고 있습니다. <b>{damage.Value}</b>의 피해를 줍니다."
                    : "이 적은 플레이어를 공격하려 하고 있습니다.";
                break;
            case IntentType.Defend:
                title = "방어";
                desc = "이 적은 방어도를 얻으려 하고 있습니다.";
                break;
            case IntentType.Buff:
                title = "강화";
                desc = "이 적은 스스로를 강화하려 하고 있습니다.";
                break;
            case IntentType.Debuff:
                title = "약화";
                desc = "이 적은 플레이어에게 디버프를 걸려 하고 있습니다.";
                break;
            default:
                title = "대기";
                desc = "이 적은 아무 행동도 하지 않습니다.";
                break;
        }
        // 공격 외 행동에도 피해가 섞여 있으면 덧붙인다 (예: 공격 + 디버프)
        if (type != IntentType.Attack && damage.HasValue)
            desc += $" 또한 <b>{damage.Value}</b>의 피해를 줍니다.";
    }
}
