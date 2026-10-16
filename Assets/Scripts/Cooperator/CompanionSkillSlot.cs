// 전투 하나 동안의 동료 스킬 상태. BattleManager가 전투 시작 때 동행 동료마다 만든다.
//   턴 충전: 진행도는 이 전투 안에서만. 내 턴이 시작될 때마다(첫 턴 제외) 1씩
//   전투 충전: 진행도는 동료 상태(CoopCharState.skillProgress)에 남아 전투 사이로 이어진다. 이긴 전투마다 1씩
//   훈련장: 모두 준비된 상태로 시작하고, 런 진행도에는 쓰지 않는다
public class CompanionSkillSlot
{
    public CoopCharState Owner { get; }
    public CompanionSkillData Skill { get; }
    public bool Enhanced { get; }
    public int Progress { get; private set; }

    private readonly bool _training;

    public CompanionSkillSlot(CoopCharState owner, CompanionSkillData skill, bool enhanced, bool training)
    {
        Owner = owner;
        Skill = skill;
        Enhanced = enhanced;
        _training = training;

        if (training) Progress = Required;
        else if (skill.chargeType == CompanionSkillCharge.Battles)
        {
            if (owner.skillProgress < 0) owner.skillProgress = skill.startReady ? Required : 0;
            Progress = System.Math.Min(owner.skillProgress, Required);
        }
        else Progress = skill.startReady ? Required : 0;
    }

    public int Required => Skill.ChargeOf(Enhanced);
    public bool IsReady => Progress >= Required;
    public bool IsTurnCharge => Skill.chargeType == CompanionSkillCharge.Turns;

    // 내 턴 시작 (첫 턴은 부르지 않는다)
    public void OnPlayerTurnStart()
    {
        if (IsTurnCharge && !IsReady) Progress++;
    }

    // 전투 승리
    public void OnBattleWon()
    {
        if (IsTurnCharge || _training) return;
        if (!IsReady) Progress++;
        Owner.skillProgress = Progress;
    }

    public void Consume()
    {
        Progress = 0;
        if (!IsTurnCharge && !_training) Owner.skillProgress = 0;
    }
}
