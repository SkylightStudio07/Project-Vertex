using System;
using UnityEngine;

public class UserExpData : ISaveData
{
    private const string UserExpDataKey = "User.ExpData";

    private UserExpDataWrapper wrapper = new UserExpDataWrapper();

    public int Experience
    {
        get => wrapper.experience;
        set => wrapper.experience = Mathf.Max(0, value);
    }

    public void SetDefaultData()
    {
        Experience = 0;
    }

    public bool LoadData()
    {
        try
        {
            string json = PlayerPrefs.GetString(UserExpDataKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                SetDefaultData();
                return true;
            }

            wrapper = JsonUtility.FromJson<UserExpDataWrapper>(json) ?? new UserExpDataWrapper();
            wrapper.experience = Mathf.Max(0, wrapper.experience);
            return true;
        }
        catch (Exception exception)
        {
            Logger.LogError(typeof(UserExpData), $"Failed to load user exp data: {exception}");
            SetDefaultData();
            return false;
        }
    }

    public bool SaveData()
    {
        try
        {
            PlayerPrefs.SetString(UserExpDataKey, JsonUtility.ToJson(wrapper));
            return true;
        }
        catch (Exception exception)
        {
            Logger.LogError(typeof(UserExpData), $"Failed to save user exp data: {exception}");
            return false;
        }
    }
}

[Serializable]
public class UserExpDataWrapper
{
    public int experience;
}
