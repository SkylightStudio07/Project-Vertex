using System.Collections.Generic;
using UnityEngine;

// 런 시작 시 기본으로 제공되는 전투 보상 카드 풀.
// 캐릭터 합류 전 첫 전투까지 이 풀에서 보상이 생성된다.
// 캐릭터 합류 시 CoopCharData의 풀이 여기에 합산된다 (GameManager.cardPools).
[CreateAssetMenu(fileName = "PlayerRewardPool", menuName = "Game Asset/Player Reward Pool")]
public class PlayerRewardPoolSO : ScriptableObject
{
    [Header("등급별 기본 보상 카드 풀")]
    public List<CardData> commonCards = new();
    public List<CardData> rareCards   = new();
    public List<CardData> uniqueCards = new();
}
