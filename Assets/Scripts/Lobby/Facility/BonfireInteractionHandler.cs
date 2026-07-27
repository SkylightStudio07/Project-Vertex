using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonfireInteractionHandler : FacilityInteractionHandler
{
    [Serializable]
    public class CharacterPlacement
    {
        [SerializeField] private CoopCharData character;
        [SerializeField] private BonfireCharacterClickTargetInteraction clickTarget;
        [SerializeField] private bool mirror;

        private Transform characterPivot;
        private SpriteRenderer characterVisual;
        private GameObject relationshipEventIndicator;
        private Coroutine entranceRoutine;
        private Action<CoopCharData> onClicked;
        private Sprite lastSprite;
        private bool wasVisible;
        private bool hasEntered;
        private bool hasRelationshipEvent;

        public CoopCharData Character => character;

        public void Apply(
            BonfireInteractionHandler owner,
            CooperationManager cooperationManager,
            Action<CoopCharData> clickHandler,
            bool playEntrance,
            float entranceDelay)
        {
            if (owner == null || clickTarget == null)
                return;

            onClicked = clickHandler;
            CacheOwnerSettings(owner);
            EnsureCharacterVisual();
            EnsureClickTarget();
            EnsureRelationshipEventIndicator();

            Sprite characterSprite = character != null ? character.charImage : null;
            bool hasMetCharacter = character != null &&
                                   cooperationManager != null &&
                                   cooperationManager.GetCoopLevel(character.charID) > 0;
            bool shouldShowCharacter = characterSprite != null && hasMetCharacter;

            characterVisual.sprite = characterSprite;
            ApplyCharacterTransform(characterSprite);
            UpdateClickBounds(characterSprite);

            hasRelationshipEvent = hasMetCharacter &&
                                   cooperationManager.IsCoopLevelUP(character.charID);

            if (clickTarget != null)
                clickTarget.SetInteractable(shouldShowCharacter && hasRelationshipEvent);

            if (!shouldShowCharacter)
            {
                SetPresentationVisible(false);
                hasEntered = false;
                wasVisible = false;
                lastSprite = characterSprite;
                return;
            }

            bool shouldAnimate = playEntrance &&
                                 shouldShowCharacter &&
                                 owner.isActiveAndEnabled;

            if (shouldAnimate)
                PlayEntrance(owner, entranceDelay);
            else if (hasEntered)
                SnapToFinalPose();
            else
                SetPresentationVisible(false);

            wasVisible = shouldShowCharacter;
            lastSprite = characterSprite;
        }

        public void Release(MonoBehaviour coroutineRunner)
        {
            if (coroutineRunner != null && entranceRoutine != null)
                coroutineRunner.StopCoroutine(entranceRoutine);

            entranceRoutine = null;

            if (clickTarget != null)
                clickTarget.Clear();

            onClicked = null;
            wasVisible = false;
            hasEntered = false;
            hasRelationshipEvent = false;
        }

        private void EnsureCharacterVisual()
        {
            if (characterPivot != null && characterVisual != null)
                return;

            Transform root = clickTarget.transform;
            Transform existingVisual = root.Find("CharacterVisual");
            if (existingVisual != null)
            {
                characterPivot = existingVisual;

                Image legacyImage = existingVisual.GetComponent<Image>();
                if (legacyImage != null)
                    legacyImage.enabled = false;

                Button legacyButton = existingVisual.GetComponent<Button>();
                if (legacyButton != null)
                    legacyButton.enabled = false;

                SpriteRenderer legacyRenderer = existingVisual.GetComponent<SpriteRenderer>();
                if (legacyRenderer != null)
                    legacyRenderer.enabled = false;
            }

            if (characterPivot == null)
            {
                GameObject pivotObject = new("CharacterVisual");
                pivotObject.transform.SetParent(root, false);
                characterPivot = pivotObject.transform;
            }

            Transform spriteChild = characterPivot.Find("CharacterSprite");
            if (spriteChild == null)
            {
                GameObject spriteObject = new("CharacterSprite");
                spriteObject.transform.SetParent(characterPivot, false);
                spriteChild = spriteObject.transform;
            }

            characterVisual = spriteChild.GetComponent<SpriteRenderer>();
            if (characterVisual == null)
                characterVisual = spriteChild.gameObject.AddComponent<SpriteRenderer>();

            characterVisual.sortingOrder = 20;
            characterVisual.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            characterVisual.receiveShadows = false;
        }

        private void EnsureClickTarget()
        {
            if (clickTarget == null)
            {
                Debug.LogWarning($"[Bonfire] Character placement for {character?.charID ?? "(empty)"} needs a BonfireCharacterClickTargetInteraction reference.");
                return;
            }

            clickTarget.Initialize(HandleClicked);
        }

        private void ApplyCharacterTransform(Sprite sprite)
        {
            characterPivot.SetParent(clickTarget.transform, false);
            characterPivot.localPosition = ownerVisualLocalOffset;
            characterPivot.localRotation = Quaternion.identity;
            characterPivot.localScale = CalculateTargetScale(sprite);

            Transform spriteTransform = characterVisual.transform;
            spriteTransform.localPosition = CalculateBottomPivotOffset(sprite);
            spriteTransform.localRotation = Quaternion.identity;
            spriteTransform.localScale = new Vector3(mirror ? -1f : 1f, 1f, 1f);
        }

        private float ownerCharacterHeight;
        private Vector3 ownerVisualLocalOffset;
        private Vector3 ownerIndicatorLocalOffset;
        private float ownerEntranceStartAngle;
        private float ownerEntranceOvershootAngle;
        private float ownerEntranceDuration;

        private void CacheOwnerSettings(BonfireInteractionHandler owner)
        {
            ownerCharacterHeight = owner.characterHeight;
            ownerVisualLocalOffset = owner.visualLocalOffset;
            ownerIndicatorLocalOffset = owner.indicatorLocalOffset;
            ownerEntranceStartAngle = owner.entranceStartAngle;
            ownerEntranceOvershootAngle = owner.entranceOvershootAngle;
            ownerEntranceDuration = owner.entranceDuration;
        }

        private Vector3 CalculateTargetScale(Sprite sprite)
        {
            float height = sprite != null ? sprite.bounds.size.y : 1f;
            float normalizedScale = height > Mathf.Epsilon ? ownerCharacterHeight / height : ownerCharacterHeight;
            return new Vector3(normalizedScale, normalizedScale, normalizedScale);
        }

        private static Vector3 CalculateBottomPivotOffset(Sprite sprite)
        {
            if (sprite == null)
                return Vector3.zero;

            Bounds bounds = sprite.bounds;
            return new Vector3(-bounds.center.x, -bounds.min.y, 0f);
        }

        private void UpdateClickBounds(Sprite sprite)
        {
            if (clickTarget == null)
                return;

            BoxCollider boxCollider = clickTarget.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                Debug.LogWarning($"[Bonfire] {clickTarget.name} needs a BoxCollider for character clicking.");
                return;
            }

            if (sprite == null)
            {
                boxCollider.enabled = false;
                return;
            }

            Bounds bounds = sprite.bounds;
            Vector3 targetScale = CalculateTargetScale(sprite);
            Vector3 spriteOffset = CalculateBottomPivotOffset(sprite);
            boxCollider.enabled = true;
            boxCollider.center = ownerVisualLocalOffset + Vector3.Scale(spriteOffset + bounds.center, targetScale);
            boxCollider.size = new Vector3(
                Mathf.Max(bounds.size.x * targetScale.x, 0.05f),
                Mathf.Max(bounds.size.y * targetScale.y, 0.05f),
                0.18f);
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
            {
                relationshipEventIndicator.transform.localPosition = ownerIndicatorLocalOffset;
                return;
            }

            Transform root = clickTarget.transform;
            Transform existingIndicator = root.Find("RelationshipEventIndicator");
            relationshipEventIndicator = existingIndicator != null
                ? existingIndicator.gameObject
                : new GameObject("RelationshipEventIndicator");

            relationshipEventIndicator.transform.SetParent(root, false);
            relationshipEventIndicator.transform.localPosition = ownerIndicatorLocalOffset;
            relationshipEventIndicator.transform.localRotation = Quaternion.identity;
            relationshipEventIndicator.transform.localScale = Vector3.one;

            EnsureIndicatorPart(
                "Bubble",
                relationshipEventIndicator.transform,
                new Color(1f, 1f, 1f, 0.96f),
                new Vector3(0.46f, 0.34f, 1f),
                Vector3.zero,
                Quaternion.identity,
                40);

            EnsureIndicatorPart(
                "Tail",
                relationshipEventIndicator.transform,
                new Color(1f, 1f, 1f, 0.96f),
                new Vector3(0.12f, 0.12f, 1f),
                new Vector3(0f, -0.2f, 0.01f),
                Quaternion.Euler(0f, 0f, 45f),
                39);

            EnsureIndicatorPart(
                "Marker",
                relationshipEventIndicator.transform,
                new Color(1f, 0.46f, 0.08f, 1f),
                new Vector3(0.1f, 0.22f, 1f),
                Vector3.zero,
                Quaternion.identity,
                41);
        }

        private static void EnsureIndicatorPart(
            string name,
            Transform parent,
            Color color,
            Vector3 scale,
            Vector3 localPosition,
            Quaternion localRotation,
            int sortingOrder)
        {
            Transform existingPart = parent.Find(name);
            GameObject part = existingPart != null
                ? existingPart.gameObject
                : new GameObject(name);

            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = scale;

            SpriteRenderer renderer = part.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = part.AddComponent<SpriteRenderer>();

            renderer.sprite = BonfireCharacterClickTargetInteraction.WhiteSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void PlayEntrance(MonoBehaviour coroutineRunner, float entranceDelay)
        {
            if (entranceRoutine != null)
                coroutineRunner.StopCoroutine(entranceRoutine);

            entranceRoutine = coroutineRunner.StartCoroutine(PlayEntranceRoutine(entranceDelay));
        }

        private IEnumerator PlayEntranceRoutine(float entranceDelay)
        {
            Vector3 finalPosition = ownerVisualLocalOffset;
            Quaternion finalRotation = Quaternion.identity;
            Vector3 finalScale = CalculateTargetScale(characterVisual.sprite);
            Quaternion startRotation = Quaternion.Euler(ownerEntranceStartAngle, 0f, mirror ? 3f : -3f);
            Quaternion overshootRotation = Quaternion.Euler(-ownerEntranceOvershootAngle, 0f, mirror ? -1f : 1f);
            characterPivot.localPosition = finalPosition;
            characterPivot.localRotation = startRotation;
            characterPivot.localScale = finalScale;
            SetVisualAlpha(0f);
            SetPresentationVisible(false);

            if (entranceDelay > 0f)
                yield return new WaitForSecondsRealtime(entranceDelay);

            SetPresentationVisible(true);
            float duration = Mathf.Max(1f, ownerEntranceDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                float liftT = Mathf.Clamp01(t / 0.78f);
                float liftEase = EaseOutBack(liftT);
                float alphaT = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 1.8f));

                Quaternion currentRotation = Quaternion.SlerpUnclamped(startRotation, overshootRotation, liftEase);
                if (t > 0.78f)
                    currentRotation = Quaternion.SlerpUnclamped(
                        overshootRotation,
                        finalRotation,
                        Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.78f, 1f, t)));

                characterPivot.localPosition = finalPosition;
                characterPivot.localRotation = currentRotation;
                characterPivot.localScale = finalScale;
                SetVisualAlpha(alphaT);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            SnapToFinalPose();
            hasEntered = true;
            entranceRoutine = null;
        }

        private void SnapToFinalPose()
        {
            if (characterPivot == null || characterVisual == null)
                return;

            characterPivot.localPosition = ownerVisualLocalOffset;
            characterPivot.localRotation = Quaternion.identity;
            characterPivot.localScale = CalculateTargetScale(characterVisual.sprite);

            Transform spriteTransform = characterVisual.transform;
            spriteTransform.localPosition = CalculateBottomPivotOffset(characterVisual.sprite);
            spriteTransform.localRotation = Quaternion.identity;
            spriteTransform.localScale = new Vector3(mirror ? -1f : 1f, 1f, 1f);
            SetVisualAlpha(1f);
            SetPresentationVisible(true);
        }

        private void SetVisualAlpha(float alpha)
        {
            if (characterVisual == null)
                return;

            Color color = characterVisual.color;
            color.a = alpha;
            characterVisual.color = color;
        }

        private void SetPresentationVisible(bool visible)
        {
            if (characterPivot != null)
                characterPivot.gameObject.SetActive(visible);

            if (relationshipEventIndicator != null)
                relationshipEventIndicator.SetActive(visible && hasRelationshipEvent);
        }

        private static float EaseOutBack(float value)
        {
            value = Mathf.Clamp01(value);
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(value - 1f, 3f) + c1 * Mathf.Pow(value - 1f, 2f);
        }
    }

    [SerializeField] private List<CharacterPlacement> characterPlacements = new();
    [Header("Character Presentation")]
    [SerializeField, Min(0.1f)] private float characterHeight = 2.6f;
    [SerializeField] private Vector3 visualLocalOffset;
    [SerializeField] private Vector3 indicatorLocalOffset = new(0f, 2.85f, -0.02f);
    [SerializeField, Range(5f, 89f)] private float entranceStartAngle = 76f;
    [SerializeField, Range(0f, 20f)] private float entranceOvershootAngle = 7f;
    [SerializeField, Min(1f)] private float entranceDuration = 1f;
    [SerializeField, Min(0f)] private float entranceStaggerDelay = 0.05f;
    [SerializeField] private DialogueView dialogueView;
    [SerializeField] private TextAsset defaultRelationshipDialogue;

    private bool isPlayingRelationshipEvent;
    private LobbyUIManager lobbyUIManager;

    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribeLobbyUIManager();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnsubscribeLobbyUIManager();

        foreach (CharacterPlacement placement in characterPlacements)
            placement?.Release(this);

        isPlayingRelationshipEvent = false;
    }

    protected override void OnOpenInteraction(FacilityState facilityState)
    {
        RefreshCharacterPlacements();
    }

    public void RefreshCharacterPlacements(bool playEntrance = false)
    {
        CooperationManager cooperationManager = CooperationManager.Instance;
        for (int i = 0; i < characterPlacements.Count; i++)
        {
            float entranceDelay = playEntrance ? i * entranceStaggerDelay : 0f;
            characterPlacements[i]?.Apply(this, cooperationManager, HandleCharacterClicked, playEntrance, entranceDelay);
        }
    }

    private void HandleCharacterClicked(CoopCharData character)
    {
        if (isPlayingRelationshipEvent || character == null)
            return;

        CooperationManager cooperationManager = CooperationManager.Instance;
        if (cooperationManager == null)
        {
            Debug.LogWarning("[Bonfire] CooperationManager.Instance is missing.");
            return;
        }

        if (!cooperationManager.IsCoopLevelUP(character.charID))
            return;

        DialogueView view = GetDialogueView();
        if (view == null)
        {
            Debug.LogWarning("[Bonfire] DialogueView is missing.");
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

        Debug.LogWarning($"[Bonfire] Dialogue for {character.charID} level {eventCoopLevel} is missing and defaultRelationshipDialogue is empty.");
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

    private void SubscribeLobbyUIManager()
    {
        if (lobbyUIManager != null)
            return;

        lobbyUIManager = GetComponentInParent<LobbyUIManager>();
        if (lobbyUIManager == null)
            lobbyUIManager = FindFirstObjectByType<LobbyUIManager>();

        if (lobbyUIManager != null)
            lobbyUIManager.FacilityViewShown += HandleFacilityViewShown;
    }

    private void UnsubscribeLobbyUIManager()
    {
        if (lobbyUIManager == null)
            return;

        lobbyUIManager.FacilityViewShown -= HandleFacilityViewShown;
        lobbyUIManager = null;
    }

    private void HandleFacilityViewShown(GameObject facilityView, FacilityState facilityState)
    {
        if (facilityState.FacilityType != FacilityType)
            return;

        if (facilityView != FacilityRoot)
            return;

        RefreshCharacterPlacements(true);
    }
}
