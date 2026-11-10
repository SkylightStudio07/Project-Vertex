using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ============================================================
// filename   : BlessingChoiceRowUI.cs
// description: 축복 화면의 개별 선택지 줄 UI 컴포넌트.
//              [번호] [아이콘 칸] [제목 + 영문 라벨] | [설명] [→] 구조를 제어합니다. (축복 v1 — 로비 스타일)
//              판 스프라이트(기본/호버/비활성)는 Button SpriteSwap이, 호버 시 내용 밀림·아이콘 칸 반전은 여기서 처리한다.
// ============================================================

public class BlessingChoiceRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;

    [Header("축복 v1 (비워두면 쓰지 않음)")]
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private TextMeshProUGUI engText;
    [Tooltip("호버 때 왼쪽으로 밀리는 내용 묶음")]
    [SerializeField] private RectTransform content;
    [SerializeField] private float hoverShift = -8f;
    [SerializeField] private Image iconCell;
    [SerializeField] private Sprite cellNormal, cellHover, cellDisabled;
    [Tooltip("호버(먹색 칸)용 흰 아이콘. 원래 아이콘 이름 + \"_White\"로 찾는다")]
    [SerializeField] private Sprite[] hoverIcons;
    [Tooltip("비활성일 때 흐리게 할 글자·기호")]
    [SerializeField] private Graphic[] dimOnDisabled;

    [Header("축복 A안 (판·기호 상태 교체)")]
    [Tooltip("content 안에 들어 있는 판. 호버 때 내용과 함께 밀린다")]
    [SerializeField] private Image panelImage;
    [SerializeField] private Sprite panelNormal, panelHover, panelDisabled;
    [SerializeField] private Image chevronImage;
    [SerializeField] private Sprite chevronNormal, chevronHover;
    [Tooltip("켜면 아이콘을 항상 \"_White\" 버전으로 쓴다 (어두운 마름모 칸)")]
    [SerializeField] private bool alwaysWhiteIcon;

    private static readonly Color DimColor = new(0.55f, 0.58f, 0.62f, 1f);
    private Color[] _baseColors;
    private Sprite _icon, _iconHover;
    private bool _available = true;

    [Header("입력 반응")]
    [SerializeField, Range(1f, 1.08f)] private float hoverScale = 1.022f;
    [SerializeField, Range(0.9f, 1f)] private float pressScale = 0.98f;
    [SerializeField, Min(0f)] private float enterStagger = 0.06f;
    private Image _sweep;
    private CanvasGroup _group;
    private bool _hovering;

    public Button Button => button;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (dimOnDisabled != null)
        {
            _baseColors = new Color[dimOnDisabled.Length];
            for (int i = 0; i < dimOnDisabled.Length; i++)
                if (dimOnDisabled[i] != null) _baseColors[i] = dimOnDisabled[i].color;
        }
    }

    public void Setup(BlessingChoice choice, Action onClick)
    {
        if (button == null) button = GetComponent<Button>();

        if (iconImage != null)
        {
            if (choice.icon != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = choice.icon;
                _icon = choice.icon;
                _iconHover = null;
                if (hoverIcons != null)
                    foreach (var s in hoverIcons)
                        if (s != null && s.name == choice.icon.name + "_White") { _iconHover = s; break; }
                if (alwaysWhiteIcon && _iconHover != null) { _icon = _iconHover; iconImage.sprite = _icon; }
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        if (titleText != null)
        {
            titleText.text = choice.title;
            if (numberText == null) titleText.color = choice.titleColor; // v1 레이아웃에서는 제목을 먹색으로 고정
        }

        if (descText != null)
        {
            descText.text = choice.description;
        }

        if (numberText != null) numberText.text = (transform.GetSiblingIndex() + 1).ToString("00");
        if (engText != null) engText.text = EnglishLabel(choice);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }
        }

        SetAvailable(IsAvailable(choice));
    }

    // 고를 수 없는 선택지(강화할 카드 없음 등)는 비활성 판 + 흐린 글자
    private static bool IsAvailable(BlessingChoice choice)
    {
        var deck = DeckManager.Instance;
        return choice.effectType switch
        {
            BlessingEffectType.UpgradeCards => deck == null || deck.GetUpgradableCards().Count > 0,
            BlessingEffectType.RemoveCard => deck == null || deck.PlayerDeck.Count > 0,
            _ => true,
        };
    }

    private void SetAvailable(bool available)
    {
        _available = available;
        if (button != null) button.interactable = available;
        if (iconCell != null && cellNormal != null) iconCell.sprite = available ? cellNormal : (cellDisabled != null ? cellDisabled : cellNormal);
        if (panelImage != null && panelNormal != null) panelImage.sprite = available ? panelNormal : (panelDisabled != null ? panelDisabled : panelNormal);
        if (chevronImage != null && chevronNormal != null) chevronImage.sprite = chevronNormal;
        if (dimOnDisabled == null) return;
        if (_baseColors == null || _baseColors.Length != dimOnDisabled.Length) Awake();
        for (int i = 0; i < dimOnDisabled.Length; i++)
            if (dimOnDisabled[i] != null) dimOnDisabled[i].color = available ? _baseColors[i] : DimColor;
    }

    private static string EnglishLabel(BlessingChoice choice) => choice.effectType switch
    {
        BlessingEffectType.AffinityTalk => "COMMUNE",
        BlessingEffectType.RemoveCard => "PURIFY",
        BlessingEffectType.UpgradeCards => "REFINE",
        BlessingEffectType.GainRandomItems => "SUPPLY",
        BlessingEffectType.GainGold => "FUNDS",
        BlessingEffectType.HealHP => "MEND",
        BlessingEffectType.MaxHP => "VIGOR",
        _ => "",
    };

    public void OnPointerEnter(PointerEventData eventData) => SetHover(true);
    public void OnPointerExit(PointerEventData eventData) => SetHover(false);
    public void OnPointerDown(PointerEventData eventData) { if (_available) ScaleTo(pressScale, 0.06f); }
    public void OnPointerUp(PointerEventData eventData) { if (_available) ScaleTo(_hovering ? hoverScale : 1f, 0.12f); }

    private void OnDisable()
    {
        SetHover(false, instant: true);
        transform.DOKill();
        transform.localScale = Vector3.one;
        if (_group != null) { _group.DOKill(); _group.alpha = 1f; }
    }

    // 선택지가 나타날 때 위에서부터 차례로 밀려 들어온다
    private void OnEnable()
    {
        if (_group == null && !TryGetComponent(out _group)) _group = gameObject.AddComponent<CanvasGroup>();
        float delay = transform.GetSiblingIndex() * enterStagger;
        _group.DOKill();
        _group.alpha = 0f;
        _group.DOFade(1f, 0.22f).SetDelay(delay).SetUpdate(true).SetLink(gameObject);
        if (content == null) return;
        content.DOKill();
        content.anchoredPosition = new Vector2(56f, content.anchoredPosition.y);
        content.DOAnchorPosX(0f, 0.3f).SetDelay(delay).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(gameObject);
    }

    private void ScaleTo(float scale, float duration)
    {
        transform.DOKill();
        transform.DOScale(scale, duration).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(gameObject);
    }

    // 호버 순간 판 위로 빛줄기가 한 번 지나가고 아이콘이 튄다
    private void PlayHoverAccent()
    {
        if (iconCell != null)
        {
            iconCell.rectTransform.DOComplete();
            iconCell.rectTransform.DOPunchScale(Vector3.one * 0.12f, 0.25f, 5, 0.5f).SetUpdate(true).SetLink(gameObject);
        }
        if (panelImage == null) return;
        if (_sweep == null)
        {
            var fx = GetComponentInParent<BlessingAmbienceFx>(true);
            if (fx == null || fx.BandSprite == null) return;
            var go = new GameObject("HoverSweep", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(panelImage.transform.parent, false);
            go.transform.SetSiblingIndex(panelImage.transform.GetSiblingIndex() + 1);
            _sweep = go.GetComponent<Image>();
            _sweep.sprite = fx.BandSprite;
            _sweep.material = fx.AdditiveMaterial;
            _sweep.raycastTarget = false;
            var rt = _sweep.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(240f, panelImage.rectTransform.rect.height * 1.25f);
            rt.localEulerAngles = new Vector3(0f, 0f, -14f);
        }
        var sweep = _sweep.rectTransform;
        float h = panelImage.rectTransform.rect.height, w = panelImage.rectTransform.rect.width;
        sweep.DOKill(); _sweep.DOKill();
        sweep.anchoredPosition = new Vector2(40f, -h * 0.5f);
        _sweep.color = new Color(0.8f, 0.93f, 1f, 0f);
        DOTween.Sequence().SetUpdate(true).SetLink(gameObject).SetTarget(sweep)
            .Append(sweep.DOAnchorPosX(w - 40f, 0.42f).SetEase(Ease.OutQuad))
            .Join(_sweep.DOFade(0.5f, 0.1f))
            .Insert(0.18f, _sweep.DOFade(0f, 0.24f));
    }

    private void SetHover(bool on, bool instant = false)
    {
        bool hover = on && _available;
        if (!instant && hover != _hovering)
        {
            ScaleTo(hover ? hoverScale : 1f, 0.14f);
            if (hover) PlayHoverAccent();
        }
        _hovering = hover;
        if (iconCell != null && cellNormal != null && _available)
            iconCell.sprite = hover && cellHover != null ? cellHover : cellNormal;
        if (panelImage != null && panelNormal != null && _available)
            panelImage.sprite = hover && panelHover != null ? panelHover : panelNormal;
        if (chevronImage != null && chevronNormal != null)
            chevronImage.sprite = hover && chevronHover != null ? chevronHover : chevronNormal;
        // 먹색 칸 위에서는 흰 아이콘으로 바꿔 끼운다
        if (iconImage != null && _icon != null) iconImage.sprite = hover && _iconHover != null ? _iconHover : _icon;
        if (content == null) return;
        content.DOKill();
        float x = hover ? hoverShift : 0f;
        if (instant) content.anchoredPosition = new Vector2(x, content.anchoredPosition.y);
        else content.DOAnchorPosX(x, 0.15f).SetEase(Ease.OutCubic).SetLink(gameObject);
    }
}
