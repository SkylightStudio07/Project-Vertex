using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 훈련장 동료 초상 카드: 동행(청록 아래 선 + "동행" 태그) / 기본 / 잠김(실루엣 + 자물쇠 + 호감도 조건).
public class TrainingCompanionCard : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image frame;
    [SerializeField] private UICroppedArt portrait;
    [SerializeField] private Image lockIcon;
    [SerializeField] private GameObject companionTag;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI conditionText;

    [Header("스프라이트")]
    [SerializeField] private Sprite frameNormal;
    [SerializeField] private Sprite frameHover;
    [SerializeField] private Sprite frameSelected;
    [SerializeField] private Sprite frameLocked;
    [Header("빈 동행 칸 (메인)")]
    [SerializeField] private Sprite emptyNormal;
    [SerializeField] private Sprite emptyHover;
    [SerializeField] private GameObject plusIcon;
    [SerializeField] private TextMeshProUGUI emptyLabel;

    [Header("잠김")]
    [SerializeField] private Material silhouetteMaterial;
    [SerializeField] private Color silhouetteColor = new(0.62f, 0.64f, 0.67f, 1f);
    [Tooltip("초상 크롭 — 얼굴 쪽을 남긴다")]
    [SerializeField] private Vector2 portraitFocus = new(0.5f, 0.85f);
    [SerializeField, Min(1f)] private float portraitZoom = 1.6f;

    private Action _onClick;

    private void Awake() => button.onClick.AddListener(() => _onClick?.Invoke());

    public void Bind(CoopCharData data, int level, bool available, bool selected, Action onClick)
    {
        _onClick = available ? onClick : null;

        frame.sprite = !available ? frameLocked : selected ? frameSelected : frameNormal;
        var ss = button.spriteState;
        ss.highlightedSprite = available && !selected ? frameHover : null;
        ss.pressedSprite = ss.highlightedSprite;
        button.spriteState = ss;
        button.interactable = available;

        // 잠긴 동료는 배경이 투명한 전투 스탠딩만 실루엣으로 쓴다 (초상화는 배경이 칠해진 그림일 수 있다)
        Sprite art = !available ? data.standingSprite
                   : data.charImage != null ? data.charImage : data.standingSprite;
        var raw = portrait.GetComponent<RawImage>();
        portrait.SetSprite(art, portraitFocus, portraitZoom);
        raw.material = available ? null : silhouetteMaterial;
        raw.color = available ? Color.white : silhouetteColor;
        lockIcon.enabled = !available;
        companionTag.SetActive(selected);
        SetEmptyVisible(false);

        nameText.gameObject.SetActive(available);
        levelText.gameObject.SetActive(available);
        conditionText.gameObject.SetActive(!available);
        nameText.text = string.IsNullOrWhiteSpace(data.charName) ? data.charID : data.charName;
        levelText.text = $"Lv.{level}";
        conditionText.text = $"호감도 Lv.{data.trainingRequiredLevel} 필요";
    }

    // 메인 화면의 빈 동행 칸 — 누르면 동료 편성 페이지
    public void BindEmpty(Action onClick)
    {
        _onClick = onClick;
        frame.sprite = emptyNormal != null ? emptyNormal : frameNormal;
        var ss = button.spriteState;
        ss.highlightedSprite = emptyHover != null ? emptyHover : frameHover;
        ss.pressedSprite = ss.highlightedSprite;
        button.spriteState = ss;
        button.interactable = true;
        portrait.GetComponent<RawImage>().enabled = false;
        lockIcon.enabled = false;
        companionTag.SetActive(false);
        nameText.gameObject.SetActive(false);
        levelText.gameObject.SetActive(false);
        conditionText.gameObject.SetActive(false);
        SetEmptyVisible(true);
    }

    private void SetEmptyVisible(bool visible)
    {
        if (plusIcon != null) plusIcon.SetActive(visible);
        if (emptyLabel != null) emptyLabel.gameObject.SetActive(visible);
    }
}
