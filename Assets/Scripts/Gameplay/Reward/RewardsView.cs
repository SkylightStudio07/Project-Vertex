using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

// 전체 보상 버튼 UI 띄우는 스크립트.
// 전투 승리 시 BattleManager에서 보상 데이터 받아와서 버튼 띄우는 뷰.
public class RewardsView: MonoBehaviour
{
    [SerializeField] private RewardItemButton rewardButtonPrefab;
    [SerializeField] private Transform rewardButtonContainer;
    private Reward reward;

    // 현재 활성화된 보상 버튼들
    private List<RewardItemButton> buttons;

    [SerializeField] private CardRewardView cardRewardView;

    private void Start()
    {
        BattleManager.Instance.OnBattleVictory += Open;
    }

    private void Open(Reward reward)
    {
        this.reward = reward;
        buttons = new List<RewardItemButton>();
        Refresh();
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }
    public void Close()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void DestroyButton(RewardItemButton destroyedButton)
    {
        if (buttons == null) return;

        if(buttons.Contains(destroyedButton))
        {
            destroyedButton.OnDestroyed -= DestroyButton;
            buttons.Remove(destroyedButton);
            Destroy(destroyedButton.gameObject);
        }

        if (buttons is null || buttons.Count == 0)
        {
            Close();
        }
    }

    private void Refresh()
    {
        foreach (Transform child in rewardButtonContainer){
            if(child.TryGetComponent<RewardItemButton>(out var button))
            {
                button.OnCardReward -= cardRewardView.Open;
            }
            Destroy(child.gameObject);
        }

        if (reward == null) return;

        // 버튼마다 RewardItem 구조체를 바인딩
        foreach (RewardItem item in reward.GetRewardList())
        {
            var button = Instantiate(rewardButtonPrefab, rewardButtonContainer);
            buttons.Add(button);
            button.Bind(item);
            button.OnCardReward += cardRewardView.Open;
            button.OnDestroyed += DestroyButton;
        }
    }

    private void OnDestroy()
    {
        BattleManager.Instance.OnBattleVictory -= Open;
    }
}
