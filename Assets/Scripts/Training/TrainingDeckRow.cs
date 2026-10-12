using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 훈련장 덱 한 줄: 카드 그림 크롭 · 이름 · 장수. 덱 편집 줄이면 −/+ 버튼이 붙는다(메인 요약 줄은 없음).
public class TrainingDeckRow : MonoBehaviour
{
    [SerializeField] private UICroppedArt icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;

    private Action _onMinus, _onPlus;

    private void Awake()
    {
        if (minusButton != null) minusButton.onClick.AddListener(() => _onMinus?.Invoke());
        if (plusButton != null) plusButton.onClick.AddListener(() => _onPlus?.Invoke());
    }

    public void Bind(CardData card, int count, Action onMinus = null, Action onPlus = null)
    {
        // 요약 줄은 "× 5", 편집 줄은 −/+ 사이에 숫자만
        Bind(card.CardImage, card.CardName, minusButton != null ? count.ToString() : $"× {count}", onMinus, onPlus);
        if (minusButton != null) minusButton.interactable = onMinus != null && count > 0;
    }

    // 카드 말고 다른 것(동료 편성 줄 — 초상 · 이름 · Lv)에도 같은 줄을 쓴다
    public void Bind(Sprite iconSprite, string title, string right, Action onMinus = null, Action onPlus = null)
    {
        icon.SetSprite(iconSprite);
        nameText.text = title;
        countText.text = right;
        _onMinus = onMinus;
        _onPlus = onPlus;
        if (minusButton != null) minusButton.interactable = onMinus != null;
        if (plusButton != null) plusButton.gameObject.SetActive(onPlus != null);
    }
}
