using UnityEngine;

// 로비 상점 상품 하나. 값은 모두 탈린이다.
// 기획: 노션 VERTEX 기획서 v2 > 8. 재화 & 거점 개발도 / Docs/기획/로비_상점.md
[CreateAssetMenu(fileName = "ShopItem", menuName = "Game Asset/Lobby Shop Item")]
public class ShopItemData : ScriptableObject
{
    [Tooltip("저장 키. 바꾸면 산 기록(선물 수·해금)이 끊긴다")]
    public string itemId;
    public string displayName;
    [Tooltip("이름 아래 작은 글자. 비우면 자동 (\"파츠 ×10\", \"선물 · 1개\", \"카드 · 해금\")")]
    public string subtitle;
    public Sprite icon;
    public ShopCategory category;
    [Min(0)] public int price = 100;

    [Header("강화 재료")]
    [Tooltip("한 번 살 때 받는 파츠. 파츠 상품은 스토리 진행마다 채워지는 재고를 1개씩 쓴다")]
    [Min(1)] public int partsAmount = 10;

    [Header("무색 카드")]
    [Tooltip("해금할 무색 카드. 한 번만 살 수 있다")]
    public CardData card;

    public ShopStockRule Stock => category switch
    {
        ShopCategory.Material => ShopStockRule.StoryRestock,
        ShopCategory.ColorlessCard => ShopStockRule.OneTime,
        _ => ShopStockRule.Unlimited,
    };

    public string Subtitle
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(subtitle)) return subtitle;
            return category switch
            {
                ShopCategory.Material => $"파츠 ×{partsAmount}",
                ShopCategory.Gift => "선물 · 1개",
                ShopCategory.ColorlessCard => "카드 · 해금",
                _ => "",
            };
        }
    }
}

public enum ShopCategory
{
    [InspectorName("강화 재료 (파츠)")] Material,
    [InspectorName("선물")] Gift,
    [InspectorName("무색 카드 해금")] ColorlessCard,
}

public enum ShopStockRule
{
    Unlimited,     // 선물
    StoryRestock,  // 파츠: 스토리 진행마다 MetaProgress.ShopPartsRestock개로 채움 (파츠 상품끼리 공유)
    OneTime,       // 무색 카드: 한 번 사면 해금 완료
}
