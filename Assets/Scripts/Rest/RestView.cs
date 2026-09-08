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
    // 강화 가능한 카드가 없을 때 잠글 버튼. (MapUIController의 ScrollRect처럼 using 추가 없이 정규화 표기)
    [SerializeField] private UnityEngine.UI.Button upgradeButton;


    public void Open()
    {
        gameObject.SetActive(true);
        DisplayCoopPortraits();
        RefreshUpgradeButton();
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
        var upgradable = DeckManager.Instance.GetUpgradableCards();
        if (upgradable.Count == 0) return;

        CardListView.Instance.OpenAsSelector(
            "강화할 카드를 선택하세요",
            upgradable,
            onCardSelected: ShowUpgradePreview,
            closeOnSelect: false);
    }

    // 강화 가능한 카드가 하나도 없으면 버튼을 잠근다.
    private void RefreshUpgradeButton()
    {
        if (upgradeButton == null) return;
        upgradeButton.interactable = DeckManager.Instance.GetUpgradableCards().Count > 0;
    }

    // 강화 후 모습을 보여주려고 임시 복사본을 강화 상태로 만들어 확인창에 띄운다.
    // 원본을 건드리지 않으므로 취소해도 덱은 그대로다.
    private void ShowUpgradePreview(CardData card)
    {
        var preview = Instantiate(card);
        preview.isUpgraded = true;

        CardDetailView.Instance.ShowWithConfirmation(
            preview, "강화",
            onConfirm: () => { Destroy(preview); ApplyUpgrade(card); },
            onCancel:  () => Destroy(preview));
    }

    private void ApplyUpgrade(CardData card)
    {
        if (!DeckManager.Instance.UpgradeCard(card)) return;

        CardListView.Instance.Close();
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
