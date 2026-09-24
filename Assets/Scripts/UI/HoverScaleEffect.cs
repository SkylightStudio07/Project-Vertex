using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 마우스를 올리면 확대되는 범용 호버 연출. 카드(보상/덱 목록/상점)뿐 아니라
// 상태 칩(StatusChipView) 등 localScale로 다루는 UI 어디에나 붙여 쓴다.
// 다른 컴포넌트(예: 툴팁 트리거)와는 무관하게 독립 동작한다 — 같은 오브젝트에
// IPointerEnterHandler/IPointerExitHandler를 구현한 컴포넌트를 몇 개를 더 붙여도
// Unity EventSystem이 전부 호출해주므로, 툴팁 표시 같은 다른 반응은 별도 컴포넌트로 분리하면 된다.
// (원래 이름은 CardHoverScale이었고 카드 목록 전용처럼 보였지만 로직 자체는 처음부터 범용이라 이름/위치만 옮김.)
public class HoverScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScaleMultiplier = 1.2f;
    [SerializeField] private float animationDuration = 0.08f;

    [Tooltip("확대 중 옆 요소들보다 위에 그리기. 크게 키우는 곳(상점 카드 등)에서 켠다")]
    [SerializeField] private bool bringToFront;
    [SerializeField] private int frontSortingOffset = 50;
    [Tooltip("확대했을 때 화면(루트 캔버스) 밖으로 나가면 안쪽으로 밀어 넣기. 가장자리 줄의 상점 카드 등")]
    [SerializeField] private bool keepInsideScreen;
    [SerializeField] private float screenMargin = 12f;

    private Vector3 originalScale;
    private Vector3 originalLocalPos;
    private bool hasOriginal;
    private bool hovering;
    private Coroutine scaleCoroutine;
    private Canvas frontCanvas;

    // 원래 크기는 Awake가 아니라 "호버가 시작될 때"(되돌아가는 중이 아닐 때) 잡는다.
    // 생성 직후 다른 코드가 스케일을 바꾸는 경우(ShopView가 상점 카드를 0.95로 조정 등)
    // Awake 값을 기준으로 키우면 오히려 작아지는 문제가 있었다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!hovering && scaleCoroutine == null)
        {
            originalScale = transform.localScale;
            originalLocalPos = transform.localPosition;
            hasOriginal = true;
        }
        hovering = true;
        SetFront(true);
        AnimateTo(originalScale * hoverScaleMultiplier, originalLocalPos + InsideScreenOffset());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        if (hasOriginal) AnimateTo(originalScale, originalLocalPos);
    }

    // 확대 후 크기를 미리 계산해 루트 캔버스 영역을 벗어나는 만큼 반대로 밀어낼 로컬 오프셋.
    private Vector3 InsideScreenOffset()
    {
        if (!keepInsideScreen || transform.parent == null) return Vector3.zero;
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return Vector3.zero;
        var root = canvas.rootCanvas.transform as RectTransform;

        // 원래 크기 기준 자식 전체(카드 그림·가격)의 영역을 루트 캔버스 좌표로 구하고, 피벗 중심으로 배율만큼 키운다
        transform.localScale = originalScale;
        Bounds b = RectTransformUtility.CalculateRelativeRectTransformBounds(root, transform);
        Vector3 pivot = root.InverseTransformPoint(transform.position);
        Vector3 min = pivot + (b.min - pivot) * hoverScaleMultiplier;
        Vector3 max = pivot + (b.max - pivot) * hoverScaleMultiplier;

        Rect area = root.rect;
        Vector2 shift = Vector2.zero;
        if (max.y > area.yMax - screenMargin) shift.y = area.yMax - screenMargin - max.y;
        else if (min.y < area.yMin + screenMargin) shift.y = area.yMin + screenMargin - min.y;
        if (max.x > area.xMax - screenMargin) shift.x = area.xMax - screenMargin - max.x;
        else if (min.x < area.xMin + screenMargin) shift.x = area.xMin + screenMargin - min.x;
        if (shift == Vector2.zero) return Vector3.zero;

        // 루트 캔버스 좌표 이동량 → 부모 로컬 좌표 이동량
        Vector3 worldDelta = root.TransformVector(shift);
        return transform.parent.InverseTransformVector(worldDelta);
    }

    private void OnDisable()
    {
        // 확대된 채로 꺼지면(구매 후 목록 갱신 등) 다음에 켜질 때 큰 크기가 원래 크기로 잡히지 않게 되돌린다
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = null;
        if (hasOriginal)
        {
            transform.localScale = originalScale;
            if (keepInsideScreen) transform.localPosition = originalLocalPos;
        }
        hovering = false;
        SetFront(false);
    }

    private void AnimateTo(Vector3 targetScale, Vector3 targetPos)
    {
        // 이전 코루틴을 반드시 중단
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = null;
        if (!isActiveAndEnabled) { transform.localScale = targetScale; transform.localPosition = targetPos; return; }
        scaleCoroutine = StartCoroutine(AnimateScale(targetScale, targetPos));
    }

    private IEnumerator AnimateScale(Vector3 targetScale, Vector3 targetPos)
    {
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.localPosition;
        // 화면 밀어넣기를 안 쓰면 위치는 건드리지 않는다(레이아웃이 잡은 위치를 그대로 둠)
        bool movePos = keepInsideScreen;
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            if (movePos) transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
        if (movePos) transform.localPosition = targetPos;
        scaleCoroutine = null;
        if (!hovering) SetFront(false); // 완전히 줄어든 뒤에 순서를 내려야 옆 카드에 잘려 보이지 않는다
    }

    // 레이아웃 그룹 안에서 SetAsLastSibling으로 앞으로 빼면 배치 순서가 바뀌므로,
    // 중첩 Canvas의 sortingOrder로만 앞에 그린다. 중첩 Canvas는 자기 GraphicRaycaster가 있어야 클릭을 받는다.
    private void SetFront(bool front)
    {
        if (!bringToFront) return;
        if (frontCanvas == null)
        {
            if (!front) return;
            frontCanvas = GetComponent<Canvas>();
            if (frontCanvas == null) frontCanvas = gameObject.AddComponent<Canvas>();
            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
        }

        if (front)
        {
            // 중첩 Canvas 중 overrideSorting이 꺼진 것은 sortingOrder 값이 의미가 없어서 루트 기준으로 잡는다
            var parentCanvas = transform.parent != null ? transform.parent.GetComponentInParent<Canvas>() : null;
            int baseOrder = parentCanvas == null ? 0
                : parentCanvas.overrideSorting || parentCanvas.isRootCanvas ? parentCanvas.sortingOrder
                : parentCanvas.rootCanvas.sortingOrder;
            frontCanvas.overrideSorting = true;
            frontCanvas.sortingOrder = baseOrder + frontSortingOffset;
        }
        else
        {
            frontCanvas.overrideSorting = false;
        }
    }
}
