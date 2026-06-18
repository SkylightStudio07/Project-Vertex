using System.Collections.Generic;
using UnityEngine;

// 성소 데이터 SO
[CreateAssetMenu(fileName = "NewHolyPlaceData")]
public class HolyPlaceData : ScriptableObject
{
    public List<SelectableCharDataInHolyPlace> selectableCharDatas;
}

// 층별 선택 가능한 캐릭터들의 ID를 담는 구조체
[System.Serializable]
public struct SelectableCharDataInHolyPlace
{
    public int floor;
    public List<string> charIDs;
}