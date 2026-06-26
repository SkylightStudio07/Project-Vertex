using UnityEngine;
using System.Collections.Generic;


// 협력자 캐릭터 선택을 관리하는 통합 UI
public class SelectCoopCharUI : MonoBehaviour
{
    [SerializeField] private RectTransform selectedUI;
    [SerializeField] private List<SelectCoopCharBtn> selectCoopCharBtns;
    [SerializeField] private FadeController fadeController;
    [SerializeField] private DialogueView dialogueView;

    public DialogueView DialogueView => dialogueView;

    // 협력자 선택 이벤트 활성화 시 UI를 초기화하는 메소드

    private void Awake()
    {
        selectCoopCharBtns = new List<SelectCoopCharBtn>(GetComponentsInChildren<SelectCoopCharBtn>());
    }

    public void Init()
    {
        fadeController.FadeIn();

        if (HolyPlaceManager.Instance == null)
        {
            Debug.LogWarning("[SelectCoopCharUI] HolyPlaceManager.Instance가 없음. 씬(또는 부트 씬)에 HolyPlaceManager가 있는지 확인 필요.");
            return;
        }

        List<string> selectableCharIDList = HolyPlaceManager.Instance.GetSeletableChar(RunData.Instance.currentFloor);
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

    public void CloseUI()
    {
        // FadeOut()을 부르면서 동시에 SetActive(false)하면 페이드 애니메이션이 재생될 틈도 없이
        // 오브젝트가 꺼져버린다. 페이드 비주얼을 살리려면 타임라인 종료 Signal에 맞춰 닫는 작업이
        // 추후 필요함 (지금은 정확성 우선 — 닫혔는데 맵 클릭이 막히는 버그를 피하는 쪽을 택함).
        gameObject.SetActive(false);
        if (MapUIController.Instance != null) MapUIController.Instance.OpenMap();
    }
}
