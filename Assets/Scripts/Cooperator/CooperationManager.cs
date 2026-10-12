using System.Collections.Generic;
using UnityEngine;

// 런타임 중 현재 캐릭터 호감도 상태 저장
// 6-27 | 박근혁 : 런 중에 합류했는지 여부 나타내는 isJoinedInRun 추가
[System.Serializable]
public class CoopCharState
{
    public string charID;
    public CoopCharData charData;
    public int currentCoopLevel;
    public int currentCoopPoint;
    public bool isLevelUp;
    public bool isJoinedInRun;      // 런 중에 합류했는지 여부(런이 시작될 때마다 false로 초기화 되어야 함!!)
    public Dictionary<int, RankEventData> rankEventDatasDict;
    public CardData joinCardInstance; // 합류 때 덱에 넣은 합류 카드 복사본 (귀환 때 이 한 장만 뺀다)
}

// 협력자 호감도 관리 스크립트
//
// 씬 전환: 호감도 상태(coopCharDict)는 static이라 씬을 넘어 유지되고, 컴포넌트는 씬마다 하나씩 있는 창구다(가장 최근 씬 것이 Instance).
// DontDestroyOnLoad를 쓰지 않는다 — 전투 씬에서는 GameManager 등과 같은 오브젝트(GameplayManager)에 붙어 있다.
// 씬마다 coopCharList에 등록한 캐릭터를 합친다(이미 있는 캐릭터의 호감도는 건드리지 않음).
// → 로비에 Cp_01만, 전투 씬에 Cp_01~03이 등록돼 있어도 로비에서 출정하면 세 명 모두 쓸 수 있다.
public class CooperationManager : MonoBehaviour
{
    public static CooperationManager Instance { get; private set; }

    [SerializeField] private List<CoopCharData> coopCharList = new List<CoopCharData>();
    private static Dictionary<string, CoopCharState> coopCharDict = new Dictionary<string, CoopCharState>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { coopCharDict = new Dictionary<string, CoopCharState>(); Instance = null; }

