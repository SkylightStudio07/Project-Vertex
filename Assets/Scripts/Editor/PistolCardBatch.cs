using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// 한 번 생성한 에셋은 덮어쓰지 않는다. 이후 수치는 Card Authoring에서 편집한다.
public static class PistolCardBatch
{
    public static readonly string[] Names = {
        "첫발의 기선", "견제 초탄", "마지막 한 발", "쏘고 숨기", "남김없이", "탄창 숙련",
        "이동 사격", "엄폐 사격", "사선 이탈", "엄폐 재장전", "엄폐 전환", "사격 후 이동",
        "장전 점검", "빈 탄창 교체", "발포 준비", "권총 속사 준비", "권총으로 후퇴", "장전의 여유", "확장 탄창", "마침표"
    };
    public static readonly string[] Kinds = { "공격","공격","공격","공격","공격","파워","공격","공격","스킬","스킬","스킬","파워","스킬","스킬","스킬","스킬","스킬","파워","스킬","공격" };
    public static string PathFor(int index) => $"Assets/Data/Cards/Player/일반/{Kinds[index]}/{Names[index]}.asset";
    private static void Field(object obj, string field, object value) => obj.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(obj, value);
    private static void Folder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        Folder(path.Substring(0, slash));
        AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
    }
    private static StatusDefinition Status(string id, string name, string description, StatusBehavior behavior)
    {
        string path = $"Assets/Data/Status/{id}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<StatusDefinition>(path);
        if (existing != null) return existing;
        var status = ScriptableObject.CreateInstance<StatusDefinition>();
        Field(status, "statusId", id); Field(status, "displayName", name); Field(status, "description", description);
        Field(status, "disposition", StatusDisposition.Buff);
        Field(status, "durationPolicy", StatusDurationPolicy.Permanent);
        Field(status, "behaviors", new List<StatusBehavior> { behavior });
        AssetDatabase.CreateAsset(status, path);
        return status;
    }
    private static ApplyStatusEffect Buff(StatusDefinition status, int value) => new() { status = status, amount = 1, potency = value, targets = EffectTargetSelector.Source };
    private static DamageEffect Damage(int amount) => new() { amount = amount };
    private static BlockEffect Block(int amount) => new() { amount = amount };
    private static ConditionalEffect If(CardCondition condition, params CardEffect[] effects) => new() { condition = condition, effectsWhenMet = new List<CardEffect>(effects) };
    private static DamageEffect Bonus(int amount, CardCondition condition, int bonus)
        => new() { amount = amount, conditionalBonuses = new List<ConditionalDamageBonus> { new() { condition = condition, amount = bonus } } };
    private static CardData Card(int i, int ammo, string description, params CardEffect[] effects)
    {
        string path = PathFor(i);
        var existing = AssetDatabase.LoadAssetAtPath<CardData>(path);
        if (existing != null) return existing;
        Folder(path.Substring(0, path.LastIndexOf('/')));
        var card = ScriptableObject.CreateInstance<CardData>();
        CardAuthoringUtility.Initialize(card);
        card.name = Names[i]; card.cardDescription = description;
        var type = Kinds[i] == "공격" ? CardData.CardType.Attack : Kinds[i] == "파워" ? CardData.CardType.Power : CardData.CardType.Skill;
        Field(card, "cardType", type); Field(card, "cardRarity", CardData.CardRarity.Common); Field(card, "cardOwner", CardData.CardOwner.Player);
        Field(card, "useMode", type == CardData.CardType.Attack ? CardData.CardUseMode.SelectEnemy : CardData.CardUseMode.DropToPlayArea);
        Field(card, "normalState", new CardUpgradeState { cardName = Names[i], energyCost = 1, ammoCost = ammo, isExhaust = i == 18, effects = new List<CardEffect>(effects) });
        CardAuthoringUtility.CopyNormalToUpgrade(card);
        AssetDatabase.CreateAsset(card, path);
        return card;
    }

    [MenuItem("Tools/Cards/Create Pistol Concept Cards")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("편집 모드에서 생성하세요.");
        var pistol = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/권총.asset");
        var pool = AssetDatabase.LoadAssetAtPath<PlayerRewardPoolSO>(CardAuthoringCatalog.DefaultPoolPath);
        if (pistol == null || pool == null) throw new InvalidOperationException("권총 또는 보상 풀 참조가 없습니다.");
        Folder("Assets/Data/Status");
        var mastery = Status("PistolBoundaryMastery", "탄창 숙련", "초탄과 막탄인 카드의 피해가 각각 증가합니다.", new AmmoBoundaryDamageStatusBehavior());
        var movement = Status("AmmoMovementBlock", "사선 이탈", "이번 턴 동안 사용한 탄약마다 방어도를 얻습니다.", new AmmoSpentBlockStatusBehavior { perAmmo = true, expiresAtTurnEnd = true });
        var firstBlock = Status("FirstAmmoBlock", "사격 후 이동", "재장전 후 처음 탄약을 사용하는 카드 사용 시 방어도를 얻습니다.", new AmmoSpentBlockStatusBehavior { firstOnly = true });
        var nextDamage = Status("NextAmmoDamage", "발포 준비", "다음 탄약을 사용하는 카드의 피해가 증가합니다.", new NextAmmoDamageStatusBehavior());
        var discount = Status("NextAmmoDiscount", "권총 속사 준비", "이번 턴 다음 탄약을 사용하는 카드의 에너지 비용이 감소합니다.", new NextAmmoDiscountStatusBehavior());
        var reloadBlock = Status("ReloadBlock", "장전의 여유", "재장전할 때마다 방어도를 얻습니다.", new OnReloadEffectsStatusBehavior { effects = new List<CardEffect> { new BlockEffect { scaling = new EffectValue { source = EffectValueSource.TriggerStatusPotency, multiplier = 1 } } } });
        var cards = new List<CardData> {
            Card(0,1,"피해를 7 줍니다. 초탄이면 피해가 4 증가합니다.", Bonus(7,new FirstAmmoUseCondition(),4)),
            Card(1,1,"피해를 6 줍니다. 초탄이면 방어도를 7 얻습니다.",Damage(6),If(new FirstAmmoUseCondition(),Block(7))),
            Card(2,1,"피해를 8 줍니다. 막탄이면 피해가 10 증가합니다.",Bonus(8,new LastAmmoUseCondition(),10)),
            Card(3,1,"피해를 7 줍니다. 막탄이면 방어도를 8 얻습니다.",Damage(7),If(new LastAmmoUseCondition(),Block(8))),
            Card(4,2,"피해를 5씩 2번 줍니다. 막탄이면 피해를 5 한 번 더 줍니다.",new DamageEffect { amount=5,hitCount=2 },If(new LastAmmoUseCondition(),Damage(5))),
            Card(5,0,"이번 전투 동안 초탄인 카드와 막탄인 카드의 피해가 각각 4 증가합니다.",Buff(mastery,4)),
            Card(6,1,"피해를 8 줍니다. 이번 턴에 방어도를 얻었다면 피해가 5 증가합니다.",Bonus(8,new PlayerGainedBlockCondition(),5)),
            Card(7,1,"방어도를 5 얻고 피해를 7 줍니다.",Block(5),Damage(7)),
            Card(8,0,"이번 턴 동안 탄약을 사용하면 사용한 탄약 1개당 방어도를 5 얻습니다.",Buff(movement,5)),
            Card(9,0,"방어도를 6 얻고 재장전합니다.",Block(6),new ReloadAmmoEffect()),
            Card(10,0,"방어도를 7 얻고 권총으로 전환합니다.",Block(7),new ChangeWeaponEffect { weapon=pistol }),
            Card(11,0,"이번 전투 동안 재장전 후 처음 탄약을 사용하는 카드를 사용할 때마다 방어도를 5 얻습니다.",Buff(firstBlock,5)),
            Card(12,0,"재장전하고 카드를 1장 뽑습니다.",new ReloadAmmoEffect(),new DrawEffect { count=1 }),
            Card(13,0,"재장전합니다. 재장전 직전 탄약이 0이었다면 카드를 2장 뽑습니다.",new ReloadAmmoEffect { effectsWhenEmpty=new List<CardEffect> { new DrawEffect { count=2 } } }),
            Card(14,0,"재장전합니다. 다음 탄약을 사용하는 카드의 피해가 6 증가합니다.",new ReloadAmmoEffect(),Buff(nextDamage,6)),
            Card(15,0,"권총으로 전환합니다. 이번 턴 다음 탄약을 사용하는 카드의 에너지 비용이 1 감소합니다.",new ChangeWeaponEffect { weapon=pistol },Buff(discount,1)),
            Card(16,0,"권총으로 전환합니다. 방어도를 5 얻고 카드를 1장 뽑습니다.",new ChangeWeaponEffect { weapon=pistol },Block(5),new DrawEffect { count=1 }),
            Card(17,0,"이번 전투 동안 재장전할 때마다 방어도를 4 얻습니다.",Buff(reloadBlock,4)),
            Card(18,0,"소멸. 이번 전투 동안 권총의 최대 탄약이 1 증가합니다. 현재 권총을 들고 있다면 재장전합니다.",new IncreaseMagazineEffect { weapon=pistol,amount=1 }),
            Card(19,1,"피해를 9 줍니다. 이 피해로 적을 처치하면 재장전합니다.",new DamageEffect { amount=9,onKillEffects=new List<CardEffect> { new ReloadAmmoEffect() } })
        };
        foreach (var card in cards)
            if (!pool.commonCards.Contains(card) && !pool.rareCards.Contains(card) && !pool.uniqueCards.Contains(card)) pool.commonCards.Add(card);
        EditorUtility.SetDirty(pool);
        AssetDatabase.SaveAssets();
        Debug.Log($"[PistolCardBatch] 카드 {cards.Count}장 생성/확인 및 보상 풀 등록 완료.");
    }
}
