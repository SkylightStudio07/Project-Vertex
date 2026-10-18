using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// 속성 & 특효: 특효 배율 1.2, 여러 속성이 겹쳐도 1회, 키워드 표기
public class EnemyTraitTests
{
    private readonly List<Object> _assets = new();

    [TearDown]
    public void TearDown()
    {
        foreach (var asset in _assets)
            if (asset != null) Object.DestroyImmediate(asset);
        _assets.Clear();
    }

    private EnemyInstance Enemy(int hp, EnemyTrait traits)
    {
        var data = ScriptableObject.CreateInstance<EnemyData>();
        data.health = hp;
        data.traits = traits;
        _assets.Add(data);
        return new EnemyInstance(data);
    }

    private static int Hit(DamageEffect effect, EnemyInstance target)
    {
        var state = new BattleState { Enemies = { target } };
        int before = target.HP;
        effect.Execute(new CardContext { State = state, SourceOverride = Enemy0, PrimaryTargetOverride = target });
        return before - target.HP;
    }

    private static EnemyInstance Enemy0;

    [SetUp]
    public void SetUp() => Enemy0 = Enemy(100, EnemyTrait.None);

    [Test]
    public void Effective_MultipliesOnce_EvenWithSeveralMatchingTraits()
    {
        var effect = new DamageEffect { amount = 10, effectiveAgainst = EnemyTrait.Human | EnemyTrait.Mystic };

        Assert.AreEqual(10, Hit(effect, Enemy(100, EnemyTrait.Machine)), "특효 없음");
        Assert.AreEqual(12, Hit(effect, Enemy(100, EnemyTrait.Human)), "인간 특효 ×1.2");
        Assert.AreEqual(12, Hit(effect, Enemy(100, EnemyTrait.Human | EnemyTrait.Mystic)), "둘 다 가져도 1회");
    }

    [Test]
    public void TargetTraitCondition_MatchesAnyListedTrait()
    {
        var condition = new TargetTraitCondition { traits = EnemyTrait.HeavyArmor | EnemyTrait.Machine };
        Assert.IsTrue(condition.IsMet(new CardContext { PrimaryTargetOverride = Enemy(10, EnemyTrait.Machine | EnemyTrait.Star) }));
        Assert.IsFalse(condition.IsMet(new CardContext { PrimaryTargetOverride = Enemy(10, EnemyTrait.Human) }));
    }

    [Test]
    public void Label_JoinsInDeclaredOrder()
    {
        Assert.AreEqual("인간·신비", EnemyTraits.Label(EnemyTrait.Mystic | EnemyTrait.Human));
        Assert.AreEqual("", EnemyTraits.Label(EnemyTrait.None));
    }
}
