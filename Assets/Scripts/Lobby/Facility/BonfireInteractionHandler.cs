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
        private GameObject relationshipEventIndicator;

        public CoopCharData Character => character;

        public void Apply(CooperationManager cooperationManager)
        {
            if (placementPoint == null)
                return;

            EnsureCharacterVisual();
            EnsureRelationshipEventIndicator();

            Sprite characterSprite = character != null ? character.charImage : null;
            characterVisual.sprite = characterSprite;
            characterVisual.gameObject.SetActive(characterSprite != null);

            Vector3 scale = characterVisual.rectTransform.localScale;
            scale.x = Mathf.Abs(scale.x) * (mirror ? -1f : 1f);
            characterVisual.rectTransform.localScale = scale;

            bool hasRelationshipEvent = character != null &&
                                        cooperationManager != null &&
                                        cooperationManager.IsCoopLevelUP(character.charID);
            relationshipEventIndicator.SetActive(hasRelationshipEvent);
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
            characterVisual.raycastTarget = false;
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

    protected override void OnOpenInteraction(FacilityState facilityState)
    {
        RefreshCharacterPlacements();
    }

    public void RefreshCharacterPlacements()
    {
        CooperationManager cooperationManager = CooperationManager.Instance;
        foreach (CharacterPlacement placement in characterPlacements)
            placement?.Apply(cooperationManager);
    }
}
