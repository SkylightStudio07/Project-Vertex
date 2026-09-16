// 인벤토리에 아이템을 지급하는 효과.
// 인벤토리가 꽉 차 있으면 AddItem이 실패하고 경고만 남는다 — 이벤트 선택지에서 쓸 때는
// InventoryHasSpaceCondition으로 선택 자체를 잠그는 편이 낫다.
[System.Serializable]
public class GainItemEffect : CardEffect
{
    public ItemData item;

    public override void Execute(CardContext context)
    {
        if (ItemInventoryManager.Instance == null || item == null) return;
        ItemInventoryManager.Instance.AddItem(item);
    }
}
