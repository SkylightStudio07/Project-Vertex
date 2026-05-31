using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[System.Serializable]
public struct CoopCharState
{
    public string charID;
    public CoopCharData charData;
    public int currentCoopLevel;
    public int currentCoopPoint;
    public bool isLevelUp;
    public Dictionary<int, RankEventData> rankEventDatas;
}

public class CooperationManager : MonoBehaviour
{
    public static CooperationManager Instance { get; private set; }

    [SerializeField] private List<CoopCharData> coopCharList = new List<CoopCharData>();
    [SerializeField] private Dictionary<string, CoopCharState> coopCharDict = new Dictionary<string, CoopCharState>();

    //Debug용 필드
    [SerializeField] private CoopCharState debugCoopChar;
    
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
                rankEventDatasDict[rankEventData.targetRank] = rankEventData;
            }

            coopCharDict[coopCharData.charID] = new CoopCharState
            {
                charID = coopCharData.charID,
                charData = coopCharData,
                currentCoopLevel = 0,
                currentCoopPoint = 0,
                isLevelUp = false,
                rankEventDatas = rankEventDatasDict
            };

            // Debug
            debugCoopChar = new CoopCharState
            {
                charID = coopCharData.charID,
                charData = coopCharData,
                currentCoopLevel = 0,
                currentCoopPoint = 0,
                isLevelUp = false,
                rankEventDatas = rankEventDatasDict
            };
            // Debug

            Debug.Log($"{coopCharDict[coopCharData.charID].charID} 캐릭터 호감도 : {coopCharDict[coopCharData.charID].currentCoopPoint}");
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
        //Debug
        debugCoopChar.currentCoopPoint += point;
        // 호감도 레벨업 체크
        if (debugCoopChar.currentCoopLevel+1 < debugCoopChar.charData.maxCoopLevel)
        {
            if (debugCoopChar.currentCoopPoint >= debugCoopChar.charData.rankEventDatas[debugCoopChar.currentCoopLevel + 1].requiredPoint)
            {
                debugCoopChar.isLevelUp = true;
            }
        }
        return;
        //Debug

        if (coopCharDict.TryGetValue(charID, out var charState))
        {
            charState.currentCoopPoint += point;
            // 호감도 레벨업 체크
            if (charState.currentCoopLevel+1 < charState.charData.maxCoopLevel)
            {
                if (charState.currentCoopPoint >= charState.charData.rankEventDatas[charState.currentCoopLevel + 1].requiredPoint)
                {
                    charState.isLevelUp = true;
                }
            }
            
            coopCharDict[charID] = charState; // 업데이트된 상태 저장
        }
    }

    public void SettlePoint(string charID)
    {
        // 호감도 레벨업 체크
        if (debugCoopChar.currentCoopLevel + 1 < debugCoopChar.charData.maxCoopLevel)
        {
            if (debugCoopChar.currentCoopPoint >= debugCoopChar.charData.rankEventDatas[debugCoopChar.currentCoopLevel + 1].requiredPoint)
            {
                debugCoopChar.currentCoopPoint -= debugCoopChar.charData.rankEventDatas[debugCoopChar.currentCoopLevel + 1].requiredPoint;
            }
        }

        debugCoopChar.currentCoopLevel++;
        debugCoopChar.isLevelUp = false;

        return;

        if (coopCharDict.TryGetValue(charID, out var charState))
        {
            // 호감도 레벨업 체크
            if (charState.currentCoopLevel + 1 < charState.charData.maxCoopLevel)
            {
                if (charState.currentCoopPoint >= charState.charData.rankEventDatas[charState.currentCoopLevel + 1].requiredPoint)
                {
                    charState.currentCoopPoint -= charState.charData.rankEventDatas[charState.currentCoopLevel + 1].requiredPoint;
                }
            }

            charState.currentCoopLevel++;
            charState.isLevelUp = false;

            coopCharDict[charID] = charState;
        }

    }

    //호감도 포인트 증가하는지 테스트하는 메소드
    public void AddCoopPoint_Debug()
    {
        AddCoopPoint("Cp_01", 2);
    }

    // 이벤트 완료 후 상태를 정산하는 메소드
    public void SettlePoint_Debug()
    {
        SettlePoint("Cp_01");
    }
}

