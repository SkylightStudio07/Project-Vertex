using UnityEngine;

// 막(챕터) 하나의 표시 정보. GameManager의 acts 목록에 등록하면 해당 막 번호일 때 쓰인다.
// 지금은 맵 도입 연출(막 이름 배너 + 보스 → 시작점 맵 패닝)에만 쓰고,
// 막별 MapConfig·배경 등 막 단위 데이터가 생기면 여기에 모은다.
[CreateAssetMenu(fileName = "Act", menuName = "Game Asset/Act Data")]
public class ActData : ScriptableObject
{
    [Min(1)] public int actNumber = 1;
    [Tooltip("막 이름. 비워 두면 배너를 띄우지 않는다")]
    public string actName;

    [Header("맵 도입 연출")]
    [Tooltip("새 막 맵을 처음 열 때 보스 쪽 끝에서 시작점까지 당겨 오는 시간(초). 0이면 패닝 없이 시작점에서 연다")]
    [Min(0f)] public float mapPanDuration = 3.2f;
}
