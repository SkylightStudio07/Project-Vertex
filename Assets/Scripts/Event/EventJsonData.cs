[System.Serializable]
public class EventJsonData
{
    public string title;
    public string description;
    public EventChoiceJson[] choices;
}

[System.Serializable]
public class EventChoiceJson
{
    public string choiceText;
    public string resultText;
}
