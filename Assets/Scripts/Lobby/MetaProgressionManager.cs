using System;
using System.Collections.Generic;
using UnityEngine;

public class MetaProgressionManager : MonoBehaviour
{
    private const string ExperiencePrefsKey = "MetaProgression.Experience";
    private const string CompletedObjectivesPrefsKey = "MetaProgression.CompletedObjectives";

    public static MetaProgressionManager Instance { get; private set; }

    [SerializeField] private int startingExperience;
    [SerializeField] private bool loadOnAwake = true;

    private readonly HashSet<string> completedObjectiveIds = new HashSet<string>();

    public int Experience { get; private set; }
    public IReadOnlyCollection<string> CompletedObjectiveIds => completedObjectiveIds;

    public event Action<int> OnExperienceChanged;

    public static MetaProgressionManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        GameObject managerObject = new GameObject(nameof(MetaProgressionManager));
        return managerObject.AddComponent<MetaProgressionManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadOnAwake)
            Load();
        else
            SetExperience(startingExperience, false);
    }

    public void Load()
    {
        int savedExperience = PlayerPrefs.GetInt(ExperiencePrefsKey, startingExperience);
        SetExperience(savedExperience, false);
        LoadCompletedObjectives();
    }

    public void Save()
    {
        PlayerPrefs.SetInt(ExperiencePrefsKey, Experience);
        PlayerPrefs.SetString(CompletedObjectivesPrefsKey, JsonUtility.ToJson(new CompletedObjectiveSaveData(completedObjectiveIds)));
        PlayerPrefs.Save();
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
            return;

        SetExperience(Experience + amount, true);
    }

    public bool TryGrantObjectiveReward(string objectiveId, int rewardExperience)
    {
        if (string.IsNullOrWhiteSpace(objectiveId))
        {
            Debug.LogWarning("Objective reward ignored because objectiveId is empty.");
            return false;
        }

        if (completedObjectiveIds.Contains(objectiveId))
            return false;

        if (rewardExperience <= 0)
            return false;

        completedObjectiveIds.Add(objectiveId);
        SetExperience(Experience + rewardExperience, false);

        Save();
        return true;
    }

    public void GrantObjectiveReward(string objectiveId, int rewardExperience)
    {
        TryGrantObjectiveReward(objectiveId, rewardExperience);
    }

    public bool IsObjectiveRewardClaimed(string objectiveId)
    {
        return !string.IsNullOrWhiteSpace(objectiveId) && completedObjectiveIds.Contains(objectiveId);
    }

    public bool HasExperience(int requiredExperience)
    {
        return Experience >= Mathf.Max(0, requiredExperience);
    }

    public void ResetProgressForDebug()
    {
        completedObjectiveIds.Clear();
        SetExperience(startingExperience, true);
    }

    private void LoadCompletedObjectives()
    {
        completedObjectiveIds.Clear();

        string json = PlayerPrefs.GetString(CompletedObjectivesPrefsKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
            return;

        CompletedObjectiveSaveData saveData = JsonUtility.FromJson<CompletedObjectiveSaveData>(json);
        if (saveData?.objectiveIds == null)
            return;

        foreach (string objectiveId in saveData.objectiveIds)
        {
            if (!string.IsNullOrWhiteSpace(objectiveId))
                completedObjectiveIds.Add(objectiveId);
        }
    }

    private void SetExperience(int value, bool save)
    {
        int clampedValue = Mathf.Max(0, value);
        if (Experience == clampedValue)
        {
            if (save)
                Save();

            return;
        }

        Experience = clampedValue;

        if (save)
            Save();

        OnExperienceChanged?.Invoke(Experience);
    }

    [Serializable]
    private class CompletedObjectiveSaveData
    {
        public List<string> objectiveIds = new List<string>();

        public CompletedObjectiveSaveData()
        {
        }

        public CompletedObjectiveSaveData(HashSet<string> completedObjectiveIds)
        {
            objectiveIds.AddRange(completedObjectiveIds);
        }
    }
}
