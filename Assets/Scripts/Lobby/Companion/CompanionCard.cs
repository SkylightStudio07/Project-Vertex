using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 동료 목록 칸 하나. 상태: 합류(초상 + 이름·소속 + 호감도 마름모) / 미합류(먹색 실루엣 + ??? + 합류 조건) / 빈 칸(+).
public class CompanionCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image frame;
    [SerializeField] private UICroppedArt portrait;
    [SerializeField] private GameObject band;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI subText;
    [SerializeField] private Image[] pips;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject emptyPlus;
    [SerializeField] private RectTransform lift;          // 마우스 올림 때 살짝 위로 (판 묶음)

    [Header("모양")]
    [SerializeField] private Sprite frameNormal;
    [SerializeField] private Sprite frameHover;
    [SerializeField] private Sprite frameLocked;
    [SerializeField] private Sprite frameEmpty;
    [SerializeField] private Sprite pipEmpty;
    [SerializeField] private Sprite pipFilled;
    [SerializeField] private Material silhouetteMaterial;
    [SerializeField] private Color silhouetteColor = new(0.016f, 0.018f, 0.02f, 1f); // UI/Silhouette는 리니어 값

    public CoopCharData Data { get; private set; }
    public bool Joined { get; private set; }
    public event Action<CompanionCard> OnClicked;

    private bool _hover;

    private void Awake() => button.onClick.AddListener(() => { if (Joined) OnClicked?.Invoke(this); });

    public void BindEmpty(int number)
    {
        Data = null; Joined = false;
        numberText.text = number.ToString("00");
        frame.sprite = frameEmpty;
        portrait.gameObject.SetActive(false);
        band.SetActive(false);
        lockIcon.SetActive(false);
        emptyPlus.SetActive(true);
        button.interactable = false;
    }

    public void Bind(CoopCharData data, int number, bool joined, int level, string condition)
    {
        Data = data; Joined = joined;
        numberText.text = number.ToString("00");
        emptyPlus.SetActive(false);
        band.SetActive(true);
        lockIcon.SetActive(!joined);
        button.interactable = joined;

        bool fullArt = data.cardUseFullArt && data.RosterFullArt != null;
        var art = fullArt ? data.RosterFullArt : data.charImage != null ? data.charImage : data.standingSprite;
        portrait.gameObject.SetActive(art != null);
        if (art != null)
        {
            // 카드는 얼굴보다 넓게(상반신): 얼굴 크롭 배율의 절반 정도. 전신 그림을 쓰는 동료는 자기 크롭 값(cardFocus · cardZoom)
            if (fullArt) portrait.SetSprite(art, data.cardFocus, data.cardZoom);
            else portrait.SetSprite(art, new Vector2(data.faceFocus.x, Mathf.Clamp01(data.faceFocus.y - 0.12f)), Mathf.Max(1f, data.faceZoom * 0.55f));
            var raw = portrait.GetComponent<RawImage>();
            raw.material = joined ? null : silhouetteMaterial;
            raw.color = joined ? Color.white : silhouetteColor;
        }

        nameText.text = joined ? data.charName : "???";
        levelText.text = joined ? $"Lv.{level}" : "";
        subText.text = joined ? data.affiliation : condition;
        for (int i = 0; i < pips.Length; i++)
        {
            pips[i].gameObject.SetActive(joined && i < Mathf.Max(1, data.maxCoopLevel));
            pips[i].sprite = i < level ? pipFilled : pipEmpty;
        }
        Apply();
    }

    public void OnPointerEnter(PointerEventData eventData) { _hover = true; Apply(); }
    public void OnPointerExit(PointerEventData eventData) { _hover = false; Apply(); }
    private void OnDisable() { _hover = false; if (lift != null) lift.anchoredPosition = Vector2.zero; }

    private void Apply()
    {
        if (Data == null) return;
        frame.sprite = !Joined ? frameLocked : _hover ? frameHover : frameNormal;
        if (lift != null) lift.anchoredPosition = new Vector2(0f, Joined && _hover ? 6f : 0f);
    }
}
