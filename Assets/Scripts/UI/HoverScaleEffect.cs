using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

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

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;


    private void Awake() => originalScale = transform.localScale;

    public void OnPointerEnter(PointerEventData eventData) => AnimateTo(originalScale * hoverScaleMultiplier);
    public void OnPointerExit(PointerEventData eventData) => AnimateTo(originalScale);

    private void AnimateTo(Vector3 target)
    {
        // 이전 코루틴을 반드시 중단
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(AnimateScale(target));
    }

    private IEnumerator AnimateScale(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / animationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
        scaleCoroutine = null;
    }
}
