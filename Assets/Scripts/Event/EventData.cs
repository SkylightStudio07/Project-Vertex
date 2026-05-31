using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventData", menuName = "Game Asset/Event Data")]
public class EventData : ScriptableObject
{
    public TextAsset eventJson;
    public List<EventChoiceEffect> choiceEffects;
}

[System.Serializable]
public class EventChoiceEffect
{
    public List<CardEffect> effects;
}
