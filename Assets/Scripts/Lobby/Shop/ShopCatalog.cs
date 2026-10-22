using System.Collections.Generic;
using UnityEngine;

// 로비 상점 진열 목록. 위에서부터 칸 순서대로 놓인다.
[CreateAssetMenu(fileName = "ShopCatalog", menuName = "Game Asset/Lobby Shop Catalog")]
public class ShopCatalog : ScriptableObject
{
    public List<ShopItemData> items = new();
}
