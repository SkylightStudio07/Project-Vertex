using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 동료 편성 페이지 오른쪽 아래 동행 큰 칸. 채운 칸은 초상 + 이름 + × 버튼, 빈 칸은 점선 + "+".
public class TrainingPartySlot : MonoBehaviour
{
    [SerializeField] private Image frame;
    [SerializeField] private UICroppedArt portrait;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button removeButton;
    [SerializeField] private GameObject plusIcon;
    [SerializeField] private Sprite frameFilled;
    [SerializeField] private Sprite frameEmpty;
    [SerializeField] private Vector2 portraitFocus = new(0.5f, 0.85f);
    [SerializeField, Min(1f)] private float portraitZoom = 1.5f;

    private Action _onRemove;

    private void Awake() => removeButton.onClick.AddListener(() => _onRemove?.Invoke());

    public void Bind(CoopCharData data, Action onRemove)
    {
        bool filled = data != null;
        _onRemove = onRemove;
        frame.sprite = filled ? frameFilled : frameEmpty;
        portrait.SetSprite(filled ? (data.charImage != null ? data.charImage : data.standingSprite) : null,
            filled ? data.faceFocus : portraitFocus, filled ? Mathf.Max(1f, data.faceZoom * 0.95f) : portraitZoom);
        nameText.gameObject.SetActive(filled);
        if (filled) nameText.text = string.IsNullOrWhiteSpace(data.charName) ? data.charID : data.charName;
        removeButton.gameObject.SetActive(filled);
        plusIcon.SetActive(!filled);
    }
}
