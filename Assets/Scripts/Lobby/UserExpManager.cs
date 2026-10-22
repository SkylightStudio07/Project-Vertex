using System;
using UnityEngine;

// 로비 상단 게이지·의뢰 게시판 머리가 보던 "로비 경험치" 창구. 이제 거점 개발도(MetaProgress)를 보여 준다.
//   - 개발도는 쓰지 않고 쌓기만 한다. 이정표(동료 첫 합류, 보스 첫 격파, 스토리 의뢰 …)마다 MetaProgress가 올린다
//   - Experience = 누적 개발도, MaxExperience = 다음 레벨에 필요한 누적 개발도 (최대 레벨이면 현재 값)
//   - 예전 저장 키(VERTEX_USER_EXP)는 더 이상 읽지 않는다
// 기획: 노션 VERTEX 기획서 v2 > 8. 재화 & 거점 개발도
public class UserExpManager : SingletonBehaviour<UserExpManager>
{
    public int Experience => MetaProgress.Development;
    public int Level => MetaProgress.DevelopmentLevel;

    public int MaxExperience
    {
        get
        {
            var (current, total) = MetaProgress.LevelProgress();
            return total > 0 ? MetaProgress.Development - current + total : MetaProgress.Development;
        }
    }

    public event Action<int> OnExperienceChanged;

    protected override void Init()
    {
        m_IsDestroyOnLoad = true;
        base.Init();
        MetaProgress.OnChanged += Notify;
    }

    private void OnDestroy() => MetaProgress.OnChanged -= Notify;

    private void Notify() => OnExperienceChanged?.Invoke(Experience);

    public bool HasExperience(int requiredExperience) => Experience >= Mathf.Max(0, requiredExperience);

    [ContextMenu("Debug/Add 10 Development")]
    private void DebugAdd10()
    {
        MetaProgress.AwardDevelopment("debug:" + DateTime.Now.Ticks, 10);
        Logger.Log(this, $"Development {Experience} (Lv.{Level})");
    }
}
