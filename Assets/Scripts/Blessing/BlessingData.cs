using System;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// filename   : BlessingData.cs
// description: 축복 노드(Blessing)에 등장하는 존재(Entity),
//              조우 진입 대화 시퀀스, 1~3번 은총 선택지, 4번 교감 선택지,
//              심화 대화 시퀀스 풀을 정의하는 ScriptableObject.
// ============================================================

public enum BlessingEffectType
{
    RemoveCard,       // 덱에서 카드 제거
    UpgradeCards,     // 시작/기본 카드 강화
    GainRandomItems,  // 무작위 아이템 획득
    GainGold,         // 골드 획득
    HealHP,           // 체력 회복
    MaxHP,            // 최대 체력 증가
    AffinityTalk      // 친밀도 대화 및 세계관/진엔딩 복선
}

[Serializable]
public class BlessingChoice
{
    [Tooltip("선택지 고유 식별자 (예: cleanse, refine, supply, affinity_talk)")]
    public string choiceId;

    [Tooltip("선택지 타이틀 (예: 정화, 연마, 보급, 교감)")]
    public string title;

    [Tooltip("선택지 설명문 (예: 덱에서 불필요한 카드 1장을 완전히 제거합니다.)")]
    [TextArea(1, 3)]
    public string description;

    [Tooltip("선택지 좌측 아이콘")]
    public Sprite icon;

    [Tooltip("선택지 효과 타입")]
    public BlessingEffectType effectType;

    [Tooltip("수치 파라미터 (예: 제거/강화할 카드 수, 아이템 개수, 골드량 등)")]
    public int valueCount = 1;

    [Tooltip("타이틀 텍스트 강조 색상")]
    public Color titleColor = new Color(1f, 0.82f, 0.4f); // 기본 골드
}

[CreateAssetMenu(fileName = "NewBlessingData", menuName = "Game Asset/Blessing Data")]
public class BlessingData : ScriptableObject
{
    [Header("존재 기본 정보")]
    [Tooltip("존재 고유 ID (예: machina, fox_god, engineer)")]
    public string entityId = "machina";

    [Tooltip("존재 표시 이름 (예: 마키나)")]
    public string entityName = "마키나";

    [Tooltip("존재 칭호/설명 (예: 백색 피안화의 사신)")]
    public string entityTitle = "백색 피안화의 사신";

    [Tooltip("말풍선 좌측 미니 화자 아이콘")]
    public Sprite speakerIcon;

    [Tooltip("전용 배경 일러스트 (선택사항, null이면 기본 씬 배경 유지)")]
    public Sprite background;

    [Header("등장 막/조건")]
    [Tooltip("등장 막 (0: 프롤로그/0층, 1: 1막, 2: 2막, 3: 3막)")]
    public int chapter = 0;

    [Header("대사 (시퀀스 미지정 시 기본 폴백)")]
    [Tooltip("말풍선에 표시될 기본 대사")]
    [TextArea(2, 4)]
    public string dialogueText = "「눈을 떠라, 방랑자여... 길을 떠나기 전 그대에게 한 가지 은총을 베풀어주마.」";

    [Header("1. 조우 진입 핑퐁 대화 풀")]
    [Tooltip("조우 시 조건(친밀도, 동료 합류, 이벤트 플래그)에 따라 자동 선별되는 진입 핑퐁 대사들")]
    public List<BlessingDialogueSequence> encounterSequences = new();

    [Header("2. 일반 은총 선택지 풀 (1~3번)")]
    public List<BlessingChoice> choices = new();

    [Header("3. 4번째 전용 선택지: [교감 / 대화]")]
    [Tooltip("4번째 고정 슬롯 선택지. 비어있으면 기본 교감 선택지 반환")]
    public BlessingChoice talkChoice = new BlessingChoice
    {
        choiceId = "affinity_talk",
        title = "교감",
        description = "마키나와 대화를 나누어 친밀도를 1 상승시키고, 버텍스의 숨겨진 기억을 듣습니다.",
        effectType = BlessingEffectType.AffinityTalk,
        valueCount = 1,
        titleColor = new Color(0.78f, 0.58f, 0.98f) // 연보라
    };

    [Header("4. 4번째 선택 시 심화 핑퐁 대화 풀")]
    [Tooltip("교감 선택 시 친밀도 티어 및 플래그에 따라 진행되는 심화 대사들")]
    public List<BlessingDialogueSequence> affinityTalkSequences = new();

    /// <summary>
    /// 조건 풀에서 우선순위가 가장 높고 조건을 만족하는 시퀀스 1개 선별
    /// </summary>
    public BlessingDialogueSequence SelectDialogueSequence(
        List<BlessingDialogueSequence> pool,
        float currentAffinity,
        Func<string, bool> isCompanionJoined,
        HashSet<string> currentFlags)
    {
        if (pool == null || pool.Count == 0) return null;

        BlessingDialogueSequence best = null;
        int highestPriority = int.MinValue;

        for (int i = 0; i < pool.Count; i++)
        {
            var seq = pool[i];
            if (seq == null) continue;
            if (!seq.Evaluate(currentAffinity, isCompanionJoined, currentFlags)) continue;

            if (seq.priority > highestPriority)
            {
                highestPriority = seq.priority;
                best = seq;
            }
        }

        return best;
    }

    /// <summary>
    /// 4번째 교감 선택지 안전 반환
    /// </summary>
    public BlessingChoice GetTalkChoice()
    {
        if (talkChoice != null && !string.IsNullOrEmpty(talkChoice.title))
        {
            talkChoice.effectType = BlessingEffectType.AffinityTalk;
            return talkChoice;
        }

        return new BlessingChoice
        {
            choiceId = "affinity_talk",
            title = "교감",
            description = "마키나와 대화를 나누어 친밀도를 1 상승시키고, 버텍스의 숨겨진 기억을 듣습니다.",
            effectType = BlessingEffectType.AffinityTalk,
            valueCount = 1,
            titleColor = new Color(0.78f, 0.58f, 0.98f)
        };
    }
}
