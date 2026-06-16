using UnityEngine;

public abstract class Facility : MonoBehaviour
{
    [SerializeField] private string facilityId;
    [SerializeField] private bool isActive = true;

    public string FacilityId => string.IsNullOrWhiteSpace(facilityId) ? gameObject.name : facilityId;
    public bool IsActive => isActive;
    public bool IsInteracting { get; private set; }

    protected virtual void OnEnable()
    {
        LobbyManager.Instance?.RegisterFacility(this);
    }

    protected virtual void OnDisable()
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
        OnInteract();
        return true;
    }

    public void EndInteraction()
    {
        if (!IsInteracting)
            return;

        IsInteracting = false;
        LobbyManager.Instance?.EndInteraction(this);
        OnInteractionEnded();
    }

    protected abstract void OnInteract();

    protected virtual void OnInteractionEnded()
    {
    }
}
