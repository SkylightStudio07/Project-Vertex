using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로비 오른쪽 위 재화 바 (명일방주식 무테). 윗줄 = 거점 개발도 "BASE DEV Lv.3 ━━ 12 / 30", 아랫줄 = 탈린(+) · 파츠 · 아티팩트.
// 아랫줄은 오른쪽 끝(설정 버튼 앞)에 붙어 있어서 숫자가 길어지면 왼쪽으로 늘어난다.
// 아트: ArtDirection/CurrencyBarMockup (F안). 기획: 노션 VERTEX 기획서 v2 > 8. 재화 & 거점 개발도
public class LobbyCurrencyBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI talinText;
    [SerializeField] private TextMeshProUGUI partsText;
    [SerializeField] private TextMeshProUGUI artifactText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI devValueText;
    [SerializeField] private RectTransform devTrack;
    [SerializeField] private RectTransform devFill;
    [SerializeField] private Button talinPlusButton; // 로비 상점이 생기면 연결한다
    [SerializeField, Min(0f)] private float minFillWidth = 8f; // 0이어도 청록 시작점은 보인다

    private void OnEnable()
    {
        MetaProgress.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable() => MetaProgress.OnChanged -= Refresh;

    private void Refresh()
    {
        if (talinText != null) talinText.text = MetaProgress.Talin.ToString("N0");
        if (partsText != null) partsText.text = MetaProgress.Parts.ToString("N0");
        if (artifactText != null) artifactText.text = MetaProgress.Artifacts.ToString("N0");

        int level = MetaProgress.DevelopmentLevel;
        var (current, total) = MetaProgress.LevelProgress();
        float ratio = total > 0 ? Mathf.Clamp01((float)current / total) : 1f;

        if (levelText != null) levelText.text = $"BASE DEV  <size=110%>Lv.{level}</size>";
        if (devValueText != null) devValueText.text = total > 0 ? $"{current} / {total}" : "MAX";
        if (devFill != null && devTrack != null)
            devFill.sizeDelta = new Vector2(Mathf.Max(minFillWidth, devTrack.rect.width * ratio), devFill.sizeDelta.y);
    }
}
