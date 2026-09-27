using DG.Tweening;
using TMPro;
using UnityEngine;

// 새 막의 맵을 처음 열 때 화면 가운데에 "1막 / 막 이름"을 띄웠다가 천천히 사라지게 하는 배너.
// 입력은 막지 않는다(뒤의 맵 노드를 바로 눌러도 된다). MapUIController가 새 맵을 만들 때 Play를 부른다.
// 막 이름은 ActData(막마다 SO)에서 가져온다.
[RequireComponent(typeof(CanvasGroup))]
public class ActTitleBanner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actLabel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField, Min(0f)] private float delay = 0.25f;   // 맵 패널이 들어오는 동안 잠깐 기다린다
    [SerializeField, Min(0f)] private float fadeIn = 0.6f;
    [SerializeField, Min(0f)] private float hold = 1.6f;
    [SerializeField, Min(0f)] private float fadeOut = 1.8f;

    private CanvasGroup _group;
    private Sequence _sequence;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        _group.blocksRaycasts = false;
        _group.interactable = false;
        _group.alpha = 0f; // 평소엔 켜 둔 채 투명하게 둔다
    }

    public void Play(ActData act)
    {
        if (act == null || string.IsNullOrWhiteSpace(act.actName)) return;

        _sequence?.Kill();
        if (actLabel != null) actLabel.text = $"{act.actNumber}막";
        if (titleText != null) titleText.text = act.actName;

        _group.alpha = 0f;
        var titleRect = titleText != null ? titleText.rectTransform : null;
        if (titleRect != null) titleRect.localScale = Vector3.one * 1.06f;

        _sequence = DOTween.Sequence().SetLink(gameObject)
            .AppendInterval(delay)
            .Append(_group.DOFade(1f, fadeIn).SetEase(Ease.OutQuad))
            .AppendInterval(hold)
            .Append(_group.DOFade(0f, fadeOut).SetEase(Ease.InOutSine));
        // 제목이 아주 살짝 가라앉으며 자리 잡는다
        if (titleRect != null) _sequence.Insert(delay, titleRect.DOScale(1f, fadeIn + hold).SetEase(Ease.OutCubic));
    }
}
