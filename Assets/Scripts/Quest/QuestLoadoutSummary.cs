using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 출정 준비 헤더 오른쪽: "진행 중 의뢰 N / 2" + 수주한 의뢰 칩(최대 2). 칩에 마우스를 올리면 아래에 목표 툴팁.
// 수주한 의뢰가 없으면 흐린 글자 "의뢰 없음 · 의뢰 게시판에서 수주".
// LoadoutKit(1672×941 좌표) 안, 왼쪽 위 기준 (1100,26)에 310×56으로 둔다. 조각은 QuestRunSkin.
public class QuestLoadoutSummary : MonoBehaviour
{
    private const float W = 310f, ChipW = 150f, ChipH = 30f, ChipGap = 10f;

    private QuestRunSkin _skin;
    private TextMeshProUGUI _header;
    private Image _empty;
    private TextMeshProUGUI _emptyText;
    private readonly List<GameObject> _chips = new();
    private RectTransform _tip;
    private TextMeshProUGUI _tipTitle, _tipLine1, _tipLine2, _tipFoot;
    private QuestManager _quests;

    private void Awake()
    {
        _skin = QuestRunSkin.Instance;
        if (_skin == null) { enabled = false; return; }

        var head = QuestRunSkin.Image("Header", transform, _skin.summaryHeader, 0f, 0f, W, 24f);
        head.preserveAspect = false;
        _header = _skin.Text("Label", head.transform, 10f, 0f, 290f, 24f, 13f, QuestRunSkin.Graphite);

        _empty = QuestRunSkin.Image("Empty", transform, _skin.summaryEmpty, 0f, 26f, W, ChipH);
        _empty.preserveAspect = false;
        _emptyText = _skin.Text("Text", _empty.transform, 12f, 0f, 286f, ChipH, 12f, new Color(0.55f, 0.57f, 0.6f, 1f));
        _emptyText.text = "의뢰 없음 · 의뢰 게시판에서 수주";

        // 목표 툴팁 (칩 아래). 다른 요소 위로 그린다
        _tip = QuestRunSkin.Rect("Tooltip", transform, 0f, 62f, 300f, 110f);
        var cv = _tip.gameObject.AddComponent<Canvas>();
        cv.overrideSorting = true;
        cv.sortingOrder = 30;
        var bg = QuestRunSkin.Image("Bg", _tip, _skin.tooltip, 0f, 0f, 300f, 110f);
        bg.preserveAspect = false;
        _tipTitle = _skin.Text("Title", _tip, 12f, 6f, 276f, 26f, 16f, QuestRunSkin.Graphite);
        _tipTitle.fontStyle = FontStyles.Bold;
        _tipLine1 = _skin.Text("Line1", _tip, 32f, 44f, 256f, 22f, 12.5f, QuestRunSkin.Graphite);
        _tipLine2 = _skin.Text("Line2", _tip, 32f, 71f, 170f, 22f, 12.5f, QuestRunSkin.Graphite);
        _tipFoot = _skin.Text("Foot", _tip, 150f, 84f, 138f, 18f, 10f, QuestRunSkin.GraphiteSoft, TextAlignmentOptions.MidlineRight);
        _tip.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (_skin == null) return;
        _quests = QuestManager.Instance;
        _quests.OnStateChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (_quests != null) _quests.OnStateChanged -= Refresh;
        if (_tip != null) _tip.gameObject.SetActive(false);
    }

    public void Refresh()
    {
        foreach (var c in _chips) Destroy(c);
        _chips.Clear();

        var accepted = new List<QuestData>(_quests.Accepted);
        _header.text = $"진행 중 의뢰   <b>{accepted.Count} / {_quests.MaxActive}</b>";
        _empty.gameObject.SetActive(accepted.Count == 0);

        for (int i = 0; i < accepted.Count && i < 2; i++)
            _chips.Add(BuildChip(accepted[i], i * (ChipW + ChipGap)));
    }

    private GameObject BuildChip(QuestData quest, float x)
    {
        var chip = QuestRunSkin.Image("Chip_" + quest.questId, transform, _skin.chip, x, 26f, ChipW, ChipH, raycast: true);
        chip.preserveAspect = false;
        QuestRunSkin.Image("Icon", chip.transform, _skin.Icon(quest.IconKind), 8f, 6f, 18f, 18f);
        var title = _skin.Text("Title", chip.transform, 32f, 0f, quest.tag != QuestTag.None ? 96f : 110f, ChipH, 12f, QuestRunSkin.Graphite);
        title.text = quest.title;
        title.fontStyle = FontStyles.Bold;
        if (quest.tag != QuestTag.None)
        {
            var dot = QuestRunSkin.Image("Dot", chip.transform, _skin.dot, 132f, 10f, 10f, 10f);
            dot.color = quest.tag == QuestTag.Story ? QuestRunSkin.Accent : new Color(0.05f, 0.72f, 0.95f, 0.45f);
        }

        var trigger = chip.gameObject.AddComponent<EventTrigger>();
        AddTrigger(trigger, EventTriggerType.PointerEnter, () => { chip.sprite = _skin.chipHover; ShowTip(quest, x); });
        AddTrigger(trigger, EventTriggerType.PointerExit, () => { chip.sprite = _skin.chip; _tip.gameObject.SetActive(false); });

        // 등장: 살짝 내려오며 페이드
        var cg = chip.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        var rt = chip.rectTransform;
        rt.anchoredPosition += new Vector2(0f, 8f);
        DOTween.Sequence().SetLink(chip.gameObject)
            .AppendInterval(0.35f + x / (ChipW + ChipGap) * 0.08f)
            .Append(cg.DOFade(1f, 0.2f))
            .Join(rt.DOAnchorPosY(rt.anchoredPosition.y - 8f, 0.25f).SetEase(Ease.OutCubic));
        return chip.gameObject;
    }

    private void ShowTip(QuestData quest, float x)
    {
        var lines = quest.Objectives();
        _tipTitle.text = quest.title;
        _tipLine1.text = lines.Count > 0 ? lines[0] : "";
        _tipLine2.text = lines.Count > 1 ? lines[1] : "";
        _tipFoot.text = quest.client;
        _tip.anchoredPosition = new Vector2(Mathf.Min(x, W - 300f + 160f), -62f);
        _tip.gameObject.SetActive(true);
        _tip.SetAsLastSibling();
    }

    private static void AddTrigger(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }
}
