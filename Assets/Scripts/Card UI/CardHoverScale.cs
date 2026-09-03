using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// 마우스를 올리면 카드를 확대하는 연출. 나열된 카드(보상/덱 목록/상점) 어디에나 붙여 쓴다.
public class CardHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
