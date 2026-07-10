using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonfireInteractionHandler : FacilityInteractionHandler
{
    [Serializable]
    public class CharacterPlacement
    {
        [SerializeField] private CoopCharData character;
        [SerializeField] private RectTransform placementPoint;
        [SerializeField] private bool mirror;

        private Image characterVisual;
        private Button characterButton;
        private GameObject relationshipEventIndicator;
        private Action<CoopCharData> onClicked;

        public CoopCharData Character => character;

        public void Apply(CooperationManager cooperationManager, Action<CoopCharData> clickHandler)
        {
            if (placementPoint == null)
                return;

            onClicked = clickHandler;
            EnsureCharacterVisual();
            EnsureCharacterButton();
            EnsureRelationshipEventIndicator();

            Sprite characterSprite = character != null ? character.charImage : null;
            bool hasMetCharacter = character != null &&
                                   cooperationManager != null &&
                                   cooperationManager.GetCoopLevel(character.charID) > 0;

            characterVisual.sprite = characterSprite;
            characterVisual.gameObject.SetActive(characterSprite != null && hasMetCharacter);

            Vector3 scale = characterVisual.rectTransform.localScale;
            scale.x = Mathf.Abs(scale.x) * (mirror ? -1f : 1f);
            characterVisual.rectTransform.localScale = scale;

            bool hasRelationshipEvent = hasMetCharacter &&
                                        cooperationManager.IsCoopLevelUP(character.charID);

            relationshipEventIndicator.SetActive(hasRelationshipEvent);
            characterButton.interactable = characterSprite != null && hasMetCharacter && hasRelationshipEvent;
        }

        public void Release()
        {
            if (characterButton != null)
                characterButton.onClick.RemoveListener(HandleClicked);

            onClicked = null;
        }

        private void EnsureCharacterVisual()
        {
            if (characterVisual != null)
                return;

            Transform existingVisual = placementPoint.Find("CharacterVisual");
            if (existingVisual != null)
                characterVisual = existingVisual.GetComponent<Image>();

            if (characterVisual == null)
            {
                GameObject visualObject = new("CharacterVisual", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                visualObject.transform.SetParent(placementPoint, false);
                characterVisual = visualObject.GetComponent<Image>();
            }

            RectTransform visualRect = characterVisual.rectTransform;
            visualRect.anchorMin = Vector2.zero;
            visualRect.anchorMax = Vector2.one;
            visualRect.offsetMin = Vector2.zero;
            visualRect.offsetMax = Vector2.zero;
            characterVisual.preserveAspect = true;
        }

        private void EnsureCharacterButton()
        {
            if (characterButton == null)
                characterButton = characterVisual.GetComponent<Button>();

            if (characterButton == null)
                characterButton = characterVisual.gameObject.AddComponent<Button>();

            characterVisual.raycastTarget = true;
            characterButton.transition = Selectable.Transition.None;
            characterButton.onClick.RemoveListener(HandleClicked);
            characterButton.onClick.AddListener(HandleClicked);
        }

        private void HandleClicked()
        {
            if (character == null)
                return;

            onClicked?.Invoke(character);
        }

        private void EnsureRelationshipEventIndicator()
        {
            if (relationshipEventIndicator != null)
                return;

            Transform existingIndicator = placementPoint.Find("RelationshipEventIndicator");
            if (existingIndicator != null)
            {
                relationshipEventIndicator = existingIndicator.gameObject;
                return;
            }

            relationshipEventIndicator = CreateIndicatorPart(
                "RelationshipEventIndicator",
                placementPoint,
                new Color(1f, 1f, 1f, 0.96f),
                new Vector2(54f, 42f));

            RectTransform indicatorRect = relationshipEventIndicator.GetComponent<RectTransform>();
            indicatorRect.anchorMin = new Vector2(0.5f, 1f);
            indicatorRect.anchorMax = new Vector2(0.5f, 1f);
            indicatorRect.pivot = new Vector2(0.5f, 0f);
            indicatorRect.anchoredPosition = new Vector2(0f, 12f);

            GameObject tail = CreateIndicatorPart(
                "Tail",
                indicatorRect,
                new Color(1f, 1f, 1f, 0.96f),
                new Vector2(14f, 14f));
            RectTransform tailRect = tail.GetComponent<RectTransform>();
            tailRect.anchorMin = tailRect.anchorMax = new Vector2(0.5f, 0f);
            tailRect.anchoredPosition = new Vector2(0f, -5f);
            tailRect.localRotation = Quaternion.Euler(0f, 0f, 45f);

            GameObject marker = CreateIndicatorPart(
                "Marker",
                indicatorRect,
                new Color(1f, 0.46f, 0.08f, 1f),
                new Vector2(14f, 22f));
            RectTransform markerRect = marker.GetComponent<RectTransform>();
            markerRect.anchorMin = markerRect.anchorMax = new Vector2(0.5f, 0.5f);
            markerRect.anchoredPosition = Vector2.zero;
        }

        private static GameObject CreateIndicatorPart(
            string name,
            Transform parent,
            Color color,
            Vector2 size)
        {
            GameObject part = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            part.transform.SetParent(parent, false);

            RectTransform rect = part.GetComponent<RectTransform>();
            rect.sizeDelta = size;

            Image image = part.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return part;
        }
    }

    [SerializeField] private List<CharacterPlacement> characterPlacements = new();
    [SerializeField] private DialogueView dialogueView;
    [SerializeField] private TextAsset defaultRelationshipDialogue;

    private bool isPlayingRelationshipEvent;

    protected override void OnDisable()
    {
        base.OnDisable();

        foreach (CharacterPlacement placement in characterPlacements)
            placement?.Release();

        isPlayingRelationshipEvent = false;
    }

    protected override void OnOpenInteraction(FacilityState facilityState)
    {
        RefreshCharacterPlacements();
    }

    public void RefreshCharacterPlacements()
    {
        CooperationManager cooperationManager = CooperationManager.Instance;
        foreach (CharacterPlacement placement in characterPlacements)
            placement?.Apply(cooperationManager, HandleCharacterClicked);
    }

    private void HandleCharacterClicked(CoopCharData character)
    {
        if (isPlayingRelationshipEvent || character == null)
            return;

        CooperationManager cooperationManager = CooperationManager.Instance;
        if (cooperationManager == null)
        {
            Debug.LogWarning("[Bonfire] CooperationManager.Instance가 없습니다.");
            return;
        }

        if (!cooperationManager.IsCoopLevelUP(character.charID))
            return;

        DialogueView view = GetDialogueView();
        if (view == null)
        {
            Debug.LogWarning("[Bonfire] DialogueView가 없습니다.");
            return;
        }

        TextAsset dialogueJson = GetRelationshipDialogue(cooperationManager, character);
        if (dialogueJson == null)
            return;

        isPlayingRelationshipEvent = true;
        view.Play(dialogueJson, () => HandleRelationshipDialogueEnded(character.charID));
    }

    private void HandleRelationshipDialogueEnded(string charID)
    {
        CooperationManager cooperationManager = CooperationManager.Instance;
        if (cooperationManager != null)
            cooperationManager.SettlePoint(charID);

        isPlayingRelationshipEvent = false;
        RefreshCharacterPlacements();
    }

    private TextAsset GetRelationshipDialogue(CooperationManager cooperationManager, CoopCharData character)
    {
        int currentCoopLevel = cooperationManager.GetCoopLevel(character.charID);
        int eventCoopLevel = currentCoopLevel + 1;
        TextAsset dialogueJson = cooperationManager.GetCoopDialogue(character.charID, eventCoopLevel);

        if (dialogueJson != null)
            return dialogueJson;

        dialogueJson = cooperationManager.GetCoopDialogue(character.charID, currentCoopLevel);
        if (dialogueJson != null)
            return dialogueJson;

        if (defaultRelationshipDialogue != null)
            return defaultRelationshipDialogue;

        Debug.LogWarning($"[Bonfire] {character.charID} 레벨 {eventCoopLevel}에 등록된 대사가 없고 defaultRelationshipDialogue도 비어 있습니다.");
        return null;
    }

    private DialogueView GetDialogueView()
    {
        if (dialogueView != null)
            return dialogueView;

        dialogueView = GetComponentInParent<DialogueView>();
        if (dialogueView == null)
            dialogueView = FindFirstObjectByType<DialogueView>();

        return dialogueView;
    }
}
