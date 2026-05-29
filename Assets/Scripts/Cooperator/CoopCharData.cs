using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCoopChar", menuName = "Game Asset/Coop Character")]
public class CoopCharData : CharData
{
    [Header("호감도 레벨 당 해금 카드")]
    public List<CardData> UnlockCardCoopLevel = new();

    [Header("합류 시 획득 카드")]
    public CardData JoinRewardCard;

    // 특정 호감도 레벨 때 이벤트가 발생한다는 것을 나타냄.
    [Header("호감도 이벤트 발생 호감도 레벨")]
    public List<int> CoopEventTriggerLevels = new();

    [Header("전투 보상 카드 풀")]
    public List<CardData> BattleRewardCardPool = new();
}
