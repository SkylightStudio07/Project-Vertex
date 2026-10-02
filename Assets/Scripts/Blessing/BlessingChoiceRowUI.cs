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

public class BlessingChoiceRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

    private static readonly Color DimColor = new(0.55f, 0.58f, 0.62f, 1f);
    private Color[] _baseColors;
    private Sprite _icon, _iconHover;
    private bool _available = true;

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
    private void OnDisable() => SetHover(false, instant: true);

    private void SetHover(bool on, bool instant = false)
    {
        bool hover = on && _available;
        if (iconCell != null && cellNormal != null && _available)
            iconCell.sprite = hover && cellHover != null ? cellHover : cellNormal;
        // 먹색 칸 위에서는 흰 아이콘으로 바꿔 끼운다
        if (iconImage != null && _icon != null) iconImage.sprite = hover && _iconHover != null ? _iconHover : _icon;
        if (content == null) return;
        content.DOKill();
        float x = hover ? hoverShift : 0f;
        if (instant) content.anchoredPosition = new Vector2(x, content.anchoredPosition.y);
        else content.DOAnchorPosX(x, 0.15f).SetEase(Ease.OutCubic).SetLink(gameObject);
    }
}
