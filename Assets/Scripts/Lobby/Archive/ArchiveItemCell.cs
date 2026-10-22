using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 아이템 도감 칸 하나. 얻었으면 아이콘, 아니면 검은 실루엣 + "?".
public class ArchiveItemCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image frame;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI unknownMark;
    [SerializeField] private Sprite frameNormal;
    [SerializeField] private Sprite frameHover;

    public ItemData Item { get; private set; }
    public bool Known { get; private set; }
    public event Action<ArchiveItemCell> OnHovered;
    public event Action<ArchiveItemCell> OnUnhovered;
    public event Action<ArchiveItemCell> OnClicked;

    private bool _selected, _hover;

    private void Awake() => button.onClick.AddListener(() => OnClicked?.Invoke(this));

    public void Bind(ItemData item, bool known)
    {
        Item = item;
        Known = known;
        icon.sprite = item.ItemIcon;
        icon.enabled = item.ItemIcon != null;
        icon.color = known ? Color.white : new Color(0f, 0f, 0f, 0.85f);
        unknownMark.gameObject.SetActive(!known);
    }

    public void SetSelected(bool on)
    {
        _selected = on;
        Apply();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hover = true;
        Apply();
        OnHovered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hover = false;
        Apply();
        OnUnhovered?.Invoke(this);
    }

    private void Apply() => frame.sprite = _selected || _hover ? frameHover : frameNormal;
}
