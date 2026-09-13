// 플레이어 최대 체력을 증감하는 효과(음수면 감소). 규칙은 GameManager.ModifyMaxHP 참고.
// 전투 컨텍스트 없이 동작하므로 이벤트 선택지에서도 쓸 수 있다.
[System.Serializable]
public class ModifyMaxHPEffect : CardEffect
{
    public int amount;

    public override void Execute(CardContext context)
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.ModifyMaxHP(amount);
    }
}
