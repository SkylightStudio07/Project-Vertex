using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyData : ISaveData
{
    private const string LobbyDataKey = "Lobby.Data";

    private LobbyDataWrapper wrapper = new LobbyDataWrapper();

    public IReadOnlyList<LobbyDataProgressData> FacilityProgressDataList => wrapper.facilityProgressDataList;

    public void SetDefaultData()
    {
        wrapper = new LobbyDataWrapper();

        foreach (Facility facility in UnityEngine.Object.FindObjectsByType<Facility>(FindObjectsSortMode.None))
        {
            if (facility == null)
                continue;

            LobbyDataProgressData progressData = new LobbyDataProgressData(facility.FacilityId)
            {
                isActive = facility.IsActive
            };
            wrapper.facilityProgressDataList.Add(progressData);
        }
    }

    public bool LoadData()
    {
        try
        {
            string json = PlayerPrefs.GetString(LobbyDataKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                SetDefaultData();
                return true;
            }

            wrapper = JsonUtility.FromJson<LobbyDataWrapper>(json) ?? new LobbyDataWrapper();
            wrapper.facilityProgressDataList ??= new List<LobbyDataProgressData>();
            return true;
        }
        catch (Exception exception)
        {
            Logger.LogError(typeof(LobbyData), $"Failed to load lobby data: {exception}");
            SetDefaultData();
            return false;
        }
    }

    public bool SaveData()
    {
        try
        {
            PlayerPrefs.SetString(LobbyDataKey, JsonUtility.ToJson(wrapper));
            return true;
        }
        catch (Exception exception)
        {
            Logger.LogError(typeof(LobbyData), $"Failed to save lobby data: {exception}");
            return false;
        }
    }

    public LobbyDataProgressData GetFacilityProgressData(string facilityId)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return null;

        return wrapper.facilityProgressDataList.Find(data => data.facilityId == facilityId);
    }

    public LobbyDataProgressData GetOrCreateFacilityProgressData(string facilityId)
    {
        LobbyDataProgressData progressData = GetFacilityProgressData(facilityId);
        if (progressData != null)
            return progressData;

        progressData = new LobbyDataProgressData(facilityId);
        wrapper.facilityProgressDataList.Add(progressData);
        return progressData;
    }
}

[Serializable]
public class LobbyDataProgressData
{
    public string facilityId;
    public bool isActive = true;

    public LobbyDataProgressData()
    {
    }

    public LobbyDataProgressData(string facilityId)
    {
        this.facilityId = facilityId;
    }
}

[Serializable]
public class LobbyDataWrapper
{
    public List<LobbyDataProgressData> facilityProgressDataList = new List<LobbyDataProgressData>();
}
