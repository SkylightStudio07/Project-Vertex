//각 시설 InteractionHandler의 공통 기능을 제공하는 추상 클래스

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class FacilityInteractionHandler : MonoBehaviour
{
    [Header("공통 시설 UI")]
    [SerializeField] private FacilityType facilityType;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject facilityRoot;

    [Header("시설 상태별 표시 대상")]
    [SerializeField] private List<GameObject> activeWhenLocked = new();
    [SerializeField] private List<GameObject> activeWhenUnlocked = new();

    private LobbyScreenConverter screenConverter;

    public FacilityType FacilityType => facilityType;
    public FacilityManager FacilityManager { get; private set; }
    protected GameObject FacilityRoot => facilityRoot != null ? facilityRoot : gameObject;

    protected virtual void OnEnable()
    {
        upgradeButton?.onClick.AddListener(HandleUpgradeButtonClicked);
        closeButton?.onClick.AddListener(CloseInteraction);
    }

    protected virtual void OnDisable()
    {
        upgradeButton?.onClick.RemoveListener(HandleUpgradeButtonClicked);
        closeButton?.onClick.RemoveListener(CloseInteraction);
    }

    //시설 매니저에 이벤트 구독
    public void Bind(FacilityManager facilityManager)
    {
        if (FacilityManager != null)
        {
            FacilityManager.OnFacilityStateChanged -= HandleFacilityStateChanged;
        }

        FacilityManager = facilityManager;

        if (FacilityManager != null)
        {
            FacilityManager.OnFacilityStateChanged += HandleFacilityStateChanged;
        }
    }

    public void OpenInteraction(FacilityState facilityState)
    {
        RefreshFacilityUI(facilityState);
        ShowFacilityView();
        OnOpenInteraction(facilityState);
    }

    public void CloseInteraction()
    {
        GetFacilityManager()?.EndInteraction(FacilityType);
    }

    public void CloseView()
    {
        HideFacilityView();
        OnCloseInteraction();
    }

    public void RefreshFacilityUI()
    {
        FacilityManager facilityManager = GetFacilityManager();
        if (facilityManager == null)
            return;

        RefreshFacilityUI(facilityManager.GetFacilityState(FacilityType));
    }

    public void RefreshFacilityUI(FacilityState facilityState)
    {
        if (facilityState.FacilityType != FacilityType)
            return;

        SetObjectsActive(activeWhenLocked, !facilityState.IsActive);
        SetObjectsActive(activeWhenUnlocked, facilityState.IsActive);

        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(!facilityState.IsActive || facilityState.CanUpgrade);
    }

    private void HandleUpgradeButtonClicked()
    {
        RequestFacilityProgression();
    }

    public void RequestFacilityProgression()
    {
        FacilityManager facilityManager = GetFacilityManager();
        if (facilityManager == null)
            return;

        facilityManager.RequestFacilityProgression(FacilityType);
    }

    private void HandleFacilityStateChanged(FacilityState facilityState)
    {
        RefreshFacilityUI(facilityState);
    }

    private void ShowFacilityView()
    {
        LobbyScreenConverter converter = GetScreenConverter();
        if (converter != null)
            converter.ShowFacilityView(FacilityRoot);
        else
            FacilityRoot.SetActive(true);
    }

    private void HideFacilityView()
    {
        LobbyScreenConverter converter = GetScreenConverter();
        if (converter != null)
            converter.ShowMainView();
        else
            FacilityRoot.SetActive(false);
    }

    private LobbyScreenConverter GetScreenConverter()
    {
        if (screenConverter != null)
            return screenConverter;

        screenConverter = GetComponentInParent<LobbyScreenConverter>();
        if (screenConverter == null)
            screenConverter = FindFirstObjectByType<LobbyScreenConverter>();

        return screenConverter;
    }

    private FacilityManager GetFacilityManager()
    {
        if (FacilityManager != null)
            return FacilityManager;

        return LobbyManager.Instance != null ? LobbyManager.Instance.FacilityManager : null;
    }

    private static void SetObjectsActive(List<GameObject> targets, bool active)
    {
        foreach (GameObject target in targets)
            target?.SetActive(active);
    }

    protected virtual void OnOpenInteraction(FacilityState facilityState)
    {
    }

    protected virtual void OnCloseInteraction()
    {
    }
}
