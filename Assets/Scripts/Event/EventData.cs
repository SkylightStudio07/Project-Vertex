using System.Collections.Generic;
using UnityEngine;

// 이벤트 노드 하나를 표현하는 SO.
// 텍스트(eventJson, JSON)와 효과(choiceEffects, Inspector)를 분리해서 관리한다.
// 텍스트 포맷은 EventJsonData.cs 상단 주석 참고.
[CreateAssetMenu(fileName = "EventData", menuName = "Game Asset/Event Data")]
public class EventData : ScriptableObject
{
    public TextAsset eventJson;


    public List<EventChoiceEffect> choiceEffects;

    // 있으면 선택지 표시 전 DialogueView로 먼저 재생. 없으면 description만 표시.
    public TextAsset dialogueJson;

    // 이벤트별 고유 배경. 없으면 EventView의 defaultBackground 사용.
    public Sprite backgroundImage;
}

// 여기서 선택지별 효과 처리.
[System.Serializable]
public class EventChoiceEffect
{
    public List<CardEffect> effects;
}
