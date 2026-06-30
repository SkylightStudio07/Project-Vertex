using UnityEngine;
using UnityEngine.UI;

// 아이템 슬롯 클릭 시 뜨는 사용/버리기 팝업 (공용 1개, ItemInventoryView가 소유).
// 사용 로직은 추후 TryUseItem 연결 예정. 버리기는 인벤토리에서 제거.
public class ItemActionPopup : MonoBehaviour
{
    [SerializeField] private RectTransform rect;        // 위치 잡을 팝업 박스(Panel). 전체화면 root가 아님!
    [SerializeField] private Button useButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button backdropButton;     // 전체화면 투명 버튼 — 바깥 클릭 시 닫기

    private ItemData item;

    private void Awake()
    {
        if (backdropButton != null) backdropButton.onClick.AddListener(Close);  // 바깥 클릭 → 닫기
        gameObject.SetActive(false);
    }

    // 슬롯 클릭 시 호출. anchorWorldPos 기준으로 위치 잡고 표시.
    public void Open(ItemData itemData, Vector3 anchorWorldPos)
    {
        item = itemData;
        if (rect != null) rect.position = anchorWorldPos;

        // TODO: 전투 중이 아니고 item.UsableOutsideBattle == false 면 useButton.interactable = false
        if (useButton != null) useButton.interactable = true;

        gameObject.SetActive(true);
    }

    // 버튼 onClick (인스펙터 연결)
    public void OnUseClicked()
    {
        // TODO: 아이템 사용 로직 연결 (BattleManager.TryUseItem / 비전투 사용)
        //       사용 성공 시 ItemInventoryManager가 소비 처리 → OnInventoryChanged로 바 자동 갱신
        Debug.Log($"[Item] 사용: {item?.ItemName}");
        Close();
    }

    public void OnDiscardClicked()
    {
        if (item != null) ItemInventoryManager.Instance.RemoveItem(item);  // 제거 → 바 자동 갱신
        Close();
    }

    public void Close()
    {
        item = null;
        gameObject.SetActive(false);
    }
}
