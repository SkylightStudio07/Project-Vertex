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

        // 전투 중 + 플레이어 턴일 때만 사용 가능 (적 턴/비전투면 회색)
        // TODO: 비전투 사용(UsableOutsideBattle) 경로 생기면 여기 조건 확장
        if (useButton != null)
            useButton.interactable = BattleManager.Instance != null && BattleManager.Instance.CanUseItemNow;

        gameObject.SetActive(true);
    }


    public void OnUseClicked()
    {
        // 사용 성공 시 ItemInventoryManager가 소비 처리 → OnInventoryChanged로 바 자동 갱신
        if (item != null && BattleManager.Instance != null)
        {
            if (item.UseMode == ItemData.ItemUseMode.SelectTarget && targetingController != null)
            {
                // 적 지정 아이템: 즉시 사용하지 않고 타겟팅 모드로 위임 (적 클릭 시 컨트롤러가 TryUseItem 호출)
                targetingController.BeginTargeting(item, anchorWorldPos);
                Close();
                return;
            }

            // Immediate 아이템: target 없이 즉시 사용
            BattleManager.Instance.TryUseItem(item, null);
        }
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
