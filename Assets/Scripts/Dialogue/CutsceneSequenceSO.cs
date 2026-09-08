using System.Collections.Generic;
using UnityEngine;

// 컷씬↔다이얼로그가 번갈아 나오는 시퀀스 하나(프롤로그, 챕터 인트로, 엔딩 등)를 담는 에셋.
// EventRosterSO와 같은 원칙 — "순서 데이터"를 씬에 직접 박지 않고 에셋으로 분리해서
// CutscenePlayer 하나로 여러 시퀀스를 재사용 가능하게 한다.
[CreateAssetMenu(fileName = "CutsceneSequence", menuName = "Game Asset/Cutscene Sequence")]
public class CutsceneSequenceSO : ScriptableObject
{
    [System.Serializable]
    public class Segment
    {
        public enum SegmentType { Cutscene, Dialogue }
        public SegmentType type;
        public TextAsset json;
    }

    public List<Segment> segments = new();
}
