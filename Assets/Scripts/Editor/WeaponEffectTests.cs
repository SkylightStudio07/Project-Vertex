using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class WeaponEffectTests
{
    private readonly List<Object> _assets = new();
    private BattleState _state;

    [SetUp]
    public void SetUp() => _state = new BattleState { Player = new PlayerCombatant(), Ammo = 7 };

    [TearDown]
    public void TearDown()
    {
        _state.ReleaseRuntimeCards();
        foreach (var asset in _assets)
            if (asset != null) Object.DestroyImmediate(asset);
        _assets.Clear();
    }

    [Test]
    public void SwapReplacesWholeCardInEveryPileAndPreservesUpgradesAndOriginals()
    {
        var original = Card("Original", true, 1);
        var other = Card("Other", false, 2);
        var variant = Card("Variant", false, 3);
        SetField(variant, "useMode", CardData.CardUseMode.SelectEnemy);
        SetField(variant, "cardType", CardData.CardType.Skill);
        var weapon = Weapon(variant);
        var piles = new[] { _state.Hand, _state.DrawPile, _state.DiscardPile, _state.ExhaustPile };
        foreach (var pile in piles)
        {
            pile.Add(_state.CreateCard(original));
            pile[0].isUpgraded = true;
            pile.Add(_state.CreateCard(other));
        }
        var executingCard = _state.DiscardPile[0];
        var unaffected = _state.Hand[1];

        new ChangeWeaponEffect { weapon = weapon }.Execute(new CardContext { State = _state });

        foreach (var pile in piles)
        {
            Assert.That(pile.Count, Is.EqualTo(2));
            Assert.That(pile[0].CardName, Is.EqualTo("Variant+"));
            Assert.That(pile[0].EnergyCost, Is.EqualTo(4));
            Assert.That(pile[0].AmmoCost, Is.EqualTo(3));
            Assert.That(pile[0].UseMode, Is.EqualTo(CardData.CardUseMode.SelectEnemy));
            Assert.That(pile[0].Type, Is.EqualTo(CardData.CardType.Skill));
            Assert.That(pile[0].IsRetain, Is.True);
            Assert.That(pile[0].IsWeaponShootingCard, Is.True);
            Assert.That(pile[0].ActiveEffects[0], Is.Not.SameAs(variant.UpgradedEffects[0]));
            Assert.That(pile[0].GetFullDescription(), Is.EqualTo("Damage 30"));
        }
        Assert.That(_state.Hand[0], Is.Not.SameAs(_state.DrawPile[0]));
        Assert.That(_state.Hand[1], Is.SameAs(unaffected));
        Assert.That(executingCard.CardName, Is.EqualTo("Original+"));
        Assert.That(original.CardName, Is.EqualTo("Original"));
        Assert.That(variant.IsWeaponShootingCard, Is.False);
        Assert.That(variant.isUpgraded, Is.False);
        Assert.That(_state.Ammo, Is.EqualTo(7));
        Assert.That(_state.Player.Passives, Is.Empty);
    }

    [Test]
    public void RepeatedSwapAndGeneratedCardsUseLatestWeapon()
    {
        var original = Card("Original", true, 1);
        var first = Weapon(Card("First", false, 2));
        var second = Weapon(Card("Second", false, 3));
        _state.ChangePlayerWeapon(first);
        _state.Hand.Add(_state.CreateCard(original));
        Assert.That(_state.Hand[0].CardName, Is.EqualTo("First"));

        _state.ChangePlayerWeapon(second);
        Assert.That(_state.Hand[0].CardName, Is.EqualTo("Second"));
        Assert.That(_state.CreateCard(original).CardName, Is.EqualTo("Second"));
        var currentCard = _state.Hand[0];
        Assert.That(_state.ChangePlayerWeapon(second), Is.False); // 같은 무기는 변화 없음
        Assert.That(_state.Hand[0], Is.SameAs(currentCard));
        Assert.That(_state.ChangePlayerWeapon(Weapon(null)), Is.False);
        Assert.That(_state.CurrentWeapon, Is.SameAs(second));
    }

    [Test]
    public void WeaponConditionRunsThroughConditionalEffectWithoutAddingStatuses()
    {
        var weapon = Weapon(Card("Weapon", false, 1));
        var context = new CardContext { State = _state };
        var condition = new PlayerWeaponCondition { weapon = weapon };
        var effect = new ConditionalEffect
        {
            condition = condition,
            effectsWhenMet = new List<CardEffect> { new AddAmmoEffect { amount = 2 } },
        };
        effect.Execute(context);
        Assert.That(_state.Ammo, Is.EqualTo(7));
        new ChangeWeaponEffect { weapon = weapon }.Execute(context);
        Assert.That(_state.Ammo, Is.EqualTo(7));
        _state.Ammo = 4; // 탄창(7)이 가득 차 있으면 획득이 상한에 막히므로 비운 뒤 확인
        effect.Execute(context);
        Assert.That(_state.Ammo, Is.EqualTo(6));
        Assert.That(_state.Player.Passives, Is.Empty);
        Assert.That(condition.IsMet(new CardContext()), Is.False);
        Assert.That(condition.IsMet(null), Is.False);
        Assert.That(new PlayerWeaponCondition().IsMet(context), Is.False);
        Assert.DoesNotThrow(() => new ChangeWeaponEffect { weapon = weapon }.Execute(new CardContext()));
    }

    [Test]
    public void ManagerRefreshesHandAndResetsWeaponForNextBattle()
    {
        var go = new GameObject("Weapon test battle");
        _assets.Add(go);
        var battle = go.AddComponent<BattleManager>();
        var shot = Card("Original", true, 1);
        var defaultWeapon = Weapon(Card("Default", false, 2));
        var changedWeapon = Weapon(Card("Changed", false, 3));
        SetField(battle, "defaultWeapon", defaultWeapon);
        var deck = new List<CardData> { shot };
        battle.StartBattle(new List<EnemyData>(), deck, 42);
        int handChanges = 0;
        int weaponChanges = 0;
        battle.OnHandChanged += () => handChanges++;
        battle.OnWeaponChanged += _ => weaponChanges++;
        new ChangeWeaponEffect { weapon = changedWeapon }.Execute(
            new CardContext { State = battle.State, Battle = battle });
        Assert.That(handChanges, Is.EqualTo(1));
        Assert.That(weaponChanges, Is.EqualTo(1));
        battle.AddCardToHand(shot);
        battle.AddCardToDiscardPile(shot);
        battle.AddCardToDrawPile(shot);
        Assert.That(battle.Hand[0].CardName, Is.EqualTo("Changed"));
        Assert.That(battle.State.DiscardPile[0].CardName, Is.EqualTo("Changed"));
        Assert.That(battle.State.DrawPile[0].CardName, Is.EqualTo("Changed"));
        battle.StartBattle(new List<EnemyData>(), deck, 43);
        Assert.That(battle.CurrentWeapon, Is.SameAs(defaultWeapon));
        Assert.That(battle.State.DrawPile[0].CardName, Is.EqualTo("Default"));
        Assert.That(shot.CardName, Is.EqualTo("Original"));
    }

    private CardContext AmmoCard(int cost, int capacity = 3)
    {
        var card = Card("Ammo", false, cost);
        var weapon = Weapon(card);
        SetField(weapon, "maxAmmo", capacity);
        _state.ChangePlayerWeapon(weapon);
        return new CardContext { State = _state, Card = card };
    }

    [Test]
    public void FirstAndLastAreOncePerReloadAndIndependentOfOrdinaryAmmoGain()
    {
        var first = AmmoCard(1);
        _state.ConsumeAmmo(1, first);
        Assert.That(first.IsFirstAmmoUse, Is.True);
        Assert.That(first.IsLastAmmoUse, Is.False);
        new AddAmmoEffect { amount = 1 }.Execute(first);
        var last = new CardContext { State = _state, Card = first.Card };
        _state.ConsumeAmmo(3, last);
        Assert.That(last.IsFirstAmmoUse, Is.False);
        Assert.That(last.IsLastAmmoUse, Is.True);
        new AddAmmoEffect { amount = 3 }.Execute(first);
        var extra = new CardContext { State = _state, Card = first.Card };
        _state.ConsumeAmmo(3, extra);
        Assert.That(extra.IsFirstAmmoUse || extra.IsLastAmmoUse, Is.False);
        _state.ReloadAmmo();
        var both = new CardContext { State = _state, Card = first.Card };
        _state.ConsumeAmmo(3, both);
        Assert.That(both.IsFirstAmmoUse && both.IsLastAmmoUse, Is.True);
        Assert.That(first.IsFirstAmmoUse && last.IsLastAmmoUse, Is.True);
    }

    [Test]
    public void ZeroConsumptionDoesNotClaimFirstAndSingleRoundGetsBoth()
    {
        var context = AmmoCard(1, 1);
        _state.ConsumeAmmo(0, context);
        Assert.That(context.IsFirstAmmoUse || context.IsLastAmmoUse, Is.False);
        Assert.That(_state.FirstAmmoUseAvailable && _state.LastAmmoUseAvailable, Is.True);
        _state.ConsumeAmmo(1, context);
        Assert.That(context.IsFirstAmmoUse && context.IsLastAmmoUse, Is.True);
        _state.ReloadAmmo();
        Assert.That(context.IsFirstAmmoUse && context.IsLastAmmoUse, Is.True);
        Assert.That(_state.FirstAmmoUseAvailable && _state.LastAmmoUseAvailable, Is.True);
    }

    private DamageEffect BonusDamage(bool includeLast = false)
    {
        var effect = new DamageEffect { amount = 3 };
        effect.conditionalBonuses.Add(new ConditionalDamageBonus { condition = new FirstAmmoUseCondition(), amount = 4 });
        if (includeLast)
            effect.conditionalBonuses.Add(new ConditionalDamageBonus { condition = new LastAmmoUseCondition(), amount = 4 });
        return effect;
    }

    [TestCase(false, 7)]
    [TestCase(true, 11)]
    public void ConditionalBonusesProduceOneHitAndMatchPreview(bool includeLast, int expected)
    {
        var context = AmmoCard(1, 1);
        var target = new TestTarget();
        context.PrimaryTargetOverride = target;
        var damage = BonusDamage(includeLast);
        var effects = new CardEffect[] { damage };
        var preview = EffectRunner.Preview(effects, context);
        Assert.That(preview.TotalDamage, Is.EqualTo(expected));
        Assert.That(preview.HitCount, Is.EqualTo(1));
        Assert.That(damage.GetDisplayValue("amount", 3, _state, context.Card), Is.EqualTo(expected));
        Assert.That(_state.Ammo, Is.EqualTo(1));
        Assert.That(_state.FirstAmmoUseAvailable && _state.LastAmmoUseAvailable, Is.True);
        Assert.That(context.HasAmmoUsage, Is.False);
        _state.ConsumeAmmo(1, context);
        RunSequence(EffectRunner.ExecuteSequence(effects, context));
        Assert.That(target.Hits, Is.EqualTo(new[] { expected }));
        var copy = Object.Instantiate(context.Card);
        _assets.Add(copy);
        Assert.That(new FirstAmmoUseCondition().IsMet(context), Is.True);
    }

    [Test]
    public void SeparateConditionalEffectStillProducesTwoHitsAndRepeatProducesThree()
    {
        var context = AmmoCard(1);
        var target = new TestTarget();
        context.PrimaryTargetOverride = target;
        var effects = new CardEffect[]
        {
            new DamageEffect { amount = 3 },
            new ConditionalEffect { condition = new FirstAmmoUseCondition(), effectsWhenMet = new List<CardEffect> { new DamageEffect { amount = 4 } } },
        };
        var preview = EffectRunner.Preview(effects, context);
        Assert.That(preview.TotalDamage, Is.EqualTo(7));
        Assert.That(preview.HitCount, Is.EqualTo(2));
        _state.ConsumeAmmo(1, context);
        EffectRunner.ExecuteImmediate(effects, context);
        Assert.That(target.Hits, Is.EqualTo(new[] { 3, 4 }));
        target.Hits.Clear();
        new RepeatEffect { count = 3, effects = new List<CardEffect> { BonusDamage() } }.Execute(context);
        Assert.That(target.Hits, Is.EqualTo(new[] { 7, 7, 7 }));
    }

    [Test]
    public void ConsumeAllRecordsUsageBeforeDamageAndPreviewDoesNotConsumeRealAmmo()
    {
        var context = AmmoCard(0);
        var target = new TestTarget();
        context.PrimaryTargetOverride = target;
        var effects = new CardEffect[] { new AmmoConsumeAllDamageEffect { damagePerAmmo = 2 }, BonusDamage(true) };
        var preview = EffectRunner.Preview(effects, context);
        Assert.That(preview.TotalDamage, Is.EqualTo(17));
        Assert.That(_state.Ammo, Is.EqualTo(3));
        target.OnHit = () => Assert.That(context.IsFirstAmmoUse && context.IsLastAmmoUse, Is.True);
        EffectRunner.ExecuteImmediate(effects, context);
        Assert.That(context.AmmoSpent, Is.EqualTo(3));
        Assert.That(target.Hits, Is.EqualTo(new[] { 6, 11 }));
    }

    [Test]
    public void ConvertedAmmoCostPreviewAndPaymentDoNotConsumeFirstOrLast()
    {
        var context = AmmoCard(1, 1);
        var definition = ScriptableObject.CreateInstance<StatusDefinition>();
        _assets.Add(definition);
        SetField(definition, "behaviors", new List<StatusBehavior> { new CardBloodCostStatusBehavior() });
        _state.Player.AddPassive(definition.CreateInstance(1));
        var preview = context.CreateAmmoPreview();
        Assert.That(preview.AmmoSpent, Is.Zero);
        Assert.That(preview.IsFirstAmmoUse || preview.IsLastAmmoUse, Is.False);
        Assert.That(BonusDamage().GetDisplayValue("amount", 3, _state, context.Card), Is.EqualTo(3));
        var go = new GameObject("Ammo payment test");
        _assets.Add(go);
        var battle = go.AddComponent<BattleManager>();
        SetField(battle, "_state", _state);
        // 비용 보정은 별도로 검증했고, 실제 지불 경로에서 0 탄약이 판정을 소모하지 않는지 확인한다.
        typeof(BattleManager).GetMethod("PayCardPlayCost", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(battle, new object[] { new CardPlayCost(0, 0), context });
        Assert.That(context.IsFirstAmmoUse || context.IsLastAmmoUse, Is.False);
        Assert.That(_state.FirstAmmoUseAvailable && _state.LastAmmoUseAvailable, Is.True);
        typeof(BattleManager).GetMethod("PayCardPlayCost", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(battle, new object[] { new CardPlayCost(0, 1), context });
        Assert.That(context.IsFirstAmmoUse && context.IsLastAmmoUse, Is.True);
    }

    [Test]
    public void BonusConditionsSerializeAndCloneWithoutSharingUpgradeData()
    {
        var card = Card("Bonus", false, 1);
        card.CardEffect.Clear();
        card.CardEffect.Add(BonusDamage(true));
        CardAuthoringUtility.CopyNormalToUpgrade(card);
        var upgraded = (DamageEffect)card.UpgradedEffects[0];
        Assert.That(upgraded.conditionalBonuses[0].condition, Is.TypeOf<FirstAmmoUseCondition>());
        Assert.That(upgraded.conditionalBonuses[1].condition, Is.TypeOf<LastAmmoUseCondition>());
        upgraded.conditionalBonuses[0].amount = 99;
        Assert.That(((DamageEffect)card.CardEffect[0]).conditionalBonuses[0].amount, Is.EqualTo(4));
        using var serialized = new UnityEditor.SerializedObject(card);
        Assert.That(serialized.FindProperty("normalState.effects").GetArrayElementAtIndex(0)
            .FindPropertyRelative("conditionalBonuses").GetArrayElementAtIndex(0)
            .FindPropertyRelative("condition").managedReferenceValue, Is.TypeOf<FirstAmmoUseCondition>());
    }

    [Test]
    public void CompositeConditionsAndPassiveContextReuseRecordedFlags()
    {
        var context = AmmoCard(1, 1);
        var both = new AllConditions { conditions = new List<CardCondition> { new FirstAmmoUseCondition(), new LastAmmoUseCondition() } };
        Assert.That(both.IsMet(_state, context.Card), Is.True);
        Assert.That(_state.Ammo, Is.EqualTo(1));
        _state.ConsumeAmmo(1, context);
        var passive = ScriptableObject.CreateInstance<StatusDefinition>();
        _assets.Add(passive);
        var inherited = CardContext.CreatePassiveContext(_state, _state.Player, null, passive.CreateInstance(1), parent: context);
        Assert.That(both.IsMet(inherited), Is.True);
        Assert.That(inherited.AmmoSpent, Is.EqualTo(1));
        _state.ReloadAmmo();
        Assert.That(both.IsMet(context), Is.True);
    }

    private CardData Card(string name, bool isShot, int value)
    {
        var card = ScriptableObject.CreateInstance<CardData>();
        _assets.Add(card);
        SetField(card, "isWeaponShootingCard", isShot);
        SetField(card, "normalState", new CardUpgradeState
        {
            cardName = name, energyCost = value, ammoCost = value,
            effects = new List<CardEffect> { new DamageEffect { amount = value } },
        });
        SetField(card, "upgradedState", new CardUpgradeState
        {
            cardName = name + "+", energyCost = value + 1, ammoCost = value, isRetain = true,
            effects = new List<CardEffect> { new DamageEffect { amount = value * 10 } },
        });
        card.cardDescription = "Damage {0.amount}";
        return card;
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(3)]
    [TestCase(9)]
    public void ReloadSetsMaximumAndSameWeaponSwitchKeepsAmmoAndCards(int ammo)
    {
        var weapon = Weapon(Card("Shot", true, 1));
        SetField(weapon, "maxAmmo", 3);
        _state.ChangePlayerWeapon(weapon);
        var shot = _state.CreateCard(weapon.ShootingCard);
        _state.Hand.Add(shot);
        _state.Ammo = ammo;
        new ReloadAmmoEffect().Execute(new CardContext { State = _state });
        Assert.That(_state.Ammo, Is.EqualTo(3));
        Assert.That(_state.MaxAmmo, Is.EqualTo(3));
        _state.Ammo = ammo;
        Assert.That(_state.ChangePlayerWeapon(weapon), Is.False);
        Assert.That(_state.Ammo, Is.EqualTo(ammo)); // 같은 무기로 전환해도 재장전하지 않는다
        Assert.That(_state.Hand[0], Is.SameAs(shot));
    }

    [Test]
    public void ReloadWithoutWeaponDoesNotEraseAmmoAndGainIsCappedByMagazine()
    {
        var effect = new ReloadAmmoEffect();
        Assert.DoesNotThrow(() => effect.Execute(null));
        Assert.DoesNotThrow(() => effect.Execute(new CardContext()));
        effect.Execute(new CardContext { State = _state });
        Assert.That(_state.Ammo, Is.EqualTo(7));
        var weapon = Weapon(Card("Shot", false, 1));
        SetField(weapon, "maxAmmo", 3);
        _state.ChangePlayerWeapon(weapon);
        // 총기를 들면 탄약 획득은 최대 탄약에서 멈춘다 (넘친 만큼은 버림)
        _state.Ammo = 1;
        new AddAmmoEffect { amount = 1 }.Execute(new CardContext { State = _state });
        Assert.That(_state.Ammo, Is.EqualTo(2));
        new AddAmmoEffect { amount = 5 }.Execute(new CardContext { State = _state });
        Assert.That(_state.Ammo, Is.EqualTo(3));
        effect.Execute(new CardContext { State = _state });
        Assert.That(_state.Ammo, Is.EqualTo(3));
    }

    [TestCase("권총", 3)]
    [TestCase("스나이퍼", 1)]
    public void ReloadCardAssetsUseEquippedWeaponMaximumInBothUpgradeStates(string weaponName, int maximum)
    {
        var weapon = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>($"Assets/Data/Weapons/{weaponName}.asset");
        Assert.That(weapon, Is.Not.Null);
        Assert.That(weapon.MaxAmmo, Is.EqualTo(maximum));
        _state.ChangePlayerWeapon(weapon);
        foreach (var path in new[] { "기본/재장전", "일반/스킬/긴급 보급" })
        {
            var template = UnityEditor.AssetDatabase.LoadAssetAtPath<CardData>($"Assets/Data/Cards/Player/{path}.asset");
            Assert.That(template, Is.Not.Null);
            var card = _state.CreateCard(template);
            foreach (bool upgraded in new[] { false, true })
            {
                card.isUpgraded = upgraded;
                Assert.That(card.ActiveEffects.Count, Is.EqualTo(1));
                Assert.That(card.ActiveEffects[0], Is.TypeOf<ReloadAmmoEffect>());
                _state.Ammo = 9;
                EffectRunner.ExecuteImmediate(card.ActiveEffects, new CardContext { State = _state, Card = card });
                Assert.That(_state.Ammo, Is.EqualTo(maximum));
                Assert.That(card.GetFullDescription(), Does.Not.Contain("{0.amount}"));
            }
        }
    }

    private WeaponData Weapon(CardData shot)
    {
        var weapon = ScriptableObject.CreateInstance<WeaponData>();
        _assets.Add(weapon);
        SetField(weapon, "shootingCard", shot);
        SetField(weapon, "maxAmmo", 7);
        SetField(weapon, "usesAmmoBoundary", true);
        return weapon;
    }

    private static void SetField(object target, string name, object value)
        => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    [TestCase(0, false)]
    [TestCase(9, false)]
    [TestCase(0, true)]
    [TestCase(9, true)]
    public void SwitchingShotAssetSetsAmmoThenDealsThreeHitsThenGainsTwo(int ammo, bool upgraded)
    {
        var template = UnityEditor.AssetDatabase.LoadAssetAtPath<CardData>("Assets/Data/Cards/Player/테스트/전환사격.asset");
        Assert.That(template, Is.Not.Null);
        var card = _state.CreateCard(template);
        card.isUpgraded = upgraded;
        var target = new TestTarget();
        var context = new CardContext { State = _state, Card = card, PrimaryTargetOverride = target };
        // 다른 무기에서 권총으로 전환하면 이전 탄약 수에 관계없이 세 발을 쏜다.
        var weapon = ((ChangeWeaponEffect)card.ActiveEffects[0]).weapon;
        _state.ChangePlayerWeapon(Weapon(Card("Other", false, 1)));
        _state.Ammo = ammo;
        target.OnHit = () => Assert.That(_state.Ammo, Is.Zero);
        RunSequence(EffectRunner.ExecuteSequence(card.ActiveEffects, context));
        Assert.That(target.Hits, Is.EqualTo(new[] { 2, 2, 2 }));
        Assert.That(_state.CurrentWeapon, Is.SameAs(weapon));
        Assert.That(_state.Ammo, Is.EqualTo(2));
        Assert.That(_state.Player.Passives, Is.Empty);
        Assert.That(card.AmmoCost, Is.Zero);
    }

    [TestCase(0)]
    [TestCase(9)]
    public void RetreatAssetAppliesWeakBeforeSwitchingAndSetsAmmoToOne(int ammo)
    {
        var card = UnityEditor.AssetDatabase.LoadAssetAtPath<CardData>("Assets/Data/Cards/Player/테스트/빠른 후퇴.asset");
        Assert.That(card, Is.Not.Null);
        var target = new TestTarget();
        _state.Ammo = ammo;
        var context = new CardContext { State = _state, Card = card, PrimaryTargetOverride = target };
        card.ActiveEffects[0].Execute(context);
        var weak = UnityEditor.AssetDatabase.LoadAssetAtPath<StatusDefinition>("Assets/Data/Status/Weak.asset");
        Assert.That(target.Statuses.GetMagnitude(weak), Is.EqualTo(1));
        Assert.That(_state.CurrentWeapon, Is.Null);
        Assert.That(_state.Ammo, Is.EqualTo(ammo));
        card.ActiveEffects[1].Execute(context);
        Assert.That(_state.CurrentWeapon.WeaponName, Is.EqualTo("스나이퍼"));
        Assert.That(_state.Ammo, Is.EqualTo(1));
        Assert.That(_state.Player.Passives, Is.Empty);
    }

    [Test]
    public void ExistingAmmoAttackStillCombinesDamageIntoOneHit()
    {
        _state.Ammo = 3;
        var target = new TestTarget();
        new AmmoConsumeAllDamageEffect { damagePerAmmo = 8 }.Execute(
            new CardContext { State = _state, PrimaryTargetOverride = target });
        Assert.That(target.Hits, Is.EqualTo(new[] { 24 }));
        Assert.That(_state.Ammo, Is.Zero);
    }

    private static void RunSequence(System.Collections.IEnumerator sequence)
    {
        while (sequence.MoveNext())
            if (sequence.Current is System.Collections.IEnumerator nested) RunSequence(nested);
    }

    private sealed class TestTarget : ICombatant
    {
        public int HP { get; private set; } = 100;
        public int MaxHP => 100;
        public int Block => 0;
        public bool IsDead => HP <= 0;
        public List<IPassiveLogic> Passives { get; } = new();
        public StatusContainer Statuses { get; }
        public List<int> Hits { get; } = new();
        public System.Action OnHit;
        public TestTarget() => Statuses = new StatusContainer(Passives);
        public void TakeDamage(DamageInfo info) { Hits.Add(info.Amount); HP -= info.Amount; OnHit?.Invoke(); }
        public void AddBlock(int amount) { }
        public void Heal(int amount) => HP += amount;
        public void AddPassive(IPassiveLogic passive) => Statuses.Add(passive);
        public void ResetBlock() { }
        public void RemoveExpiredPassives() => Statuses.RemoveExpired();
    }
}
