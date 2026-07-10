using System.Collections.Generic;
using UnityEngine;

// 휴식 노드 UI 컨트롤러
public class RestView : MonoBehaviour
{
    [Header("복귀")]
    [SerializeField] private MapUIController mapUIController;

    [Header("합류 캐릭터")]
    [SerializeField] private Transform portraitSlotParent;
    [SerializeField] private GameObject portraitSlotPrefab;
    private List<PortraitSlot> portraitSlots = new List<PortraitSlot>();

    [Header("행동")]
    // 휴식 회복량. CardEffect가 순수 클래스로 전환되며 SO 에셋 참조가 불가능해져 int로 단순화 —
    // 구 HealEffect.Execute도 GameManager.HealPlayer 호출이 전부였다. (구 RestHealEffect.asset 값: 20)
    [SerializeField] private int restHealAmount = 20;


    public void Open()
    {
        gameObject.SetActive(true);
        DisplayCoopPortraits();
    }

    // 합류한 캐릭터들의 초상화 슬롯을 생성하여 표시
    private void DisplayCoopPortraits()
    {
        if (portraitSlotPrefab == null || portraitSlotParent == null)
        {
            Debug.LogWarning("[RestView] portraitSlotPrefab 또는 portraitSlotParent가 Inspector에서 지정되지 않았습니다.");
            return;
        }

        if (CooperationManager.Instance == null)
        {
            Debug.LogWarning("[RestView] CooperationManager.Instance가 null입니다.");
            return;
        }

        // 기존 슬롯 제거
        foreach (var slot in portraitSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        portraitSlots.Clear();
        // 현재 합류한 캐릭터들의 초상화 슬롯 생성
        foreach (var coopCharState in CooperationManager.Instance.GetJoinedInRunCharStates())
        {
            var slotObj = Instantiate(portraitSlotPrefab, portraitSlotParent);
            var slot = slotObj.GetComponent<PortraitSlot>();
            slot.SetData(coopCharState);
            portraitSlots.Add(slot);
        }
    }

    public void OnRestClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.HealPlayer(restHealAmount);
        Finish();
    }
    public void OnUpgradeClicked()
    {
        // TODO: 강화할 카드 리스트 UI 열기
        Finish();
    }

    private void Finish()
    {
        if (mapUIController == null)
        {
            Debug.LogWarning("[RestView] mapUIController 참조가 Inspector에서 연결되지 않았습니다.");
            return;
        }
        gameObject.SetActive(false);
        mapUIController.OpenMap();
    }
}
