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

    [Header("맵 화면")]
    [Tooltip("작전 지도 스크롤 영역에 깔리는 이 막 전용 지형 선화 (3320×600, 층 간격 200 · 16층 기준). 비우면 맵의 기존 바탕을 쓴다")]
    public Texture2D mapTerrain;

    [Header("막 클리어")]
    [Tooltip("이 막 보스를 격파하면 런 클리어. 끄면 다음 막으로 넘어간다 (다음 막 ActData가 없어도 막 번호만 올려 진행)")]
    public bool isFinalAct;
}
