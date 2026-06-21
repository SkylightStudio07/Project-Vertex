using UnityEngine;

public class FacilityController : MonoBehaviour
{
    [SerializeField] private Facility facility;
    [SerializeField] private FacilityInteractionHandler interactionHandler;

    private bool isActive;

    public Facility Facility => facility;
    public string FacilityId => facility != null ? facility.FacilityId : gameObject.name;
    public string DisplayName => facility != null ? facility.DisplayName : gameObject.name;
    public bool DefaultActive => facility == null || facility.DefaultActive;
    public int RequiredExp => facility != null ? facility.RequiredExp : 0;
    public string Description => facility != null ? facility.Description : string.Empty;
    public bool IsActive => isActive;
    public bool IsInteracting { get; private set; }

    private void Awake()
    {
        isActive = DefaultActive;

        if (interactionHandler == null)
            interactionHandler = GetComponent<FacilityInteractionHandler>();
        interactionHandler?.Bind(this);
    }

    private void OnEnable()
    {
        LobbyManager.Instance?.RegisterFacility(this);
    }

    private void OnDisable()
    {
        if (IsInteracting)
            EndInteraction();

        LobbyManager.Instance?.UnregisterFacility(this);
    }

    public void SetFacilityActive(bool active)
    {
        isActive = active;

        if (!isActive && IsInteracting)
            EndInteraction();
    }

    public void Interact()
    {
        TryInteract();
    }

    public bool TryInteract()
    {
        if (!isActive)
            return false;

        if (IsInteracting)
            return true;

        if (LobbyManager.Instance != null && !LobbyManager.Instance.TryBeginInteraction(this))
            return false;

        IsInteracting = true;
        interactionHandler?.OpenInteraction();
        return true;
    }

    public void EndInteraction()
    {
        if (!IsInteracting)
            return;

        IsInteracting = false;
        LobbyManager.Instance?.EndInteraction(this);
        interactionHandler?.CloseInteractionFromController();
    }

    // Debug buttons for testing facility interaction in the Inspector.
    [ContextMenu("Debug/Interact")]
    private void DebugInteract()
    {
        bool result = TryInteract();
        Logger.Log(this, $"Debug interact result: {result}");
    }

    [ContextMenu("Debug/End Interaction")]
    private void DebugEndInteraction()
    {
        EndInteraction();
        Logger.Log(this, "Debug interaction ended.");
    }

    [ContextMenu("Debug/Toggle Active")]
    private void DebugToggleActive()
    {
        SetFacilityActive(!IsActive);
        Logger.Log(this, $"Debug active changed to {IsActive}.");
    }
}
