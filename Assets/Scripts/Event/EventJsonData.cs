// 이벤트 노드 텍스트(JSON) 역직렬화용 클래스.
// EventData.eventJson(TextAsset)을 JsonUtility.FromJson<EventJsonData>()로 파싱.
// choices[]의 순서는 EventData.choiceEffects[]와 인덱스로 매칭된다.
// (choices[0] ↔ choiceEffects[0], choices[1] ↔ choiceEffects[1] ...)
// 두 배열의 길이가 다를 때 체크하는 코드는 없으므로 Inspector에서 직접 맞출 것.
[System.Serializable]
public class EventJsonData
{
    public string title;
    public string[] description; // 페이지 단위. 화살표/스페이스/엔터로 한 페이지씩 진행(EventView 참고).
    public EventChoiceJson[] choices;
}

[System.Serializable]
public class EventChoiceJson
{
    public string choiceText;   // 선택지 버튼에 표시될 문구
    public string[] resultText; // 선택 후 표시되는 결과 텍스트. description과 동일하게 페이지 단위.
}
