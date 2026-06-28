using UnityEngine;
using UnityEngine.InputSystem;

// 디버그용 트리거: 우클릭하면 테스트 캐릭터(Cp_01)의 호감도 랭크업 대사를 재생한다.
// 줄 넘기기는 DialogueView 자체 버튼이 처리하므로 여기서 따로 호출하지 않는다.
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
        // 디버그용 입력코드
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            if (!isInProgressDialogue)
            {
                dialogueManager.LoadRelationshipEvent("Cp_01");
                Debug.Log("이벤트 시작");
                isInProgressDialogue = true;
            }
        }
    }

    private void isDialogueEventEnd() => isInProgressDialogue = false;
}
