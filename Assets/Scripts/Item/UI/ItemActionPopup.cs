using UnityEngine;
using UnityEngine.UI;

// 아이템 슬롯 클릭 시 뜨는 사용/버리기 팝업 (공용 1개, ItemInventoryView가 소유).
// 사용 로직은 추후 TryUseItem 연결 예정. 버리기는 인벤토리에서 제거.
public class ItemActionPopup : MonoBehaviour
{
    [SerializeField] private RectTransform rect;        // 위치 잡을 팝업 박스(Panel).
    [SerializeField] private Button useButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button backdropButton;     // 전체화면 투명 버튼 — 바깥 클릭 시 닫기

    [Header("타겟팅")]
    [SerializeField] private ItemTargetingController targetingController;  // SelectTarget 아이템 타겟팅 UI

    private ItemData item;
    private Vector3 anchorWorldPos;   // 슬롯 위치 — SelectTarget 화살표 시작점으로 사용

    private void Awake()
    {
        if (backdropButton != null) backdropButton.onClick.AddListener(Close);  // 바깥 클릭 → 닫기
        gameObject.SetActive(false);
    }

    // 슬롯 클릭 시 호출. anchorWorldPos 기준으로 위치 잡고 표시.
    public void Open(ItemData itemData, Vector3 anchorWorldPos)
    {
        item = itemData;
        this.anchorWorldPos = anchorWorldPos;
        if (rect != null) rect.position = anchorWorldPos;

        // 전투 중이면 플레이어 턴에만, 전투 밖이면 UsableOutsideBattle + 즉발형 아이템만 사용 가능
        if (useButton != null)
            useButton.interactable = CanUseNow();

        gameObject.SetActive(true);
    }


    // 사용 가능 여부. 전투 중이면 플레이어 턴일 때만, 전투 밖이면 비전투 사용이 허용된 즉발형만.
    // 적 지정형(SelectTarget)은 전투 밖에서 대상이 없으므로 제외한다.
    private bool CanUseNow()
    {
        if (item == null) return false;

        bool inBattle = BattleManager.Instance != null && BattleManager.Instance.IsInBattle;
        if (inBattle) return BattleManager.Instance.CanUseItemNow;

        return item.UsableOutsideBattle && item.UseMode == ItemData.ItemUseMode.Immediate;
    }

    public void OnUseClicked()
    {
        if (!CanUseNow())
        {
            Close();
            return;
        }

        // 전투 밖 사용 — 인벤토리 매니저가 효과 실행/지속 효과 등록/소비를 처리한다.
        if (BattleManager.Instance == null || !BattleManager.Instance.IsInBattle)
        {
            if (ItemInventoryManager.Instance != null)
                ItemInventoryManager.Instance.UseOutsideBattle(item);
            Close();
            return;
        }

        // 사용 성공 시 ItemInventoryManager가 소비 처리 → OnInventoryChanged로 바 자동 갱신
        if (item.UseMode == ItemData.ItemUseMode.SelectTarget && targetingController != null)
        {
            // 적 지정 아이템: 즉시 사용하지 않고 타겟팅 모드로 위임 (적 클릭 시 컨트롤러가 TryUseItem 호출)
            targetingController.BeginTargeting(item, anchorWorldPos);
            Close();
            return;
        }

        // Immediate 아이템: target 없이 즉시 사용
        BattleManager.Instance.TryUseItem(item, null);
        Close();
    }

    public void OnDiscardClicked()
    {
        if (item != null) ItemInventoryManager.Instance.RemoveItem(item);
        Close();
    }

    public void Close()
    {
        item = null;
        gameObject.SetActive(false);
    }
}
