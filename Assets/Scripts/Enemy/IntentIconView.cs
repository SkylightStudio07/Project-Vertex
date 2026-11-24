using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 의도 하나의 아이콘과 공격 수치. 배치는 프리팹의 LayoutGroup이 담당한다.
public class IntentIconView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private CanvasGroup canvasGroup;

    private Tween _pulse;
    private bool _initialized;
    private Vector3 _iconScale, _valueScale;
    private Color _iconColor, _valueColor;
    private float _opacity = 1f;

    private void Awake() => CacheDefaults();

    private void CacheDefaults()
    {
        if (_initialized) return;
        _initialized = true;
        _iconScale = iconImage.rectTransform.localScale;
        _valueScale = valueText.rectTransform.localScale;
        _iconColor = iconImage.color;
        _valueColor = valueText.color;
    }

    public void Bind(Sprite sprite, int? damage, bool lookahead = false)
    {
        ResetVisuals();
        iconImage.sprite = sprite;
        iconImage.enabled = sprite != null;
        valueText.gameObject.SetActive(damage.HasValue);
        valueText.text = damage.HasValue ? damage.Value.ToString() : string.Empty;
        transform.localScale = lookahead ? Vector3.one * 0.62f : Vector3.one;
        _opacity = lookahead ? 0.72f : 1f;
        canvasGroup.blocksRaycasts = !lookahead;
        SetAlpha(1f);
    }

    public void ApplyAttackTier(int? damage, float heavyScale, float lethalScale, Color lethalColor)
    {
        var tier = HeavyAttack.Of(damage);
        float scale = tier == HeavyAttack.Tier.Lethal ? lethalScale : tier == HeavyAttack.Tier.Heavy ? heavyScale : 1f;
        iconImage.rectTransform.localScale = _iconScale * scale;
        valueText.rectTransform.localScale = _valueScale * (tier == HeavyAttack.Tier.Lethal ? 1.2f : 1f);
        valueText.color = tier == HeavyAttack.Tier.Lethal ? lethalColor : _valueColor;
        if (tier == HeavyAttack.Tier.Lethal)
            _pulse = iconImage.DOColor(lethalColor, 0.45f).SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine).SetLink(gameObject);
    }

    public void SetAlpha(float alpha) => canvasGroup.alpha = _opacity * alpha;

    // 풀에 반환할 때 이전 수치·강공격 연출·투명도가 다음 의도에 남지 않게 한다.
    public void ResetForPool()
    {
        ResetVisuals();
        iconImage.sprite = null;
        iconImage.enabled = false;
        valueText.text = string.Empty;
        valueText.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void ResetVisuals()
    {
        CacheDefaults();
        StopPulse();
        iconImage.color = _iconColor;
        valueText.color = _valueColor;
        iconImage.rectTransform.localScale = _iconScale;
        valueText.rectTransform.localScale = _valueScale;
        transform.localScale = Vector3.one;
        _opacity = 1f;
        SetAlpha(1f);
    }

    private void StopPulse()
    {
        _pulse?.Pause(); // 반환 즉시 재생을 멈춘 뒤 트윈을 정리한다.
        _pulse?.Kill();
        _pulse = null;
    }

    private void OnDisable() => StopPulse();
    private void OnDestroy() => StopPulse();
}
