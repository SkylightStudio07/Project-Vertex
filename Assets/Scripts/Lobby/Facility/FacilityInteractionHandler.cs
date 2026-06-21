using UnityEngine;

public abstract class FacilityInteractionHandler : MonoBehaviour
{
    public FacilityController Controller { get; private set; }

    public void Bind(FacilityController controller)
    {
        Controller = controller;
    }

    public void OpenInteraction()
    {
        OnOpenInteraction();
    }

    public void CloseInteraction()
    {
        if (Controller != null)
        {
            Controller.EndInteraction();
            return;
        }

        OnCloseInteraction();
    }

    public void CloseInteractionFromController()
    {
        OnCloseInteraction();
    }

    protected abstract void OnOpenInteraction();

    protected virtual void OnCloseInteraction()
    {
    }
}
