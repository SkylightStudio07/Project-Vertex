using UnityEngine;
using System.Collections.Generic;



// 성소 관리 매니저
public class HolyPlaceManager : MonoBehaviour
{
    public static HolyPlaceManager Instance { get; private set; }

    // 인스펙터에서 층마다 선택 가능한 캐릭터 ID 리스트를 받고, 
    // 실행 시 딕셔너리에 저장하여 현재 층에 따라 선택 가능한 캐릭터를 불러오는 방식
    private Dictionary<int, List<string>> selectableCharPerFloor;
    [SerializeField] private HolyPlaceData holyPlaceData;

    private void Awake()
    {
        #region 싱글톤 패턴
        // 전투(런) 씬 전용: 같은 오브젝트(GameplayManager)에 GameManager 등이 붙어 있어 DontDestroyOnLoad를 쓰지 않는다.
        // 씬을 다시 불러오면 새 인스턴스가 Instance가 된다.
        Instance = this;
#endregion

        // 층마다 선택 가능한 캐릭터 ID 리스트를 딕셔너리에 저장
        selectableCharPerFloor = new Dictionary<int, List<string>>();
        foreach (var data in holyPlaceData.selectableCharDatas)
        {
            selectableCharPerFloor[data.floor] = data.charIDs;
        }
    }

    // 현재 층에 따라 선택 가능한 캐릭터 ID 리스트 반환
    public List<string> GetSeletableChar(int floor)
    {
        if (selectableCharPerFloor.TryGetValue(floor, out var charIDs))
        {
            return charIDs;
        }
        return null;
    }
    
    // 성소 이벤트 오픈 메소드
    public void OpenHolyPlaceEvent()
    {

    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
