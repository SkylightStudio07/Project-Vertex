using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로비 오른쪽 위 게이지 = 거점 개발도. "Lv.2  12 / 20" + 청록 채움 막대.
// 로비 키트의 Progress 그림은 채움이 그려진 고정 이미지라, 시작할 때 민무늬 트랙으로 바꾸고 그 위에 채움을 얹는다.
// 기획: 노션 VERTEX 기획서 v2 > 8. 재화 & 거점 개발도
public class LobbyDevelopmentGauge : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Image track;
    [SerializeField] private Color trackColor = new(0.85f, 0.86f, 0.87f, 1f);
    [SerializeField] private Color fillColor = new(0.051f, 0.722f, 0.949f, 1f);

    private RectTransform _fill;

    private void OnEnable()
    {
        Build();
        MetaProgress.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable() => MetaProgress.OnChanged -= Refresh;

    private void Build()
    {
        if (_fill != null || track == null) return;
        track.sprite = null;
        track.color = trackColor;
        var go = new GameObject("DevelopmentFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = track.gameObject.layer;
        _fill = (RectTransform)go.transform;
        _fill.SetParent(track.transform, false);
        _fill.anchorMin = Vector2.zero;
        _fill.anchorMax = new Vector2(0f, 1f);
        _fill.pivot = new Vector2(0f, 0.5f);
        _fill.offsetMin = _fill.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = fillColor;
        img.raycastTarget = false;
    }

    private void Refresh()
    {
        int level = MetaProgress.DevelopmentLevel;
        var (current, total) = MetaProgress.LevelProgress();
        float ratio = total > 0 ? Mathf.Clamp01((float)current / total) : 1f;

        if (_fill != null && track != null)
            _fill.sizeDelta = new Vector2(track.rectTransform.rect.width * ratio, 0f);
        if (valueText != null)
        {
            valueText.textWrappingMode = TextWrappingModes.NoWrap;
            valueText.overflowMode = TextOverflowModes.Overflow;
            valueText.text = total > 0
                ? $"<size=75%>Lv.</size>{level}  <size=80%>{current} / {total}</size>"
                : $"<size=75%>Lv.</size>{level}  <size=80%>MAX</size>";
        }
    }
}
