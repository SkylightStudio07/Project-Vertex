using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// 로비 상점 구매. MetaProgress 저장값을 쓰므로 보관·복원한다.
public class LobbyShopTests
{
    private const string Key = "vertex.meta_progress";
    private string _backup;
    private bool _had;
    private ShopItemData _parts, _gift, _card;

    [SetUp]
    public void SetUp()
    {
        _had = PlayerPrefs.HasKey(Key);
        _backup = PlayerPrefs.GetString(Key, string.Empty);
        PlayerPrefs.DeleteKey(Key);
        ResetCache();

        _parts = Item("parts_bundle", ShopCategory.Material, 120); _parts.partsAmount = 10;
        _gift = Item("coffee", ShopCategory.Gift, 60);
        _card = Item("colorless_test", ShopCategory.ColorlessCard, 240);
    }

    [TearDown]
    public void TearDown()
    {
        if (_had) PlayerPrefs.SetString(Key, _backup);
        else PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
        ResetCache();
        Object.DestroyImmediate(_parts); Object.DestroyImmediate(_gift); Object.DestroyImmediate(_card);
    }

    private static void ResetCache()
        => typeof(MetaProgress).GetField("s_data", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);

    private static ShopItemData Item(string id, ShopCategory category, int price)
    {
        var i = ScriptableObject.CreateInstance<ShopItemData>();
        i.itemId = id; i.displayName = id; i.category = category; i.price = price;
        return i;
    }

    [Test]
    public void Parts_SpendsTalin_UsesStoryStock()
    {
        MetaProgress.AddTalin(500);
        MetaProgress.RestockShop(); // 3
        Assert.AreEqual(LobbyShop.Result.Ok, LobbyShop.Buy(_parts, 2));
        Assert.AreEqual(260, MetaProgress.Talin);
        Assert.AreEqual(20, MetaProgress.Parts);
        Assert.AreEqual(1, LobbyShop.Remaining(_parts));
        Assert.AreEqual(LobbyShop.Result.SoldOut, LobbyShop.Check(_parts, 2));
    }

    [Test]
    public void NoStock_IsSoldOut_AndChargesNothing()
    {
        MetaProgress.AddTalin(500);
        Assert.AreEqual(LobbyShop.Result.SoldOut, LobbyShop.Buy(_parts));
        Assert.AreEqual(500, MetaProgress.Talin);
        Assert.AreEqual(0, MetaProgress.Parts);
    }

    [Test]
    public void NotEnoughTalin_ChangesNothing()
    {
        MetaProgress.AddTalin(100);
        MetaProgress.RestockShop();
        Assert.AreEqual(LobbyShop.Result.NotEnoughTalin, LobbyShop.Buy(_parts));
        Assert.AreEqual(100, MetaProgress.Talin);
        Assert.AreEqual(3, MetaProgress.ShopPartsStock);
        Assert.AreEqual(0, LobbyShop.MaxBuyable(_parts));
    }

    [Test]
    public void Gift_IsUnlimited_AndCounted()
    {
        MetaProgress.AddTalin(200);
        Assert.AreEqual(3, LobbyShop.MaxBuyable(_gift));
        Assert.AreEqual(LobbyShop.Result.Ok, LobbyShop.Buy(_gift, 3));
        Assert.AreEqual(3, MetaProgress.GiftCount("coffee"));
        Assert.AreEqual(20, MetaProgress.Talin);
        Assert.IsTrue(MetaProgress.TryTakeGift("coffee"));
        Assert.AreEqual(2, MetaProgress.GiftCount("coffee"));
    }

    [Test]
    public void ColorlessCard_UnlocksOnce()
    {
        MetaProgress.AddTalin(1000);
        Assert.AreEqual(LobbyShop.Result.InvalidQuantity, LobbyShop.Check(_card, 2));
        Assert.AreEqual(LobbyShop.Result.Ok, LobbyShop.Buy(_card));
        Assert.IsTrue(LobbyShop.IsOwned(_card));
        Assert.AreEqual(LobbyShop.Result.AlreadyOwned, LobbyShop.Buy(_card));
        Assert.AreEqual(760, MetaProgress.Talin);
    }
}
