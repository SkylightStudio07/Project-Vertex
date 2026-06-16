using UnityEngine;

public class BonfireFacility : Facility
{
    [SerializeField] private GameObject interactionView;

    protected override void OnInteract()
    {
        if (interactionView != null)
        {
            interactionView.SetActive(true);
            return;
        }
    }

    public void CloseInteraction()
    {
        if (interactionView != null)
            interactionView.SetActive(false);

        EndInteraction();
    }
}
