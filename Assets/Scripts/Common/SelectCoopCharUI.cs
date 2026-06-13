using UnityEngine;
using System.Collections.Generic;


// 협력자 캐릭터 선택을 관리하는 통합 UI
public class SelectCoopCharUI : MonoBehaviour
{
    [SerializeField] private RectTransform selectedUI;
    [SerializeField] private List<SelectCoopCharBtn> selectCoopCharBtns;

    // 협력자 선택 이벤트 활성화 시 UI를 초기화하는 메소드

    private void Awake()
    {
        selectCoopCharBtns = new List<SelectCoopCharBtn>(GetComponentsInChildren<SelectCoopCharBtn>());
    }

    public void Init()
    {
        //List<string> selectableCharIDList = HolyPlaceManager.Instance.GetSeletableChar(GameManager.Instance.Floor);
        List<string> selectableCharIDList = HolyPlaceManager.Instance.GetSeletableChar(1);
        if (selectableCharIDList == null || selectableCharIDList.Count == 0)
        {
            Debug.Log("현재 층에 선택 가능한 협력자 캐릭터 없음");
            return;
        }

        for (int i = 0; i < selectCoopCharBtns.Count; i++)
        {
            selectCoopCharBtns[i].SetBtn(selectableCharIDList[i]);
        }
    }

    // 선택된 캐릭터 창의 위치에 선택 표시 UI를 이동시키는 메소드
    public void Selected(Transform transform)
    {
        selectedUI.position = transform.position;
    }
}
