using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 아카이브 > 플레이 기록 (슬레이 더 스파이어의 런 기록처럼). 지금은 끝난 런마다 "어디까지 갔는지"를 최근 것부터 한 줄씩.
// 기록은 RunClearView가 런이 끝날 때(클리어·패배) PlayerRecord.AddRun으로 남긴다.
public class ArchivePlayRecords : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI summaryText;
    [SerializeField] private ArchiveRunRow rowTemplate;
    [SerializeField] private Transform list;
    [SerializeField] private TextMeshProUGUI emptyText;

    public event Action OnClosed;

    private readonly List<ArchiveRunRow> _rows = new();

    private void Awake()
    {
        rowTemplate.gameObject.SetActive(false);
        backButton.onClick.AddListener(() => { gameObject.SetActive(false); OnClosed?.Invoke(); });
    }

    private void OnEnable()
    {
        foreach (var r in _rows) if (r != null) Destroy(r.gameObject);
        _rows.Clear();

        int total = 0, clears = 0;
        PlayerRecord.RunEntry best = null;
        foreach (var run in PlayerRecord.Runs())
        {
            total++;
            if (run.cleared) clears++;
            if (best == null || Further(run, best)) best = run;
            var row = Instantiate(rowTemplate, list);
            row.gameObject.SetActive(true);
            row.Bind(run, PlayerRecord.RunCount - total + 1);
            _rows.Add(row);
        }

        emptyText.gameObject.SetActive(total == 0);
        summaryText.text = total == 0 ? "" :
            $"런 <b>{total}</b>회   ·   클리어 <b>{clears}</b>회   ·   최고 기록 <b>{(best.cleared ? "클리어" : $"{best.act}막 {best.floor}층")}</b>";
    }

    private static bool Further(PlayerRecord.RunEntry a, PlayerRecord.RunEntry b)
    {
        if (a.cleared != b.cleared) return a.cleared;
        if (a.act != b.act) return a.act > b.act;
        return a.floor > b.floor;
    }
}
