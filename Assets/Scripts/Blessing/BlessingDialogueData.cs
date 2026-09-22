using System;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// filename   : BlessingDialogueData.cs
// description: 축복 노드(Blessing)에서 마키나 및 등장인물 간의
//              1줄 대사 핑퐁(NPC 대사 + 플레이어 답변 버튼) 단계와
//              조건부 시퀀스(친밀도, 동료 합류, 이벤트 플래그)를 정의합니다.
// ============================================================

/// <summary>
/// 1회 대화 핑퐁 단위 (NPC 대사 1줄 + 하단 플레이어 답변 캡슐 버튼 텍스트)
/// </summary>
[Serializable]
public class BlessingDialogueStep
{
    [Tooltip("화자 이름 (비어있으면 기본 축복 대상자 이름 사용, 동료 대사인 경우 동료 이름)")]
    public string speakerName;

    [Tooltip("말풍선에 출력될 대사")]
    [TextArea(2, 4)]
    public string npcDialogue;

    [Tooltip("하단 플레이어 답변 버튼에 표시될 텍스트 (예: '[치하라 쇼 / 침묵] ...', '[방랑자] 계속 듣는다')")]
    public string playerAnswerText = "[방랑자] ...";

    [Tooltip("화자 아바타 스프라이트 (비어있으면 기본 축복 대상자 아바타 사용)")]
    public Sprite speakerAvatar;
}

/// <summary>
/// 조건에 따라 선택되어 순차 재생되는 대화 시퀀스
/// </summary>
[Serializable]
public class BlessingDialogueSequence
{
    [Tooltip("시퀀스 식별자 (예: enc_default_t0, enc_sho_reaction, talk_t1_memory)")]
    public string sequenceId;

    [Header("발동 조건")]
    [Tooltip("평가 우선순위 (높을수록 먼저 검사. 동료 반응은 100, 기본 티어는 0~10 등)")]
    public int priority = 0;

    [Tooltip("최소 요구 친밀도")]
    public float minAffinity = 0f;

    [Tooltip("최대 요구 친밀도")]
    public float maxAffinity = 999f;

    [Tooltip("파티에 합류해 있어야 하는 동료 ID (예: Cp_01 - 치하라 쇼). 비어있으면 무관.")]
    public string requiredCompanionCharId = "";

    [Tooltip("요구되는 이벤트 플래그 ID. 비어있으면 무관.")]
    public string requiredEventFlag = "";

    [Tooltip("해당 플래그가 없어야 발동하는 금지 플래그 (1회성 대화 연출용). 비어있으면 무관.")]
    public string forbiddenEventFlag = "";

    [Tooltip("시퀀스 대화 완료 시 자동으로 활성화할 이벤트 플래그 ID")]
    public string setEventFlagOnComplete = "";

    [Header("대화 핑퐁 단계 리스트")]
    public List<BlessingDialogueStep> steps = new();

    /// <summary>
    /// 현재 상황(친밀도, 동료 합류 여부, 이벤트 플래그)이 본 시퀀스의 발동 조건을 만족하는지 판정
    /// </summary>
    public bool Evaluate(float currentAffinity, Func<string, bool> isCompanionJoined, HashSet<string> currentFlags)
    {
        if (steps == null || steps.Count == 0) return false;
        if (currentAffinity < minAffinity || currentAffinity > maxAffinity) return false;

        if (!string.IsNullOrEmpty(requiredCompanionCharId))
        {
            if (isCompanionJoined == null || !isCompanionJoined(requiredCompanionCharId))
                return false;
        }

        if (!string.IsNullOrEmpty(requiredEventFlag))
        {
            if (currentFlags == null || !currentFlags.Contains(requiredEventFlag))
                return false;
        }

        if (!string.IsNullOrEmpty(forbiddenEventFlag))
        {
            if (currentFlags != null && currentFlags.Contains(forbiddenEventFlag))
                return false;
        }

        return true;
    }
}
