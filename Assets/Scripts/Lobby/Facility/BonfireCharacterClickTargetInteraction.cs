using System;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public sealed class BonfireCharacterClickTargetInteraction : MonoBehaviour, IPointerClickHandler
{
    private static Sprite whiteSprite;

    [SerializeField] private bool interactable;
    [SerializeField] private bool logClicks;

    private Action onClicked;
    private int lastClickFrame = -1;

    public static Sprite WhiteSprite
    {
        get
        {
            if (whiteSprite != null)
                return whiteSprite;

            Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
            {
                name = "Bonfire White Sprite Texture",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            whiteSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
            whiteSprite.name = "Bonfire White Sprite";
            whiteSprite.hideFlags = HideFlags.HideAndDontSave;
            return whiteSprite;
        }
    }

    public static void EnsureCameraRaycaster()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || mainCamera.GetComponent<PhysicsRaycaster>() != null)
            return;

        mainCamera.gameObject.AddComponent<PhysicsRaycaster>();
    }

    public void Initialize(Action clickHandler)
    {
        onClicked = clickHandler;
        EnsureCameraRaycaster();
    }

    public void SetInteractable(bool value)
    {
        interactable = value;
    }

    public void Clear()
    {
        onClicked = null;
        interactable = false;
    }

    public void CharacterClickTargetInteraction()
    {
        InvokeClick();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        CharacterClickTargetInteraction();
    }

    private void OnMouseDown()
    {
        CharacterClickTargetInteraction();
    }

    private void InvokeClick()
    {
        if (!interactable || Time.frameCount == lastClickFrame)
            return;

        lastClickFrame = Time.frameCount;
        if (logClicks)
            Debug.Log($"[Bonfire] Character click target interaction: {name}");

        onClicked?.Invoke();
    }
}
