using UnityEngine;
using System.Collections.Generic;

// 전투 승리 시 BattleManager에서 보상 데이터 받아와서 버튼 띄우는 뷰.
public class RewardsView : MonoBehaviour
{
    [SerializeField] private RewardItemButton rewardButtonPrefab;
    [SerializeField] private Transform rewardButtonContainer;
    [SerializeField] private CardRewardView cardRewardView;
    [SerializeField] private MapUIController mapUIController;

    private BattleReward reward;
    private List<RewardItemButton> buttons;

    private void Start()
    {
        BattleManager.Instance.OnBattleVictory += Open;
    }

    private void Open(BattleReward reward)
    {
        this.reward = reward;
        buttons = new List<RewardItemButton>();
        Refresh();
        foreach (Transform child in transform)
            child.gameObject.SetActive(true);
    }

    // 진행 버튼 onClick에 연결
    public void OnProceedClicked()
    {
        gameObject.SetActive(false);
        mapUIController.OpenMap();
    }

    private void DestroyButton(RewardItemButton destroyedButton)
    {
        if (buttons == null) return;

        if (buttons.Contains(destroyedButton))
        {
            destroyedButton.OnDestroyed -= DestroyButton;
            buttons.Remove(destroyedButton);
            Destroy(destroyedButton.gameObject);
        }
        // 보상 다 소진해도 자동 닫힘 없음 — 진행 버튼으로 처리
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
