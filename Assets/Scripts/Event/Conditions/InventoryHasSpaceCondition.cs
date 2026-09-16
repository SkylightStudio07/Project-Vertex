// 인벤토리에 빈 칸이 있어야 만족하는 조건.
// 아이템을 주는 선택지에 걸어두면, 가방이 꽉 찼을 때 그 선택지가 잠긴다.
[System.Serializable]
public class InventoryHasSpaceCondition : EventCondition
{
    public override bool IsMet()
    {
        if (ItemInventoryManager.Instance == null) return false;
        return ItemInventoryManager.Instance.HasSpace;
    }
}
