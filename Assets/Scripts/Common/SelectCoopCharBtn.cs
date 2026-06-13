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
        selectCoopCharUI.Selected(rectTransform);
    }

    // 버튼 OnClick 이벤트에 사용할 메소드
    public void OnClickBtn()
    {
        SelectChar();
    }

    // 현재 선택된 캐릭터 
    private void SelectChar()
    {

    }
}
