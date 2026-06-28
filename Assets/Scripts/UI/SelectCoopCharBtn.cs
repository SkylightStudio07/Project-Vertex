using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectCoopCharBtn : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image charImage;
    private SelectCoopCharUI selectCoopCharUI;
    private string charID;

    // Property
    public string CharID => charID;

    private void Start()
    {
        if (GetComponentInParent<SelectCoopCharUI>() != null)
        {
            selectCoopCharUI = GetComponentInParent<SelectCoopCharUI>();
        }
    }

    public void SetBtn(string charID)
    {
        this.charID = charID;

        if (CooperationManager.Instance == null)
        {
            Debug.LogWarning("[SelectCoopCharBtn] CooperationManager.Instance가 없음. 씬(또는 부트 씬)에 CooperationManager가 있는지 확인 필요.");
            return;
        }

        if (CooperationManager.Instance.GetCoopSprite(charID) == null)
        {
            Debug.Log($"{charID}에 해당하는 캐릭터 이미지 없음");
            return;
        }

        charImage.sprite = CooperationManager.Instance.GetCoopSprite(charID);
    }

    // 마우스가 캐릭터 창 위에 위치했을 때 선택되었다는 표시가 나타나도록 함
    public void OnPointerEnter(PointerEventData eventData)
    {
        //selectCoopCharUI.Selected(rectTransform);
    }

    // 버튼 OnClick 이벤트에 사용할 메소드 ( 현재 선택한 캐릭터의 카드 추가 )
    public void OnClickBtn()
    {
        if (CooperationManager.Instance == null)
        {
            Debug.LogWarning("[SelectCoopCharBtn] CooperationManager.Instance가 없음. 씬(또는 부트 씬)에 CooperationManager가 있는지 확인 필요.");
            return;
        }

        // 합류 시 짧은 대사가 있으면 먼저 재생, 끝나면 보상 적용 + UI 닫기.
        DialogueView dialogueView = selectCoopCharUI != null ? selectCoopCharUI.DialogueView : null;
        CoopCharData coopCharData = CooperationManager.Instance.GetCoopCharData(charID);
        if (coopCharData != null && coopCharData.joinDialogueJson != null && dialogueView != null)
        {
            dialogueView.Play(coopCharData.joinDialogueJson, FinishSelection);
        }
        else
        {
            FinishSelection();
        }
    }

    private void FinishSelection()
    {
        CooperationManager.Instance.SelectChar(charID);
        selectCoopCharUI.CloseUI();
    }
}
