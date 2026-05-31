using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CoopCharState
{
    public string charID;
    public CoopCharData charData;
    public int currentCoopLevel;
    public int currentCoopPoint;
    public bool isLevelUp;
    public Dictionary<int, RankEventData> rankEventDatasDict;
}

public class CooperationManager : MonoBehaviour
{
    public static CooperationManager Instance { get; private set; }

    [SerializeField] private List<CoopCharData> coopCharList = new List<CoopCharData>();
    [SerializeField] private Dictionary<string, CoopCharState> coopCharDict = new Dictionary<string, CoopCharState>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


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

    public string[] GetCoopDialogue(string charID, int coopLevel)
    {
        if (coopCharDict.TryGetValue(charID, out var charState))
        {
            // 다이얼로그 스크립트 넘기기
            return null;
        }
        else return null;
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
            
            coopCharDict[charID] = charState; // 업데이트된 상태 저장
        }
    }

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

            coopCharDict[charID] = charState;
        }

    }
}

