using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 덱 편집 필터 칸 (종류·소속 체크박스, 코스트 칩 공용). 켜짐/꺼짐 스프라이트와 각 호버를 바꿔 끼운다.
public class TrainingToggle : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image box;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite offHover;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite onHover;

    public bool IsOn { get; private set; }
    public string Key { get; private set; }
    public event Action<TrainingToggle> OnClicked;

    private void Awake() => button.onClick.AddListener(() => OnClicked?.Invoke(this));

    public void Setup(string key, string text)
    {
        Key = key;
        if (label != null && text != null) label.text = text; // null이면 씬에 적어 둔 글자 유지
    }

    public void SetOn(bool on)
    {
        IsOn = on;
        box.sprite = on ? onSprite : offSprite;
        var ss = button.spriteState;
        ss.highlightedSprite = on ? onHover : offHover;
        ss.pressedSprite = ss.highlightedSprite;
        button.spriteState = ss;
    }
}
