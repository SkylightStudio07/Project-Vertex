using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 마테리얼 01~05 색인 탭. 열림 / 선택(청록 아래 선) / 잠김(회색 + 자물쇠) / 잠김 선택.
public class MaterialEntryTab : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image frame;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private Image lockIcon;
    [SerializeField] private Sprite normal;
    [SerializeField] private Sprite hover;
    [SerializeField] private Sprite selected;
    [SerializeField] private Sprite locked;
    [SerializeField] private Sprite lockedSelected;
    [SerializeField] private Color inkColor = new(0.086f, 0.094f, 0.106f, 1f);
    [SerializeField] private Color lockedColor = new(0.42f, 0.44f, 0.47f, 1f);

    private Action _onClick;

    private void Awake() => button.onClick.AddListener(() => _onClick?.Invoke());

    public void Bind(int index, bool unlocked, bool isSelected, Action onClick)
    {
        _onClick = onClick;
        frame.sprite = unlocked ? (isSelected ? selected : normal) : (isSelected ? lockedSelected : locked);
        var ss = button.spriteState;
        ss.highlightedSprite = unlocked && !isSelected ? hover : null;
        ss.pressedSprite = ss.highlightedSprite;
        button.spriteState = ss;
        numberText.text = $"{index + 1:00}";
        numberText.color = unlocked ? inkColor : lockedColor;
        lockIcon.enabled = !unlocked;
    }
}
