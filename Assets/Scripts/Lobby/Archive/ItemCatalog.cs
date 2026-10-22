using System.Collections.Generic;
using UnityEngine;

// 아카이브 > 아이템 도감에 늘어놓는 아이템 목록 (위에서부터 칸 순서). 의뢰 아이템은 넣지 않는다.
[CreateAssetMenu(fileName = "ItemCatalog", menuName = "Game Asset/Item Catalog")]
public class ItemCatalog : ScriptableObject
{
    public List<ItemData> items = new();
}
