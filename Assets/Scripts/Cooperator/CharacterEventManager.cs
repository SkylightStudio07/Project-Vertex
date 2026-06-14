using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterEventManager : MonoBehaviour
{
    [SerializeField] private bool isInProgressDialogue = false;
    public bool IsInProgressCoopEvent => isInProgressDialogue;

    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private CooperationManager cooperationManager;

    private void Start()
    {
        cooperationManager = CooperationManager.Instance;
        dialogueManager.isDialogueEnd += isDialogueEventEnd;
    }

    private void OnDisable()
    {
        dialogueManager.isDialogueEnd -= isDialogueEventEnd;
    }


    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (isInProgressDialogue)
            {
                dialogueManager.ShowNextDialogue();
            }
        }

        // Debug용 입력코드
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            if (!isInProgressDialogue)
            {
                dialogueManager.LoadRelationshipEvent("Cp_01");
                Debug.Log("이벤트 시작");
                isInProgressDialogue=true;
            }
        }
    }

    private void isDialogueEventEnd() => isInProgressDialogue=false;
}
