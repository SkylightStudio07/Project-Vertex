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

        SetDefaultExperience();
    }

    public void SetDefaultExperience()
    {
        Experience = Mathf.Max(0, defaultExperience);
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
        OnExperienceChanged?.Invoke(Experience);
    }

    // Debug buttons for testing user experience changes in the Inspector.
    [ContextMenu("Debug/Add 10 Experience")]
    private void DebugAdd10Experience()
    {
        AddExperience(10);
        Logger.Log(this, $"Experience changed to {Experience}.");
    }

    [ContextMenu("Debug/Reset Experience")]
    private void DebugResetExperience()
    {
        SetDefaultExperience();
        Logger.Log(this, $"Experience reset to {Experience}.");
    }
}
