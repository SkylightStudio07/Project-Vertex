using System.Collections.Generic;
using UnityEngine;

// 런타임 중 현재 캐릭터 호감도 상태 저장
[System.Serializable]
public class CoopCharState
{
    public string charID;
    public CoopCharData charData;
    public int currentCoopLevel;
    public int currentCoopPoint;
    public bool isLevelUp;
    public Dictionary<int, RankEventData> rankEventDatasDict;
}

// 협력자 호감도 관리 스크립트
public class CooperationManager : MonoBehaviour
{
    public static CooperationManager Instance { get; private set; }

    [SerializeField] private List<CoopCharData> coopCharList = new List<CoopCharData>();
    private Dictionary<string, CoopCharState> coopCharDict = new Dictionary<string, CoopCharState>();

    
    private void Awake()
    {
        #region 싱글톤
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        #endregion

        foreach (var coopCharData in coopCharList)
        {
            Dictionary<int, RankEventData> rankEventDatasDict = new Dictionary<int, RankEventData>();
            foreach (var rankEventData in coopCharData.rankEventDatas)
            {
                rankEventDatasDict[rankEventData.targetLevel] = rankEventData;
            }

            coopCharDict[coopCharData.charID] = new CoopCharState
            {
                charID = coopCharData.charID,
                charData = coopCharData,
                currentCoopLevel = 0,
                currentCoopPoint = 0,
                isLevelUp = false,
                rankEventDatasDict = rankEventDatasDict
            };
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
            // 호감도 레벨업 체크
            if (charState.currentCoopLevel + 1 < charState.charData.maxCoopLevel)
            {
                if (charState.currentCoopPoint >= charState.rankEventDatasDict[charState.currentCoopLevel].requiredPoint)
                {
                    charState.isLevelUp = true;
                }
            }
        }
        Debug.Log($"{charID} 호감도 포인트 추가 완료. 현재 레벨: {coopCharDict[charID].currentCoopLevel}, 현재 포인트: {coopCharDict[charID].currentCoopPoint}");
    }

    // 호감도 이벤트 완료 후 호감도 상태 결산 메소드 
    public void SettlePoint(string charID)
    {
        if (coopCharDict.TryGetValue(charID, out var charState))
        {
            if (charState.currentCoopLevel == 0) return;
            // 호감도 레벨업 체크
            if (charState.currentCoopLevel + 1 <= charState.charData.maxCoopLevel)
            {
                charState.currentCoopPoint -= charState.rankEventDatasDict[charState.currentCoopLevel].requiredPoint;
                if (charState.currentCoopPoint >= charState.rankEventDatasDict[charState.currentCoopLevel].requiredPoint)
                {
                    charState.isLevelUp = true;
                }
                else
                {
                    charState.isLevelUp = false;
                }
                charState.currentCoopLevel++;
            }
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
        int currentCoopLevel = Mathf.Min(GetCoopLevel(charID), coopCharData.unlockCardCoopLevel.Count);
        if (coopCharData == null) return;

        if (coopCharData.joinRewardCard != null)
        {
            DeckManager.Instance.AddCardToPlayerDeck(coopCharData.joinRewardCard);
        }

        foreach (CardData card in coopCharData.battleRewardCardPool)
        {
            GameManager.Instance.AddCardToRewardPool(card);
        }

        for (int coopLevel = 0; coopLevel < currentCoopLevel; coopLevel++)
        {
            GameManager.Instance.AddCardToRewardPool(coopCharData.unlockCardCoopLevel[coopLevel]);
        }

        Debug.Log($"{charID} 성소 보상 적용 완료");
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
        }
    }
}
