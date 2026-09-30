using System.Collections;
using DG.Tweening;
using UnityEngine;

// 턴 시작 시 짧게 보여주는 배너. 내용 자체는 범용이라(특정 진영 의존 없음)
// 적 턴/플레이어 턴 배너 둘 다 이 컴포넌트를 각각 다른 오브젝트에 붙여서 재사용한다.
// (클래스명은 EnemyTurnBannerView지만 실제로는 어떤 턴 배너에도 쓸 수 있음 — 스크립트 GUID가
// 끊기지 않도록 이름은 그대로 둠)
// BattleManager의 코루틴이 ShowAndWait()를 yield해서 사용한다.
public class EnemyTurnBannerView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration  = 0.25f;
    [SerializeField] private float holdDuration    = 0.8f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    [Header("전투 HUD v3 배너 (선택)")]
    [Tooltip("나타날 때 왼쪽부터 그어지는 선들 (피벗 x=0이어야 한다)")]
    [SerializeField] private RectTransform[] drawLines;
    [Tooltip("나타날 때 살짝 옆으로 밀려 들어오는 글자 묶음")]
    [SerializeField] private RectTransform slideIn;
    [SerializeField] private float slideDistance = 40f;

    private Vector2 _slideBase;
    private bool _hasSlideBase;

    private void Awake()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public IEnumerator ShowAndWait()
    {
        if (canvasGroup == null)
        {
            Debug.LogWarning("[EnemyTurnBannerView] canvasGroup이 연결되지 않음.");
            yield break;
        }

        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;

        if (drawLines != null)
            foreach (var line in drawLines)
            {
                if (line == null) continue;
                line.DOKill();
                line.localScale = new Vector3(0f, 1f, 1f);
                line.DOScaleX(1f, fadeInDuration + 0.25f).SetEase(Ease.OutCubic).SetLink(line.gameObject);
            }
        if (slideIn != null)
        {
            if (!_hasSlideBase) { _slideBase = slideIn.anchoredPosition; _hasSlideBase = true; }
            slideIn.DOKill();
            slideIn.anchoredPosition = _slideBase - new Vector2(slideDistance, 0f);
            slideIn.DOAnchorPos(_slideBase, fadeInDuration + 0.2f).SetEase(Ease.OutCubic).SetLink(slideIn.gameObject);
        }

        yield return canvasGroup.DOFade(1f, fadeInDuration).WaitForCompletion();
        yield return new WaitForSeconds(holdDuration);
        yield return canvasGroup.DOFade(0f, fadeOutDuration).WaitForCompletion();

        gameObject.SetActive(false);
    }
}
