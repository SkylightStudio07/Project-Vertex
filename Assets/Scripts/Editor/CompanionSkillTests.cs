using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// 동료 스킬: 흡혈 효과, 충전 규칙. 기획: Docs/기획/동료_스킬.md
public class CompanionSkillTests
{
    private readonly List<Object> _assets = new();

    [TearDown]
    public void TearDown()
    {
        foreach (var asset in _assets)
            if (asset != null) Object.DestroyImmediate(asset);
        _assets.Clear();
    }

    private EnemyInstance Enemy(int hp)
    {
        var data = ScriptableObject.CreateInstance<EnemyData>();
        data.health = hp;
        _assets.Add(data);
        return new EnemyInstance(data);
    }

    private CompanionSkillData Skill(CompanionSkillCharge type, int charge, bool startReady)
    {
        var skill = ScriptableObject.CreateInstance<CompanionSkillData>();
        skill.chargeType = type;
        skill.chargeAmount = charge;
        skill.startReady = startReady;
        _assets.Add(skill);
        return skill;
    }

    [Test]
    public void LifeDrain_HealsOnlyDamageThatGotThroughBlock()
    {
        var caster = Enemy(30);
        var target = Enemy(30);
        caster.TakeDamage(new DamageInfo(10));
        target.AddBlock(4);
        var state = new BattleState { Enemies = { caster, target } };
        var ctx = new CardContext { State = state, SourceOverride = caster, PrimaryTargetOverride = target };

        new LifeDrainEffect { amount = 6 }.Execute(ctx);

        Assert.AreEqual(28, target.HP, "방어도 4에 막히고 2만 들어감");
        Assert.AreEqual(22, caster.HP, "들어간 2만큼만 회복");
    }

    [Test]
    public void BattleCharge_StartsReady_RechargesAfterThreeWins_AndCarriesOver()
    {
        var owner = new CoopCharState { charID = "test" };
        var skill = Skill(CompanionSkillCharge.Battles, 3, startReady: true);

        var slot = new CompanionSkillSlot(owner, skill, enhanced: false, training: false);
        Assert.IsTrue(slot.IsReady);
        slot.Consume();
        Assert.IsFalse(slot.IsReady);
        slot.OnBattleWon();

        for (int i = 0; i < 2; i++)
        {
            slot = new CompanionSkillSlot(owner, skill, false, false); // 다음 전투
            Assert.IsFalse(slot.IsReady);
            slot.OnBattleWon();
        }

        Assert.IsTrue(new CompanionSkillSlot(owner, skill, false, false).IsReady);
    }

    [Test]
    public void TurnCharge_NotReadyAtStart_ReadyAfterThreeTurnStarts_ResetsEachBattle()
    {
        var owner = new CoopCharState { charID = "test" };
        var skill = Skill(CompanionSkillCharge.Turns, 3, startReady: false);

        var slot = new CompanionSkillSlot(owner, skill, false, false);
        Assert.IsFalse(slot.IsReady);
        for (int i = 0; i < 3; i++) slot.OnPlayerTurnStart();
        Assert.IsTrue(slot.IsReady);
        slot.OnBattleWon();

        Assert.IsFalse(new CompanionSkillSlot(owner, skill, false, false).IsReady);
    }

    [Test]
    public void Training_AlwaysReady_AndLeavesRunProgressAlone()
    {
        var owner = new CoopCharState { charID = "test", skillProgress = 1 };
        var skill = Skill(CompanionSkillCharge.Battles, 3, startReady: false);

        var slot = new CompanionSkillSlot(owner, skill, false, training: true);
        Assert.IsTrue(slot.IsReady);
        slot.Consume();
        slot.OnBattleWon();
        Assert.AreEqual(1, owner.skillProgress);
    }

    [Test]
    public void Enhanced_UsesEnhancedEffectsAndCharge()
    {
        var skill = Skill(CompanionSkillCharge.Battles, 3, true);
        skill.effects = new List<CardEffect> { new LifeDrainEffect { amount = 6 } };
        Assert.AreEqual(3, skill.ChargeOf(true), "강화 효과가 없으면 강화도 없음");

        skill.enhancedEffects = new List<CardEffect> { new LifeDrainEffect { amount = 6 }, new BlockEffect() };
        skill.enhancedChargeAmount = 2;
        Assert.AreEqual(2, skill.ChargeOf(true));
        Assert.AreEqual(3, skill.ChargeOf(false));
        Assert.AreEqual(2, skill.EffectsOf(true).Count);
        Assert.AreEqual(1, skill.EffectsOf(false).Count);
    }
}
