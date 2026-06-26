// JsonUtility는 Dictionary와 폴리모피즘 역직렬화를 지원하지 않는다.
// 그래서 노드는 id를 가진 배열로, line/choice/end는 하나의 클래스에 담아 type으로 구분한다.
// 포맷 전체 설명: Assets/Data/Dialogue/지침.md

using System.Collections.Generic;

[System.Serializable]
public class DialogueCharacterData
{
    public string id;
    public string name;
    public int slot; // 0(왼쪽) ~ 3(오른쪽), 최대 4명
}

[System.Serializable]
public class DialogueChoiceOption
{
    public string text;
    public string next; // 점프할 노드 id

    // 선택 시 실행할 부수효과 (호감도 포인트 지급 등). Card/Event 효과와 같은 CardEffect 라이브러리 재사용.
    public List<CardEffect> effects;
}

[System.Serializable]
public class DialogueLineData
{
    public string type; // "line" | "choice" | "end"

    // type == "line"
    public string speaker;  // characters[].id 참조
    public string emotion;  // idle/laugh/embarrassed/blush/annoyed/sad/confident/despise
    public string text;

    // type == "choice"
    public DialogueChoiceOption[] options;
}

[System.Serializable]
public class DialogueNodeData
{
    public string id;
    public DialogueLineData[] lines;
}

[System.Serializable]
public class DialogueScriptData
{
    public DialogueCharacterData[] characters;
    public string startNode;
    public DialogueNodeData[] nodes;
}
