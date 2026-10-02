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

    [Tooltip("이벤트로만 합류하는 동료 (성소 후보·잠긴 후보 실루엣에 나오지 않음)")]
    public bool isEventCompanion;

    [Tooltip("얼굴 크롭 — 마테리얼·훈련장 초상 칸에서 초상화(charImage)의 어디를 남길지 (0~1, 좌하단 원점)")]
    public Vector2 faceFocus = new(0.55f, 0.97f);
    [Tooltip("얼굴 크롭 확대 배율 (클수록 얼굴만)")]
    [Min(1f)] public float faceZoom = 3f;

    [Tooltip("마테리얼 글 JSON — 호감도 레벨마다 한 편 (형식: Docs/기획/마테리얼.md)")]
    public TextAsset materialJson;

    [Tooltip("합류하는 막 (훈련장 편성 '소속' 필터). 이벤트 동료는 isEventCompanion으로 따로 묶인다")]
    [Range(1, 3)] public int actNumber = 1;

    [Tooltip("훈련장에 데려가려면 필요한 호감도 레벨")]
    [Min(0)] public int trainingRequiredLevel = 2;

    [Header("전투 스탠딩 표시 (애니메이션이 없을 때)")]
    [Tooltip("스탠딩 그림 크기 배율 (드론처럼 작은 동료는 1보다 작게)")]
    public float battleScale = 1f;
    [Tooltip("슬롯 기준 위치 보정(px). 공중에 떠 있으면 y를 올린다")]
    public Vector2 battleOffset;
    [Tooltip("위아래로 떠다니는 폭(px). 0이면 정지")]
    public float floatAmplitude;
    [Tooltip("떠다니기 한 번 왕복 시간(초)")]
    public float floatPeriod = 2.4f;

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
    [Tooltip("휴식 노드에 들어설 때 하는 한 마디. 현재 호감도 레벨 이하인 묶음 중 minCoopLevel이 가장 높은 묶음에서 무작위")]
    public List<LeveledLines> restArrivalLines = new();

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

    // 이 동료 몫의 카드 전부 (합류 카드 · 보상 풀 · 호감도 해금 카드). 동료 귀환 시 덱에서 빼고, 훈련장 소속 필터에 쓴다.
    public IEnumerable<CardData> OwnedCards()
    {
        if (joinRewardCard != null) yield return joinRewardCard;
        foreach (var list in new[] { rewardPoolCommon, rewardPoolRare, rewardPoolUnique, unlockCardCoopLevel })
            if (list != null)
                foreach (var card in list)
                    if (card != null) yield return card;
    }

    // 호감도 레벨에 맞는 휴식 진입 대사 한 줄. 해당하는 묶음이 없으면 null.
    public string PickRestArrivalLine(int coopLevel)
    {
        LeveledLines best = null;
        foreach (var group in restArrivalLines)
        {
            if (group == null || group.lines == null || group.lines.Count == 0 || group.minCoopLevel > coopLevel) continue;
            if (best == null || group.minCoopLevel > best.minCoopLevel) best = group;
        }
        return best != null ? best.lines[Random.Range(0, best.lines.Count)] : null;
    }
}

// 호감도 레벨별로 달라지는 대사 묶음. minCoopLevel 이상이면 이 묶음을 쓸 수 있다.
[System.Serializable]
public class LeveledLines
{
    public int minCoopLevel;
    [TextArea(1, 3)]
    public List<string> lines = new();
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
