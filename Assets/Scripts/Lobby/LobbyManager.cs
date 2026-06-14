using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LobbyFacility
{
    Tent,
    Armory,
    Informant,
    TrainingGround,
    RequestBoard,
    Cooperator
}

public enum LobbyAction
{
    StartRun,
    ViewStartingDeck,
    OpenSettings,
    SelectStartingWeapon,
    ViewCardUnlocks,
    ViewEnemyCodex,
    ViewEventRecords,
    OpenTraining,
    ViewRequests,
    ViewMetaProgression,
    ViewCooperatorEvent
}

[Serializable]
public class LobbyFeatureUnlock
{
    public LobbyAction action;
    public string displayName;
    public int requiredExperience;
    public bool implemented = true;
    [TextArea] public string description;
}

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [Header("Scene")]
    [SerializeField] private string runSceneName = "SampleScene";

    [Header("Progression")]
    [SerializeField] private MetaProgressionManager progressionManager;
    [SerializeField] private List<LobbyFeatureUnlock> featureUnlocks = new List<LobbyFeatureUnlock>
    {
        new LobbyFeatureUnlock { action = LobbyAction.StartRun, displayName = "Start Run", requiredExperience = 0, implemented = true },
        new LobbyFeatureUnlock { action = LobbyAction.ViewStartingDeck, displayName = "Starting Deck", requiredExperience = 0, implemented = true },
        new LobbyFeatureUnlock { action = LobbyAction.OpenSettings, displayName = "Settings", requiredExperience = 0, implemented = true },
        new LobbyFeatureUnlock { action = LobbyAction.SelectStartingWeapon, displayName = "Starting Weapon", requiredExperience = 0, implemented = false },
        new LobbyFeatureUnlock { action = LobbyAction.ViewCardUnlocks, displayName = "Card Unlocks", requiredExperience = 0, implemented = false },
        new LobbyFeatureUnlock { action = LobbyAction.ViewEnemyCodex, displayName = "Enemy Codex", requiredExperience = 0, implemented = true },
        new LobbyFeatureUnlock { action = LobbyAction.ViewEventRecords, displayName = "Event Records", requiredExperience = 0, implemented = true },
        new LobbyFeatureUnlock { action = LobbyAction.OpenTraining, displayName = "Training Ground", requiredExperience = 100, implemented = false },
        new LobbyFeatureUnlock { action = LobbyAction.ViewRequests, displayName = "Requests", requiredExperience = 0, implemented = true },
        new LobbyFeatureUnlock { action = LobbyAction.ViewMetaProgression, displayName = "Meta Progression", requiredExperience = 0, implemented = true },
        new LobbyFeatureUnlock { action = LobbyAction.ViewCooperatorEvent, displayName = "Cooperator Event", requiredExperience = 0, implemented = false }
    };

    public LobbyFacility CurrentFacility { get; private set; } = LobbyFacility.Tent;
    public LobbyAction CurrentAction { get; private set; } = LobbyAction.ViewMetaProgression;
    public int CurrentExperience => Progression.Experience;
    public IReadOnlyList<LobbyFeatureUnlock> FeatureUnlocks => featureUnlocks;

    public event Action<int> OnExperienceChanged;
    public event Action<LobbyFacility> OnFacilityChanged;
    public event Action<LobbyAction> OnActionOpened;
    public event Action<LobbyAction, string> OnActionUnavailable;

    private MetaProgressionManager Progression
    {
        get
        {
            if (progressionManager == null)
                progressionManager = MetaProgressionManager.EnsureInstance();

            return progressionManager;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _ = Progression;
    }

    private void OnEnable()
    {
        Progression.OnExperienceChanged += HandleExperienceChanged;
    }

    private void Start()
    {
        HandleExperienceChanged(CurrentExperience);
        SelectFacility(CurrentFacility);
    }

    private void OnDisable()
    {
        if (progressionManager != null)
            progressionManager.OnExperienceChanged -= HandleExperienceChanged;
    }

    public void SelectFacility(LobbyFacility facility)
    {
        CurrentFacility = facility;
        OnFacilityChanged?.Invoke(CurrentFacility);
    }

    public bool TryOpenAction(LobbyAction action)
    {
        LobbyFeatureUnlock unlock = GetFeatureUnlock(action);
        if (unlock == null)
        {
            CurrentAction = action;
            OnActionOpened?.Invoke(action);
            return true;
        }

        if (!unlock.implemented)
        {
            OnActionUnavailable?.Invoke(action, "This lobby feature is not implemented yet.");
            return false;
        }

        if (!Progression.HasExperience(unlock.requiredExperience))
        {
            string message = $"Requires {unlock.requiredExperience} meta experience.";
            OnActionUnavailable?.Invoke(action, message);
            return false;
        }

        CurrentAction = action;
        OnActionOpened?.Invoke(action);
        return true;
    }

    public void StartRun()
    {
        SelectFacility(LobbyFacility.Tent);

        if (!TryOpenAction(LobbyAction.StartRun))
            return;

        if (string.IsNullOrWhiteSpace(runSceneName))
        {
            OnActionUnavailable?.Invoke(LobbyAction.StartRun, "Run scene name is empty.");
            return;
        }

        SceneManager.LoadScene(runSceneName);
    }

    public void OpenStartingDeck()
    {
        SelectFacility(LobbyFacility.Tent);
        TryOpenAction(LobbyAction.ViewStartingDeck);
    }

    public void OpenSettings()
    {
        SelectFacility(LobbyFacility.Tent);
        TryOpenAction(LobbyAction.OpenSettings);
    }

    public void OpenArmory()
    {
        SelectFacility(LobbyFacility.Armory);
        TryOpenAction(LobbyAction.SelectStartingWeapon);
    }

    public void OpenCardUnlocks()
    {
        SelectFacility(LobbyFacility.Armory);
        TryOpenAction(LobbyAction.ViewCardUnlocks);
    }

    public void OpenInformant()
    {
        SelectFacility(LobbyFacility.Informant);
        TryOpenAction(LobbyAction.ViewEnemyCodex);
    }

    public void OpenEventRecords()
    {
        SelectFacility(LobbyFacility.Informant);
        TryOpenAction(LobbyAction.ViewEventRecords);
    }

    public void OpenTrainingGround()
    {
        SelectFacility(LobbyFacility.TrainingGround);
        TryOpenAction(LobbyAction.OpenTraining);
    }

    public void OpenRequestBoard()
    {
        SelectFacility(LobbyFacility.RequestBoard);
        TryOpenAction(LobbyAction.ViewRequests);
    }

    public void OpenMetaProgression()
    {
        TryOpenAction(LobbyAction.ViewMetaProgression);
    }

    public void OpenCooperatorEvent()
    {
        SelectFacility(LobbyFacility.Cooperator);
        TryOpenAction(LobbyAction.ViewCooperatorEvent);
    }

    public bool IsActionUnlocked(LobbyAction action)
    {
        LobbyFeatureUnlock unlock = GetFeatureUnlock(action);
        return unlock == null || Progression.HasExperience(unlock.requiredExperience);
    }

    public bool IsActionImplemented(LobbyAction action)
    {
        LobbyFeatureUnlock unlock = GetFeatureUnlock(action);
        return unlock == null || unlock.implemented;
    }

    public LobbyFeatureUnlock GetFeatureUnlock(LobbyAction action)
    {
        return featureUnlocks.Find(feature => feature.action == action);
    }

    public void AddMetaExperienceForDebug(int amount)
    {
        Progression.AddExperience(amount);
    }

    private void HandleExperienceChanged(int experience)
    {
        OnExperienceChanged?.Invoke(experience);
    }
}
