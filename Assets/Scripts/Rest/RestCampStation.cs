using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 휴식 캠프에서 합류 캐릭터 한 명이 "자기 자리에서 할 일을 하고 있는" 스테이션.
// 씬에 자리(위치·크기)만 배치해 두면 RestView가 합류 순서대로 캐릭터를 채운다. 빈 자리는 숨긴다.
// 클릭 판정은 캐릭터 그림 전체(button)로 받는다.
public class RestCampStation : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI activityText;
    [Tooltip("호감도 랭크업 이벤트가 대기 중일 때 켜지는 표시(!)")]
    [SerializeField] private GameObject eventMarker;
    [SerializeField] private Button button;

    public CoopCharState State { get; private set; }
    public event Action<RestCampStation> OnClicked;

    private void Awake()
    {
        if (button != null) button.onClick.AddListener(() => OnClicked?.Invoke(this));
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
        if (activityText != null)
        {
            activityText.text = data.campActivity;
            activityText.gameObject.SetActive(!string.IsNullOrWhiteSpace(data.campActivity));
        }
        RefreshMarker();
    }

    public void RefreshMarker()
    {
        if (eventMarker != null) eventMarker.SetActive(State != null && State.isLevelUp);
    }
}
