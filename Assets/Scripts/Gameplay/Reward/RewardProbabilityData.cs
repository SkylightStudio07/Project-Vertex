using UnityEngine;

// 보상 등장 확률 관리하기 위한 ScriptableObject
// 현재 진행 중인 막과 전투 유형에 따라 보상으로 나오는 카드 확률이 다름

[CreateAssetMenu(fileName = "RewardProbabilityData", menuName = "Reward/RewardProbabilityData")]
public class RewardProbabilityData : ScriptableObject
{
    [field: Header("챕터")]
    [field: SerializeField] public int Chapter {  get; private set; }

    [field: Header("전투 유형")]
    [field: SerializeField] public BattleType BattleType { get; private set; }

    [field: Header("카드 등급별 가중치")]
    [field: SerializeField] public int CommonCardProb { get; private set; }
    [field: SerializeField] public int RareCardProb { get; private set; }
    [field: SerializeField] public int UniqueCardProb { get; private set; }

    [field: Header("강화된 카드일 확률")]
    [field: SerializeField] public int UpgradeProbability { get; private set; }


    [field: Header("아이템 등장 확률")]
    [field: SerializeField] public int ItemProbability { get; private set; }

    [field: Header("아이템 등급별 가중치")]
    [field: SerializeField] public int CommonItemProb { get; private set; }
    [field: SerializeField] public int UncommonItemProb { get; private set; }
    [field: SerializeField] public int RareItemProb { get; private set; }
}
