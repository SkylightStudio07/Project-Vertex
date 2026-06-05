// description   : 아이템 기본 정보를 담는 ScriptableObject 클래스
//              최대한 CardData와 동일하게 구현?
// ============================================================
// 업데이트 로그
// ------------------------------------------------------------
// 2026-06-05 | 박근혁 | 최초 작성.
// 비전투시 사용 가능 여부로 usableOutsideBattle 필드가 있긴 한데 아직 전투 비전투 구분이랑 처리가 애매띠해서 활용은 일단 보류 중.
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
    [SerializeField] public List<CardEffect> itemEffects = new();
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
