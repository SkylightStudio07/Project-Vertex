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
        Assert.That(_state.ChangePlayerWeapon(second), Is.False);
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
        effect.Execute(context);
        Assert.That(_state.Ammo, Is.EqualTo(9));
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

    private WeaponData Weapon(CardData shot)
    {
        var weapon = ScriptableObject.CreateInstance<WeaponData>();
        _assets.Add(weapon);
        SetField(weapon, "shootingCard", shot);
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
        // 같은 권총으로 다시 전환해도 이전 탄약 수에 관계없이 세 발을 쏜다.
        var weapon = ((ChangeWeaponEffect)card.ActiveEffects[0]).weapon;
        _state.ChangePlayerWeapon(weapon);
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
