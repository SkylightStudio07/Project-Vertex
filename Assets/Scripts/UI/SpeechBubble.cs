using DG.Tweening;
using TMPro;
using UnityEngine;

// 잠깐 떴다가 사라지는 말풍선. 상점 주인 등 NPC가 상황마다 한마디씩 할 때 쓴다.
// Show()로 띄우면 살짝 튀어나오며 나타나고, duration 뒤 서서히 사라진다. 떠 있는 중에 다시 부르면 내용만 바꾸고 시간을 연장한다.
// 숨겨져 있을 때도 오브젝트는 켜 둔 채 투명(CanvasGroup)으로만 둔다 — 레이아웃/참조가 흔들리지 않게.
[RequireComponent(typeof(CanvasGroup))]
public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField, Min(0f)] private float defaultDuration = 3f;
    [SerializeField, Min(0f)] private float fadeIn = 0.15f;
    [SerializeField, Min(0f)] private float fadeOut = 0.3f;

    private CanvasGroup _group;
    private Vector3 _baseScale;
    private Sequence _seq;

    public bool IsShowing { get; private set; }

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        _baseScale = transform.localScale;
        HideImmediate();
    }

    public void Show(string line, float duration = -1f)
    {
        if (string.IsNullOrEmpty(line)) return;
        if (_group == null) Awake();
        if (text != null) text.text = line;
        if (duration < 0f) duration = defaultDuration;

        _seq?.Kill();
        bool wasShowing = IsShowing;
        IsShowing = true;
        _group.blocksRaycasts = true;

        _seq = DOTween.Sequence().SetLink(gameObject);
        if (wasShowing)
        {
            // 이미 떠 있으면 내용만 바꾸고 살짝 튀게 한다
            _group.alpha = 1f;
            transform.localScale = _baseScale;
            _seq.Append(transform.DOPunchScale(_baseScale * 0.06f, 0.2f, 6, 0.6f));
        }
        else
        {
            _group.alpha = 0f;
            transform.localScale = _baseScale * 0.9f;
            _seq.Append(_group.DOFade(1f, fadeIn));
            _seq.Join(transform.DOScale(_baseScale, fadeIn + 0.05f).SetEase(Ease.OutBack));
        }
        _seq.AppendInterval(duration);
        _seq.Append(_group.DOFade(0f, fadeOut));
        _seq.OnComplete(HideImmediate);
    }

    public void HideImmediate()
    {
        _seq?.Kill();
        IsShowing = false;
        if (_group == null) return;
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        transform.localScale = _baseScale;
    }

    private void OnDisable() => HideImmediate();
}
