using System.Collections.Generic;
using UnityEngine;
using static CardData;
using static ItemData;

public enum ShopGoodsType { Card, Item }

// 상점 판매 상품 단일
public class ShopGoods
{
    // Data 담을 필드 - CardData | ItemData
    public object Data;
    public int Price;
    public bool IsSold;
    private ShopGoodsType type;

    public ShopGoodsType Type => type;

    public string DisplayName => Type switch
    {
        ShopGoodsType.Card => ((CardData)Data).CardName,
        ShopGoodsType.Item => ((ItemData)Data).ItemName,
        _ => "Unknown",
    };

    public ShopGoods(ShopGoodsType type, object data, int price)
    {
        this.type = type;
        Data = data;
        Price = price;
    }
}

// 상점 전체 판매 목록 클래스
public class ShopStock
{
    // == 가격/구성 상수 변수 ==
    private const int CardSlotCount = 5;
    private const int ItemSlotCount = 5;

    private const int CommonCardPrice = 50;
    private const int RareCardPrice = 75;
    private const int UniqueCardPrice = 150;

    private const int CommonItemPrice = 150;
    private const int UncommonItemPrice = 250;
    private const int RareItemPrice = 300;

    // 가격 변동 퍼센테이지
    private const int PriceVariancePercent = 10;

    // 레어도 확률 가중치
    private const int CommonCardWeight = 60, RareCardWeight = 30, UniqueCardWeight = 10;
    private const int CommonItemWeight = 50, UncommonItemWeight = 35, RareItemWeight = 15;

    private readonly List<ShopGoods> cardGoods = new();
    private readonly List<ShopGoods> itemGoods = new();

    public IReadOnlyList<ShopGoods> CardGoods => cardGoods;
    public IReadOnlyList<ShopGoods> ItemGoods => itemGoods;

    public ShopStock(Dictionary<CardRarity, List<CardData>> cardPools, List<ItemData> itemPool, System.Random rng)
    {
        GenerateCardGoods(cardPools, rng);
        GenerateItemGoods(itemPool, rng);
    }

    private void GenerateCardGoods(Dictionary<CardRarity, List<CardData>> cardPools, System.Random rng)
    {
        if (cardPools == null) return;

        List<CardData> pickedCards = new List<CardData>();
        for (int i = 0; i < CardSlotCount; i++)
        {
            var rarity = Picker.PickCardRarity(CommonCardWeight, RareCardWeight, UniqueCardWeight, rng, "ShopStock");
            // 이미 진열된 카드를 exclude로 넘겨 중복을 방지
            var card = Picker.PickCard(cardPools, rarity, rng, pickedCards);
            if (card == null)
            {
                Debug.LogWarning("[ShopStock] 카드 풀이 비어 상품을 생성하지 못함. PlayerRewardPool 확인 필요.");
                continue;
            }
            
            cardGoods.Add(new ShopGoods(ShopGoodsType.Card, card, ApplyVariance(GetBasePrice(card.Rarity), rng)));
            pickedCards.Add(card);
        }
    }

    private void GenerateItemGoods(List<ItemData> itemPool, System.Random rng)
    {
        if(itemPool == null) return;

        List<ItemData> pickedItems = new List<ItemData>();
        for (int i = 0; i < ItemSlotCount; i++)
        {
            var rarity = Picker.PickItemRarity(CommonItemWeight, UncommonItemWeight, RareItemWeight, rng, "ShopStock");

            ItemData item = null;
            for (int r = (int)rarity; r >= (int)ItemRarity.Common && item == null; r--)
            {   
                item = Picker.PickItem(itemPool, (ItemRarity)r, ItemGetType.ShopPurchase, rng, pickedItems);
            }

            if (item == null)
            {
                Debug.LogWarning("[ShopStock] 아이템 풀이 비어 상품을 생성하지 못함. 확인 필요.");
                continue;
            }

            itemGoods.Add(new ShopGoods(ShopGoodsType.Item, item, ApplyVariance(GetBasePrice(item.Rarity), rng)));
            pickedItems.Add(item);
        }
    }

    // 상점 가격 변동 퍼센테이지 내에서 랜덤 적용
    private int ApplyVariance(int basePrice, System.Random rng)
    {
        int delta = rng.Next(-PriceVariancePercent, PriceVariancePercent + 1);
        int price = basePrice + basePrice * delta / 100;
        return price;
    }

    private static int GetBasePrice(CardRarity rarity) => rarity switch
    {
        CardRarity.Common => CommonCardPrice,
        CardRarity.Rare => RareCardPrice,
        CardRarity.Unique => UniqueCardPrice,
        _ => CommonCardPrice,
    };

    private static int GetBasePrice(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Common => CommonItemPrice,
        ItemRarity.Uncommon => UncommonItemPrice,
        ItemRarity.Rare => RareItemPrice,
        _ => CommonItemPrice,
    };
}