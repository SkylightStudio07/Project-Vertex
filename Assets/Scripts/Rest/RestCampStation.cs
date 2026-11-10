using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 휴식 캠프에서 합류 캐릭터 한 명이 "자기 자리에서 할 일을 하고 있는" 스테이션.
// RestCampDiorama가 자리를 정하고, RestView가 합류 순서대로 캐릭터를 채운다. 빈 자리는 숨긴다.
// 클릭 판정은 캐릭터 그림 전체(button)로 받고, 이름표는 마우스를 올렸을 때 보인다.
public class RestCampStation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public const float BreathAmount = 0.008f;

    private const float FloatHeight = 90f;
    private const float FloatRange = 12f;
    private const float LabelGap = 18f;
    private const float ActivityHeight = 28f;
    private const float NameHeight = 40f;

    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI activityText;
    [Tooltip("호감도 랭크업 이벤트가 대기 중일 때 켜지는 표시(!)")]
    [SerializeField] private GameObject eventMarker;
    [SerializeField] private Button button;

    private Vector2 _basePosition;
    private float _phase;
    private float _facing = 1f;
    private bool _floats;
    private bool _hovered;
    private Image _activityPlate;

    public CoopCharState State { get; private set; }
    public Vector2 Size { get; private set; }
    public event Action<RestCampStation> OnClicked;

    public static Color FireTint(float flicker) => Color.Lerp(new Color(0.80f, 0.77f, 0.80f, 1f), new Color(1f, 0.92f, 0.84f, 1f), flicker);

    private void Awake()
    {
        if (button != null) button.onClick.AddListener(() => OnClicked?.Invoke(this));
    }

    private void OnDisable()
    {
        _hovered = false;
        SetLabelVisible(false);
    }

    public void Bind(CoopCharState state)
    {
        State = state;
        gameObject.SetActive(state != null);
        if (state == null) return;

        var data = state.charData;
        Sprite sprite = data.campSprite != null ? data.campSprite
                      : data.standingSprite != null ? data.standingSprite
                      : data.charImage;
        if (characterImage != null)
        {
            characterImage.sprite = sprite;
            characterImage.enabled = sprite != null;
            characterImage.preserveAspect = true;
        }
        if (nameText != null) nameText.text = data.charName;
        if (activityText != null) activityText.text = data.campActivity;
        SetLabelVisible(false);
        RefreshMarker();
    }

    // 발밑 위치(1920×1080, 왼쪽 위 원점)와 키에 맞춰 자리를 잡는다. 떠 있는 동료는 발밑보다 위에 뜬다
    public void Place(Vector2 feet, float height, bool mirror, bool floats)
    {
        Sprite sprite = characterImage != null ? characterImage.sprite : null;
        float width = sprite != null ? height * sprite.rect.width / sprite.rect.height : height;
        Size = new Vector2(width, height);
        _floats = floats;
        _facing = mirror ? -1f : 1f;
        _phase = feet.x * 0.013f;
        _basePosition = new Vector2(feet.x - 960f, 540f - feet.y + (floats ? FloatHeight : 0f));

        var rect = (RectTransform)transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = _basePosition;
        rect.sizeDelta = Size;

        if (characterImage != null)
        {
            RectTransform image = characterImage.rectTransform;
            image.anchorMin = Vector2.zero;
            image.anchorMax = Vector2.one;
            image.pivot = new Vector2(0.5f, 0f);
            image.offsetMin = image.offsetMax = Vector2.zero;
            image.localScale = new Vector3(_facing, 1f, 1f);
        }
        PlaceLabel();
    }

    public void Tick(float flicker, float now)
    {
        if (characterImage != null)
        {
            characterImage.color = _hovered ? Color.white : FireTint(flicker);
            float breath = _floats ? 0f : Mathf.Sin(now * 1.6f + _phase) * BreathAmount;
            characterImage.rectTransform.localScale = new Vector3(_facing * (1f - breath * 0.4f), 1f + breath, 1f);
        }
        if (_floats)
            ((RectTransform)transform).anchoredPosition = _basePosition + new Vector2(0f, Mathf.Sin(now * 1.3f + _phase) * FloatRange);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;
        SetLabelVisible(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
        SetLabelVisible(false);
    }

    public void RefreshMarker()
    {
        if (eventMarker != null) eventMarker.SetActive(State != null && State.isLevelUp);
    }

    // 이름표와 하는 일을 머리 위로 옮기고, 글자 길이에 맞춰 판을 늘린다
    private void PlaceLabel()
    {
        Color textColor = RestCampMenu.Hex(0xD1D0CC);
        bool hasActivity = activityText != null && !string.IsNullOrWhiteSpace(activityText.text);
        if (activityText != null)
        {
            if (_activityPlate == null)
            {
                _activityPlate = RestCampMenu.NewImage((RectTransform)transform, "ActivityPlate", RestCampMenu.Load("Tag_Activity"));
                _activityPlate.type = Image.Type.Sliced;
                _activityPlate.transform.SetSiblingIndex(activityText.transform.GetSiblingIndex());
            }
            RestCampMenu.Style(activityText, activityText.text, 14f, 2f, FontStyles.Normal, TextAlignmentOptions.Center, textColor);
            float width = Mathf.Max(168f, activityText.GetPreferredValues().x + 22f);
            RectTransform plate = _activityPlate.rectTransform;
            plate.anchorMin = plate.anchorMax = new Vector2(0.5f, 1f);
            plate.pivot = new Vector2(0.5f, 0f);
            plate.anchoredPosition = new Vector2(0f, LabelGap);
            plate.sizeDelta = new Vector2(width, ActivityHeight);

            RectTransform activity = activityText.rectTransform;
            activity.anchorMin = activity.anchorMax = new Vector2(0.5f, 1f);
            activity.pivot = new Vector2(0.5f, 0f);
            activity.anchoredPosition = new Vector2(0f, LabelGap);
            activity.sizeDelta = new Vector2(width, ActivityHeight);
        }
        if (nameText != null && nameText.transform.parent is RectTransform tag)
        {
            RestCampMenu.Style(nameText, nameText.text, 22f, 0f, FontStyles.Bold, TextAlignmentOptions.Center, textColor);
            tag.anchorMin = tag.anchorMax = new Vector2(0.5f, 1f);
            tag.pivot = new Vector2(0.5f, 0f);
            tag.anchoredPosition = new Vector2(0f, LabelGap + (hasActivity ? ActivityHeight : 0f));
            tag.sizeDelta = new Vector2(Mathf.Max(194f, nameText.GetPreferredValues().x + 28f), NameHeight);
            if (tag.TryGetComponent(out Image plate))
            {
                plate.sprite = RestCampMenu.Load("Tag_Name");
                plate.type = Image.Type.Sliced;
                plate.color = Color.white;
            }
            RectTransform label = nameText.rectTransform;
            label.anchorMin = Vector2.zero;
            label.anchorMax = Vector2.one;
            label.offsetMin = label.offsetMax = Vector2.zero;
        }
        SetLabelVisible(_hovered);
    }

    private void SetLabelVisible(bool visible)
    {
        if (nameText != null && nameText.transform.parent != transform)
            nameText.transform.parent.gameObject.SetActive(visible);
        bool activity = visible && activityText != null && !string.IsNullOrWhiteSpace(activityText.text);
        if (activityText != null) activityText.gameObject.SetActive(activity);
        if (_activityPlate != null) _activityPlate.gameObject.SetActive(activity);
    }
}
