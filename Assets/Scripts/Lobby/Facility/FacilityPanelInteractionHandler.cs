using UnityEngine;

/// <summary>별도 로직 없이 공통 UI 패널만 여는 시설에서 공유하는 Handler.</summary>
public class FacilityPanelInteractionHandler : FacilityInteractionHandler
{
    [SerializeField] private LobbyScreenRouter screenRouter;
    [SerializeField] private GameObject interactionView;

    protected override void OnOpenInteraction()
    {
        if (screenRouter != null)
            screenRouter.ShowFacilityView(interactionView);
        else
            interactionView?.SetActive(true);
    }

    protected override void OnCloseInteraction()
    {
        interactionView?.SetActive(false);
        screenRouter?.ShowMainView();
    }
}
