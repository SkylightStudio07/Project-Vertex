using System;
using UnityEngine;

public class UserExpManager : SingletonBehaviour<UserExpManager>
{
    [SerializeField] private int defaultExperience;
    [SerializeField, Min(0)] private int maxExperience = 100;

    public int Experience { get; private set; }
    public int MaxExperience => Mathf.Max(0, maxExperience);

    public event Action<int> OnExperienceChanged;

    private const string PrefsKey = "VERTEX_USER_EXP";

    protected override void Init()
    {
        m_IsDestroyOnLoad = true;
        base.Init();

        // 로비 경험치는 로비 씬을 나갔다 들어와도(런 귀환) 이어져야 하므로 PlayerPrefs에 둔다. 저장값이 없으면 기본값.
        if (PlayerPrefs.HasKey(PrefsKey)) Experience = Mathf.Clamp(PlayerPrefs.GetInt(PrefsKey), 0, MaxExperience);
        else SetDefaultExperience();
    }

    public void SetDefaultExperience()
    {
        Experience = Mathf.Clamp(defaultExperience, 0, MaxExperience);
        Save();
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
        Save();
        OnExperienceChanged?.Invoke(Experience);
    }

    private void Save()
    {
        PlayerPrefs.SetInt(PrefsKey, Experience);
        PlayerPrefs.Save();
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
