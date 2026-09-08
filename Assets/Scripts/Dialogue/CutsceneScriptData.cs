// 컷씬 JSON 역직렬화용. 다이얼로그(DialogueScriptData)와 달리 분기가 필요 없어
// 순서대로 재생되는 beats 배열 하나로 충분하다. JsonUtility 제약(Dictionary/폴리모피즘 불가)은
// 다이얼로그 포맷과 동일하게 따른다 — 상세: Assets/Data/Dialogue/지침.md

[System.Serializable]
public class CutsceneBeatData
{
    public string background; // CutsceneView.backgrounds에 등록된 키. 비워두면 이전 배경 유지.
    public string fade;       // "in" | "out" | "cut"(기본값 — 페이드 없이 즉시 전환)
    public string caption;    // 자막. 비워두면 자막 없이 그림만 표시.
}

[System.Serializable]
public class CutsceneScriptData
{
    public CutsceneBeatData[] beats;
}
