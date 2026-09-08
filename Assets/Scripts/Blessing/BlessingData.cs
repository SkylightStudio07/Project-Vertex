using System;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// filename   : BlessingData.cs
// description: 축복 노드(Blessing)에 등장하는 존재(Entity),
//              대사, 선택지 풀을 정의하는 ScriptableObject.
//              1~3막별 존재 분기 및 다양한 축복 효과를 지원합니다.
// ============================================================

public enum BlessingEffectType
{
    RemoveCard,       // 덱에서 카드 제거
    UpgradeCards,     // 시작/기본 카드 강화
    GainRandomItems,  // 무작위 아이템 획득
    GainGold,         // 골드 획득
    HealHP,           // 체력 회복
    MaxHP             // 최대 체력 증가
}

[Serializable]
public class BlessingChoice
{
    [Tooltip("선택지 고유 식별자 (예: cleanse, refine, supply)")]
    public string choiceId;

    [Tooltip("선택지 타이틀 (예: 정화, 연마, 보급)")]
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

    [Header("대사")]
    [Tooltip("말풍선에 표시될 존재의 대사")]
    [TextArea(2, 4)]
    public string dialogueText = "「눈을 떠라, 방랑자여... 길을 떠나기 전 그대에게 한 가지 은총을 베풀어주마.」";

    [Header("선택지 풀")]
    public List<BlessingChoice> choices = new();
}
