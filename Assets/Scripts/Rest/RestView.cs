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
    [SerializeField] private HealEffect restHealEffect;        // 휴식 회복용 효과 SO


    public void Open()
    {
        gameObject.SetActive(true);
        DisplayCoopPortraits();
    }

    // 합류한 캐릭터들의 초상화 슬롯을 생성하여 표시
    private void DisplayCoopPortraits()
    {
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
        if (restHealEffect != null)
        {
            restHealEffect.Execute(new CardContext());
        }
        Finish();
    }
    public void OnUpgradeClicked()
    {
        // TODO: 강화할 카드 리스트 UI 열기
        Finish();
    }

    private void Finish()
    {
        gameObject.SetActive(false);
        mapUIController.OpenMap();
    }
}
