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

    // dialogueJson 없이 description만 보여주는 이벤트용 일러스트(캐릭터/오브젝트 등).
    // dialogueJson이 있으면 그쪽 DialogueView의 캐릭터 슬롯을 쓰므로 이 필드는 선택 사항 —
    // 비워두면 그냥 표시 안 됨(배경만 남음), 기존 이벤트엔 영향 없음.
    public Sprite illustration;

    // JSON의 choices[] 개수와 choiceEffects 개수가 어긋나면 인덱스가 밀려서 엉뚱한 선택지에
    // 엉뚱한 효과가 실행된다(에러 없이 조용히). 저장/인스펙터 수정 시마다 자동 체크해서 경고.
    private void OnValidate()
    {
        if (eventJson == null || choiceEffects == null) return;

        EventJsonData json;
        try { json = JsonUtility.FromJson<EventJsonData>(eventJson.text); }
        catch { return; } // 작성 중인 JSON이 일시적으로 깨진 상태일 수 있으니 파싱 실패는 조용히 무시

        if (json?.choices == null) return;

        if (json.choices.Length != choiceEffects.Count)
            Debug.LogWarning($"[EventData] '{name}': JSON 선택지 수({json.choices.Length})와 choiceEffects 수({choiceEffects.Count})가 다름 — 인덱스가 밀려서 엉뚱한 선택지에 엉뚱한 효과가 실행될 수 있음.");
    }
}

// 여기서 선택지별 효과 처리.
[System.Serializable]
public class EventChoiceEffect
{
    [SerializeReference, SubclassPicker] public List<CardEffect> effects;
}
