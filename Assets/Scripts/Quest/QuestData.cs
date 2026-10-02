using System.Collections.Generic;
using UnityEngine;

// 의뢰 태그. 게시판 카드 오른쪽에 표시된다.
public enum QuestTag
{
    None,
    Story,   // 스토리 진행에 필요한 의뢰
    Rescue,  // 합류 캐릭터 해금(구출) 의뢰
}

// 퀘스트 아이템을 얻는 계기
public enum QuestItemTrigger
{
    None,         // 퀘스트 아이템 없음 (토벌 등)
    ReachFloor,   // 특정 막·층 노드에 도달
    DefeatEnemy,  // 특정 적 처치
}

// 완료 조건
public enum QuestGoal
{
    HoldItemAtRunEnd, // 회수: 런이 끝날 때(클리어·사망 무관) 퀘스트 아이템을 들고 있음
    DeliverToNode,    // 배달: 퀘스트 아이템을 들고 특정 종류 노드 방문 (아이템은 소모)
    DefeatCount,      // 토벌: 특정 적 N회 처치
}

// 유형 기호 (게시판 카드·런 중 알림·칩·맵 태그 공용)
public enum QuestIconKind { Recovery, Delivery, Elimination, Rescue }

// 의뢰 한 건. Resources/QuestDatabase에 등록해야 게시판·런에서 인식된다. 기획: Docs/의뢰.md
[CreateAssetMenu(fileName = "Quest", menuName = "Game Asset/Quest")]
public class QuestData : ScriptableObject
{
    [Header("표시")]
    public string questId;
    public string title;
    public string client;              // 의뢰인 (세력·인물)
    [TextArea(2, 4)] public string description;
    public QuestTag tag;
    [Tooltip("게시판 카드 오른쪽 삽화 (칸 비율에 맞춰 잘린다)")]
    public Sprite cardArt;
    [Tooltip("상세 패널 상단 의뢰인 문장/엠블럼")]
    public Sprite clientEmblem;
    [Tooltip("게시 조건을 못 채웠을 때 잠긴 카드에 보일 문구 (예: 블랙박스 회수 완료 시 게시)")]
    public string lockedHint;

    [Header("퀘스트 아이템 (기존 ItemData, isQuestItem 체크 · 일반 아이템 칸 차지)")]
    public ItemData questItem;
    public QuestItemTrigger itemTrigger;
    [Min(1)] public int triggerChapter = 1;
    [Tooltip("ReachFloor: 0부터 세는 층 번호 (MapConfig와 같은 기준)")]
    [Min(0)] public int triggerFloor;
    [Tooltip("DefeatEnemy: 이 적을 처치하면 획득")]
    public EnemyData triggerEnemy;

    [Header("완료 조건")]
    public QuestGoal goal;
    [Tooltip("DeliverToNode: 이 종류의 노드에 들어가면 배달 완료")]
    public NodeType deliverNodeType = NodeType.Shop;
    [Tooltip("DefeatCount: 대상 적과 횟수")]
    public EnemyData goalEnemy;
    [Min(1)] public int goalCount = 1;

    [Header("보상")]
    [Min(0)] public int rewardExp = 20;
    [Tooltip("완료 시 켤 스토리 플래그")]
    public List<string> rewardFlags = new();
    [Tooltip("Rescue: 해금할 합류 캐릭터 ID. 완료 시 coop_rescued_{ID} 플래그가 켜진다")]
    public string rescueCharId;

    [Header("게시 조건")]
    [Tooltip("모두 켜져 있어야 게시판에 뜬다 (보스 대사와 같은 플래그 체계)")]
    public List<string> requiredFlags = new();

    public string RescueFlag => string.IsNullOrEmpty(rescueCharId) ? null : "coop_rescued_" + rescueCharId;

    public string TypeName => goal switch
    {
        QuestGoal.DeliverToNode => "배달",
        QuestGoal.DefeatCount => "토벌",
        _ => tag == QuestTag.Rescue ? "구출" : "회수",
    };

    public QuestIconKind IconKind => goal switch
    {
        QuestGoal.DeliverToNode => QuestIconKind.Delivery,
        QuestGoal.DefeatCount => QuestIconKind.Elimination,
        _ => tag == QuestTag.Rescue ? QuestIconKind.Rescue : QuestIconKind.Recovery,
    };

    // 퀘스트 아이템을 얻는 계기 한 줄 (게시판 카드 우하단·목표 첫 줄)
    public string TriggerText => itemTrigger switch
    {
        QuestItemTrigger.ReachFloor => $"{triggerChapter}막 {triggerFloor + 1}층 도달 시 획득",
        QuestItemTrigger.DefeatEnemy => $"{(triggerEnemy != null ? triggerEnemy.enemyName : "대상")} 처치 시 획득",
        _ => null,
    };

    // 상세 패널 목표 체크리스트
    public List<string> Objectives()
    {
        var list = new List<string>();
        if (TriggerText != null) list.Add(TriggerText);
        switch (goal)
        {
            case QuestGoal.HoldItemAtRunEnd: list.Add("런 종료 시 보유"); break;
            case QuestGoal.DeliverToNode: list.Add($"{NodeName(deliverNodeType)} 방문 시 전달"); break;
            case QuestGoal.DefeatCount: list.Add($"{(goalEnemy != null ? goalEnemy.enemyName : "대상")} {goalCount}회 처치"); break;
        }
        return list;
    }

    // 게시판 카드 우하단 요약
    public string Summary => goal switch
    {
        QuestGoal.DefeatCount => $"{(goalEnemy != null ? goalEnemy.enemyName : "대상")} {goalCount}회 처치",
        QuestGoal.DeliverToNode => $"{NodeName(deliverNodeType)}에 전달",
        _ => itemTrigger == QuestItemTrigger.DefeatEnemy && triggerEnemy != null ? $"{triggerChapter}막 · {triggerEnemy.enemyName} 처치"
           : itemTrigger == QuestItemTrigger.ReachFloor ? $"{triggerChapter}막 {triggerFloor + 1}층 도달" : "",
    };

    public static string NodeName(NodeType type) => type switch
    {
        NodeType.Shop => "상점", NodeType.Rest => "휴식", NodeType.Event => "이벤트",
        NodeType.Elite => "엘리트", NodeType.Boss => "보스", NodeType.Sanctuary => "성소", _ => type.ToString(),
    };
}
