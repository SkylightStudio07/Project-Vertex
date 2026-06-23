using System;
using UnityEngine;

public class UserExpManager : SingletonBehaviour<UserExpManager>
{
    [SerializeField] private int defaultExperience;
    [SerializeField, Min(0)] private int maxExperience = 100;

    public int Experience { get; private set; }
    public int MaxExperience => Mathf.Max(0, maxExperience);

    public event Action<int> OnExperienceChanged;

    protected override void Init()
    {
        base.Init();

        SetDefaultExperience();
    }

    public void SetDefaultExperience()
    {
        Experience = Mathf.Clamp(defaultExperience, 0, MaxExperience);
        OnExperienceChanged?.Invoke(Experience);
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
            return;

        long increasedExperience = (long)Experience + amount;
        SetExperience((int)Math.Min(increasedExperience, MaxExperience));
    }

    public bool HasExperience(int requiredExperience)
    {
        return Experience >= Mathf.Max(0, requiredExperience);
    }

    public bool TrySpendExperience(int amount)
    {
        if (amount < 0 || !HasExperience(amount))
            return false;

        SetExperience(Experience - amount);
        return true;
    }

    private void SetExperience(int experience)
    {
        int clampedExperience = Mathf.Clamp(experience, 0, MaxExperience);
        if (Experience == clampedExperience)
            return;

        Experience = clampedExperience;
        OnExperienceChanged?.Invoke(Experience);
    }

    // 디버깅용 버튼 함수들.
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

    [ContextMenu("Debug/Set Max Experience")]
    private void DebugSetMaxExperience()
    {
        SetExperience(MaxExperience);
        Logger.Log(this, $"Experience set to max: {Experience}.");
    }

}
