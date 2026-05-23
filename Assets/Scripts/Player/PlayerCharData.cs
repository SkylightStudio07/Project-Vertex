using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newPlayerCharData", menuName = "Game Asset/Player Character")]
public class PlayerCharData : CharData
{
    [Header("플레이어 캐릭터 기본 능력치")]
    public float CharMaxHp;
    public int StartingEnegy;
    public int StartingGold;
    public int DrawCountPerTurn;

    [Header("플레이어 기본 카드 풀")]
    public List<CardData> DefaultCardPool = new();

    [Header("플레이어 카드 전투 보상 카드 풀")]
    public List<CardData> BattleRewardCardPool = new();
}
