using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCoopChar", menuName = "Game Asset/Coop Character")]
public class CoopCharData : CharData
{
    [Header("호감도 레벨 당 해금 카드")]
    public List<CardData> unlockCardCoopLevel = new();

    [Header("합류 시 획득 카드")]
    public CardData joinRewardCard;

    [Header("합류 시 재생할 짧은 대사 (선택)")]
    public TextAsset joinDialogueJson;

    [Header("전투 보상 카드 풀")]
    public List<CardData> battleRewardCardPool = new();

    [Header("호감도 최대 레벨")]
    public int maxCoopLevel;

    [Header("호감도 랭크 별 이벤트 데이터")]
    public List<RankEventData> rankEventDatas;
}

// 호감도 랭크업 시 재생할 이벤트 데이터.
// dialogueJson 포맷: Assets/Data/Dialogue/지침.md (선택지가 필요하면 JSON 안에서 직접 작성)
[System.Serializable]
public class RankEventData
{
    public int targetLevel;
    public int requiredPoint;
    public TextAsset dialogueJson;
}
