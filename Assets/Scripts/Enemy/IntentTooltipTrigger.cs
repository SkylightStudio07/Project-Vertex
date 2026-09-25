using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 적 인텐트 아이콘에 마우스를 올리면 "이 적이 무엇을 하려는지" 툴팁을 띄운다.
// 툴팁 패널은 상태 칩과 같은 StatusTooltipView를 공유하고, 인텐트 아이콘 왼쪽에 뜬다.
// EnemyView가 Bind 때 SetOwner로 대상 적을 넘겨준다.
[RequireComponent(typeof(Graphic))]
public class IntentTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private EnemyInstance _enemy;
    private Image _icon;
    private bool _shown;

    private void Awake()
    {
        _icon = GetComponent<Image>();
        GetComponent<Graphic>().raycastTarget = true; // 인텐트 아이콘은 원래 클릭을 받지 않아서 켜 준다
    }

    public void SetOwner(EnemyInstance enemy)
    {
        _enemy = enemy;
        if (_shown) Refresh(); // 떠 있는 중에 인텐트가 바뀌면 내용 갱신
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_enemy == null || _enemy.IsDead || _enemy.GetCurrentAction() == null) return;
        _shown = true;
        Refresh();
    }

    public void OnPointerExit(PointerEventData eventData) => Hide();

    private void OnDisable() => Hide();

    private void Hide()
    {
        if (!_shown) return;
        _shown = false;
        StatusTooltipView.Instance?.Hide();
    }

    private void Refresh()
    {
        var action = _enemy?.GetCurrentAction();
        if (action == null) { Hide(); return; }
        Describe(action.intentType, _enemy.GetIntentDamageAmount(), out string title, out string desc);
        StatusTooltipView.Instance?.ShowNear((RectTransform)transform, _icon != null ? _icon.sprite : null, title, desc);
    }

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
