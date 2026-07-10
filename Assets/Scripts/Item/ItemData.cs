// description   : 아이템 기본 정보를 담는 ScriptableObject 클래스
// ============================================================
// 업데이트 로그
// ------------------------------------------------------------
// 2026-06-05 | 박근혁 | 최초 작성.
// 최대한 CardData 참고해서 비슷하게 구성하려고 하는 중
// 레어도는 그냥 CardRariry 쓸까 했는데 일단 새로 만듦
// ============================================================

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Items/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private ItemRarity itemRarity;
    [SerializeField] private bool usableOutsideBattle;      // 비전투 시 사용 가능 여부
    [SerializeField] private ItemGetType itemGetTypes;

    [Header("아이템 설명 및 효과")]
    [SerializeField] public string itemDescription;
    [SerializeReference, SubclassPicker] public List<CardEffect> itemEffects = new();
    [SerializeField] private ItemUseMode useMode;

    // --- Public Accessors ---
    public string ItemName => itemName;
    public Sprite ItemIcon => itemIcon;
    public bool UsableOutsideBattle => usableOutsideBattle;
    public ItemRarity Rarity => itemRarity; 
    public string ItemDescription => itemDescription;
    public List<CardEffect> ItemEffects => itemEffects;
    public ItemUseMode UseMode => useMode;
    public ItemGetType ItemTypes => itemGetTypes;

    // --- enum ---
    public enum ItemRarity { Common, Uncommon, Rare }
    public enum ItemUseMode { Immediate, SelectTarget }
    [System.Flags]
    public enum ItemGetType { BattleReward = 1, EliteReward = 2, BossReward = 4, ShopPurchase = 8, EventReward = 16 }
}
