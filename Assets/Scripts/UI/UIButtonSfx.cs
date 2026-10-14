using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 버튼에 붙이면 호버·클릭 때 UI 효과음을 낸다. 누를 수 없는 버튼(interactable 꺼짐)은 조용하다.
// 소리 이름은 Resources/Audio/UI 아래 파일 이름. 비우면 그 소리는 내지 않는다.
public class UIButtonSfx : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private string hoverSfx = "ui_hover";
    [SerializeField] private string clickSfx = "ui_click";

    private Selectable _selectable;

    private void Awake() => _selectable = GetComponent<Selectable>();

    private bool Interactable => _selectable == null || _selectable.IsInteractable();

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Interactable) UISfx.Play(hoverSfx);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && Interactable) UISfx.Play(clickSfx);
    }

    public void Configure(string hover, string click)
    {
        hoverSfx = hover;
        clickSfx = click;
    }
}
