using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PortraitSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI name;
    //[SerializeField] private TextMeshProUGUI level;

    // 호버 연출 색상
    [SerializeField] private Color normalColor = new Color(0.6f, 0.6f, 0.6f, 1f); // 평소 약간 어둡게
    [SerializeField] private Color hoverColor = Color.white;                      // 호버 시 밝게

    private CoopCharState charState;
    private Action<CoopCharState> onClickCallback;

    public void SetData(CoopCharState state, Action<CoopCharState> onClick = null)
    {
        charState = state;
        onClickCallback = onClick;
        portraitImage.sprite = state.charData.charImage;
        portraitImage.color = normalColor;
        name.text = state.charData.charName;
        //level.text = state.currentCoopLevel.ToString();
    }


    // 추후 대화 이벤트 등을 위한 상호작용 함수들
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(portraitImage != null)
        {
            portraitImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (portraitImage != null)
        {
            portraitImage.color = normalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(charState);
        Debug.Log($"Clicked on {charState.charData.charName}");
    }
}
