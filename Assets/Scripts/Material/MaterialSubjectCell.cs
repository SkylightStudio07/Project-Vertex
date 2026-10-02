using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 마테리얼 격자의 대상 칸 (적 / 동료). 아직 한 편도 열리지 않은 대상은 실루엣 + "???".
public class MaterialSubjectCell : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image frame;
    [SerializeField] private UICroppedArt portrait;
    [SerializeField] private Image lockIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statusText; // 없어도 된다 (아트 v1은 점만)
    [Tooltip("기록 점 5개 — 열린 편 수만큼 채운다")]
    [SerializeField] private List<Image> dots = new();
    [SerializeField] private Sprite dotFilled;
    [SerializeField] private Sprite dotEmpty;

    [Header("스프라이트")]
    [SerializeField] private Sprite frameNormal;
    [SerializeField] private Sprite frameHover;
    [SerializeField] private Sprite frameSelected;
    [SerializeField] private Sprite frameLocked;

    [Header("미확인")]
    [SerializeField] private Material silhouetteMaterial;
    [SerializeField] private Color silhouetteColor = new(0.62f, 0.64f, 0.67f, 1f);
    [SerializeField] private Color inkColor = new(0.086f, 0.094f, 0.106f, 1f);
    [SerializeField] private Color subColor = new(0.42f, 0.44f, 0.47f, 1f);
    [SerializeField] private Color cyanColor = new(0.05f, 0.72f, 0.95f, 1f);

    private Action _onClick;

    private void Awake() => button.onClick.AddListener(() => _onClick?.Invoke());

    public void Bind(Sprite art, Vector2 focus, float zoom, string displayName, string status, int unlocked, bool known, bool selected, Action onClick)
    {
        _onClick = onClick;
        frame.sprite = !known ? frameLocked : selected ? frameSelected : frameNormal;
        var ss = button.spriteState;
        ss.highlightedSprite = selected ? null : frameHover;
        ss.pressedSprite = ss.highlightedSprite;
        button.spriteState = ss;

        var raw = portrait.GetComponent<RawImage>();
        portrait.SetSprite(art, focus, zoom);
        raw.material = known ? null : silhouetteMaterial;
        raw.color = known ? Color.white : silhouetteColor;
        lockIcon.enabled = !known;

        nameText.text = known ? displayName : "???";
        nameText.color = known ? inkColor : subColor;
        if (statusText != null)
        {
            statusText.text = status;
            statusText.color = selected ? cyanColor : subColor;
        }
        for (int i = 0; i < dots.Count; i++) dots[i].sprite = i < unlocked ? dotFilled : dotEmpty;
    }
}
