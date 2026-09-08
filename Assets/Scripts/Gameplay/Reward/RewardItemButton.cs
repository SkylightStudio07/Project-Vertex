using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardItemButton : MonoBehaviour
{
    private RewardItem rewardItem;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private UnityEngine.UI.Image iconImage;

    [Header("타입별 고정 아이콘")]
    // 골드·카드는 데이터에 스프라이트가 없어 여기에 등록한다. 아이템은 ItemData.ItemIcon을 쓴다.
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite cardIcon;

    public event Action<List<CardData>, RewardItemButton> OnCardReward;

    // 버튼 삭제 위한 이벤트. 보상 획득 후 실제 버튼 오브젝트 삭제는 RewardsView에서 처리.
    public event Action<RewardItemButton> OnDestroyed;

    public void Bind(RewardItem item)
    {
        rewardItem = item;
        buttonText.text = item.ItemDescription;
        SetIcon(item);
    }

    private void SetIcon(RewardItem item)
    {
        if (iconImage == null) return;

        Sprite icon = item.Type switch
        {
            RewardType.Gold => goldIcon,
            RewardType.Card => cardIcon,
            RewardType.Item => (item.Data as ItemData)?.ItemIcon,
            _               => null
        };

        // 스프라이트 미등록 시 흰 사각형이 뜨지 않게 컴포넌트를 끈다 (CardView와 동일한 관례)
        iconImage.sprite  = icon;
        iconImage.enabled = icon != null;
    }

    public void OnClick()
    {
        switch(rewardItem.Type)
        {
            case RewardType.Gold:
                GameManager.Instance.PlayerGold += (int)rewardItem.Data;
                CompleteReward();
                break;
            case RewardType.Card:
                DisplayCardReward();
                break;
            case RewardType.Item:
                GetItemReward();
                break;
            default:
                Debug.LogWarning("Unknown reward type clicked.");
                break;
        }
    }

    private void DisplayCardReward()
    {
        if (rewardItem.Data == null || rewardItem.Type != RewardType.Card)
            return;

        var cards = (List<CardData>)rewardItem.Data;
        OnCardReward?.Invoke(cards, this);
    }

    private void GetItemReward()
    {
        ItemData itemData = rewardItem.Data as ItemData;
        if (ItemInventoryManager.Instance.AddItem(itemData))
        {
            CompleteReward();
        }
    }
    
    public void CompleteReward()
    {
        OnDestroyed?.Invoke(this);
    }
}