    private void Awake()
    {
        Instance = this;
        Register(coopCharList);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // 아직 없는 캐릭터만 새 상태로 등록한다
    private static void Register(List<CoopCharData> list)
    {
        foreach (var coopCharData in list)
        {
            if (coopCharData == null || coopCharDict.ContainsKey(coopCharData.charID)) continue;
            Dictionary<int, RankEventData> rankEventDatasDict = new Dictionary<int, RankEventData>();
            foreach (var rankEventData in coopCharData.rankEventDatas)
            {
                rankEventDatasDict[rankEventData.targetLevel] = rankEventData;
            }

            // 호감도는 PlayerRecord에 저장된 값으로 시작한다 (게임을 껐다 켜도 유지)
            PlayerRecord.TryGetCoop(coopCharData.charID, out int savedLevel, out int savedPoint);
            coopCharDict[coopCharData.charID] = new CoopCharState
            {
                charID = coopCharData.charID,
                charData = coopCharData,
                currentCoopLevel = savedLevel,
                currentCoopPoint = savedPoint,
                isLevelUp = false,
                isJoinedInRun = false,
                rankEventDatasDict = rankEventDatasDict
            };
        }
    }

    // 런 시작 시, 모든 캐릭터의 isJoinedInRun 상태를 false로 초기화
    public void ResetOnRunStart()
    {
        foreach (var charState in coopCharDict.Values)
        {
            charState.isJoinedInRun = false;
        }
    }

    // 특정 캐릭터의 호감도 레벨 불러오기
    public int GetCoopLevel(string charID)
    {
        if (coopCharDict.TryGetValue(charID, out var charState))
        {
            return charState.currentCoopLevel;
        }
        return 0;
    }

    // 호감도 이벤트 발생 여부를 체크
    public bool IsCoopLevelUP(string charID)
    {
        if (coopCharDict.TryGetValue(charID, out var charState))
        {
            return charState.isLevelUp;
        }

        return false;
    }

    // 호감도 랭크업 시 재생할 대사 JSON 반환. 해당 레벨에 등록된 이벤트가 없으면 null.
    public TextAsset GetCoopDialogue(string charID, int coopLevel)
    {
        if (coopCharDict.TryGetValue(charID, out var charState) &&
            charState.rankEventDatasDict.TryGetValue(coopLevel, out var rankEvent))
        {
            return rankEvent.dialogueJson;
        }
        return null;
    }

    // charID로 협력자 원본 데이터를 가져오는 메소드
    public CoopCharData GetCoopCharData(string charID)
    {
        if (coopCharDict.TryGetValue(charID, out var charState))
        {
            return charState.charData;
        }

        Debug.LogWarning($"{charID}에 해당하는 협력자 데이터 없음");
        return null;
    }

    // 경고 없이 조회 (대사 화자 ID처럼 협력자가 아닐 수도 있는 값을 확인할 때)
    public bool TryGetCoopCharData(string charID, out CoopCharData data)
    {
        data = null;
        if (string.IsNullOrEmpty(charID) || !coopCharDict.TryGetValue(charID, out var charState)) return false;
        data = charState.charData;
        return data != null;
    }

    // 등록된 협력자 전체 (성소 잠긴 후보 칸 채우기 등)
    public IEnumerable<CoopCharData> AllCharData()
    {
        foreach (var charState in coopCharDict.Values)
            if (charState.charData != null) yield return charState.charData;
    }

    public Sprite GetCoopSprite(string CharID)
    {
        if (coopCharDict.TryGetValue(CharID, out var charState))
        {
            return charState.charData.charImage;
        }
        return null;
    }

    // 호감도 포인트 추가 메소드
    public void AddCoopPoint(string charID, int point)
    {
        if (coopCharDict.TryGetValue(charID, out var charState))
        {
            if (charState.currentCoopLevel == 0) return;

            charState.currentCoopPoint += point;
            // 호감도 레벨업 체크 — 다음 레벨 이벤트가 있고 포인트가 찼으면 랭크업 대기
            if (TryGetNextRankEvent(charState, out var next) && charState.currentCoopPoint >= next.requiredPoint)
                charState.isLevelUp = true;
            SaveCoop(charState);
        }
        Debug.Log($"{charID} 호감도 포인트 추가 완료. 현재 레벨: {coopCharDict[charID].currentCoopLevel}, 현재 포인트: {coopCharDict[charID].currentCoopPoint}");
    }

    // 호감도 이벤트 완료 후 호감도 상태 결산 메소드 
    public void SettlePoint(string charID)
    {
        if (coopCharDict.TryGetValue(charID, out var charState))
        {
            if (charState.currentCoopLevel == 0) return;
            // 레벨업 확정 → 남은 포인트로 다음 랭크업도 바로 대기할지 본다
            if (TryGetNextRankEvent(charState, out var current))
            {
                charState.currentCoopPoint -= current.requiredPoint;
                charState.currentCoopLevel++;
                charState.isLevelUp = TryGetNextRankEvent(charState, out var next) && charState.currentCoopPoint >= next.requiredPoint;
            }
            SaveCoop(charState);
        }
        Debug.Log($"{charID} 호감도 레벨업 완료. 현재 레벨: {coopCharDict[charID].currentCoopLevel}, 현재 포인트: {coopCharDict[charID].currentCoopPoint}");

    }

    // 성소에서 캐릭터를 선택할 때, 카드 풀을 GameManager에 추가하는 메소드
    public void SelectChar(string charID)
    {
        if (string.IsNullOrEmpty(charID))
        {
            Debug.LogWarning("선택된 협력자 ID 없음");
            return;
        }

        CoopCharData coopCharData = GetCoopCharData(charID);
        // 널 검사는 반드시 역참조보다 먼저. (예전엔 아래 Mathf.Min에서 먼저 터져서
        //  coopCharList에 등록 안 된 협력자를 합류시키면 NullReferenceException이 났다)
        if (coopCharData == null) return;

        int currentCoopLevel = Mathf.Min(GetCoopLevel(charID), coopCharData.unlockCardCoopLevel.Count);

        if (coopCharData.joinRewardCard != null)
        {
            coopCharDict[charID].joinCardInstance = DeckManager.Instance.AddCardToPlayerDeck(coopCharData.joinRewardCard);
        }

        // 등급별 리스트를 직접 합산 — battleRewardCardPool(flat) 대신 rarity 분리된 리스트 사용
        GameManager.Instance.AddCardsToRewardPool(coopCharData.rewardPoolCommon, CardData.CardRarity.Common);
        GameManager.Instance.AddCardsToRewardPool(coopCharData.rewardPoolRare,   CardData.CardRarity.Rare);
        GameManager.Instance.AddCardsToRewardPool(coopCharData.rewardPoolUnique, CardData.CardRarity.Unique);

        for (int coopLevel = 0; coopLevel < currentCoopLevel; coopLevel++)
        {
            GameManager.Instance.AddCardToRewardPool(coopCharData.unlockCardCoopLevel[coopLevel]);
        }

        Debug.Log($"{charID} 성소 보상 적용 완료");

        // 사전에 없는 ID면 인덱서는 KeyNotFoundException을 던진다 — 경고만 남기고 넘어간다.
        if (!coopCharDict.TryGetValue(charID, out var charState))
        {
            Debug.LogWarning($"{charID} 상태가 없어 합류 처리(isJoinedInRun)를 건너뜀 — CooperationManager의 coopCharList 등록 확인 필요.");
            return;
        }
        charState.isJoinedInRun = true;
        // 처음 합류하면 인연이 생긴다 (호감도 Lv.0 → Lv.1). Lv.0에서는 포인트도 랭크업도 받지 않으므로 이게 시작점이다.
        if (charState.currentCoopLevel == 0)
        {
            charState.currentCoopLevel = 1;
            SaveCoop(charState);
        }
    }

    // 특정 협력자가 이번 런에 합류했는지 여부. 이벤트 조건(CompanionJoinedCondition) 등에서 사용.
    public bool IsJoinedInRun(string charID)
    {
        return coopCharDict.TryGetValue(charID, out var charState) && charState.isJoinedInRun;
    }

    // 현재 런에 합류한 캐릭터들의 상태를 반환하는 메소드
    public List<CoopCharState> GetJoinedInRunCharStates()
    {
        List<CoopCharState> joinedCharStates = new List<CoopCharState>();
        foreach (var charState in coopCharDict.Values)
        {
            if (charState.isJoinedInRun)
            {
                joinedCharStates.Add(charState);
            }
        }
        joinedCharStates.Sort((x, y) => x.charID.CompareTo(y.charID));
        return joinedCharStates;
    }

    // 런 동행 인원 상한 (플레이어 포함 4인). 가득 차면 성소에서 합류할 수 없다 — 휴식 노드에서 귀환시켜 자리를 비운다.
    public const int MaxCompanionsInRun = 3;
    public bool IsPartyFull => GetJoinedInRunCharStates().Count >= MaxCompanionsInRun;

    // 휴식 노드 "동료 귀환": 런에서 빠지고, 그 동료 몫의 카드는 덱과 보상 풀에서 뺀다.
    //   - 합류 카드: 합류 때 넣은 그 한 장만 (시작 덱의 같은 카드는 남긴다 — 예: 치하라 쇼의 재장전)
    //   - 보상 풀·호감도 카드: 플레이어 기본 카드(시작 덱·기본 보상 풀)와 겹치지 않는 것만 전부
    public void DismissFromRun(string charID)
    {
        if (!coopCharDict.TryGetValue(charID, out var charState) || !charState.isJoinedInRun) return;
        var toRemove = CardsLeavingWith(charID);
        charState.isJoinedInRun = false;

        if (DeckManager.Instance != null)
            DeckManager.Instance.RemoveCardsWhere(toRemove.Contains);
        charState.joinCardInstance = null;
        if (GameManager.Instance != null) GameManager.Instance.RemoveCardsFromRewardPool(ExclusiveCards(charState.charData));
        Debug.Log($"[CooperationManager] {charID} 거점 귀환 — 덱에서 {toRemove.Count}장 제거");
    }

    // 귀환하면 덱에서 빠질 카드(덱 안의 복사본들). 확인 창 안내에도 쓴다.
    public List<CardData> CardsLeavingWith(string charID)
    {
        var result = new List<CardData>();
        if (!coopCharDict.TryGetValue(charID, out var charState) || DeckManager.Instance == null) return result;
        var exclusiveKeys = new HashSet<string>(ExclusiveCards(charState.charData).ConvertAll(PlayerRecord.KeyOf));
        foreach (var card in DeckManager.Instance.PlayerDeck)
            if (card != null && (card == charState.joinCardInstance || exclusiveKeys.Contains(PlayerRecord.KeyOf(card))))
                result.Add(card);
        return result;
    }

    // 동료 몫 카드 중 플레이어 기본 카드와 겹치지 않는 것
    private static List<CardData> ExclusiveCards(CoopCharData data)
    {
        var list = new List<CardData>();
        foreach (var card in data.OwnedCards())
        {
            bool playerCard = (DeckManager.Instance != null && DeckManager.Instance.IsStartingCard(card))
                              || (GameManager.Instance != null && GameManager.Instance.IsBasePoolCard(card));
            if (!playerCard) list.Add(card);
        }
        return list;
    }

    // 지금 레벨에서 다음 레벨로 가는 랭크업 이벤트 (최대 레벨이거나 이벤트가 없으면 false).
    // 예전 판정(currentLevel + 1 < max)은 최대 레벨 하나 전에서 멈췄다 — 최대 3이면 Lv.2까지만 올랐음.
    private static bool TryGetNextRankEvent(CoopCharState charState, out RankEventData rankEvent)
    {
        rankEvent = null;
        return charState.currentCoopLevel < charState.charData.maxCoopLevel
               && charState.rankEventDatasDict.TryGetValue(charState.currentCoopLevel, out rankEvent);
    }

    private static void SaveCoop(CoopCharState charState)
        => PlayerRecord.SetCoop(charState.charID, charState.currentCoopLevel, charState.currentCoopPoint);

    // 훈련장 동행: 합류 카드·보상 풀 없이 전투 자리만 차지한다
    public void JoinForTraining(string charID)
    {
        if (coopCharDict.TryGetValue(charID, out var charState))
            charState.isJoinedInRun = true;
    }

    // 호감도 증가 테스트 메소드
    public void Debug1()
    {
        AddCoopPoint("Cp_01", 10);
    }

    // 호감도 증가 테스트 메소드 2
    public void Debug2()
    {
        SettlePoint("Cp_01");
    }

    // 캐릭터 호감도 증가 테스트 메소드 3
    public void Debug3()
    {
        if (coopCharDict.TryGetValue("Cp_01", out var charState))
        {
            charState.currentCoopLevel = 1;
            SaveCoop(charState);
        }
    }

    // 테스트용: 카드 보상 없이 합류 상태(isJoinedInRun)만 켜기
    public void DebugJoin(string charID)
    {
        if (coopCharDict.TryGetValue(charID, out var charState))
        {
            charState.isJoinedInRun = true;
            Debug.Log($"[Test] {charID} 합류 처리");
        }
        else Debug.LogWarning($"[Test] {charID} 없음 — coopCharList에 등록된 charID인지 확인");
    }

    // 인스펙터 우클릭으로 실행하는 테스트 합류 (Cp_01)
    [ContextMenu("Test/Join Cp_01")]
    private void TestJoinCp01() => DebugJoin("Cp_01");
}
