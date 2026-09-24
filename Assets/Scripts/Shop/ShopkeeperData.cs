using System;
using System.Collections.Generic;
using UnityEngine;

// 상점 주인(막마다 다른 NPC — 1막 토냐 등)의 방문 대사와 호감도 설정.
// 호감도·이벤트 플래그는 마키나와 같은 BlessingAffinityManager에 entityId로 저장된다(PlayerPrefs, 런 종료 후에도 유지).
//
// 대사 선택은 축복 노드(BlessingData.SelectDialogueSequence)와 같은 규칙: 조건을 만족하는 시퀀스 중 우선순위가 가장 높은 것 하나.
//   - 첫 만남      : priority 100, forbiddenEventFlag = "<id>_met",   setEventFlagOnComplete = "<id>_met"   → 게임 전체에서 1회
//   - 등급업 이벤트 : priority 50,  minAffinity = 해당 등급 하한, forbidden/set = "<id>_tierN"          → 등급마다 1회
//   - 일반 인사     : priority 0~,  조건 없음(또는 호감도 구간별)                                     → 매 방문
// 단계(step)의 playerAnswerText가 비어 있으면 클릭으로 넘기고, 채워져 있으면 답변 버튼이 뜬다.
// 일반 인사는 비워 두고, 등급업 이벤트에만 답변을 넣는 것이 원칙.
[CreateAssetMenu(fileName = "NewShopkeeperData", menuName = "Game Asset/Shopkeeper Data")]
public class ShopkeeperData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("호감도·플래그 저장 키 (예: tonya)")]
    public string entityId = "tonya";

    [Tooltip("대사창 이름표에 표시될 이름")]
    public string entityName = "토냐";

    [Tooltip("등장 막 (1: 1막, 2: 2막, 3: 3막)")]
    public int chapter = 1;

    [Header("호감도")]
    [Tooltip("상점 방문 1회마다 오르는 호감도 (마키나 조우와 같은 0.5)")]
    public float visitAffinityGain = 0.5f;

    [Header("방문 대사")]
    public List<BlessingDialogueSequence> visitSequences = new();

    [Header("말풍선 잡담")]
    [Tooltip("상점 화면의 고정 말풍선 문구. 상점 주인이나 말풍선을 클릭할 때마다 무작위로 바뀐다")]
    [TextArea(1, 3)]
    public List<string> idleLines = new();

    // 직전 문구와 다른 것을 무작위로 고른다(2개 이상일 때)
    public string PickIdleLine(string current)
    {
        if (idleLines == null || idleLines.Count == 0) return current;
        if (idleLines.Count == 1) return idleLines[0];
        string next = current;
        for (int i = 0; i < 8 && next == current; i++) // 같은 문구만 들어 있어도 멈추지 않도록 시도 횟수 제한
            next = idleLines[UnityEngine.Random.Range(0, idleLines.Count)];
        return next;
    }

    public BlessingDialogueSequence SelectVisitSequence(float affinity, Func<string, bool> isCompanionJoined, HashSet<string> flags)
    {
        BlessingDialogueSequence best = null;
        int highest = int.MinValue;
        foreach (var seq in visitSequences)
        {
            if (seq == null || !seq.Evaluate(affinity, isCompanionJoined, flags)) continue;
            if (seq.priority > highest)
            {
                highest = seq.priority;
                best = seq;
            }
        }
        return best;
    }
}
