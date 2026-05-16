using UnityEngine;

// 카드 보상 확률 관리하기 위한 ScriptableObject
// 현재 진행 중인 막과 전투 유형에 따라 보상으로 나오는 카드 확률이 다름

[CreateAssetMenu(fileName = "RewardProbabilityData", menuName = "Reward/RewardProbabilityData")]
public class RewardProbabilityData : ScriptableObject
{
    [field: Header("챕터")]
    [field: SerializeField] public int Chapter {  get; private set; }

    [field: Header("전투 유형")]
    [field: SerializeField] public BattleType BattleType { get; private set; }

    [field: Header("등급별 가중치")]
    [field: SerializeField] public int CommonProbability { get; private set; }
    [field: SerializeField] public int RareProbability { get; private set; }
    [field: SerializeField] public int UniqueProbability { get; private set; }

    [field: Header("강화된 카드일 확률")]
    [field: SerializeField] public int UpgradeProbability { get; private set; }
}
