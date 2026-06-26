using System;
using UnityEngine;

// 협력자 호감도 랭크업 이벤트 발생 시 DialogueView로 대사를 재생하는 매니저.
// 줄 단위 진행/선택지 분기는 DialogueView가 전부 책임지므로, 여기서는
// "재생할 JSON을 찾아서 넘겨주는 것"과 "끝났다는 이벤트를 알리는 것"만 담당한다.
public class DialogueManager : MonoBehaviour
{
    [SerializeField] private DialogueView dialogueView;
    private CooperationManager cooperationManager;

    public event Action isDialogueEnd;

    private void Start()
    {
        cooperationManager = CooperationManager.Instance;
    }

    public void LoadRelationshipEvent(string charID)
    {
        if (cooperationManager == null)
        {
            Debug.LogWarning("[DialogueManager] CooperationManager.Instance가 없음.");
            return;
        }

        if (!cooperationManager.IsCoopLevelUP(charID))
        {
            Debug.Log("현재 이벤트 발생 조건이 부족합니다.");
            return;
        }

        int currentCoopLevel = cooperationManager.GetCoopLevel(charID);
        TextAsset dialogueJson = cooperationManager.GetCoopDialogue(charID, currentCoopLevel);

        if (dialogueJson == null)
        {
            Debug.LogWarning($"[DialogueManager] {charID} 레벨 {currentCoopLevel}에 등록된 대사가 없음.");
            isDialogueEnd?.Invoke();
            return;
        }

        dialogueView.Play(dialogueJson, () => isDialogueEnd?.Invoke());
    }
}
