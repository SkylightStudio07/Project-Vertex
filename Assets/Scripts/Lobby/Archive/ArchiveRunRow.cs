using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 플레이 기록 한 줄: 번호 · 날짜 · 결과(클리어/실패) · 도달(n막 n층) · 격퇴·턴·카드 · 탈린.
public class ArchiveRunRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI indexText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Image resultBadge;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI reachText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI talinText;
    [SerializeField] private Color clearColor = new(0.702f, 0.149f, 0.227f, 1f);   // 진홍
    [SerializeField] private Color failColor = new(0.3f, 0.29f, 0.33f, 1f);

    public void Bind(PlayerRecord.RunEntry run, int number)
    {
        indexText.text = $"#{number:00}";
        dateText.text = run.Ended.ToString("yyyy.MM.dd  HH:mm");
        resultBadge.color = run.cleared ? clearColor : failColor;
        resultText.text = run.cleared ? "클리어" : "실패";
        reachText.text = run.cleared ? $"{run.act}막 돌파" : $"{run.act}막 {run.floor}층";
        statsText.text = $"격퇴 {run.kills}   ·   턴 {run.turns}   ·   카드 {run.cards}";
        talinText.text = run.talin > 0 ? $"탈린 +{run.talin}" : "";
    }
}
