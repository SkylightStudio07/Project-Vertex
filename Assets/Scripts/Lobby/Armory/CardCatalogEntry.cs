using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 무기고 카드 목록의 카드 한 칸. CardCatalogView가 카드 풀 수만큼 찍어 낸다.
// 해금 카드는 아트·이름·설명, 잠긴 카드는 잠금 틀에 "???"와 해금 조건만 보여준다.
public class CardCatalogEntry : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image frame;
    [SerializeField] private UICroppedArt art;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI lockText;
    [SerializeField] private GameObject selectedMark;

    [Header("틀 스프라이트")]
    [SerializeField] private Sprite frameNormal;
    [SerializeField] private Sprite frameHover;
    [SerializeField] private Sprite frameLocked;

    public CardData Card { get; private set; }
    public bool Locked { get; private set; }
    public event Action<CardCatalogEntry> OnClicked;

    private void Awake()
    {
        if (button != null) button.onClick.AddListener(() => OnClicked?.Invoke(this));
    }

    public void Bind(CardData card, bool locked, string lockCondition)
    {
        Card = card;
        Locked = locked;

        frame.sprite = locked ? frameLocked : frameNormal;
        var ss = button.spriteState; ss.highlightedSprite = locked ? null : frameHover; ss.pressedSprite = ss.highlightedSprite; button.spriteState = ss;
        button.transition = locked ? Selectable.Transition.None : Selectable.Transition.SpriteSwap;

        art.gameObject.SetActive(!locked);
        if (!locked) art.SetSprite(card.CardImage);

        nameText.text = locked ? "???" : card.CardName;
        descText.gameObject.SetActive(!locked);
        if (!locked) descText.text = card.GetFullDescription();
        lockText.gameObject.SetActive(locked);
        lockText.text = lockCondition;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedMark != null) selectedMark.SetActive(selected);
    }
}
