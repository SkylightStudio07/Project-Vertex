using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCoopChar", menuName = "Game Asset/Coop Character")]
public class CoopCharData : CharData
{
    [Header("호감도 레벨 당 해금 카드")]
    public List<CardData> unlockCardCoopLevel = new();

    [Header("합류 시 획득 카드")]
    public CardData joinRewardCard;

    [Header("전투 보상 카드 풀")]
    public List<CardData> battleRewardCardPool = new();

    [Header("호감도 최대 레벨")]
    public int maxCoopLevel;

    [Header("호감도 레벨 별 이벤트 데이터")]
    public List<RankEventData> rankEventDatas;
}

[System.Serializable]
public class RankEventData
{
    public int targetLevel;

    public int requiredPoint;

    public string dialogueID;

    public List<DialogueChoiceData> choices;
}

[System.Serializable]
public class DialogueChoiceData
{
    public string choiceText;
    public int coopPoint;
}
