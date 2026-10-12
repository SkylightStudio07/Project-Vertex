using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardItemButton : MonoBehaviour
{
    private RewardItem rewardItem;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private UnityEngine.UI.Image iconImage;
    [SerializeField] private TextMeshProUGUI numberText;      // 01, 02 ...
    [SerializeField] private TextMeshProUGUI descriptionText; // 이름 아래 작은 설명 ("1장 선택", "아이템"). 없으면 이름이 세로 가운데로

    [Header("줄 배치 (보상 화면 v1, 줄 왼쪽 위 기준)")]
    // 골드·카드는 흰 선화 아이콘(80), 아이템은 데이터 그림이라 칸에 꽉 차게(94) 넣는다
    [SerializeField] private float lineIconSize = 80f;
    [SerializeField] private float itemArtSize = 94f;
    [SerializeField] private float nameYWithDescription = 24f;
    [SerializeField] private float nameYWithoutDescription = 43f;

    [Header("타입별 고정 아이콘")]
    // 골드·카드는 데이터에 스프라이트가 없어 여기에 등록한다. 아이템은 ItemData.ItemIcon을 쓴다.
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite cardIcon;

    public event Action<List<CardData>, RewardItemButton> OnCardReward;

    // 획득 연출(RewardsView)에서 아이콘 위치·보상 종류를 읽는 용도
    public RewardItem Item => rewardItem;
    public UnityEngine.UI.Image Icon => iconImage;
    public TextMeshProUGUI Label => buttonText;

    // 버튼 삭제 위한 이벤트. 보상 획득 후 실제 버튼 오브젝트 삭제는 RewardsView에서 처리.
    public event Action<RewardItemButton> OnDestroyed;

    public void Bind(RewardItem item, int index = -1)
    {
        rewardItem = item;
        buttonText.text = item.ItemDescription;
        if (numberText != null)
        {
            numberText.gameObject.SetActive(index >= 0);
            numberText.text = (index + 1).ToString("00");
        }
        SetDescription(item.Type switch
        {
            RewardType.Card => "1장 선택",
            RewardType.Item => "아이템",
            _               => null
        });
        SetIcon(item);
    }

    private void SetDescription(string description)
    {
        bool has = !string.IsNullOrEmpty(description);
        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(has);
            descriptionText.text = description ?? "";
        }
        // 설명이 없으면 이름을 줄 세로 가운데로 내린다
        if (descriptionText != null && buttonText.rectTransform.anchorMin.y >= 1f)
        {
            var rt = buttonText.rectTransform;
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -(has ? nameYWithDescription : nameYWithoutDescription));
        }
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
        if (descriptionText != null) // v1 줄일 때만 크기를 바꾼다
        {
            float size = item.Type == RewardType.Item ? itemArtSize : lineIconSize;
            iconImage.rectTransform.sizeDelta = new Vector2(size, size);
            iconImage.preserveAspect = true;
        }
    }

    // 보상을 이미 받았는지. 카드 보상은 고른 뒤 연출이 끝나야 버튼이 사라지므로, 그 사이 재클릭을 막는다.
    private bool _claimed;

    public void MarkClaimed()
    {
        _claimed = true;
        if (TryGetComponent<UnityEngine.UI.Button>(out var uiButton)) uiButton.interactable = false;
    }

    public void OnClick()
    {
        if (_claimed) return;
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