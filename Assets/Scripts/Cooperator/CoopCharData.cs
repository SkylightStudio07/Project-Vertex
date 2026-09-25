using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCoopChar", menuName = "Game Asset/Coop Character")]
public class CoopCharData : CharData
{
    [Header("성소 선택 화면 아트 (비어 있으면 기존 캐릭터 이미지 사용)")]
    public Sprite sanctuarySelectionArt;
    public Sprite sanctuaryHoverArt;

    [Tooltip("성소 기본 아트의 패널 내 정렬 위치. 원본 비율과 전체 이미지는 유지됩니다. (0,0)은 좌하단, (1,1)은 우상단")]
    public Vector2 sanctuarySelectionFocus = new Vector2(0.5f, 0.5f);
    [Tooltip("호버 아트의 패널 내 정렬 위치. 아트가 미지정이면 기본 아트의 정렬 위치를 사용")]
    public Vector2 sanctuaryHoverFocus = new Vector2(0.5f, 0.5f);

    [Header("성소 선택 상세 화면 전신 아트 (초상화와 별도)")]
    public Sprite sanctuaryFullArt;

    [Header("호감도 레벨 당 해금 카드")]
    public List<CardData> unlockCardCoopLevel = new();

    [Header("휴식 캠프 (휴식 노드에서 각자 자리에서 하는 일)")]
    [Tooltip("캠프에서 보일 포즈 스프라이트. 비우면 standingSprite → charImage 순으로 대체")]
    public Sprite campSprite;
    [Tooltip("캠프에서 하고 있는 일 (예: 장비 정비 중)")]
    public string campActivity;
    [Tooltip("캐릭터를 눌렀을 때 무작위로 나오는 잡담 한 줄")]
    [TextArea(1, 3)]
    public List<string> campLines = new();

    [Header("합류 시 획득 카드")]
    public CardData joinRewardCard;

    [Header("합류 시 재생할 짧은 대사 (선택)")]
    public TextAsset joinDialogueJson;

    [Header("전투 보상 카드 풀 (등급별)")]
    public List<CardData> rewardPoolCommon = new();
    public List<CardData> rewardPoolRare   = new();
    public List<CardData> rewardPoolUnique = new();

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
