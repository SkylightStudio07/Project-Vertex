// ============================================================
// filename   : ItemTargetingController.cs
// 작성자     : -
// 작성일     : 2026-07-10
// description: SelectTarget 아이템의 타겟팅 UI를 담당.
//              "사용" 클릭 시 타겟팅 모드로 진입해 origin(아이템 슬롯)에서
//              포인터까지 TargetArrow를 그리고, 적을 클릭하면
//              EnemyTargeting으로 판정 후 BattleManager.TryUseItem 을 호출한다.
//              (CardHandler 의 Targeting 상태 흐름을 아이템용으로 재구성)
// ============================================================

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 씬의 메인 캔버스(또는 그 하위)에 하나 배치하고, ItemActionPopup 에 참조를 연결한다.
public class ItemTargetingController : MonoBehaviour
{
    [Header("Arrow")]
    [SerializeField] private Color arrowColor = new Color(0.85f, 0.15f, 0.15f, 0.95f);
    [SerializeField] private int arrowSortingOrder = 200;

    private Canvas rootCanvas;
    private RectTransform canvasRect;
    private TargetArrow arrow;

    private ItemData targetingItem;
    private Vector3 originWorldPos;
    private bool isTargeting;

    public bool IsTargeting => isTargeting;

    // ItemActionPopup 의 "사용" 클릭에서 호출. originWorldPos 는 화살표 시작점(아이템 슬롯 위치).
    public void BeginTargeting(ItemData item, Vector3 originWorldPos)
    {
        if (item == null) return;
        if (BattleManager.Instance == null || !BattleManager.Instance.CanUseItemNow) return;

        targetingItem = item;
        this.originWorldPos = originWorldPos;
        isTargeting = true;

        EnsureArrow();
        UpdateArrow(GetPointerScreenPosition());
    }

    public void CancelTargeting()
    {
        isTargeting = false;
        targetingItem = null;
        if (arrow != null) arrow.Hide();
    }

    private void Update()
    {
        if (!isTargeting) return;

        // 전투 상태가 바뀌면(적 턴 전환 등) 타겟팅 취소
        if (BattleManager.Instance == null || !BattleManager.Instance.CanUseItemNow)
        {
            CancelTargeting();
            return;
        }

        Vector2 pointer = GetPointerScreenPosition();
        UpdateArrow(pointer);

        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;

        // 우클릭 / ESC → 취소
        if ((mouse != null && mouse.rightButton.wasPressedThisFrame) ||
            (keyboard != null && keyboard.escapeKey.wasPressedThisFrame))
        {
            CancelTargeting();
            return;
        }

        // 좌클릭 → 적 판정
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            ResolveClick(pointer);
    }

    private void ResolveClick(Vector2 pointerScreenPosition)
    {
        var eventData = new PointerEventData(EventSystem.current) { position = pointerScreenPosition };

        if (EnemyTargeting.TryGetUnderPointer(eventData, out EnemyInstance target))
            BattleManager.Instance.TryUseItem(targetingItem, target);   // 성공 시 소비는 TryUseItem 내부에서 처리

        // 적을 맞췄든(사용) 빈 곳을 찍었든(취소) 타겟팅 종료
        CancelTargeting();
    }

    private static Vector2 GetPointerScreenPosition()
    {
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
    }

    private void EnsureArrow()
    {
        if (arrow != null) return;

        if (rootCanvas == null)
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            rootCanvas = parentCanvas != null ? parentCanvas.rootCanvas : null;
        }
        if (rootCanvas == null) return;
        canvasRect = rootCanvas.transform as RectTransform;
        if (canvasRect == null) return;

        var arrowObject = new GameObject(
            "Item Target Arrow",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasRenderer));
        RectTransform rect = arrowObject.GetComponent<RectTransform>();
        rect.SetParent(canvasRect, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();

        var arrowCanvas = arrowObject.GetComponent<Canvas>();
        arrowCanvas.overrideSorting = true;
        arrowCanvas.sortingLayerID = rootCanvas.sortingLayerID;
        arrowCanvas.sortingOrder = arrowSortingOrder;

        arrow = arrowObject.AddComponent<TargetArrow>();
        arrow.raycastTarget = false;
        arrow.color = arrowColor;
        arrow.Hide();
    }

    private void UpdateArrow(Vector2 pointerScreenPosition)
    {
        EnsureArrow();
        if (arrow == null || canvasRect == null) return;

        Camera camera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera;
        Vector2 originScreen = RectTransformUtility.WorldToScreenPoint(camera, originWorldPos);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, originScreen, camera, out Vector2 originLocal) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, pointerScreenPosition, camera, out Vector2 pointerLocal))
            return;

        arrow.SetPoints(originLocal, pointerLocal);
    }

    private void OnDestroy()
    {
        if (arrow != null) Destroy(arrow.gameObject);
    }
}
