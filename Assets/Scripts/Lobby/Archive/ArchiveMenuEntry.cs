using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 아카이브 입구의 기록 메뉴 한 칸 (큰 진홍 첫 글자 + 나머지 + 영문 + 수집 수). 마우스를 올리면 판·마름모·아이콘이 밝아진다.
public class ArchiveMenuEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image panel;
    [SerializeField] private Image diamond;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;

    [Header("모양")]
    [SerializeField] private Sprite panelNormal;
    [SerializeField] private Sprite panelHover;
    [SerializeField] private Sprite diamondNormal;
    [SerializeField] private Sprite diamondHover;
    [SerializeField] private Color iconNormal = new(0.933f, 0.941f, 0.949f, 1f);
    [SerializeField] private Color iconHover = new(0.831f, 0.227f, 0.314f, 1f);   // #D43A50

    public event Action<ArchiveMenuEntry> OnHovered;
    public event Action<ArchiveMenuEntry> OnClicked;

    private void Awake() => button.onClick.AddListener(() => OnClicked?.Invoke(this));

    private void OnDisable() => SetHover(false);

    public void SetCount(string text) => countText.text = text;

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHover(true);
        OnHovered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData) => SetHover(false);

    private void SetHover(bool on)
    {
        panel.sprite = on ? panelHover : panelNormal;
        diamond.sprite = on ? diamondHover : diamondNormal;
        icon.color = on ? iconHover : iconNormal;
    }
}
