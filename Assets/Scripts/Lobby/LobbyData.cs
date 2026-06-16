using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyData
{
    private LobbyDataWrapper wrapper = new LobbyDataWrapper();

    public IReadOnlyList<LobbyDataProgressData> FacilityProgressDataList => wrapper.facilityProgressDataList;

    public void SetDefaultData(IEnumerable<Facility> facilities)
    {
        wrapper = new LobbyDataWrapper();

        if (facilities == null)
            return;

        foreach (Facility facility in facilities)
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
