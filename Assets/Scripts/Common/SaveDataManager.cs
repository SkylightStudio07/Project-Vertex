using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveDataManager : SingletonBehaviour<SaveDataManager>
{
    private const string ExistsSavedDataKey = "SaveData.Exists";

    public List<ISaveData> UserDataList { get; private set; }
    public bool ExistsSavedData { get; private set; }


    protected override void Init()
    {
        base.Init();

        UserDataList = new List<ISaveData>();
        RegisterSaveData(new LobbyData());
        RegisterSaveData(new UserExpData());

        ExistsSavedData = PlayerPrefs.GetInt(ExistsSavedDataKey, 0) == 1;
        if (ExistsSavedData)
            LoadData();
        else
            SetDefaultData();
    }

    public void RegisterSaveData(ISaveData saveData)
    {
        UserDataList ??= new List<ISaveData>();

        if (saveData == null || UserDataList.Contains(saveData))
            return;

        UserDataList.Add(saveData);
    }

    public T GetSaveData<T>() where T : class, ISaveData
    {
        return UserDataList.OfType<T>().FirstOrDefault();
    }

    public bool LoadData()
    {
        bool isSuccess = true;
        ExistsSavedData = PlayerPrefs.GetInt(ExistsSavedDataKey, 0) == 1;

        foreach (ISaveData saveData in UserDataList)
            isSuccess &= saveData.LoadData();

        return isSuccess;
    }

    public bool SaveData()
    {
        bool isSuccess = true;

        foreach (ISaveData saveData in UserDataList)
            isSuccess &= saveData.SaveData();

        if (isSuccess)
        {
            ExistsSavedData = true;
            PlayerPrefs.SetInt(ExistsSavedDataKey, 1);
            PlayerPrefs.Save();
        }

        return isSuccess;
    }

    public void SetDefaultData()
    {
        foreach (ISaveData saveData in UserDataList)
            saveData.SetDefaultData();
    }
}
