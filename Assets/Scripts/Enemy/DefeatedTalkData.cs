using System.Collections.Generic;
using UnityEngine;

// 격퇴 후 대화 (던파식): 쓰러뜨려도 사라지지 않고 서 있다가, 보상 화면을 닫은 뒤 클릭하면 스탠딩 + 대사.
// 호감도는 마키나 · 토냐와 같은 BlessingAffinityManager에 entityId로 쌓인다 (등급 0~4 = 0 / 2 / 4 / 6 / 8).
// 합류 후보면 호감도가 높을 때만 "데려간다 / 두고 간다" 선택지가 뜬다 (낮으면 아예 따라오지 않는다).
// 붙이는 곳: EnemyData.defeatedTalk. 흐름: EnemyView(격퇴 시 남기기) → RewardsView(진행) → DefeatedTalkSession
[CreateAssetMenu(fileName = "DefeatedTalk", menuName = "Game Asset/Defeated Talk")]
public class DefeatedTalkData : ScriptableObject
{
    [Header("대상")]
    [Tooltip("호감도 · 플래그 저장 키 (예: karina)")]
    public string entityId = "karina";
    public string displayName = "카리나 빈터발트";
    public string affiliation = "1막 엘리트 / 보급 창고";
    public Color themeColor = new(0.85f, 0.72f, 0.38f, 1f);
    [Tooltip("대사창 초상 칸 (얼굴)")]
    public Sprite portrait;
    [Tooltip("클릭했을 때 화면에 뜨는 스탠딩")]
    public Sprite standing;

    [Header("호감도")]
    [Tooltip("격퇴할 때마다")]
    public float defeatAffinityGain = 0.5f;
    [Tooltip("대화를 나눴을 때 (한 번 만날 때 한 번)")]
    public float talkAffinityGain = 1f;

    [Header("대사 — 조건(호감도 · 동료 · 플래그)이 맞는 것 중 우선순위가 가장 높은 하나")]
    public List<BlessingDialogueSequence> sequences = new();

    [Header("합류 (비우면 합류 없음)")]
    [Tooltip("데려갈 수 있는 동료. 성소에는 뜨지 않게 isEventCompanion을 켜 둘 것")]
    public CoopCharData recruit;
    [Tooltip("이 등급 이상이어야 데려가기 선택지가 뜬다")]
    [Range(0, 4)] public int recruitMinTier = 3;
    [TextArea(1, 3)] public string recruitPrompt = "…따라가 줄 수도 있어. 어쩔래?";
    public string recruitYes = "데려간다";
    public string recruitNo = "두고 간다";
    [Tooltip("데려갔을 때 켜는 플래그 (이후 대사 · 이벤트 조건용)")]
    public string recruitedFlag = "karina_recruited";

    public BlessingDialogueSequence Pick(float affinity, System.Func<string, bool> isJoined, HashSet<string> flags)
    {
        BlessingDialogueSequence best = null;
        foreach (var s in sequences)
            if (s != null && s.Evaluate(affinity, isJoined, flags) && (best == null || s.priority > best.priority))
                best = s;
        return best;
    }
}
