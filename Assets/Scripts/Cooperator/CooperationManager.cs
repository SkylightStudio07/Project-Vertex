using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CoopCharState
{
    public CoopCharData CharData;
    public int CoopLevel;
    public int CoopValue;
    public bool isLevelUp;
}




public class CooperationManager : MonoBehaviour
{
    // 호감도 레벨업에 필요한 호감도 포인트
    public static int[] CoopLevelUpValue = new int[] { 0, 2, 2, 2, 2, 3, 3, 3, 3, 3 };
    // 호감도 최대 레벨
    public static int MaxCoopLevel = 10;

    [SerializeField] private List<CoopCharData> coopCharList = new List<CoopCharData>();
    private Dictionary<string, CoopCharState> coopCharDict = new Dictionary<string, CoopCharState>();

    private void Awake()
    {
        foreach (var charData in coopCharList)
        {
            coopCharDict[charData.CharID] = new CoopCharState
            {
                CharData = charData,
                CoopLevel = 0,
                CoopValue = 0,
                isLevelUp = false
            };
        }
    }

    // 특정 캐릭터의 호감도 레벨 불러오기
    public int GetCoopLevel(string charID)
    {
        if (coopCharDict.TryGetValue(charID, out var charState))
        {
            return charState.CoopLevel;
        }
        return 0;
    }

    public bool IsCoopLevelUP(string charID)
    {
        if (coopCharDict.TryGetValue(charID, out var charState))
        {
            return charState.isLevelUp;
        }

        return false;
    }

    public void AddCoopValue(string charID, int value)
    {
        if (coopCharDict.TryGetValue(charID, out var charState))
        {
            charState.CoopValue += value;
            // 호감도 레벨업 체크
            while (charState.CoopLevel < MaxCoopLevel && charState.CoopValue >= CoopLevelUpValue[charState.CoopLevel])
            {
                charState.CoopValue -= CoopLevelUpValue[charState.CoopLevel];
                charState.CoopLevel++;
                charState.isLevelUp = true;
            }
            coopCharDict[charID] = charState; // 업데이트된 상태 저장
        }
    }
}

