using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 훈련장 동료 편성 격자의 초상 칸. 기본 / 동행 중(청록 테두리 + 오른쪽 위 "동행" 배지) / 잠김(실루엣 + 자물쇠 + 조건).
// 마우스를 올리면 오른쪽 "선택한 동료" 칸에 정보를 띄운다(OnHovered).
public class TrainingPortraitCell : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image frame;
    [SerializeField] private UICroppedArt portrait;
    [SerializeField] private Image lockIcon;
    [SerializeField] private GameObject partyBadge;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("스프라이트")]
    [SerializeField] private Sprite frameNormal;
    [SerializeField] private Sprite frameHover;
    [SerializeField] private Sprite frameParty;
    [SerializeField] private Sprite frameLocked;

    [Header("잠김")]
    [SerializeField] private Material silhouetteMaterial;
    [SerializeField] private Color silhouetteColor = new(0.62f, 0.64f, 0.67f, 1f);
    [SerializeField] private Color inkColor = new(0.086f, 0.094f, 0.106f, 1f);
    [SerializeField] private Color subColor = new(0.42f, 0.44f, 0.47f, 1f);
    [SerializeField] private Color cyanColor = new(0.05f, 0.72f, 0.95f, 1f);
    [SerializeField] private Vector2 portraitFocus = new(0.5f, 0.85f);
    [SerializeField, Min(1f)] private float portraitZoom = 1.5f;

    private Action _onClick;
    public CoopCharData Data { get; private set; }
    public event Action<TrainingPortraitCell> OnHovered;

    private void Awake() => button.onClick.AddListener(() => _onClick?.Invoke());

    public void OnPointerEnter(PointerEventData eventData) => OnHovered?.Invoke(this);

    public void Bind(CoopCharData data, int level, bool available, bool inParty, Action onClick)
    {
        Data = data;
        _onClick = available ? onClick : null;

        frame.sprite = !available ? frameLocked : inParty ? frameParty : frameNormal;
        var ss = button.spriteState;
        ss.highlightedSprite = available && !inParty ? frameHover : null;
        ss.pressedSprite = ss.highlightedSprite;
        button.spriteState = ss;
        button.interactable = available;

        // 잠긴 동료는 배경이 투명한 전투 스탠딩만 실루엣으로 쓴다 (초상화는 배경이 칠해진 그림일 수 있다)
        Sprite art = !available ? data.standingSprite : data.charImage != null ? data.charImage : data.standingSprite;
        var raw = portrait.GetComponent<RawImage>();
        portrait.SetSprite(art, available ? data.faceFocus : portraitFocus, available ? data.faceZoom : portraitZoom);
        raw.material = available ? null : silhouetteMaterial;
        raw.color = available ? Color.white : silhouetteColor;
        lockIcon.enabled = !available;
        partyBadge.SetActive(inParty);

        nameText.text = !available ? "잠긴 동료" : string.IsNullOrWhiteSpace(data.charName) ? data.charID : data.charName;
        nameText.color = available ? inkColor : subColor;
        statusText.text = !available ? $"호감도 Lv.{data.trainingRequiredLevel} 필요" : inParty ? "동행 중" : $"동행 가능  ·  Lv.{level}";
        statusText.color = inParty ? cyanColor : subColor;
    }
}
