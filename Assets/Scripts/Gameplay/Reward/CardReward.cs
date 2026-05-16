

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

// 보상 카드 UI 프리팹에 부착하는 컴포넌트
// 카드 보상 클릭 시 카드 획득 처리
// 지금은 애니메이션 코루틴으로 처리하는데 DOTween 같은 트윈 라이브러리 도입하는 거 건의해보기
public class CardReward : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private CardData cardData;
    private Vector3 originalScale;
    private float animationDuration = 0.08f;

    private CardRewardView cardRewardView;
    public event Action Onclick;

    private void OnEnable()
    {
        originalScale = transform.localScale;
    }
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        cardData = GetComponent<CardView>().Data;
        GameManager.Instance.playerDeck.Add(cardData); 
        Debug.Log($"Card '{cardData.CardName}' added to player's deck.");
        Onclick?.Invoke();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // 카드 확대 애니메이션
        Vector3 zoomScale = originalScale * 1.2f;
        StartCoroutine(AnimateScale(zoomScale, animationDuration));
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        // 카드 원래 크기로 복원 애니메이션
        StartCoroutine(AnimateScale(originalScale, animationDuration));
    }

    private IEnumerator AnimateScale(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }
}
