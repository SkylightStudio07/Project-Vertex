using UnityEngine;

public class RewardsView: MonoBehaviour
{
    [SerializeField] private RewardItemButton RewardButtonPrefab;
    [SerializeField] private Transform RewardButtonContainer;
    private Reward reward;

    [SerializeField] private CardRewardView cardRewardView;

    private void Start()
    {
        BattleManager.Instance.OnBattleVictory += InitReward;
    }

    private void InitReward(Reward reward)
    {
        this.reward = reward;
        Refresh();
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void Refresh()
    {
        foreach (Transform child in RewardButtonContainer)
            Destroy(child.gameObject);

        if (reward == null) return;

        // 버튼마다 RewardItem 구조체를 바인딩
        foreach (RewardItem item in reward.GetRewardList())
        {
            var button = Instantiate(RewardButtonPrefab, RewardButtonContainer);
            button.Bind(item);
            button.OnCardReward += cardRewardView.Open;
        }
    }

    private void OnDestroy()
    {
        BattleManager.Instance.OnBattleVictory -= InitReward;
    }
}
