using TMPro;
using UnityEngine;

// 시설 화면 위쪽 재화 표시 (탈린 · 파츠 · 아티팩트). MetaProgress가 바뀌면 다시 그린다.
public class CurrencyReadout : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI talinText;
    [SerializeField] private TextMeshProUGUI partsText;
    [SerializeField] private TextMeshProUGUI artifactText;

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
    }
}
