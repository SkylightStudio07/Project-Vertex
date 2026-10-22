using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 아카이브 > 아이템 도감. 한 줄에 10칸씩 아이템을 늘어놓고, 마우스를 올리거나 누르면 오른쪽에 자세히 보여 준다.
// 아직 얻지 못한 아이템은 실루엣 + "???". 얻은 기록은 PlayerRecord.AddObtainedItem (인벤토리에 처음 들어올 때).
public class ArchiveItemCodex : MonoBehaviour
{
    [SerializeField] private ItemCatalog catalog;
    [SerializeField] private Button backButton;
    [SerializeField] private ArchiveItemCell cellTemplate;
    [SerializeField] private Transform grid;
    [SerializeField] private TextMeshProUGUI countText;

    [Header("자세히")]
    [SerializeField] private Image detailIcon;
    [SerializeField] private TextMeshProUGUI detailName;
    [SerializeField] private TextMeshProUGUI detailRarity;
    [SerializeField] private TextMeshProUGUI detailBody;
    [SerializeField] private Color crimson = new(0.702f, 0.149f, 0.227f, 1f);

    public event Action OnClosed;

    private readonly List<ArchiveItemCell> _cells = new();
    private ArchiveItemCell _pinned;

    private void Awake()
    {
        cellTemplate.gameObject.SetActive(false);
        backButton.onClick.AddListener(() => { gameObject.SetActive(false); OnClosed?.Invoke(); });
    }

    private void OnEnable() => Rebuild();

    private void Rebuild()
    {
        foreach (var c in _cells) if (c != null) Destroy(c.gameObject);
        _cells.Clear();
        _pinned = null;

        int found = 0;
        if (catalog != null)
            foreach (var item in catalog.items)
            {
                if (item == null) continue;
                bool known = PlayerRecord.HasObtained(item);
                if (known) found++;
                var cell = Instantiate(cellTemplate, grid);
                cell.gameObject.SetActive(true);
                cell.Bind(item, known);
                cell.OnHovered += Show;
                cell.OnUnhovered += _ => Show(_pinned);
                cell.OnClicked += c => { _pinned = c; Refresh(); Show(c); };
                _cells.Add(cell);
            }
        countText.text = $"수집 {found} / {_cells.Count}";

        _pinned = _cells.Find(c => c.Known);
        Refresh();
        Show(_pinned);
    }

    private void Refresh()
    {
        foreach (var c in _cells) c.SetSelected(c == _pinned);
    }

    private void Show(ArchiveItemCell cell)
    {
        bool has = cell != null;
        bool known = has && cell.Known;
        var item = has ? cell.Item : null;

        detailIcon.enabled = has && item.ItemIcon != null;
        if (has)
        {
            detailIcon.sprite = item.ItemIcon;
            detailIcon.color = known ? Color.white : new Color(0f, 0f, 0f, 0.85f);
        }
        detailName.text = known ? item.ItemName : "???";
        detailRarity.text = known ? RarityLabel(item.Rarity) : "";
        detailRarity.color = crimson;
        detailBody.text = !has ? "아이템을 고르세요."
            : known ? item.ItemDescription
            : "아직 손에 넣지 못한 물건이에요.";
    }

    private static string RarityLabel(ItemData.ItemRarity r) => r switch
    {
        ItemData.ItemRarity.Rare => "희귀",
        ItemData.ItemRarity.Uncommon => "특별",
        _ => "일반",
    };
}
