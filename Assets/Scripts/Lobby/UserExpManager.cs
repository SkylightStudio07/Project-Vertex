using System;
using UnityEngine;

public class UserExpManager : SingletonBehaviour<UserExpManager>
{
    [SerializeField] private int defaultExperience;

    public int Experience { get; private set; }

    public event Action<int> OnExperienceChanged;

    protected override void Init()
    {
        base.Init();

        LoadExperience();
    }

    public void LoadExperience()
    {
        UserExpData userExpData = SaveDataManager.Instance.GetSaveData<UserExpData>();
        Experience = userExpData != null ? userExpData.Experience : Mathf.Max(0, defaultExperience);
        OnExperienceChanged?.Invoke(Experience);
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
            return;

        SetExperience(Experience + amount);
    }

    public bool HasExperience(int requiredExperience)
    {
        return Experience >= Mathf.Max(0, requiredExperience);
    }

    private void SetExperience(int experience)
    {
        int clampedExperience = Mathf.Max(0, experience);
        if (Experience == clampedExperience)
            return;

        Experience = clampedExperience;
        UserExpData userExpData = SaveDataManager.Instance.GetSaveData<UserExpData>();
        if (userExpData != null)
        {
            userExpData.Experience = Experience;
            SaveDataManager.Instance.SaveData();
        }

        OnExperienceChanged?.Invoke(Experience);
    }
}
