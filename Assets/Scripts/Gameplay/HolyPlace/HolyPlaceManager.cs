using UnityEngine;
using System.Collections.Generic;

// 성소 관리 매니저
public class HolyPlaceManager : MonoBehaviour
{
    public static HolyPlaceManager Instance { get; private set; }

    // 인스펙터에서 층별 선택 가능 캐릭터 ID 리스트를 받고,
    // 시작 시 딕셔너리로 변환해 현재 층에서 선택 가능한 캐릭터를 불러오는 방식
    private Dictionary<int, List<string>> selectableCharPerFloor;
    [SerializeField] private HolyPlaceData holyPlaceData;

    // 구출 의뢰를 완료하면 켜지는 플래그 (QuestData.RescueFlag 와 같은 규칙)
    public static string RescueFlagOf(string charID) => "coop_rescued_" + charID;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // 디버그: 층과 상관없이 성소 후보로 올릴 캐릭터 (CooperationManagerEditor 에서 토글)
    public static readonly HashSet<string> DebugForcedCandidates = new HashSet<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetDebug() => DebugForcedCandidates.Clear();
#endif

    private void Awake()
    {
        #region 싱글톤 설정
        // 씬(층) 별 존재: 같은 오브젝트(GameplayManager)에 GameManager 등과 붙어 있어 DontDestroyOnLoad를 쓰지 않는다.
        // 씬을 다시 불러오면 새 인스턴스가 Instance가 된다.
        Instance = this;
        #endregion

        // 데이터에 적힌 층별 캐릭터 ID 리스트를 딕셔너리로 변환
        selectableCharPerFloor = new Dictionary<int, List<string>>();
        foreach (var data in holyPlaceData.selectableCharDatas)
        {
            selectableCharPerFloor[data.floor] = data.charIDs;
        }
    }

    // 현재 층에서 선택 가능한 캐릭터 ID 리스트 반환.
    // 층 목록에 더해, 구출 의뢰를 끝낸 캐릭터(coop_rescued_{ID})는 어느 층의 성소에서든 후보가 된다.
    public List<string> GetSeletableChar(int floor)
    {
        var result = new List<string>();
        if (selectableCharPerFloor.TryGetValue(floor, out var charIDs)) result.AddRange(charIDs);

        var coop = CooperationManager.Instance;
        var flags = BlessingAffinityManager.Instance;
        if (coop != null && flags != null)
        {
            foreach (var data in coop.AllCharData())
                if (!result.Contains(data.charID) && flags.HasFlag(RescueFlagOf(data.charID))) result.Add(data.charID);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        foreach (var id in DebugForcedCandidates)
            if (!result.Contains(id)) result.Add(id);
#endif
        return result.Count > 0 ? result : null;
    }

    // 성소 이벤트 실행 메소드
    public void OpenHolyPlaceEvent()
    {

    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
