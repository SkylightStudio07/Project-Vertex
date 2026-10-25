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
    [Tooltip("자기 자신 대신 이 자식들을 함께 키운다 (자기 피벗 기준으로 위치도 같이 벌어진다). 예: 상점 받침은 두고 카드·가격표만. 비우면 자기 자신")]
    [SerializeField] private Transform[] scaleParts;

    private bool Grouped => scaleParts != null && scaleParts.Length > 0;
    private Transform[] _parts;
    private Vector3[] _origScale, _origPos;
    private bool hasOriginal;
    private bool hovering;
    private Coroutine scaleCoroutine;
    private Canvas frontCanvas;

    private Transform[] Parts => _parts ??= Grouped ? scaleParts : new[] { transform };

    // 원래 크기는 Awake가 아니라 "호버가 시작될 때"(되돌아가는 중이 아닐 때) 잡는다.
    // 생성 직후 다른 코드가 스케일을 바꾸는 경우(ShopView가 상점 카드를 0.95로 조정 등)
    // Awake 값을 기준으로 키우면 오히려 작아지는 문제가 있었다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!hovering && scaleCoroutine == null)
        {
            var parts = Parts;
            _origScale = new Vector3[parts.Length];
            _origPos = new Vector3[parts.Length];
            for (int i = 0; i < parts.Length; i++) { _origScale[i] = parts[i].localScale; _origPos[i] = parts[i].localPosition; }
            hasOriginal = true;
        }
        hovering = true;
        SetFront(true);
        Vector3 shift = InsideScreenOffset();
        var targetScale = new Vector3[Parts.Length];
        var targetPos = new Vector3[Parts.Length];
        for (int i = 0; i < Parts.Length; i++)
        {
            targetScale[i] = _origScale[i] * hoverScaleMultiplier;
            // 묶음은 자기 피벗(원점) 기준으로 위치도 같이 벌려야 한 덩어리로 커진다
            targetPos[i] = (Grouped ? _origPos[i] * hoverScaleMultiplier : _origPos[i]) + shift;
        }
        AnimateTo(targetScale, targetPos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        if (hasOriginal) AnimateTo(_origScale, _origPos);
    }

    // 확대 후 크기를 미리 계산해 루트 캔버스 영역을 벗어나는 만큼 반대로 밀어낼 로컬 오프셋 (움직이는 대상의 부모 좌표).
    private Vector3 InsideScreenOffset()
    {
        if (!keepInsideScreen || transform.parent == null) return Vector3.zero;
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return Vector3.zero;
        var root = canvas.rootCanvas.transform as RectTransform;

        // 원래 크기 기준 자식 전체(카드 그림·가격)의 영역을 루트 캔버스 좌표로 구하고, 피벗 중심으로 배율만큼 키운다
        for (int i = 0; i < Parts.Length; i++) { Parts[i].localScale = _origScale[i]; Parts[i].localPosition = _origPos[i]; }
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

        // 루트 캔버스 좌표 이동량 → 움직이는 대상의 부모 로컬 좌표 이동량
        Vector3 worldDelta = root.TransformVector(shift);
        return (Grouped ? transform : transform.parent).InverseTransformVector(worldDelta);
    }

    private void OnDisable()
    {
        // 확대된 채로 꺼지면(구매 후 목록 갱신 등) 다음에 켜질 때 큰 크기가 원래 크기로 잡히지 않게 되돌린다
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = null;
        if (hasOriginal)
            for (int i = 0; i < Parts.Length; i++)
            {
                Parts[i].localScale = _origScale[i];
                if (keepInsideScreen || Grouped) Parts[i].localPosition = _origPos[i];
            }
        hovering = false;
        SetFront(false);
    }

    private void AnimateTo(Vector3[] targetScale, Vector3[] targetPos)
    {
        // 이전 코루틴을 반드시 중단
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = null;
        if (!isActiveAndEnabled)
        {
            for (int i = 0; i < Parts.Length; i++) { Parts[i].localScale = targetScale[i]; Parts[i].localPosition = targetPos[i]; }
            return;
        }
        scaleCoroutine = StartCoroutine(AnimateScale(targetScale, targetPos));
    }

    private IEnumerator AnimateScale(Vector3[] targetScale, Vector3[] targetPos)
    {
        var parts = Parts;
        var startScale = new Vector3[parts.Length];
        var startPos = new Vector3[parts.Length];
        for (int i = 0; i < parts.Length; i++) { startScale[i] = parts[i].localScale; startPos[i] = parts[i].localPosition; }
        // 화면 밀어넣기를 안 쓰는 단일 대상은 위치를 건드리지 않는다(레이아웃이 잡은 위치를 그대로 둠)
        bool movePos = keepInsideScreen || Grouped;
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i].localScale = Vector3.Lerp(startScale[i], targetScale[i], t);
                if (movePos) parts[i].localPosition = Vector3.Lerp(startPos[i], targetPos[i], t);
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].localScale = targetScale[i];
            if (movePos) parts[i].localPosition = targetPos[i];
        }
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
