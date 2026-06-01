using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogueUI;
    private CooperationManager cooperationManager;

    private string[] currentEventDialogues;

    private int currentEventDialoguesIndex = 0;

    public event Action isDialogueEnd;

    private void Start()
    {
        cooperationManager = CooperationManager.Instance;
    }
    public void LoadRelationshipEvent(string charID)
    {
        if (cooperationManager.IsCoopLevelUP(charID))
        {
            int currentCoopLevel = cooperationManager.GetCoopLevel(charID);
            currentEventDialogues = cooperationManager.GetCoopDialogue(charID, currentCoopLevel);

            currentEventDialoguesIndex = 0;
            ShowNextDialogue();

        }
        else
        {
            Debug.Log("현재 이벤트 발생 조건이 부족합니다.");
        }
    }


    public void ShowNextDialogue()
    {
        if (!dialogueUI.gameObject.activeSelf)
        {
            dialogueUI.gameObject.SetActive(true);
        }

        if (currentEventDialoguesIndex > currentEventDialogues.Length)
        {
            dialogueUI.gameObject.SetActive(false);

            isDialogueEnd?.Invoke();
            return;
        }

        if (currentEventDialogues[currentEventDialoguesIndex] == "Choices")
        {
            //Choices 스크립트 출력하기
        }
        else
        {
            // DialogueUI에서 다음 스크립트 출력하기
        }



    }
}
