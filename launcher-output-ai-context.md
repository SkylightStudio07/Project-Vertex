# Unity Code Graph AI Context

This file is generated from the local Unity Code Graph analysis. It is intended to be pasted into an AI coding assistant as compact project context.

## Instructions For AI

- Treat graph relationships and evidence references as extracted facts.
- Treat role estimates and likely flows as heuristics, not as semantic compiler results.
- Use this context to decide which files, types, and methods to inspect first.
- Ask for source files when evidence is insufficient instead of inventing implementation details.
- Preferred response language: Korean.

## Graph Summary

- Source: `H:\Unity\ProjectV\ProjectV`
- Quality: `standard`
- Types: 142
- Relationships: 534
- Methods: 416
- Method calls: 280
- Systems: 15
- Exported: 2026-09-22T05:25:31.561Z

## System Index

| System | Types | Internal | External | Entry Candidates |
| --- | ---: | ---: | ---: | ---: |
| Card System | 28 | 151 | 156 | 8 |
| Battle System | 28 | 72 | 189 | 8 |
| Action Event Pipeline | 27 | 8 | 147 | 8 |
| Map Generation | 14 | 101 | 28 | 8 |
| UI Layer | 7 | 19 | 23 | 8 |
| Passive | 5 | 3 | 76 | 0 |
| Dialogue | 4 | 4 | 19 | 3 |
| Player / Input | 4 | 3 | 26 | 6 |
| Char | 3 | 2 | 37 | 0 |
| Holy | 3 | 2 | 0 | 1 |
| Rng | 2 | 3 | 4 | 0 |
| Cooperation | 1 | 0 | 29 | 1 |
| Cooperator | 1 | 0 | 8 | 2 |
| Game | 1 | 0 | 37 | 5 |
| Inventory | 1 | 0 | 8 | 1 |

## System: Card System

Anchor: `systems/card-system.md`

### Role Estimate

Card System appears to be an externally connected area around card, reward, gameplay, effect, hand. It contains 30 types, including 14 Unity-facing types.

### Stats

- Types: 28
- Internal relationships: 151
- External relationships: 156
- Entry candidates: 8
- Keywords: `card`, `reward`, `gameplay`, `effect`, `hand`, `pile`, `rarity`, `target`, `add`, `arrow`, `battle`, `button`

### Start Here

- `BattleReward.GenerateReward(Dictionary<CardRarity, List<CardData>>, RewardProbabilityData)` - flow_candidate / Reward/BattleReward.cs:53
- `BattleReward.GenerateItemReward(RewardProbabilityData)` - flow_candidate / Reward/BattleReward.cs:113
- `BattleReward.GenerateCardReward(Dictionary<CardRarity, List<CardData>>, RewardProbabilityData)` - flow_candidate / Reward/BattleReward.cs:70
- `DeckManager.Awake()` - unity_lifecycle / Gameplay/DeckManager.cs:11
- `HandView.Start()` - unity_lifecycle / Card/HandView.cs:17
- `CardInteractionView.OnDestroy()` - unity_lifecycle / Card/CardInteractionView.cs:187
- `RewardsView.Start()` - unity_lifecycle / Reward/RewardsView.cs:15
- `CardReward.OnEnable()` - unity_lifecycle / Reward/CardReward.cs:21

### Core Types

- `CardData` - class / Unity / 16 out / 88 in
- `BattleReward` - class / 48 out / 6 in
- `CardContext` - class / 9 out / 31 in
- `CardEffect` - class / 5 out / 26 in
- `CardHandler` - class / Unity / 27 out / 0 in
- `CardInteractionView` - class / Unity / 5 out / 21 in
- `CardData+CardRarity` - enum / 0 out / 17 in
- `RewardItemButton` - class / Unity / 6 out / 10 in

### Likely Method Flows

- `BattleReward.GenerateReward(Dictionary<CardRarity, List<CardData>>, RewardProbabilityData)`
  - `BattleReward.GenerateReward(Dictionary<CardRarity, List<CardData>>, RewardProbabilityData)`
  - `BattleReward.GenerateGoldReward(BattleType) / terminal`
- `BattleReward.GenerateItemReward(RewardProbabilityData)`
  - `BattleReward.GenerateItemReward(RewardProbabilityData)`
  - `BattleReward.IsItemRewardGiven(RewardProbabilityData) / terminal`
- `BattleReward.GenerateCardReward(Dictionary<CardRarity, List<CardData>>, RewardProbabilityData)`
  - `BattleReward.GenerateCardReward(Dictionary<CardRarity, List<CardData>>, RewardProbabilityData)`
  - `BattleReward.GetCardRarity(RewardProbabilityData) / terminal`
- `DeckManager.Awake()`
  - `DeckManager.Awake()`
  - `DeckManager.InitializeStartingDeck() / terminal`

### Internal Type Relationships

- `CardHandler` -> `CardInteractionView` - calls member / 11 refs
  - Evidence: `Card/CardHandler.cs:77 / interactionView.MoveVisualCenterToPointer(eventData)`
- `BattleReward` -> `RewardProbabilityData` - accepts parameter / 7 refs
  - Evidence: `Reward/BattleReward.cs:46 / RewardProbabilityData`
- `DeckManager` -> `CardData` - calls member / 6 refs
  - Evidence: `Gameplay/DeckManager.cs:48 / PlayerDeck.Add(Instantiate(strikeCard))`
- `DeckManager` -> `CardData` - has field type / 5 refs
  - Evidence: `Gameplay/DeckManager.cs:31 / CardData`
- `BattleReward` -> `CardData` - accepts parameter / 4 refs
  - Evidence: `Reward/BattleReward.cs:46 / Dictionary<CardRarity, List<CardData>>`
- `BattleReward` -> `CardData` - calls member / 4 refs
  - Evidence: `Reward/BattleReward.cs:72 / cardRewards.Clear()`
- `BattleReward` -> `RewardItem` - creates / 4 refs
  - Evidence: `Reward/BattleReward.cs:227 / List<RewardItem>`
- `BattleReward` -> `CardData+CardRarity` - accepts parameter / 3 refs
  - Evidence: `Reward/BattleReward.cs:46 / Dictionary<CardRarity, List<CardData>>`
- `CardData` -> `CardEffect` - has property type / 3 refs
  - Evidence: `Card/CardData.cs:95 / List<CardEffect>`
- `CardHandler` -> `CardHandler+CardState` - accepts parameter / 3 refs
  - Evidence: `Card/CardHandler.cs:151 / CardState`
- `PlayerRewardPoolSO` -> `CardData` - has field type / 3 refs
  - Evidence: `Gameplay/PlayerRewardPoolSO.cs:11 / List<CardData>`
- `BattleReward` -> `RewardItem` - calls member / 3 refs
  - Evidence: `Reward/BattleReward.cs:230 / items.Add(new RewardItem(RewardType.Gold, goldReward))`
- `CardInteractionView` -> `TargetArrow` - calls member / 3 refs
  - Evidence: `Card/CardInteractionView.cs:331 / debugTargetArrow.Hide()`
- `HandView` -> `CardInteractionView` - calls member / 3 refs
  - Evidence: `Card/HandView.cs:47 / interactionView.SetRestingSortingOrder(i)`
- `RewardsView` -> `RewardItemButton` - calls member / 3 refs
  - Evidence: `Reward/RewardsView.cs:44 / buttons.Contains(destroyedButton)`
- `BattleReward` -> `CardData` - uses local type / 2 refs
  - Evidence: `Reward/BattleReward.cs:79 / CardData`

### External Touchpoints

- `BattleManager` -> `CardData` - incoming / accepts parameter / 10 refs
  - Evidence: `Gameplay/BattleManager.cs:79 / List<CardData>`
- `GameManager` -> `CardData` - incoming / creates / 9 refs
  - Evidence: `Gameplay/GameManager.cs:37 / Dictionary<CardData.CardRarity, List<CardData>>`
- `CoopCharData` -> `CardData` - incoming / has field type / 5 refs
  - Evidence: `Cooperator/CoopCharData.cs:8 / List<CardData>`
- `BattleState` -> `CardData` - incoming / has field type / 4 refs
  - Evidence: `Gameplay/BattleState.cs:19 / List<CardData>`
- `GameManager` -> `CardData` - incoming / calls member / 4 refs
  - Evidence: `Gameplay/GameManager.cs:131 / pool.Contains(card)`
- `CardContext` -> `EnemyInstance` - outgoing / has field type / 3 refs
  - Evidence: `Card/CardContext.cs:22 / EnemyInstance`
- `DamageEffect` -> `CardContext` - incoming / accepts parameter / 3 refs
  - Evidence: `Effects/DamageEffect.cs:14 / CardContext`
- `ItemTargetingController` -> `TargetArrow` - incoming / calls member / 3 refs
  - Evidence: `UI/ItemTargetingController.cs:52 / arrow.Hide()`
- `AmmoConsumeAllDamageEffect` -> `CardContext` - incoming / accepts parameter / 2 refs
  - Evidence: `Effects/AmmoConsumeAllDamageEffect.cs:10 / CardContext`
- `BattleReward` -> `BattleType` - outgoing / accepts parameter / 2 refs
  - Evidence: `Reward/BattleReward.cs:92 / BattleType`
- `BattleReward` -> `ItemData` - outgoing / has field type / 2 refs
  - Evidence: `Reward/BattleReward.cs:34 / List<ItemData>`
- `GameManager` -> `CardData` - incoming / accepts parameter / 2 refs
  - Evidence: `Gameplay/GameManager.cs:123 / CardData`
- `GameManager` -> `CardData+CardRarity` - incoming / accepts parameter / 2 refs
  - Evidence: `Gameplay/GameManager.cs:136 / CardData.CardRarity`
- `GameManager` -> `CardData+CardRarity` - incoming / has field type / 2 refs
  - Evidence: `Gameplay/GameManager.cs:37 / Dictionary<CardData.CardRarity, List<CardData>>`
- `PlayerCharData` -> `CardData` - incoming / has field type / 2 refs
  - Evidence: `Player/PlayerCharData.cs:14 / List<CardData>`
- `CardHandler` -> `EnemyTargeting` - outgoing / calls member / 2 refs
  - Evidence: `Card/CardHandler.cs:89 / EnemyTargeting.TryGetUnderPointer(eventData, out EnemyInstance hovered)`

### Internal Method Calls

- `TargetArrow.OnPopulateMesh(VertexHelper)` -> `TargetArrow.AddVertex(VertexHelper, Vector2)` / 7 refs
  - Evidence: `Card/TargetArrow.cs:48 / AddVertex(vertexHelper, startPoint + perpendicular * shaftWidth * 0.5f)`
- `CardInteractionView.TargetingMoveCoroutine()` -> `CardInteractionView.TryGetTargetingWorldPosition(Vector3)` / 3 refs
  - Evidence: `Card/CardInteractionView.cs:196 / TryGetTargetingWorldPosition(out _)`
- `BattleReward.GenerateReward(Dictionary<CardRarity, List<CardData>>, RewardProbabilityData)` -> `BattleReward.GenerateGoldReward(BattleType)` / 2 refs
  - Evidence: `Reward/BattleReward.cs:59 / GenerateGoldReward(BattleType.Normal)`
- `CardHandler.OnDrag(PointerEventData)` -> `CardHandler.SetState(CardState)` / 2 refs
  - Evidence: `Card/CardHandler.cs:98 / SetState(CardState.Dragging)`
- `CardHandler.OnDrag(PointerEventData)` -> `CardInteractionView.MoveVisualCenterToPointer(PointerEventData)` / 2 refs
  - Evidence: `Card/CardHandler.cs:99 / interactionView.MoveVisualCenterToPointer(eventData)`
- `CardHandler.OnEndDrag(PointerEventData)` -> `CardHandler.SetState(CardState)` / 2 refs
  - Evidence: `Card/CardHandler.cs:122 / SetState(CardState.Returning)`
- `CardInteractionView.TargetingMoveCoroutine()` -> `CardInteractionView.UpdateDebugTargetArrow(Vector2)` / 2 refs
  - Evidence: `Card/CardInteractionView.cs:216 / UpdateDebugTargetArrow(targetingPointerPosition)`
- `DeckManager.Awake()` -> `DeckManager.InitializeStartingDeck()` / 1 refs
  - Evidence: `Gameplay/DeckManager.cs:27 / InitializeStartingDeck()`
- `CardRewardView.Open(List<CardData>, RewardItemButton)` -> `CardRewardView.Refresh(List<CardData>)` / 1 refs
  - Evidence: `Reward/CardRewardView.cs:16 / Refresh(cardRewardList)`
- `HandView.Start()` -> `HandView.Refresh()` / 1 refs
  - Evidence: `Card/HandView.cs:20 / Refresh()`
- `RewardsView.Open(BattleReward)` -> `RewardsView.Refresh()` / 1 refs
  - Evidence: `Reward/RewardsView.cs:28 / Refresh()`
- `RewardItemButton.OnClick()` -> `RewardItemButton.DisplayCardReward()` / 1 refs
  - Evidence: `Reward/RewardItemButton.cs:31 / DisplayCardReward()`
- `RewardItemButton.OnClick()` -> `RewardItemButton.GetItemReward()` / 1 refs
  - Evidence: `Reward/RewardItemButton.cs:34 / GetItemReward()`
- `RewardItemButton.OnClick()` -> `RewardItemButton.CompleteReward()` / 1 refs
  - Evidence: `Reward/RewardItemButton.cs:28 / CompleteReward()`
- `CardView.SetCard(CardData)` -> `CardView.RefreshDescription(EnemyInstance)` / 1 refs
  - Evidence: `Card/CardView.cs:32 / RefreshDescription()`
- `CardEffect.ExecuteCoroutine(CardContext)` -> `CardEffect.Execute(CardContext)` / 1 refs
  - Evidence: `Card/CardEffect.cs:30 / Execute(context)`

### Evidence

- Likely flow - BattleReward.GenerateReward(Dictionary<CardRarity, List<CardData>>, RewardProbabilityData) -> BattleReward.GenerateGoldReward(BattleType) / terminal
- Likely flow - BattleReward.GenerateItemReward(RewardProbabilityData) -> BattleReward.IsItemRewardGiven(RewardProbabilityData) / terminal
- Internal call - TargetArrow.OnPopulateMesh(VertexHelper) -> TargetArrow.AddVertex(VertexHelper, Vector2)
  - `Card/TargetArrow.cs:48 / AddVertex(vertexHelper, startPoint + perpendicular * shaftWidth * 0.5f)`
- Internal call - CardInteractionView.TargetingMoveCoroutine() -> CardInteractionView.TryGetTargetingWorldPosition(Vector3)
  - `Card/CardInteractionView.cs:196 / TryGetTargetingWorldPosition(out _)`
- Internal call - BattleReward.GenerateReward(Dictionary<CardRarity, List<CardData>>, RewardProbabilityData) -> BattleReward.GenerateGoldReward(BattleType)
  - `Reward/BattleReward.cs:59 / GenerateGoldReward(BattleType.Normal)`
- Incoming accepts parameter - BattleManager -> CardData / 10 refs
  - `Gameplay/BattleManager.cs:79 / List<CardData>`
- Incoming creates - GameManager -> CardData / 9 refs
  - `Gameplay/GameManager.cs:37 / Dictionary<CardData.CardRarity, List<CardData>>`
- Incoming has field type - CoopCharData -> CardData / 5 refs
  - `Cooperator/CoopCharData.cs:8 / List<CardData>`

### Suggested AI Task

Use the Card System context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: Battle System

Anchor: `systems/battle-system.md`

### Role Estimate

Battle System appears to be an externally connected area around enemy, passive, effects, status, gameplay. It contains 29 types, including 7 Unity-facing types.

### Stats

- Types: 28
- Internal relationships: 72
- External relationships: 189
- Entry candidates: 8
- Keywords: `enemy`, `passive`, `effects`, `status`, `gameplay`, `battle`, `ammo`, `damage`, `card`, `turn`, `effect`, `intent`

### Start Here

- `BattleManager.StartBattle(List<EnemyData>, List<CardData>, int, BattleType)` - flow_candidate / Gameplay/BattleManager.cs:79
- `EnemyZoneView.Start()` - unity_lifecycle / Enemy/EnemyZoneView.cs:14
- `EnemyView.OnDestroy()` - unity_lifecycle / Enemy/EnemyView.cs:114
- `BattleManager.Update()` - unity_lifecycle / Gameplay/BattleManager.cs:529
- `BattleManager.SetupBattleDeck(List<CardData>)` - flow_candidate / Gameplay/BattleManager.cs:121
- `EnemyView.SpawnHitEffect()` - flow_candidate / Enemy/EnemyView.cs:148
- `DebugBattleStarter.Start()` - unity_lifecycle / Develop/DebugBattleStarter.cs:10
- `BattleManager.Awake()` - unity_lifecycle / Gameplay/BattleManager.cs:13

### Core Types

- `BattleState` - class / 7 out / 49 in
- `DamageInfo` - struct / 2 out / 53 in
- `EnemyInstance` - class / 22 out / 30 in
- `BattleManager` - class / Unity / 38 out / 3 in
- `DamageCalculator` - class / 5 out / 8 in
- `EnemyData` - class / Unity / 1 out / 11 in
- `AmmoConsumeAllDamageEffect` - class / 11 out / 0 in
- `EnemyView` - class / Unity / 8 out / 3 in

### Likely Method Flows

- `BattleManager.StartBattle(List<EnemyData>, List<CardData>, int, BattleType)`
  - `BattleManager.StartBattle(List<EnemyData>, List<CardData>, int, BattleType)`
  - `BattleManager.SetupEnemies(List<EnemyData>) / terminal`
- `EnemyZoneView.Start()`
  - `EnemyZoneView.Start()`
  - `EnemyZoneView.Refresh() / terminal`
- `EnemyView.OnDestroy()`
  - `EnemyView.OnDestroy()`
  - `EnemyView.Unbind() / terminal`
- `BattleManager.Update()`
  - `BattleManager.Update()`
  - `BattleManager.TestVictory(BattleType)`
  - `BattleManager.Victory()`
  - `BattleManager.NotifyHandChanged() / terminal`

### Internal Type Relationships

- `EnemyInstance` -> `BattleState` - accepts parameter / 3 refs
  - Evidence: `Enemy/EnemyInstance.cs:96 / BattleState`
- `BattleManager` -> `EnemyInstance` - creates / 3 refs
  - Evidence: `Gameplay/BattleManager.cs:50 / List<EnemyInstance>`
- `BattleManager` -> `BattleType` - accepts parameter / 2 refs
  - Evidence: `Gameplay/BattleManager.cs:79 / BattleType`
- `BattleManager` -> `EnemyData` - accepts parameter / 2 refs
  - Evidence: `Gameplay/BattleManager.cs:79 / List<EnemyData>`
- `BattleManager` -> `EnemyInstance` - accepts parameter / 2 refs
  - Evidence: `Gameplay/BattleManager.cs:261 / EnemyInstance`
- `BattleManager` -> `EnemyTurnBannerView` - has field type / 2 refs
  - Evidence: `Gameplay/BattleManager.cs:65 / EnemyTurnBannerView`
- `DamageNullifiedStatus` -> `BattleState` - accepts parameter / 2 refs
  - Evidence: `Effects/DamageNullifiedStatus.cs:11 / BattleState`
- `DamageNullifiedStatus` -> `DamageInfo` - accepts parameter / 2 refs
  - Evidence: `Effects/DamageNullifiedStatus.cs:11 / DamageInfo`
- `DamageNullifiedStatus` -> `DamageInfo` - returns / 2 refs
  - Evidence: `Effects/DamageNullifiedStatus.cs:11 / DamageInfo`
- `EnemyInstance` -> `BattleManager` - accepts parameter / 2 refs
  - Evidence: `Enemy/EnemyInstance.cs:163 / BattleManager`
- `AmmoConsumeAllDamageEffect` -> `DamageCalculator` - calls member / 2 refs
  - Evidence: `Effects/AmmoConsumeAllDamageEffect.cs:27 / DamageCalculator.Resolve(new DamageInfo(totalDamage, attacker, false, true), target, ctx.State)`
- `BattleManager` -> `EnemyTurnBannerView` - calls member / 2 refs
  - Evidence: `Gameplay/BattleManager.cs:149 / playerTurnBanner.ShowAndWait()`
- `EnemyView` -> `EnemyInstance` - calls member / 2 refs
  - Evidence: `Enemy/EnemyView.cs:169 / Instance.GetCurrentAction()`
- `AmmoConsumeAllDamageEffect` -> `DamageInfo` - creates / 2 refs
  - Evidence: `Effects/AmmoConsumeAllDamageEffect.cs:27 / DamageInfo`
- `AmmoAttackBonusPassive` -> `BattleState` - accepts parameter / 1 refs
  - Evidence: `Powers/AmmoAttackBonusPassive.cs:10 / BattleState`
- `AmmoAttackBonusPassive` -> `DamageInfo` - accepts parameter / 1 refs
  - Evidence: `Powers/AmmoAttackBonusPassive.cs:10 / DamageInfo`

### External Touchpoints

- `BattleManager` -> `CardData` - outgoing / accepts parameter / 10 refs
  - Evidence: `Gameplay/BattleManager.cs:79 / List<CardData>`
- `IPassiveLogic` -> `BattleState` - incoming / accepts parameter / 7 refs
  - Evidence: `Passive/IPassiveLogic.cs:3 / BattleState`
- `PowerPassiveBase` -> `BattleState` - incoming / accepts parameter / 7 refs
  - Evidence: `Passive/PowerPassiveBase.cs:6 / BattleState`
- `StatusEffectBase` -> `BattleState` - incoming / accepts parameter / 7 refs
  - Evidence: `Passive/StatusEffectBase.cs:29 / BattleState`
- `DamageEffect` -> `DamageInfo` - incoming / creates / 7 refs
  - Evidence: `Effects/DamageEffect.cs:27 / DamageInfo`
- `DamageEffect` -> `DamageCalculator` - incoming / calls member / 6 refs
  - Evidence: `Effects/DamageEffect.cs:27 / DamageCalculator.Resolve(new DamageInfo(amount, attacker, false, isAmmoAttack), single, context.State)`
- `BattleState` -> `CardData` - outgoing / has field type / 4 refs
  - Evidence: `Gameplay/BattleState.cs:19 / List<CardData>`
- `IPassiveLogic` -> `DamageInfo` - incoming / accepts parameter / 4 refs
  - Evidence: `Passive/IPassiveLogic.cs:8 / DamageInfo`
- `IPassiveLogic` -> `DamageInfo` - incoming / returns / 4 refs
  - Evidence: `Passive/IPassiveLogic.cs:8 / DamageInfo`
- `PowerPassiveBase` -> `DamageInfo` - incoming / accepts parameter / 4 refs
  - Evidence: `Passive/PowerPassiveBase.cs:9 / DamageInfo`
- `PowerPassiveBase` -> `DamageInfo` - incoming / returns / 4 refs
  - Evidence: `Passive/PowerPassiveBase.cs:9 / DamageInfo`
- `StatusEffectBase` -> `DamageInfo` - incoming / accepts parameter / 4 refs
  - Evidence: `Passive/StatusEffectBase.cs:32 / DamageInfo`
- `StatusEffectBase` -> `DamageInfo` - incoming / returns / 4 refs
  - Evidence: `Passive/StatusEffectBase.cs:32 / DamageInfo`
- `CardContext` -> `EnemyInstance` - incoming / has field type / 3 refs
  - Evidence: `Card/CardContext.cs:22 / EnemyInstance`
- `EnemyInstance` -> `StatusEffectBase` - outgoing / type check / 3 refs
  - Evidence: `Enemy/EnemyInstance.cs:89 / StatusEffectBase`
- `MapConfig` -> `EnemyData` - incoming / has field type / 3 refs
  - Evidence: `Map/MapConfig.cs:54 / List<EnemyData>`

### Internal Method Calls

- `AmmoConsumeAllDamageEffect.Execute(CardContext)` -> `DamageCalculator.Resolve(DamageInfo, ICombatant, BattleState)` / 2 refs
  - Evidence: `Effects/AmmoConsumeAllDamageEffect.cs:27 / DamageCalculator.Resolve(new DamageInfo(totalDamage, attacker, false, true), target, ctx.State)`
- `AmmoConsumeAllDamageEffect.Execute(CardContext)` -> `AmmoConsumeAllDamageEffect.ResolveSingleTarget(CardContext)` / 1 refs
  - Evidence: `Effects/AmmoConsumeAllDamageEffect.cs:25 / ResolveSingleTarget(ctx)`
- `EnemyZoneView.Start()` -> `EnemyZoneView.Refresh()` / 1 refs
  - Evidence: `Enemy/EnemyZoneView.cs:17 / Refresh()`
- `BattleManager.StartBattle(List<EnemyData>, List<CardData>, int, BattleType)` -> `BattleManager.SetupEnemies(List<EnemyData>)` / 1 refs
  - Evidence: `Gameplay/BattleManager.cs:94 / SetupEnemies(enemyDataList)`
- `BattleManager.StartBattle(List<EnemyData>, List<CardData>, int, BattleType)` -> `BattleManager.SetupBattleDeck(List<CardData>)` / 1 refs
  - Evidence: `Gameplay/BattleManager.cs:95 / SetupBattleDeck(masterDeck)`
- `EnemyView.Bind(EnemyInstance)` -> `EnemyView.Unbind()` / 1 refs
  - Evidence: `Enemy/EnemyView.cs:92 / Unbind()`
- `EnemyView.Bind(EnemyInstance)` -> `EnemyView.RefreshHP()` / 1 refs
  - Evidence: `Enemy/EnemyView.cs:110 / RefreshHP()`
- `EnemyView.Bind(EnemyInstance)` -> `EnemyView.RefreshIntent()` / 1 refs
  - Evidence: `Enemy/EnemyView.cs:111 / RefreshIntent()`
- `EnemyInstance.TickPassives(BattleState)` -> `EnemyInstance.RemoveExpiredPassives()` / 1 refs
  - Evidence: `Enemy/EnemyInstance.cs:109 / RemoveExpiredPassives()`
- `EnemyView.OnDestroy()` -> `EnemyView.Unbind()` / 1 refs
  - Evidence: `Enemy/EnemyView.cs:114 / Unbind()`
- `BattleManager.SetupBattleDeck(List<CardData>)` -> `BattleManager.Shuffle(List<CardData>)` / 1 refs
  - Evidence: `Gameplay/BattleManager.cs:130 / Shuffle(_state.DrawPile)`
- `EnemyView.HandleDamaged(int)` -> `EnemyView.SpawnHitEffect()` / 1 refs
  - Evidence: `Enemy/EnemyView.cs:129 / SpawnHitEffect()`
- `EnemyView.HandleDamaged(int)` -> `EnemyView.RefreshHP()` / 1 refs
  - Evidence: `Enemy/EnemyView.cs:128 / RefreshHP()`
- `EnemyInstance.GetIntentDamageAmount()` -> `EnemyInstance.GetCurrentAction()` / 1 refs
  - Evidence: `Enemy/EnemyInstance.cs:140 / GetCurrentAction()`
- `BattleManager.PlayerTurnStart(bool)` -> `BattleManager.PlayerTurnStartSequence(bool)` / 1 refs
  - Evidence: `Gameplay/BattleManager.cs:143 / PlayerTurnStartSequence(showBanner)`
- `BattleManager.PlayerTurnStartSequence(bool)` -> `EnemyTurnBannerView.ShowAndWait()` / 1 refs
  - Evidence: `Gameplay/BattleManager.cs:149 / playerTurnBanner.ShowAndWait()`

### Evidence

- Likely flow - BattleManager.StartBattle(List<EnemyData>, List<CardData>, int, BattleType) -> BattleManager.SetupEnemies(List<EnemyData>) / terminal
- Likely flow - EnemyZoneView.Start() -> EnemyZoneView.Refresh() / terminal
- Internal call - AmmoConsumeAllDamageEffect.Execute(CardContext) -> DamageCalculator.Resolve(DamageInfo, ICombatant, BattleState)
  - `Effects/AmmoConsumeAllDamageEffect.cs:27 / DamageCalculator.Resolve(new DamageInfo(totalDamage, attacker, false, true), target, ctx.State)`
- Internal call - AmmoConsumeAllDamageEffect.Execute(CardContext) -> AmmoConsumeAllDamageEffect.ResolveSingleTarget(CardContext)
  - `Effects/AmmoConsumeAllDamageEffect.cs:25 / ResolveSingleTarget(ctx)`
- Internal call - EnemyZoneView.Start() -> EnemyZoneView.Refresh()
  - `Enemy/EnemyZoneView.cs:17 / Refresh()`
- Outgoing accepts parameter - BattleManager -> CardData / 10 refs
  - `Gameplay/BattleManager.cs:79 / List<CardData>`
- Incoming accepts parameter - IPassiveLogic -> BattleState / 7 refs
  - `Passive/IPassiveLogic.cs:3 / BattleState`
- Incoming accepts parameter - PowerPassiveBase -> BattleState / 7 refs
  - `Passive/PowerPassiveBase.cs:6 / BattleState`

### Suggested AI Task

Use the Battle System context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: Action Event Pipeline

Anchor: `systems/action-event-pipeline.md`

### Role Estimate

Action Event Pipeline appears to be an externally connected area around effect, card, effects, event, choice. It contains 27 types, including 4 Unity-facing types.

### Stats

- Types: 27
- Internal relationships: 8
- External relationships: 147
- Entry candidates: 8
- Keywords: `effect`, `card`, `effects`, `event`, `choice`, `power`, `add`, `ammo`, `block`, `cooperator`, `dialogue`, `hint`

### Start Here

- `EventView.BuildChoices()` - flow_candidate / Event/EventView.cs:105
- `CharacterEventManager.Update()` - unity_lifecycle / Cooperator/CharacterEventManager.cs:25
- `EffectTokenHintDrawer.BuildHint(SerializedProperty)` - flow_candidate / Editor/EffectTokenHintDrawer.cs:42
- `CharacterEventManager.Start()` - unity_lifecycle / Cooperator/CharacterEventManager.cs:14
- `ItemActionPopup.Awake()` - unity_lifecycle / UI/ItemActionPopup.cs:19
- `CharacterEventManager.OnDisable()` - unity_lifecycle / Cooperator/CharacterEventManager.cs:20
- `EventView.Awake()` - unity_lifecycle / Event/EventView.cs:37
- `HitEffectSpawner.Spawn(GameObject, Transform, float)` - flow_candidate / Gameplay/HitEffectSpawner.cs:10

### Core Types

- `StatusEffectBase` - class / 21 out / 21 in
- `DamageEffect` - class / 32 out / 1 in
- `ApplyStatusEffect` - class / 12 out / 0 in
- `EventView` - class / Unity / 8 out / 2 in
- `ItemActionPopup` - class / Unity / 4 out / 2 in
- `BlockEffect` - class / 5 out / 0 in
- `EffectTokenHintDrawer` - class / 4 out / 0 in
- `EventData` - class / Unity / 1 out / 3 in

### Likely Method Flows

- `EventView.BuildChoices()`
  - `EventView.BuildChoices()`
  - `EventView.HideChoices() / terminal`
- `CharacterEventManager.Update()`
  - `CharacterEventManager.Update() / terminal`
- `EffectTokenHintDrawer.BuildHint(SerializedProperty)`
  - `EffectTokenHintDrawer.BuildHint(SerializedProperty) / terminal`
- `CharacterEventManager.Start()`
  - `CharacterEventManager.Start() / terminal`

### Internal Type Relationships

- `ApplyStatusEffect` -> `StatusEffectBase` - returns / 1 refs
  - Evidence: `Effects/ApplyStatusEffect.cs:37 / StatusEffectBase`
- `EffectTokenHintDrawer` -> `EffectTokenHintAttribute` - attribute type argument / 1 refs
  - Evidence: `Editor/EffectTokenHintDrawer.cs:10 / CustomPropertyDrawer(typeof(EffectTokenHintAttribute))`
- `EffectTokenHintDrawer` -> `EffectTokenHintAttribute` - type check / 1 refs
  - Evidence: `Editor/EffectTokenHintDrawer.cs:44 / EffectTokenHintAttribute`
- `EventData` -> `EventChoiceEffect` - has field type / 1 refs
  - Evidence: `Event/EventData.cs:13 / List<EventChoiceEffect>`
- `EventJsonData` -> `EventChoiceJson` - has field type / 1 refs
  - Evidence: `Event/EventJsonData.cs:11 / EventChoiceJson[]`
- `EventView` -> `EventData` - accepts parameter / 1 refs
  - Evidence: `Event/EventView.cs:47 / EventData`
- `EventView` -> `EventData` - has field type / 1 refs
  - Evidence: `Event/EventView.cs:33 / EventData`
- `EventView` -> `EventJsonData` - has field type / 1 refs
  - Evidence: `Event/EventView.cs:34 / EventJsonData`

### External Touchpoints

- `StatusEffectBase` -> `BattleState` - outgoing / accepts parameter / 7 refs
  - Evidence: `Passive/StatusEffectBase.cs:29 / BattleState`
- `DamageEffect` -> `DamageInfo` - outgoing / creates / 7 refs
  - Evidence: `Effects/DamageEffect.cs:27 / DamageInfo`
- `DamageEffect` -> `DamageCalculator` - outgoing / calls member / 6 refs
  - Evidence: `Effects/DamageEffect.cs:27 / DamageCalculator.Resolve(new DamageInfo(amount, attacker, false, isAmmoAttack), single, context.State)`
- `DamageEffect` -> `ICombatant` - outgoing / uses local type / 4 refs
  - Evidence: `Effects/DamageEffect.cs:18 / ICombatant`
- `PlayerCombatant` -> `StatusEffectBase` - incoming / type check / 4 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:69 / StatusEffectBase`
- `StatusEffectBase` -> `DamageInfo` - outgoing / accepts parameter / 4 refs
  - Evidence: `Passive/StatusEffectBase.cs:32 / DamageInfo`
- `StatusEffectBase` -> `DamageInfo` - outgoing / returns / 4 refs
  - Evidence: `Passive/StatusEffectBase.cs:32 / DamageInfo`
- `StatusEffectBase` -> `ICombatant` - outgoing / accepts parameter / 4 refs
  - Evidence: `Passive/StatusEffectBase.cs:29 / ICombatant`
- `DamageEffect` -> `CardContext` - outgoing / accepts parameter / 3 refs
  - Evidence: `Effects/DamageEffect.cs:14 / CardContext`
- `EnemyInstance` -> `StatusEffectBase` - incoming / type check / 3 refs
  - Evidence: `Enemy/EnemyInstance.cs:89 / StatusEffectBase`
- `DamageEffect` -> `EnemyInstance` - outgoing / calls member / 3 refs
  - Evidence: `Effects/DamageEffect.cs:42 / alive.Add(e)`
- `DamageEffect` -> `EnemyInstance` - outgoing / creates / 3 refs
  - Evidence: `Effects/DamageEffect.cs:40 / List<EnemyInstance>`
- `EnemyInstance` -> `StatusEffectBase` - incoming / calls member / 2 refs
  - Evidence: `Enemy/EnemyInstance.cs:89 / existing.TryMerge(passive)`
- `PlayerCombatant` -> `StatusEffectBase` - incoming / calls member / 2 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:73 / existing.TryMerge(newStatus)`
- `PlayerHUDView` -> `HitEffectSpawner` - incoming / calls member / 2 refs
  - Evidence: `Gameplay/PlayerHUDView.cs:143 / HitEffectSpawner.Spawn(hitEffectPrefab, anchor)`
- `AddAmmoEffect` -> `CardContext` - outgoing / accepts parameter / 1 refs
  - Evidence: `Effects/AddAmmoEffect.cs:6 / CardContext`

### Internal Method Calls

- `ApplyStatusEffect.Execute(CardContext)` -> `ApplyStatusEffect.ApplyTo(EnemyInstance)` / 2 refs
  - Evidence: `Effects/ApplyStatusEffect.cs:16 / ApplyTo(context.Target)`
- `ItemActionPopup.OnUseClicked()` -> `ItemActionPopup.Close()` / 2 refs
  - Evidence: `UI/ItemActionPopup.cs:50 / Close()`
- `EventView.Open(EventData)` -> `EventView.ShowDescriptionAndChoices()` / 2 refs
  - Evidence: `Event/EventView.cs:78 / ShowDescriptionAndChoices()`
- `ApplyStatusEffect.Execute(CardContext)` -> `ApplyStatusEffect.CreatePassive()` / 1 refs
  - Evidence: `Effects/ApplyStatusEffect.cs:25 / CreatePassive()`
- `DamageEffect.Execute(CardContext)` -> `DamageEffect.ResolveSingleTarget(CardContext)` / 1 refs
  - Evidence: `Effects/DamageEffect.cs:24 / ResolveSingleTarget(context)`
- `EffectTokenHintDrawer.GetPropertyHeight(SerializedProperty, GUIContent)` -> `EffectTokenHintDrawer.BuildHint(SerializedProperty)` / 1 refs
  - Evidence: `Editor/EffectTokenHintDrawer.cs:18 / BuildHint(property)`
- `EffectTokenHintDrawer.OnGUI(Rect, SerializedProperty, GUIContent)` -> `EffectTokenHintDrawer.BuildHint(SerializedProperty)` / 1 refs
  - Evidence: `Editor/EffectTokenHintDrawer.cs:34 / BuildHint(property)`
- `ApplyStatusEffect.ApplyTo(EnemyInstance)` -> `ApplyStatusEffect.CreatePassive()` / 1 refs
  - Evidence: `Effects/ApplyStatusEffect.cs:33 / CreatePassive()`
- `StatusEffectBase.PreviewOutgoingDamage(DamageInfo, BattleState)` -> `StatusEffectBase.ModifyOutgoingDamage(DamageInfo, BattleState)` / 1 refs
  - Evidence: `Passive/StatusEffectBase.cs:39 / ModifyOutgoingDamage(info, state)`
- `StatusEffectBase.PreviewIncomingDamage(DamageInfo, BattleState)` -> `StatusEffectBase.ModifyIncomingDamage(DamageInfo, BattleState)` / 1 refs
  - Evidence: `Passive/StatusEffectBase.cs:41 / ModifyIncomingDamage(info, state)`
- `EventView.Open(EventData)` -> `EventView.HideChoices()` / 1 refs
  - Evidence: `Event/EventView.cs:69 / HideChoices()`
- `DamageEffect.ExecuteCoroutine(CardContext)` -> `DamageEffect.Execute(CardContext)` / 1 refs
  - Evidence: `Effects/DamageEffect.cs:61 / Execute(ctx)`
- `DamageEffect.ExecuteCoroutine(CardContext)` -> `DamageEffect.ResolveSingleTarget(CardContext)` / 1 refs
  - Evidence: `Effects/DamageEffect.cs:74 / ResolveSingleTarget(ctx)`
- `ItemActionPopup.OnDiscardClicked()` -> `ItemActionPopup.Close()` / 1 refs
  - Evidence: `UI/ItemActionPopup.cs:63 / Close()`
- `EventView.ShowDescriptionAndChoices()` -> `EventView.BuildChoices()` / 1 refs
  - Evidence: `Event/EventView.cs:95 / BuildChoices()`
- `EventView.BuildChoices()` -> `EventView.HideChoices()` / 1 refs
  - Evidence: `Event/EventView.cs:107 / HideChoices()`

### Evidence

- Likely flow - EventView.BuildChoices() -> EventView.HideChoices() / terminal
- Likely flow - CharacterEventManager.Update() -> terminal
- Internal call - ApplyStatusEffect.Execute(CardContext) -> ApplyStatusEffect.ApplyTo(EnemyInstance)
  - `Effects/ApplyStatusEffect.cs:16 / ApplyTo(context.Target)`
- Internal call - ItemActionPopup.OnUseClicked() -> ItemActionPopup.Close()
  - `UI/ItemActionPopup.cs:50 / Close()`
- Internal call - EventView.Open(EventData) -> EventView.ShowDescriptionAndChoices()
  - `Event/EventView.cs:78 / ShowDescriptionAndChoices()`
- Outgoing accepts parameter - StatusEffectBase -> BattleState / 7 refs
  - `Passive/StatusEffectBase.cs:29 / BattleState`
- Outgoing creates - DamageEffect -> DamageInfo / 7 refs
  - `Effects/DamageEffect.cs:27 / DamageInfo`
- Outgoing calls member - DamageEffect -> DamageCalculator / 6 refs
  - `Effects/DamageEffect.cs:27 / DamageCalculator.Resolve(new DamageInfo(amount, attacker, false, isAmmoAttack), single, context.State)`

### Suggested AI Task

Use the Action Event Pipeline context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: Map Generation

Anchor: `systems/map-generation.md`

### Role Estimate

Map Generation appears to be an internally dense area around map, node, dialogue, config, connection. It contains 14 types, including 5 Unity-facing types.

### Stats

- Types: 14
- Internal relationships: 101
- External relationships: 28
- Entry candidates: 8
- Keywords: `map`, `node`, `dialogue`, `config`, `connection`, `floor`, `generator`, `guarantee`, `line`, `sprite`, `ui`, `weight`

### Start Here

- `MapGenerator.Generate(MapConfig, int)` - flow_candidate / Map/MapGenerator.cs:15
- `MapUIController.BuildMap()` - flow_candidate / Map/MapUIController.cs:62
- `MapUIController.BuildLines(MapData, float)` - flow_candidate / Map/MapUIController.cs:119
- `MapManager.InitializeMap()` - flow_candidate / Map/MapManager.cs:26
- `MapNodeView.Setup(MapNode, Action<MapNode>)` - flow_candidate / Map/MapNodeView.cs:45
- `MapUIController.Awake()` - unity_lifecycle / Map/MapUIController.cs:9
- `MapManager.Awake()` - unity_lifecycle / Map/MapManager.cs:13
- `MapUIController.Start()` - unity_lifecycle / Map/MapUIController.cs:36

### Core Types

- `MapNode` - class / 2 out / 40 in
- `MapUIController` - class / Unity / 34 out / 6 in
- `MapGenerator` - class / 36 out / 1 in
- `MapConfig` - class / Unity / 18 out / 4 in
- `NodeType` - enum / 0 out / 16 in
- `MapData` - class / 5 out / 10 in
- `MapNodeView` - class / Unity / 6 out / 8 in
- `MapManager` - class / Unity / 10 out / 0 in

### Likely Method Flows

- `MapGenerator.Generate(MapConfig, int)`
  - `MapGenerator.Generate(MapConfig, int)`
  - `MapGenerator.PickColumns(int, int, HashSet<int>, System.Random)`
  - `MapGenerator.Shuffle(List<T>, System.Random) / terminal`
- `MapUIController.BuildMap()`
  - `MapUIController.BuildMap()`
  - `MapNodeView.Setup(MapNode, Action<MapNode>)`
  - `MapNodeView.GetSprite(NodeType) / terminal`
- `MapUIController.BuildLines(MapData, float)`
  - `MapUIController.BuildLines(MapData, float)`
  - `MapConnectionLine.Setup(Vector2, Vector2) / terminal`
- `MapManager.InitializeMap()`
  - `MapManager.InitializeMap()`
  - `MapGenerator.Generate(MapConfig) / terminal`

### Internal Type Relationships

- `MapConfig` -> `NodeTypeWeight` - creates / 7 refs
  - Evidence: `Map/MapConfig.cs:37 / NodeTypeWeight`
- `MapConfig` -> `FloorGuarantee` - creates / 6 refs
  - Evidence: `Map/MapConfig.cs:63 / FloorGuarantee`
- `MapGenerator` -> `MapNode` - creates / 6 refs
  - Evidence: `Map/MapGenerator.cs:69 / MapNode`
- `MapGenerator` -> `MapNode` - accepts parameter / 5 refs
  - Evidence: `Map/MapGenerator.cs:263 / List<MapNode>`
- `MapUIController` -> `MapNodeView` - calls member / 5 refs
  - Evidence: `Map/MapUIController.cs:72 / nodeViews.Clear()`
- `MapGenerator` -> `MapNode` - calls member / 4 refs
  - Evidence: `Map/MapGenerator.cs:276 / sortedCurr.Sort((a, b) => a.column.CompareTo(b.column))`
- `MapGenerator` -> `NodeType` - calls member / 4 refs
  - Evidence: `Map/MapGenerator.cs:47 / guaranteeMap.ContainsKey(guarantee.floorIndex)`
- `MapGenerator` -> `NodeType` - creates / 4 refs
  - Evidence: `Map/MapGenerator.cs:39 / Dictionary<int, List<NodeType>>`
- `MapGenerator` -> `MapConfig` - accepts parameter / 3 refs
  - Evidence: `Map/MapGenerator.cs:9 / MapConfig`
- `MapUIController` -> `MapNode` - accepts parameter / 3 refs
  - Evidence: `Map/MapUIController.cs:173 / MapNode`
- `MapUIController` -> `MapNode` - uses local type / 3 refs
  - Evidence: `Map/MapUIController.cs:138 / MapNode`
- `MapUIController` -> `MapConnectionLine` - calls member / 3 refs
  - Evidence: `Map/MapUIController.cs:73 / lineViews.Clear()`
- `MapManager` -> `MapNode` - creates / 3 refs
  - Evidence: `Map/MapManager.cs:51 / List<MapNode>`
- `MapGenerator` -> `MapData` - returns / 2 refs
  - Evidence: `Map/MapGenerator.cs:9 / MapData`
- `MapNodeView` -> `MapNode` - accepts parameter / 2 refs
  - Evidence: `Map/MapNodeView.cs:45 / MapNode`
- `MapUIController` -> `MapConnectionLine` - has field type / 2 refs
  - Evidence: `Map/MapUIController.cs:15 / MapConnectionLine`

### External Touchpoints

- `MapConfig` -> `EnemyData` - outgoing / has field type / 3 refs
  - Evidence: `Map/MapConfig.cs:54 / List<EnemyData>`
- `DialogueView` -> `DialogueNodeData` - incoming / has field type / 2 refs
  - Evidence: `Dialogue/DialogueView.cs:27 / Dictionary<string, DialogueNodeData>`
- `DialogueNodeData` -> `DialogueLineData` - outgoing / has field type / 1 refs
  - Evidence: `Dialogue/DialogueScriptData.cs:47 / DialogueLineData[]`
- `DialogueScriptData` -> `DialogueNodeData` - incoming / has field type / 1 refs
  - Evidence: `Dialogue/DialogueScriptData.cs:55 / DialogueNodeData[]`
- `EventView` -> `MapUIController` - incoming / has field type / 1 refs
  - Evidence: `Event/EventView.cs:30 / MapUIController`
- `MapGenerator` -> `EnemyData` - outgoing / uses local type / 1 refs
  - Evidence: `Map/MapGenerator.cs:411 / List<EnemyData>`
- `MapNode` -> `EnemyData` - outgoing / has field type / 1 refs
  - Evidence: `Map/MapNode.cs:23 / List<EnemyData>`
- `MapUIController` -> `EventData` - outgoing / has field type / 1 refs
  - Evidence: `Map/MapUIController.cs:20 / List<EventData>`
- `MapUIController` -> `EventView` - outgoing / has field type / 1 refs
  - Evidence: `Map/MapUIController.cs:19 / EventView`
- `MapUIController` -> `RestView` - outgoing / has field type / 1 refs
  - Evidence: `Map/MapUIController.cs:23 / RestView`
- `MapUIController` -> `SelectCoopCharUI` - outgoing / has field type / 1 refs
  - Evidence: `Map/MapUIController.cs:25 / SelectCoopCharUI`
- `RestView` -> `MapUIController` - incoming / has field type / 1 refs
  - Evidence: `Rest/RestView.cs:8 / MapUIController`
- `RewardsView` -> `MapUIController` - incoming / has field type / 1 refs
  - Evidence: `Reward/RewardsView.cs:10 / MapUIController`
- `RunData` -> `MapData` - incoming / has field type / 1 refs
  - Evidence: `Card/RunData.cs:14 / MapData`
- `RunData` -> `MapNode` - incoming / has property type / 1 refs
  - Evidence: `Card/RunData.cs:24 / MapNode`
- `RunData` -> `NodeType` - incoming / has property type / 1 refs
  - Evidence: `Card/RunData.cs:21 / NodeType`

### Internal Method Calls

- `MapGenerator.ConnectFloors(List<MapNode>, List<MapNode>, System.Random)` -> `MapGenerator.FindClosest(MapNode, List<MapNode>)` / 2 refs
  - Evidence: `Map/MapGenerator.cs:309 / FindClosest(curr, next)`
- `MapGenerator.Generate(MapConfig, int)` -> `MapGenerator.PickColumns(int, int, HashSet<int>, System.Random)` / 1 refs
  - Evidence: `Map/MapGenerator.cs:139 / PickColumns(columnCount, nodeCount, prevCols, rng)`
- `MapGenerator.Generate(MapConfig, int)` -> `MapGenerator.ConnectFloors(List<MapNode>, List<MapNode>, System.Random)` / 1 refs
  - Evidence: `Map/MapGenerator.cs:176 / ConnectFloors(mapData.floors[f], mapData.floors[f + 1], rng)`
- `MapGenerator.Generate(MapConfig, int)` -> `MapGenerator.GetRandomNodeType(List<NodeTypeWeight>, System.Random)` / 1 refs
  - Evidence: `Map/MapGenerator.cs:114 / GetRandomNodeType(config.nodeTypeWeights, rng)`
- `MapGenerator.Generate(MapConfig, int)` -> `MapGenerator.AssignEncounter(MapNode, MapConfig, System.Random)` / 1 refs
  - Evidence: `Map/MapGenerator.cs:161 / AssignEncounter(node, config, rng)`
- `MapManager.InitializeMap()` -> `MapGenerator.Generate(MapConfig)` / 1 refs
  - Evidence: `Map/MapManager.cs:28 / MapGenerator.Generate(mapConfig)`
- `MapUIController.OpenMap()` -> `MapUIController.BuildMap()` / 1 refs
  - Evidence: `Map/MapUIController.cs:46 / BuildMap()`
- `MapUIController.OpenMap()` -> `MapUIController.RefreshNodeStates()` / 1 refs
  - Evidence: `Map/MapUIController.cs:47 / RefreshNodeStates()`
- `MapNodeView.Setup(MapNode, Action<MapNode>)` -> `MapNodeView.GetSprite(NodeType)` / 1 refs
  - Evidence: `Map/MapNodeView.cs:48 / GetSprite(data.nodeType)`
- `MapUIController.ToggleMap()` -> `MapUIController.OpenMap()` / 1 refs
  - Evidence: `Map/MapUIController.cs:59 / OpenMap()`
- `MapUIController.ToggleMap()` -> `MapUIController.CloseMap()` / 1 refs
  - Evidence: `Map/MapUIController.cs:58 / CloseMap()`
- `MapUIController.BuildMap()` -> `MapNodeView.Setup(MapNode, Action<MapNode>)` / 1 refs
  - Evidence: `Map/MapUIController.cs:106 / view.Setup(node, OnNodeClicked)`
- `MapUIController.BuildMap()` -> `MapUIController.BuildLines(MapData, float)` / 1 refs
  - Evidence: `Map/MapUIController.cs:94 / BuildLines(mapData, xOffset)`
- `MapUIController.BuildMap()` -> `MapUIController.RefreshNodeStates()` / 1 refs
  - Evidence: `Map/MapUIController.cs:111 / RefreshNodeStates()`
- `MapUIController.BuildLines(MapData, float)` -> `MapConnectionLine.Setup(Vector2, Vector2)` / 1 refs
  - Evidence: `Map/MapUIController.cs:144 / line.Setup(from, to)`
- `MapUIController.BuildLines(MapData, float)` -> `MapData.GetNode(int, int)` / 1 refs
  - Evidence: `Map/MapUIController.cs:138 / mapData.GetNode(nextFloor, nextIndex)`

### Evidence

- Likely flow - MapGenerator.Generate(MapConfig, int) -> MapGenerator.PickColumns(int, int, HashSet<int>, System.Random) -> MapGenerator.Shuffle(List<T>, System.Random) / terminal
- Likely flow - MapUIController.BuildMap() -> MapNodeView.Setup(MapNode, Action<MapNode>) -> MapNodeView.GetSprite(NodeType) / terminal
- Internal call - MapGenerator.ConnectFloors(List<MapNode>, List<MapNode>, System.Random) -> MapGenerator.FindClosest(MapNode, List<MapNode>)
  - `Map/MapGenerator.cs:309 / FindClosest(curr, next)`
- Internal call - MapGenerator.Generate(MapConfig, int) -> MapGenerator.PickColumns(int, int, HashSet<int>, System.Random)
  - `Map/MapGenerator.cs:139 / PickColumns(columnCount, nodeCount, prevCols, rng)`
- Internal call - MapGenerator.Generate(MapConfig, int) -> MapGenerator.ConnectFloors(List<MapNode>, List<MapNode>, System.Random)
  - `Map/MapGenerator.cs:176 / ConnectFloors(mapData.floors[f], mapData.floors[f + 1], rng)`
- Outgoing has field type - MapConfig -> EnemyData / 3 refs
  - `Map/MapConfig.cs:54 / List<EnemyData>`
- Incoming has field type - DialogueView -> DialogueNodeData / 2 refs
  - `Dialogue/DialogueView.cs:27 / Dictionary<string, DialogueNodeData>`
- Outgoing has field type - DialogueNodeData -> DialogueLineData / 1 refs
  - `Dialogue/DialogueScriptData.cs:47 / DialogueLineData[]`

### Suggested AI Task

Use the Map Generation context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: UI Layer

Anchor: `systems/ui-layer.md`

### Role Estimate

UI Layer appears to be an externally connected area around ui, char, coop, select, btn. It contains 7 types, including 7 Unity-facing types.

### Stats

- Types: 7
- Internal relationships: 19
- External relationships: 23
- Entry candidates: 8
- Keywords: `ui`, `char`, `coop`, `select`, `btn`, `cooperator`, `dialogue`, `fade`, `inventory`, `slot`, `targeting`

### Start Here

- `ItemTargetingController.Update()` - unity_lifecycle / UI/ItemTargetingController.cs:55
- `ItemTargetingController.BeginTargeting(ItemData, Vector3)` - flow_candidate / UI/ItemTargetingController.cs:35
- `ItemInventoryView.OnEnable()` - unity_lifecycle / UI/ItemInventoryView.cs:29
- `ItemInventoryView.Start()` - unity_lifecycle / UI/ItemInventoryView.cs:30
- `SelectCoopCharUI.Init()` - flow_candidate / UI/SelectCoopCharUI.cs:22
- `ItemSlot.Awake()` - unity_lifecycle / UI/ItemSlot.cs:13
- `SelectCoopCharBtn.Start()` - unity_lifecycle / UI/SelectCoopCharBtn.cs:15
- `SelectCoopCharUI.Awake()` - unity_lifecycle / UI/SelectCoopCharUI.cs:17

### Core Types

- `ItemInventoryView` - class / Unity / 9 out / 5 in
- `SelectCoopCharUI` - class / Unity / 7 out / 6 in
- `ItemSlot` - class / Unity / 7 out / 5 in
- `ItemTargetingController` - class / Unity / 8 out / 2 in
- `SelectCoopCharBtn` - class / Unity / 7 out / 3 in
- `FadeController` - class / Unity / 0 out / 2 in
- `DialogueUI` - class / Unity / 0 out / 0 in

### Likely Method Flows

- `ItemTargetingController.Update()`
  - `ItemTargetingController.Update()`
  - `ItemTargetingController.CancelTargeting() / terminal`
- `ItemTargetingController.BeginTargeting(ItemData, Vector3)`
  - `ItemTargetingController.BeginTargeting(ItemData, Vector3)`
  - `ItemTargetingController.GetPointerScreenPosition() / terminal`
- `ItemInventoryView.OnEnable()`
  - `ItemInventoryView.OnEnable()`
  - `ItemInventoryView.TrySubscribe()`
  - `ItemInventoryView.RefreshInventory()`
  - `ItemSlot.SetItem(ItemData, ItemInventoryView) / terminal`
- `ItemInventoryView.Start()`
  - `ItemInventoryView.Start()`
  - `ItemInventoryView.TrySubscribe()`
  - `ItemInventoryView.RefreshInventory()`
  - `ItemSlot.SetItem(ItemData, ItemInventoryView) / terminal`

### Internal Type Relationships

- `ItemInventoryView` -> `ItemSlot` - calls member / 3 refs
  - Evidence: `UI/ItemInventoryView.cs:50 / _slots.Clear()`
- `ItemSlot` -> `ItemInventoryView` - calls member / 3 refs
  - Evidence: `UI/ItemSlot.cs:34 / owner.ShowTooltip(item, transform.position)`
- `SelectCoopCharBtn` -> `SelectCoopCharUI` - unity get component / 2 refs
  - Evidence: `UI/SelectCoopCharBtn.cs:17 / SelectCoopCharUI`
- `ItemInventoryView` -> `ItemSlot` - has field type / 1 refs
  - Evidence: `UI/ItemInventoryView.cs:21 / List<ItemSlot>`
- `ItemSlot` -> `ItemInventoryView` - accepts parameter / 1 refs
  - Evidence: `UI/ItemSlot.cs:18 / ItemInventoryView`
- `ItemSlot` -> `ItemInventoryView` - has field type / 1 refs
  - Evidence: `UI/ItemSlot.cs:10 / ItemInventoryView`
- `SelectCoopCharBtn` -> `SelectCoopCharUI` - has field type / 1 refs
  - Evidence: `UI/SelectCoopCharBtn.cs:9 / SelectCoopCharUI`
- `SelectCoopCharUI` -> `FadeController` - has field type / 1 refs
  - Evidence: `UI/SelectCoopCharUI.cs:10 / FadeController`
- `SelectCoopCharUI` -> `SelectCoopCharBtn` - has field type / 1 refs
  - Evidence: `UI/SelectCoopCharUI.cs:9 / List<SelectCoopCharBtn>`
- `SelectCoopCharBtn` -> `SelectCoopCharUI` - calls member / 1 refs
  - Evidence: `UI/SelectCoopCharBtn.cs:73 / selectCoopCharUI.CloseUI()`
- `SelectCoopCharUI` -> `FadeController` - calls member / 1 refs
  - Evidence: `UI/SelectCoopCharUI.cs:24 / fadeController.FadeIn()`
- `SelectCoopCharUI` -> `SelectCoopCharBtn` - creates / 1 refs
  - Evidence: `UI/SelectCoopCharUI.cs:19 / List<SelectCoopCharBtn>`
- `ItemInventoryView` -> `ItemSlot` - unity get component / 1 refs
  - Evidence: `UI/ItemInventoryView.cs:54 / ItemSlot`
- `SelectCoopCharUI` -> `SelectCoopCharBtn` - unity get component / 1 refs
  - Evidence: `UI/SelectCoopCharUI.cs:19 / SelectCoopCharBtn`

### External Touchpoints

- `ItemTargetingController` -> `TargetArrow` - outgoing / calls member / 3 refs
  - Evidence: `UI/ItemTargetingController.cs:52 / arrow.Hide()`
- `ItemInventoryView` -> `ItemData` - outgoing / accepts parameter / 2 refs
  - Evidence: `UI/ItemInventoryView.cs:61 / ItemData`
- `ItemActionPopup` -> `ItemTargetingController` - incoming / has field type / 1 refs
  - Evidence: `UI/ItemActionPopup.cs:14 / ItemTargetingController`
- `ItemInventoryView` -> `ItemActionPopup` - outgoing / has field type / 1 refs
  - Evidence: `UI/ItemInventoryView.cs:19 / ItemActionPopup`
- `ItemSlot` -> `ItemData` - outgoing / accepts parameter / 1 refs
  - Evidence: `UI/ItemSlot.cs:18 / ItemData`
- `ItemSlot` -> `ItemData` - outgoing / has field type / 1 refs
  - Evidence: `UI/ItemSlot.cs:9 / ItemData`
- `ItemTargetingController` -> `ItemData` - outgoing / accepts parameter / 1 refs
  - Evidence: `UI/ItemTargetingController.cs:35 / ItemData`
- `ItemTargetingController` -> `ItemData` - outgoing / has field type / 1 refs
  - Evidence: `UI/ItemTargetingController.cs:28 / ItemData`
- `ItemTargetingController` -> `TargetArrow` - outgoing / has field type / 1 refs
  - Evidence: `UI/ItemTargetingController.cs:26 / TargetArrow`
- `MapUIController` -> `SelectCoopCharUI` - incoming / has field type / 1 refs
  - Evidence: `Map/MapUIController.cs:25 / SelectCoopCharUI`
- `SelectCoopCharBtn` -> `CoopCharData` - outgoing / uses local type / 1 refs
  - Evidence: `UI/SelectCoopCharBtn.cs:59 / CoopCharData`
- `SelectCoopCharBtn` -> `DialogueView` - outgoing / uses local type / 1 refs
  - Evidence: `UI/SelectCoopCharBtn.cs:58 / DialogueView`
- `SelectCoopCharUI` -> `DialogueView` - outgoing / has field type / 1 refs
  - Evidence: `UI/SelectCoopCharUI.cs:11 / DialogueView`
- `SelectCoopCharUI` -> `DialogueView` - outgoing / has property type / 1 refs
  - Evidence: `UI/SelectCoopCharUI.cs:13 / DialogueView`
- `ItemActionPopup` -> `ItemTargetingController` - incoming / calls member / 1 refs
  - Evidence: `UI/ItemActionPopup.cs:49 / targetingController.BeginTargeting(item, anchorWorldPos)`
- `ItemInventoryView` -> `ItemActionPopup` - outgoing / calls member / 1 refs
  - Evidence: `UI/ItemInventoryView.cs:79 / actionPopup.Open(item, worldPos)`

### Internal Method Calls

- `ItemTargetingController.Update()` -> `ItemTargetingController.CancelTargeting()` / 2 refs
  - Evidence: `UI/ItemTargetingController.cs:62 / CancelTargeting()`
- `SelectCoopCharUI.Init()` -> `FadeController.FadeIn()` / 1 refs
  - Evidence: `UI/SelectCoopCharUI.cs:24 / fadeController.FadeIn()`
- `ItemInventoryView.OnEnable()` -> `ItemInventoryView.TrySubscribe()` / 1 refs
  - Evidence: `UI/ItemInventoryView.cs:29 / TrySubscribe()`
- `ItemInventoryView.Start()` -> `ItemInventoryView.TrySubscribe()` / 1 refs
  - Evidence: `UI/ItemInventoryView.cs:30 / TrySubscribe()`
- `ItemSlot.OnPointerEnter(PointerEventData)` -> `ItemInventoryView.ShowTooltip(ItemData, Vector3)` / 1 refs
  - Evidence: `UI/ItemSlot.cs:34 / owner.ShowTooltip(item, transform.position)`
- `ItemTargetingController.BeginTargeting(ItemData, Vector3)` -> `ItemTargetingController.GetPointerScreenPosition()` / 1 refs
  - Evidence: `UI/ItemTargetingController.cs:45 / GetPointerScreenPosition()`
- `ItemTargetingController.BeginTargeting(ItemData, Vector3)` -> `ItemTargetingController.EnsureArrow()` / 1 refs
  - Evidence: `UI/ItemTargetingController.cs:44 / EnsureArrow()`
- `ItemTargetingController.BeginTargeting(ItemData, Vector3)` -> `ItemTargetingController.UpdateArrow(Vector2)` / 1 refs
  - Evidence: `UI/ItemTargetingController.cs:45 / UpdateArrow(GetPointerScreenPosition())`
- `ItemSlot.OnPointerExit(PointerEventData)` -> `ItemInventoryView.HideTooltip()` / 1 refs
  - Evidence: `UI/ItemSlot.cs:39 / owner.HideTooltip()`
- `ItemInventoryView.TrySubscribe()` -> `ItemInventoryView.RefreshInventory()` / 1 refs
  - Evidence: `UI/ItemInventoryView.cs:44 / RefreshInventory()`
- `ItemSlot.OnPointerClick(PointerEventData)` -> `ItemInventoryView.OpenActionPopup(ItemData, Vector3)` / 1 refs
  - Evidence: `UI/ItemSlot.cs:44 / owner.OpenActionPopup(item, transform.position)`
- `ItemInventoryView.RefreshInventory()` -> `ItemSlot.SetItem(ItemData, ItemInventoryView)` / 1 refs
  - Evidence: `UI/ItemInventoryView.cs:55 / slot.SetItem(item, this)`
- `SelectCoopCharBtn.OnClickBtn()` -> `SelectCoopCharBtn.FinishSelection()` / 1 refs
  - Evidence: `UI/SelectCoopCharBtn.cs:66 / FinishSelection()`
- `ItemTargetingController.Update()` -> `ItemTargetingController.ResolveClick(Vector2)` / 1 refs
  - Evidence: `UI/ItemTargetingController.cs:82 / ResolveClick(pointer)`
- `ItemTargetingController.Update()` -> `ItemTargetingController.GetPointerScreenPosition()` / 1 refs
  - Evidence: `UI/ItemTargetingController.cs:66 / GetPointerScreenPosition()`
- `ItemTargetingController.Update()` -> `ItemTargetingController.UpdateArrow(Vector2)` / 1 refs
  - Evidence: `UI/ItemTargetingController.cs:67 / UpdateArrow(pointer)`

### Evidence

- Likely flow - ItemTargetingController.Update() -> ItemTargetingController.CancelTargeting() / terminal
- Likely flow - ItemTargetingController.BeginTargeting(ItemData, Vector3) -> ItemTargetingController.GetPointerScreenPosition() / terminal
- Internal call - ItemTargetingController.Update() -> ItemTargetingController.CancelTargeting()
  - `UI/ItemTargetingController.cs:62 / CancelTargeting()`
- Internal call - SelectCoopCharUI.Init() -> FadeController.FadeIn()
  - `UI/SelectCoopCharUI.cs:24 / fadeController.FadeIn()`
- Internal call - ItemInventoryView.OnEnable() -> ItemInventoryView.TrySubscribe()
  - `UI/ItemInventoryView.cs:29 / TrySubscribe()`
- Outgoing calls member - ItemTargetingController -> TargetArrow / 3 refs
  - `UI/ItemTargetingController.cs:52 / arrow.Hide()`
- Outgoing accepts parameter - ItemInventoryView -> ItemData / 2 refs
  - `UI/ItemInventoryView.cs:61 / ItemData`
- Incoming has field type - ItemActionPopup -> ItemTargetingController / 1 refs
  - `UI/ItemActionPopup.cs:14 / ItemTargetingController`

### Suggested AI Task

Use the UI Layer context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: Passive

Anchor: `systems/passive.md`

### Role Estimate

Passive appears to be an externally connected area around passive, powers, block, effects, logic. It contains 5 types, including 5 plain C# types.

### Stats

- Types: 5
- Internal relationships: 3
- External relationships: 76
- Entry candidates: 0
- Keywords: `passive`, `powers`, `block`, `effects`, `logic`, `power`, `romance`, `siberian`, `tactical`, `timed`, `walk`

### Start Here

- None detected.

### Core Types

- `IPassiveLogic` - interface / 20 out / 13 in
- `PowerPassiveBase` - class / 21 out / 4 in
- `TacticalWalkPassive` - class / 9 out / 1 in
- `SiberianRomancePassive` - class / 8 out / 1 in
- `TimedBlockPassive` - class / 4 out / 1 in

### Likely Method Flows

- No internal method flow detected.

### Internal Type Relationships

- `PowerPassiveBase` -> `IPassiveLogic` - implements / 1 refs
  - Evidence: `Passive/PowerPassiveBase.cs:4 / IPassiveLogic`
- `SiberianRomancePassive` -> `PowerPassiveBase` - inherits / 1 refs
  - Evidence: `Powers/SiberianRomancePassive.cs:4 / PowerPassiveBase`
- `TacticalWalkPassive` -> `PowerPassiveBase` - inherits / 1 refs
  - Evidence: `Powers/TacticalWalkPassive.cs:4 / PowerPassiveBase`

### External Touchpoints

- `IPassiveLogic` -> `BattleState` - outgoing / accepts parameter / 7 refs
  - Evidence: `Passive/IPassiveLogic.cs:3 / BattleState`
- `PowerPassiveBase` -> `BattleState` - outgoing / accepts parameter / 7 refs
  - Evidence: `Passive/PowerPassiveBase.cs:6 / BattleState`
- `IPassiveLogic` -> `DamageInfo` - outgoing / accepts parameter / 4 refs
  - Evidence: `Passive/IPassiveLogic.cs:8 / DamageInfo`
- `IPassiveLogic` -> `DamageInfo` - outgoing / returns / 4 refs
  - Evidence: `Passive/IPassiveLogic.cs:8 / DamageInfo`
- `IPassiveLogic` -> `ICombatant` - outgoing / accepts parameter / 4 refs
  - Evidence: `Passive/IPassiveLogic.cs:3 / ICombatant`
- `PowerPassiveBase` -> `DamageInfo` - outgoing / accepts parameter / 4 refs
  - Evidence: `Passive/PowerPassiveBase.cs:9 / DamageInfo`
- `PowerPassiveBase` -> `DamageInfo` - outgoing / returns / 4 refs
  - Evidence: `Passive/PowerPassiveBase.cs:9 / DamageInfo`
- `PowerPassiveBase` -> `ICombatant` - outgoing / accepts parameter / 4 refs
  - Evidence: `Passive/PowerPassiveBase.cs:6 / ICombatant`
- `TacticalWalkPassive` -> `BattleState` - outgoing / accepts parameter / 3 refs
  - Evidence: `Powers/TacticalWalkPassive.cs:11 / BattleState`
- `TacticalWalkPassive` -> `DamageInfo` - outgoing / accepts parameter / 2 refs
  - Evidence: `Powers/TacticalWalkPassive.cs:14 / DamageInfo`
- `TacticalWalkPassive` -> `DamageInfo` - outgoing / returns / 2 refs
  - Evidence: `Powers/TacticalWalkPassive.cs:14 / DamageInfo`
- `EnemyInstance` -> `IPassiveLogic` - incoming / calls member / 2 refs
  - Evidence: `Enemy/EnemyInstance.cs:92 / _passives.Add(passive)`
- `PlayerCombatant` -> `IPassiveLogic` - incoming / calls member / 2 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:77 / _passives.Add(passive)`
- `SiberianRomancePassive` -> `PlayerCombatant` - outgoing / calls member / 2 refs
  - Evidence: `Powers/SiberianRomancePassive.cs:18 / player.AddPassive(new StrengthStatus(_strengthPerTurn))`
- `AmmoAttackBonusPassive` -> `PowerPassiveBase` - incoming / inherits / 1 refs
  - Evidence: `Powers/AmmoAttackBonusPassive.cs:4 / PowerPassiveBase`
- `AmmoPerTurnPassive` -> `PowerPassiveBase` - incoming / inherits / 1 refs
  - Evidence: `Powers/AmmoPerTurnPassive.cs:3 / PowerPassiveBase`

### Internal Method Calls

- `PowerPassiveBase.PreviewOutgoingDamage(DamageInfo, BattleState)` -> `PowerPassiveBase.ModifyOutgoingDamage(DamageInfo, BattleState)` / 1 refs
  - Evidence: `Passive/PowerPassiveBase.cs:16 / ModifyOutgoingDamage(info, state)`
- `PowerPassiveBase.PreviewIncomingDamage(DamageInfo, BattleState)` -> `PowerPassiveBase.ModifyIncomingDamage(DamageInfo, BattleState)` / 1 refs
  - Evidence: `Passive/PowerPassiveBase.cs:18 / ModifyIncomingDamage(info, state)`

### Evidence

- Internal call - PowerPassiveBase.PreviewOutgoingDamage(DamageInfo, BattleState) -> PowerPassiveBase.ModifyOutgoingDamage(DamageInfo, BattleState)
  - `Passive/PowerPassiveBase.cs:16 / ModifyOutgoingDamage(info, state)`
- Internal call - PowerPassiveBase.PreviewIncomingDamage(DamageInfo, BattleState) -> PowerPassiveBase.ModifyIncomingDamage(DamageInfo, BattleState)
  - `Passive/PowerPassiveBase.cs:18 / ModifyIncomingDamage(info, state)`
- Outgoing accepts parameter - IPassiveLogic -> BattleState / 7 refs
  - `Passive/IPassiveLogic.cs:3 / BattleState`
- Outgoing accepts parameter - PowerPassiveBase -> BattleState / 7 refs
  - `Passive/PowerPassiveBase.cs:6 / BattleState`
- Outgoing accepts parameter - IPassiveLogic -> DamageInfo / 4 refs
  - `Passive/IPassiveLogic.cs:8 / DamageInfo`
- Internal implements - PowerPassiveBase -> IPassiveLogic / 1 refs
  - `Passive/PowerPassiveBase.cs:4 / IPassiveLogic`
- Internal inherits - SiberianRomancePassive -> PowerPassiveBase / 1 refs
  - `Powers/SiberianRomancePassive.cs:4 / PowerPassiveBase`
- Internal inherits - TacticalWalkPassive -> PowerPassiveBase / 1 refs
  - `Powers/TacticalWalkPassive.cs:4 / PowerPassiveBase`

### Suggested AI Task

Use the Passive context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: Dialogue

Anchor: `systems/dialogue.md`

### Role Estimate

Dialogue appears to be an externally connected area around dialogue, character, line. It contains 4 types, including 1 Unity-facing types.

### Stats

- Types: 4
- Internal relationships: 4
- External relationships: 19
- Entry candidates: 3
- Keywords: `dialogue`, `character`, `line`

### Start Here

- `DialogueView.Update()` - unity_lifecycle / Dialogue/DialogueView.cs:220
- `DialogueView.Awake()` - unity_lifecycle / Dialogue/DialogueView.cs:33
- `DialogueView.SetupCharacterSlots()` - flow_candidate / Dialogue/DialogueView.cs:71

### Core Types

- `DialogueView` - class / Unity / 10 out / 8 in
- `DialogueLineData` - class / 1 out / 3 in
- `DialogueScriptData` - class / 2 out / 1 in
- `DialogueCharacterData` - class / 0 out / 2 in

### Likely Method Flows

- `DialogueView.Update()`
  - `DialogueView.Update()`
  - `DialogueView.OnAdvanceClicked()`
  - `DialogueView.ShowCurrentLine()`
  - `DialogueView.Finish() / terminal`
- `DialogueView.Awake()`
  - `DialogueView.Awake() / terminal`
- `DialogueView.SetupCharacterSlots()`
  - `DialogueView.SetupCharacterSlots() / terminal`

### Internal Type Relationships

- `DialogueView` -> `DialogueLineData` - accepts parameter / 2 refs
  - Evidence: `Dialogue/DialogueView.cs:136 / DialogueLineData`
- `DialogueScriptData` -> `DialogueCharacterData` - has field type / 1 refs
  - Evidence: `Dialogue/DialogueScriptData.cs:53 / DialogueCharacterData[]`
- `DialogueView` -> `DialogueScriptData` - has field type / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:26 / DialogueScriptData`

### External Touchpoints

- `DialogueView` -> `DialogueNodeData` - outgoing / has field type / 2 refs
  - Evidence: `Dialogue/DialogueView.cs:27 / Dictionary<string, DialogueNodeData>`
- `CharacterSlotView` -> `DialogueCharacterData` - incoming / accepts parameter / 1 refs
  - Evidence: `Dialogue/CharacterSlotView.cs:17 / DialogueCharacterData`
- `DialogueLineData` -> `DialogueChoiceOption` - outgoing / has field type / 1 refs
  - Evidence: `Dialogue/DialogueScriptData.cs:40 / DialogueChoiceOption[]`
- `DialogueManager` -> `DialogueView` - incoming / has field type / 1 refs
  - Evidence: `Cooperator/DialogueManager.cs:9 / DialogueView`
- `DialogueNodeData` -> `DialogueLineData` - incoming / has field type / 1 refs
  - Evidence: `Dialogue/DialogueScriptData.cs:47 / DialogueLineData[]`
- `DialogueScriptData` -> `DialogueNodeData` - outgoing / has field type / 1 refs
  - Evidence: `Dialogue/DialogueScriptData.cs:55 / DialogueNodeData[]`
- `DialogueView` -> `CharacterSlotView` - outgoing / has field type / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:15 / CharacterSlotView[]`
- `DialogueView` -> `DialogueChoiceOption` - outgoing / accepts parameter / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:185 / DialogueChoiceOption`
- `EventView` -> `DialogueView` - incoming / has field type / 1 refs
  - Evidence: `Event/EventView.cs:31 / DialogueView`
- `SelectCoopCharBtn` -> `DialogueView` - incoming / uses local type / 1 refs
  - Evidence: `UI/SelectCoopCharBtn.cs:58 / DialogueView`
- `SelectCoopCharUI` -> `DialogueView` - incoming / has field type / 1 refs
  - Evidence: `UI/SelectCoopCharUI.cs:11 / DialogueView`
- `SelectCoopCharUI` -> `DialogueView` - incoming / has property type / 1 refs
  - Evidence: `UI/SelectCoopCharUI.cs:13 / DialogueView`
- `DialogueManager` -> `DialogueView` - incoming / calls member / 1 refs
  - Evidence: `Cooperator/DialogueManager.cs:49 / dialogueView.Play(dialogueJson, () => isDialogueEnd?.Invoke())`
- `DialogueView` -> `DialogueNodeData` - outgoing / calls member / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:97 / _nodeMap.TryGetValue(nodeId, out _currentNode)`
- `EventView` -> `DialogueView` - incoming / calls member / 1 refs
  - Evidence: `Event/EventView.cs:82 / dialogueView.Play(_data.dialogueJson, ShowDescriptionAndChoices)`
- `SelectCoopCharBtn` -> `DialogueView` - incoming / calls member / 1 refs
  - Evidence: `UI/SelectCoopCharBtn.cs:62 / dialogueView.Play(coopCharData.joinDialogueJson, FinishSelection)`

### Internal Method Calls

- `DialogueView.ShowCurrentLine()` -> `DialogueView.Finish()` / 3 refs
  - Evidence: `Dialogue/DialogueView.cs:113 / Finish()`
- `DialogueView.Play(TextAsset, Action)` -> `DialogueView.SetupCharacterSlots()` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:65 / SetupCharacterSlots()`
- `DialogueView.Play(TextAsset, Action)` -> `DialogueView.GoToNode(string)` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:68 / GoToNode(_script.startNode)`
- `DialogueView.GoToNode(string)` -> `DialogueView.ShowCurrentLine()` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:104 / ShowCurrentLine()`
- `DialogueView.GoToNode(string)` -> `DialogueView.Finish()` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:100 / Finish()`
- `DialogueView.ShowCurrentLine()` -> `DialogueView.ShowLine(DialogueLineData)` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:121 / ShowLine(line)`
- `DialogueView.ShowCurrentLine()` -> `DialogueView.ShowChoices(DialogueLineData)` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:124 / ShowChoices(line)`
- `DialogueView.ShowCurrentLine()` -> `DialogueView.HideChoices()` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:109 / HideChoices()`
- `DialogueView.ShowLine(DialogueLineData)` -> `DialogueView.UpdateSpeakerHighlight(string, string)` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:141 / UpdateSpeakerHighlight(line.speaker, line.emotion)`
- `DialogueView.ShowLine(DialogueLineData)` -> `DialogueView.GetCharacterName(string)` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:139 / GetCharacterName(line.speaker)`
- `DialogueView.ShowChoices(DialogueLineData)` -> `DialogueView.OnChoiceSelected(DialogueChoiceOption)` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:180 / OnChoiceSelected(capturedOption)`
- `DialogueView.ShowChoices(DialogueLineData)` -> `DialogueView.Finish()` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:171 / Finish()`
- `DialogueView.OnChoiceSelected(DialogueChoiceOption)` -> `DialogueView.GoToNode(string)` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:207 / GoToNode(option.next)`
- `DialogueView.OnChoiceSelected(DialogueChoiceOption)` -> `DialogueView.HideChoices()` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:187 / HideChoices()`
- `DialogueView.OnChoiceSelected(DialogueChoiceOption)` -> `DialogueView.Finish()` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:203 / Finish()`
- `DialogueView.Update()` -> `DialogueView.OnAdvanceClicked()` / 1 refs
  - Evidence: `Dialogue/DialogueView.cs:228 / OnAdvanceClicked()`

### Evidence

- Likely flow - DialogueView.Update() -> DialogueView.OnAdvanceClicked() -> DialogueView.ShowCurrentLine() -> DialogueView.Finish() / terminal
- Likely flow - DialogueView.Awake() -> terminal
- Internal call - DialogueView.ShowCurrentLine() -> DialogueView.Finish()
  - `Dialogue/DialogueView.cs:113 / Finish()`
- Internal call - DialogueView.Play(TextAsset, Action) -> DialogueView.SetupCharacterSlots()
  - `Dialogue/DialogueView.cs:65 / SetupCharacterSlots()`
- Internal call - DialogueView.Play(TextAsset, Action) -> DialogueView.GoToNode(string)
  - `Dialogue/DialogueView.cs:68 / GoToNode(_script.startNode)`
- Outgoing has field type - DialogueView -> DialogueNodeData / 2 refs
  - `Dialogue/DialogueView.cs:27 / Dictionary<string, DialogueNodeData>`
- Incoming accepts parameter - CharacterSlotView -> DialogueCharacterData / 1 refs
  - `Dialogue/CharacterSlotView.cs:17 / DialogueCharacterData`
- Outgoing has field type - DialogueLineData -> DialogueChoiceOption / 1 refs
  - `Dialogue/DialogueScriptData.cs:40 / DialogueChoiceOption[]`

### Suggested AI Task

Use the Dialogue context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: Player / Input

Anchor: `systems/input.md`

### Role Estimate

Player / Input appears to be an externally connected area around player, gameplay, char, combatant, hud. It contains 4 types, including 2 Unity-facing types.

### Stats

- Types: 4
- Internal relationships: 3
- External relationships: 26
- Entry candidates: 6
- Keywords: `player`, `gameplay`, `char`, `combatant`, `hud`, `party`

### Start Here

- `PartyView.Start()` - unity_lifecycle / Player/PartyView.cs:27
- `PlayerHUDView.Update()` - unity_lifecycle / Gameplay/PlayerHUDView.cs:51
- `PlayerHUDView.SpawnHitEffect()` - flow_candidate / Gameplay/PlayerHUDView.cs:140
- `PlayerHUDView.SpawnBlockEffect()` - flow_candidate / Gameplay/PlayerHUDView.cs:146
- `PartyView.OnDestroy()` - unity_lifecycle / Player/PartyView.cs:37
- `PlayerHUDView.OnDisable()` - unity_lifecycle / Gameplay/PlayerHUDView.cs:128

### Core Types

- `PlayerCombatant` - class / 15 out / 7 in
- `PlayerCharData` - class / 3 out / 1 in
- `PlayerHUDView` - class / Unity / 4 out / 0 in
- `PartyView` - class / Unity / 2 out / 0 in

### Likely Method Flows

- `PartyView.Start()`
  - `PartyView.Start()`
  - `PartyView.Refresh()`
  - `PartyView.BindPlayer() / terminal`
- `PlayerHUDView.Update()`
  - `PlayerHUDView.Update()`
  - `PlayerHUDView.UpdateDamageSubscription() / terminal`
- `PlayerHUDView.SpawnHitEffect()`
  - `PlayerHUDView.SpawnHitEffect() / terminal`
- `PlayerHUDView.SpawnBlockEffect()`
  - `PlayerHUDView.SpawnBlockEffect() / terminal`

### Internal Type Relationships

- `PartyView` -> `PlayerCharData` - has field type / 1 refs
  - Evidence: `Player/PartyView.cs:20 / PlayerCharData`
- `PlayerHUDView` -> `PlayerCombatant` - has field type / 1 refs
  - Evidence: `Gameplay/PlayerHUDView.cs:49 / PlayerCombatant`
- `PlayerHUDView` -> `PlayerCombatant` - uses local type / 1 refs
  - Evidence: `Gameplay/PlayerHUDView.cs:107 / PlayerCombatant`

### External Touchpoints

- `PlayerCombatant` -> `StatusEffectBase` - outgoing / type check / 4 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:69 / StatusEffectBase`
- `PlayerCharData` -> `CardData` - outgoing / has field type / 2 refs
  - Evidence: `Player/PlayerCharData.cs:14 / List<CardData>`
- `PlayerCombatant` -> `IPassiveLogic` - outgoing / calls member / 2 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:77 / _passives.Add(passive)`
- `PlayerCombatant` -> `StatusEffectBase` - outgoing / calls member / 2 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:73 / existing.TryMerge(newStatus)`
- `PlayerHUDView` -> `HitEffectSpawner` - outgoing / calls member / 2 refs
  - Evidence: `Gameplay/PlayerHUDView.cs:143 / HitEffectSpawner.Spawn(hitEffectPrefab, anchor)`
- `SiberianRomancePassive` -> `PlayerCombatant` - incoming / calls member / 2 refs
  - Evidence: `Powers/SiberianRomancePassive.cs:18 / player.AddPassive(new StrengthStatus(_strengthPerTurn))`
- `BattleState` -> `PlayerCombatant` - incoming / has field type / 1 refs
  - Evidence: `Gameplay/BattleState.cs:9 / PlayerCombatant`
- `PartyView` -> `CoopCharState` - outgoing / uses local type / 1 refs
  - Evidence: `Player/PartyView.cs:100 / List<CoopCharState>`
- `PlayerCharData` -> `CharData` - outgoing / inherits / 1 refs
  - Evidence: `Player/PlayerCharData.cs:5 / CharData`
- `PlayerCombatant` -> `BattleState` - outgoing / accepts parameter / 1 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:80 / BattleState`
- `PlayerCombatant` -> `DamageInfo` - outgoing / accepts parameter / 1 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:24 / DamageInfo`
- `PlayerCombatant` -> `DexterityStatus` - outgoing / type check / 1 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:58 / DexterityStatus`
- `PlayerCombatant` -> `ICombatant` - outgoing / implements / 1 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:9 / ICombatant`
- `PlayerCombatant` -> `IPassiveLogic` - outgoing / accepts parameter / 1 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:67 / IPassiveLogic`
- `PlayerCombatant` -> `IPassiveLogic` - outgoing / has field type / 1 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:12 / List<IPassiveLogic>`
- `PlayerCombatant` -> `IPassiveLogic` - outgoing / has property type / 1 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:18 / List<IPassiveLogic>`

### Internal Method Calls

- `PartyView.Start()` -> `PartyView.Refresh()` / 1 refs
  - Evidence: `Player/PartyView.cs:34 / Refresh()`
- `PartyView.Refresh()` -> `PartyView.BindPlayer()` / 1 refs
  - Evidence: `Player/PartyView.cs:45 / BindPlayer()`
- `PartyView.Refresh()` -> `PartyView.BindCompanions()` / 1 refs
  - Evidence: `Player/PartyView.cs:46 / BindCompanions()`
- `PlayerCombatant.AddBlock(int)` -> `PlayerCombatant.PreviewBlockGain(int)` / 1 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:50 / PreviewBlockGain(amount)`
- `PlayerHUDView.Update()` -> `PlayerHUDView.UpdateDamageSubscription()` / 1 refs
  - Evidence: `Gameplay/PlayerHUDView.cs:53 / UpdateDamageSubscription()`
- `PlayerCombatant.TickPassives(BattleState)` -> `PlayerCombatant.RemoveExpiredPassives()` / 1 refs
  - Evidence: `Gameplay/PlayerCombatant.cs:93 / RemoveExpiredPassives()`
- `PlayerHUDView.HandlePlayerDamaged(int)` -> `PlayerHUDView.SpawnHitEffect()` / 1 refs
  - Evidence: `Gameplay/PlayerHUDView.cs:124 / SpawnHitEffect()`
- `PlayerHUDView.HandleBlocked(int)` -> `PlayerHUDView.SpawnBlockEffect()` / 1 refs
  - Evidence: `Gameplay/PlayerHUDView.cs:126 / SpawnBlockEffect()`

### Evidence

- Likely flow - PartyView.Start() -> PartyView.Refresh() -> PartyView.BindPlayer() / terminal
- Likely flow - PlayerHUDView.Update() -> PlayerHUDView.UpdateDamageSubscription() / terminal
- Internal call - PartyView.Start() -> PartyView.Refresh()
  - `Player/PartyView.cs:34 / Refresh()`
- Internal call - PartyView.Refresh() -> PartyView.BindPlayer()
  - `Player/PartyView.cs:45 / BindPlayer()`
- Internal call - PartyView.Refresh() -> PartyView.BindCompanions()
  - `Player/PartyView.cs:46 / BindCompanions()`
- Outgoing type check - PlayerCombatant -> StatusEffectBase / 4 refs
  - `Gameplay/PlayerCombatant.cs:69 / StatusEffectBase`
- Outgoing has field type - PlayerCharData -> CardData / 2 refs
  - `Player/PlayerCharData.cs:14 / List<CardData>`
- Outgoing calls member - PlayerCombatant -> IPassiveLogic / 2 refs
  - `Gameplay/PlayerCombatant.cs:77 / _passives.Add(passive)`

### Suggested AI Task

Use the Player / Input context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: Char

Anchor: `systems/char.md`

### Role Estimate

Char appears to be an externally connected area around char, coop, cooperator. It contains 3 types, including 1 Unity-facing types.

### Stats

- Types: 3
- Internal relationships: 2
- External relationships: 37
- Entry candidates: 0
- Keywords: `char`, `coop`, `cooperator`

### Start Here

- None detected.

### Core Types

- `CoopCharState` - class / 2 out / 23 in
- `CoopCharData` - class / 7 out / 6 in
- `CharData` - class / Unity / 1 out / 2 in

### Likely Method Flows

- No internal method flow detected.

### Internal Type Relationships

- `CoopCharData` -> `CharData` - inherits / 1 refs
  - Evidence: `Cooperator/CoopCharData.cs:5 / CharData`
- `CoopCharState` -> `CoopCharData` - has field type / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:10 / CoopCharData`

### External Touchpoints

- `CooperationManager` -> `CoopCharState` - incoming / calls member / 11 refs
  - Evidence: `Cooperator/CooperationManager.cs:74 / coopCharDict.TryGetValue(charID, out var charState)`
- `CoopCharData` -> `CardData` - outgoing / has field type / 5 refs
  - Evidence: `Cooperator/CoopCharData.cs:8 / List<CardData>`
- `CooperationManager` -> `CoopCharState` - incoming / creates / 3 refs
  - Evidence: `Cooperator/CooperationManager.cs:24 / Dictionary<string, CoopCharState>`
- `CooperationManager` -> `CoopCharState` - incoming / uses local type / 2 refs
  - Evidence: `Cooperator/CooperationManager.cs:199 / CoopCharState`
- `PortraitSlot` -> `CoopCharState` - incoming / accepts parameter / 2 refs
  - Evidence: `Rest/PortraitSlot.cs:22 / CoopCharState`
- `PortraitSlot` -> `CoopCharState` - incoming / has field type / 2 refs
  - Evidence: `Rest/PortraitSlot.cs:19 / CoopCharState`
- `CharData` -> `IPassiveLogic` - outgoing / has field type / 1 refs
  - Evidence: `Common/CharData.cs:25 / IPassiveLogic`
- `CoopCharData` -> `RankEventData` - outgoing / has field type / 1 refs
  - Evidence: `Cooperator/CoopCharData.cs:25 / List<RankEventData>`
- `CoopCharState` -> `RankEventData` - outgoing / has field type / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:15 / Dictionary<int, RankEventData>`
- `CooperationManager` -> `CoopCharData` - incoming / has field type / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:23 / List<CoopCharData>`
- `CooperationManager` -> `CoopCharData` - incoming / returns / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:104 / CoopCharData`
- `CooperationManager` -> `CoopCharData` - incoming / uses local type / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:178 / CoopCharData`
- `CooperationManager` -> `CoopCharState` - incoming / has field type / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:24 / Dictionary<string, CoopCharState>`
- `CooperationManager` -> `CoopCharState` - incoming / returns / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:204 / List<CoopCharState>`
- `PartyView` -> `CoopCharState` - incoming / uses local type / 1 refs
  - Evidence: `Player/PartyView.cs:100 / List<CoopCharState>`
- `PlayerCharData` -> `CharData` - incoming / inherits / 1 refs
  - Evidence: `Player/PlayerCharData.cs:5 / CharData`

### Internal Method Calls

- None detected.

### Evidence

- Incoming calls member - CooperationManager -> CoopCharState / 11 refs
  - `Cooperator/CooperationManager.cs:74 / coopCharDict.TryGetValue(charID, out var charState)`
- Outgoing has field type - CoopCharData -> CardData / 5 refs
  - `Cooperator/CoopCharData.cs:8 / List<CardData>`
- Incoming creates - CooperationManager -> CoopCharState / 3 refs
  - `Cooperator/CooperationManager.cs:24 / Dictionary<string, CoopCharState>`
- Internal inherits - CoopCharData -> CharData / 1 refs
  - `Cooperator/CoopCharData.cs:5 / CharData`
- Internal has field type - CoopCharState -> CoopCharData / 1 refs
  - `Cooperator/CooperationManager.cs:10 / CoopCharData`

### Suggested AI Task

Use the Char context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: Holy

Anchor: `systems/holy.md`

### Role Estimate

Holy appears to be an internally dense area around holy, place, gameplay, char, in. It contains 3 types, including 2 Unity-facing types.

### Stats

- Types: 3
- Internal relationships: 2
- External relationships: 0
- Entry candidates: 1
- Keywords: `holy`, `place`, `gameplay`, `char`, `in`, `selectable`

### Start Here

- `HolyPlaceManager.Awake()` - unity_lifecycle / HolyPlace/HolyPlaceManager.cs:16

### Core Types

- `HolyPlaceData` - class / Unity / 1 out / 1 in
- `HolyPlaceManager` - class / Unity / 1 out / 0 in
- `SelectableCharDataInHolyPlace` - struct / 0 out / 1 in

### Likely Method Flows

- `HolyPlaceManager.Awake()`
  - `HolyPlaceManager.Awake() / terminal`

### Internal Type Relationships

- `HolyPlaceData` -> `SelectableCharDataInHolyPlace` - has field type / 1 refs
  - Evidence: `HolyPlace/HolyPlaceData.cs:8 / List<SelectableCharDataInHolyPlace>`
- `HolyPlaceManager` -> `HolyPlaceData` - has field type / 1 refs
  - Evidence: `HolyPlace/HolyPlaceManager.cs:14 / HolyPlaceData`

### External Touchpoints

- None detected.

### Internal Method Calls

- None detected.

### Evidence

- Likely flow - HolyPlaceManager.Awake() -> terminal
- Internal has field type - HolyPlaceData -> SelectableCharDataInHolyPlace / 1 refs
  - `HolyPlace/HolyPlaceData.cs:8 / List<SelectableCharDataInHolyPlace>`
- Internal has field type - HolyPlaceManager -> HolyPlaceData / 1 refs
  - `HolyPlace/HolyPlaceManager.cs:14 / HolyPlaceData`

### Suggested AI Task

Use the Holy context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: Rng

Anchor: `systems/rng.md`

### Role Estimate

Rng appears to be an externally connected area around rng, run, stream. It contains 2 types, including 2 plain C# types.

### Stats

- Types: 2
- Internal relationships: 3
- External relationships: 4
- Entry candidates: 0
- Keywords: `rng`, `run`, `stream`

### Start Here

- None detected.

### Core Types

- `RunRng` - class / 4 out / 3 in
- `RngStream` - enum / 0 out / 3 in

### Likely Method Flows

- No internal method flow detected.

### Internal Type Relationships

- `RunRng` -> `RngStream` - accepts parameter / 2 refs
  - Evidence: `Common/RunRng.cs:28 / RngStream`
- `RunRng` -> `RngStream` - has field type / 1 refs
  - Evidence: `Common/RunRng.cs:24 / Dictionary<RngStream, System.Random>`

### External Touchpoints

- `BattleManager` -> `RunRng` - incoming / calls member / 1 refs
  - Evidence: `Gameplay/BattleManager.cs:474 / RunRng.For(RngStream.Reward, RunData.Instance.currentFloor, RunData.Instance.currentNodeIndex)`
- `GameManager` -> `RunRng` - incoming / calls member / 1 refs
  - Evidence: `Gameplay/GameManager.cs:115 / RunRng.SeedFor(RngStream.Battle, RunData.Instance.currentFloor, RunData.Instance.currentNodeIndex)`
- `MapUIController` -> `RunRng` - incoming / calls member / 1 refs
  - Evidence: `Map/MapUIController.cs:216 / RunRng.For(RngStream.Event, node.floorIndex, node.nodeIndex)`
- `RunRng` -> `SeedUtil` - outgoing / calls member / 1 refs
  - Evidence: `Common/RunRng.cs:39 / SeedUtil.Mix(RunData.Instance.mapData.seed, floor, node, (int)stream)`

### Internal Method Calls

- `RunRng.For(RngStream, int, int)` -> `RunRng.SeedFor(RngStream, int, int)` / 1 refs
  - Evidence: `Common/RunRng.cs:30 / SeedFor(stream, floor, node)`

### Evidence

- Internal call - RunRng.For(RngStream, int, int) -> RunRng.SeedFor(RngStream, int, int)
  - `Common/RunRng.cs:30 / SeedFor(stream, floor, node)`
- Incoming calls member - BattleManager -> RunRng / 1 refs
  - `Gameplay/BattleManager.cs:474 / RunRng.For(RngStream.Reward, RunData.Instance.currentFloor, RunData.Instance.currentNodeIndex)`
- Incoming calls member - GameManager -> RunRng / 1 refs
  - `Gameplay/GameManager.cs:115 / RunRng.SeedFor(RngStream.Battle, RunData.Instance.currentFloor, RunData.Instance.currentNodeIndex)`
- Incoming calls member - MapUIController -> RunRng / 1 refs
  - `Map/MapUIController.cs:216 / RunRng.For(RngStream.Event, node.floorIndex, node.nodeIndex)`
- Internal accepts parameter - RunRng -> RngStream / 2 refs
  - `Common/RunRng.cs:28 / RngStream`
- Internal has field type - RunRng -> RngStream / 1 refs
  - `Common/RunRng.cs:24 / Dictionary<RngStream, System.Random>`

### Suggested AI Task

Use the Rng context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: Cooperation

Anchor: `systems/cooperation.md`

### Role Estimate

Cooperation appears to be an externally connected area around cooperation, cooperator. It contains 1 types, including 1 Unity-facing types.

### Stats

- Types: 1
- Internal relationships: 0
- External relationships: 29
- Entry candidates: 1
- Keywords: `cooperation`, `cooperator`

### Start Here

- `CooperationManager.Awake()` - unity_lifecycle / Cooperator/CooperationManager.cs:27

### Core Types

- `CooperationManager` - class / Unity / 24 out / 5 in

### Likely Method Flows

- `CooperationManager.Awake()`
  - `CooperationManager.Awake() / terminal`

### Internal Type Relationships

- None detected.

### External Touchpoints

- `CooperationManager` -> `CoopCharState` - outgoing / calls member / 11 refs
  - Evidence: `Cooperator/CooperationManager.cs:74 / coopCharDict.TryGetValue(charID, out var charState)`
- `DialogueManager` -> `CooperationManager` - incoming / calls member / 3 refs
  - Evidence: `Cooperator/DialogueManager.cs:32 / cooperationManager.IsCoopLevelUP(charID)`
- `CooperationManager` -> `CoopCharState` - outgoing / creates / 3 refs
  - Evidence: `Cooperator/CooperationManager.cs:24 / Dictionary<string, CoopCharState>`
- `CooperationManager` -> `CoopCharState` - outgoing / uses local type / 2 refs
  - Evidence: `Cooperator/CooperationManager.cs:199 / CoopCharState`
- `CharacterEventManager` -> `CooperationManager` - incoming / has field type / 1 refs
  - Evidence: `Cooperator/CharacterEventManager.cs:12 / CooperationManager`
- `CooperationManager` -> `CoopCharData` - outgoing / has field type / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:23 / List<CoopCharData>`
- `CooperationManager` -> `CoopCharData` - outgoing / returns / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:104 / CoopCharData`
- `CooperationManager` -> `CoopCharData` - outgoing / uses local type / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:178 / CoopCharData`
- `CooperationManager` -> `CoopCharState` - outgoing / has field type / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:24 / Dictionary<string, CoopCharState>`
- `CooperationManager` -> `CoopCharState` - outgoing / returns / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:204 / List<CoopCharState>`
- `CooperationManager` -> `RankEventData` - outgoing / uses local type / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:43 / Dictionary<int, RankEventData>`
- `DialogueManager` -> `CooperationManager` - incoming / has field type / 1 refs
  - Evidence: `Cooperator/DialogueManager.cs:10 / CooperationManager`
- `CooperationManager` -> `CoopCharData` - outgoing / creates / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:23 / List<CoopCharData>`
- `CooperationManager` -> `RankEventData` - outgoing / creates / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:43 / Dictionary<int, RankEventData>`

### Internal Method Calls

- `CooperationManager.SelectChar(string)` -> `CooperationManager.GetCoopLevel(string)` / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:179 / GetCoopLevel(charID)`
- `CooperationManager.SelectChar(string)` -> `CooperationManager.GetCoopCharData(string)` / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:178 / GetCoopCharData(charID)`
- `CooperationManager.Debug1()` -> `CooperationManager.AddCoopPoint(string, int)` / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:221 / AddCoopPoint("Cp_01", 10)`
- `CooperationManager.Debug2()` -> `CooperationManager.SettlePoint(string)` / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:227 / SettlePoint("Cp_01")`
- `CooperationManager.TestJoinCp01()` -> `CooperationManager.DebugJoin(string)` / 1 refs
  - Evidence: `Cooperator/CooperationManager.cs:252 / DebugJoin("Cp_01")`

### Evidence

- Likely flow - CooperationManager.Awake() -> terminal
- Internal call - CooperationManager.SelectChar(string) -> CooperationManager.GetCoopLevel(string)
  - `Cooperator/CooperationManager.cs:179 / GetCoopLevel(charID)`
- Internal call - CooperationManager.SelectChar(string) -> CooperationManager.GetCoopCharData(string)
  - `Cooperator/CooperationManager.cs:178 / GetCoopCharData(charID)`
- Internal call - CooperationManager.Debug1() -> CooperationManager.AddCoopPoint(string, int)
  - `Cooperator/CooperationManager.cs:221 / AddCoopPoint("Cp_01", 10)`
- Outgoing calls member - CooperationManager -> CoopCharState / 11 refs
  - `Cooperator/CooperationManager.cs:74 / coopCharDict.TryGetValue(charID, out var charState)`
- Incoming calls member - DialogueManager -> CooperationManager / 3 refs
  - `Cooperator/DialogueManager.cs:32 / cooperationManager.IsCoopLevelUP(charID)`
- Outgoing creates - CooperationManager -> CoopCharState / 3 refs
  - `Cooperator/CooperationManager.cs:24 / Dictionary<string, CoopCharState>`

### Suggested AI Task

Use the Cooperation context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: Cooperator

Anchor: `systems/cooperator.md`

### Role Estimate

Cooperator appears to be an externally connected area around cooperator, dialogue. It contains 1 types, including 1 Unity-facing types.

### Stats

- Types: 1
- Internal relationships: 0
- External relationships: 8
- Entry candidates: 2
- Keywords: `cooperator`, `dialogue`

### Start Here

- `DialogueManager.LoadRelationshipEvent(string)` - flow_candidate / Cooperator/DialogueManager.cs:19
- `DialogueManager.Start()` - unity_lifecycle / Cooperator/DialogueManager.cs:14

### Core Types

- `DialogueManager` - class / Unity / 6 out / 2 in

### Likely Method Flows

- `DialogueManager.LoadRelationshipEvent(string)`
  - `DialogueManager.LoadRelationshipEvent(string) / terminal`
- `DialogueManager.Start()`
  - `DialogueManager.Start() / terminal`

### Internal Type Relationships

- None detected.

### External Touchpoints

- `DialogueManager` -> `CooperationManager` - outgoing / calls member / 3 refs
  - Evidence: `Cooperator/DialogueManager.cs:32 / cooperationManager.IsCoopLevelUP(charID)`
- `CharacterEventManager` -> `DialogueManager` - incoming / has field type / 1 refs
  - Evidence: `Cooperator/CharacterEventManager.cs:11 / DialogueManager`
- `DialogueManager` -> `CooperationManager` - outgoing / has field type / 1 refs
  - Evidence: `Cooperator/DialogueManager.cs:10 / CooperationManager`
- `DialogueManager` -> `DialogueView` - outgoing / has field type / 1 refs
  - Evidence: `Cooperator/DialogueManager.cs:9 / DialogueView`
- `CharacterEventManager` -> `DialogueManager` - incoming / calls member / 1 refs
  - Evidence: `Cooperator/CharacterEventManager.cs:32 / dialogueManager.LoadRelationshipEvent("Cp_01")`
- `DialogueManager` -> `DialogueView` - outgoing / calls member / 1 refs
  - Evidence: `Cooperator/DialogueManager.cs:49 / dialogueView.Play(dialogueJson, () => isDialogueEnd?.Invoke())`

### Internal Method Calls

- None detected.

### Evidence

- Likely flow - DialogueManager.LoadRelationshipEvent(string) -> terminal
- Likely flow - DialogueManager.Start() -> terminal
- Outgoing calls member - DialogueManager -> CooperationManager / 3 refs
  - `Cooperator/DialogueManager.cs:32 / cooperationManager.IsCoopLevelUP(charID)`
- Incoming has field type - CharacterEventManager -> DialogueManager / 1 refs
  - `Cooperator/CharacterEventManager.cs:11 / DialogueManager`
- Outgoing has field type - DialogueManager -> CooperationManager / 1 refs
  - `Cooperator/DialogueManager.cs:10 / CooperationManager`

### Suggested AI Task

Use the Cooperator context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: Game

Anchor: `systems/game.md`

### Role Estimate

Game appears to be an externally connected area around game, gameplay. It contains 1 types, including 1 Unity-facing types.

### Stats

- Types: 1
- Internal relationships: 0
- External relationships: 37
- Entry candidates: 5
- Keywords: `game`, `gameplay`

### Start Here

- `GameManager.InitializeRun()` - flow_candidate / Gameplay/GameManager.cs:74
- `GameManager.Start()` - unity_lifecycle / Gameplay/GameManager.cs:61
- `GameManager.InitializeBattle()` - flow_candidate / Gameplay/GameManager.cs:98
- `GameManager.Awake()` - unity_lifecycle / Gameplay/GameManager.cs:17
- `GameManager.Update()` - unity_lifecycle / Gameplay/GameManager.cs:66

### Core Types

- `GameManager` - class / Unity / 37 out / 0 in

### Likely Method Flows

- `GameManager.InitializeRun()`
  - `GameManager.InitializeRun()`
  - `GameManager.InitializeBattle() / terminal`
- `GameManager.Start()`
  - `GameManager.Start()`
  - `GameManager.InitializeRun()`
  - `GameManager.InitializeBattle() / terminal`
- `GameManager.InitializeBattle()`
  - `GameManager.InitializeBattle() / terminal`
- `GameManager.Awake()`
  - `GameManager.Awake() / terminal`

### Internal Type Relationships

- None detected.

### External Touchpoints

- `GameManager` -> `CardData` - outgoing / creates / 9 refs
  - Evidence: `Gameplay/GameManager.cs:37 / Dictionary<CardData.CardRarity, List<CardData>>`
- `GameManager` -> `CardData` - outgoing / calls member / 4 refs
  - Evidence: `Gameplay/GameManager.cs:131 / pool.Contains(card)`
- `GameManager` -> `CardData` - outgoing / accepts parameter / 2 refs
  - Evidence: `Gameplay/GameManager.cs:123 / CardData`
- `GameManager` -> `CardData+CardRarity` - outgoing / accepts parameter / 2 refs
  - Evidence: `Gameplay/GameManager.cs:136 / CardData.CardRarity`
- `GameManager` -> `CardData+CardRarity` - outgoing / has field type / 2 refs
  - Evidence: `Gameplay/GameManager.cs:37 / Dictionary<CardData.CardRarity, List<CardData>>`
- `GameManager` -> `CardData+CardRarity` - outgoing / calls member / 2 refs
  - Evidence: `Gameplay/GameManager.cs:126 / cardPools.TryGetValue(card.Rarity, out var pool)`
- `GameManager` -> `RewardProbabilityData` - outgoing / calls member / 2 refs
  - Evidence: `Gameplay/GameManager.cs:166 / rewardProbabilityTable.Find(t => t.Chapter == chapter && t.BattleType == battleType)`
- `GameManager` -> `CardData+CardRarity` - outgoing / creates / 2 refs
  - Evidence: `Gameplay/GameManager.cs:37 / Dictionary<CardData.CardRarity, List<CardData>>`
- `GameManager` -> `BattleType` - outgoing / accepts parameter / 1 refs
  - Evidence: `Gameplay/GameManager.cs:164 / BattleType`
- `GameManager` -> `BattleType` - outgoing / uses local type / 1 refs
  - Evidence: `Gameplay/GameManager.cs:100 / BattleType`
- `GameManager` -> `CardData` - outgoing / has field type / 1 refs
  - Evidence: `Gameplay/GameManager.cs:37 / Dictionary<CardData.CardRarity, List<CardData>>`
- `GameManager` -> `EnemyData` - outgoing / has field type / 1 refs
  - Evidence: `Gameplay/GameManager.cs:30 / List<EnemyData>`
- `GameManager` -> `GamePhase` - outgoing / accepts parameter / 1 refs
  - Evidence: `Gameplay/GameManager.cs:15 / GamePhase`
- `GameManager` -> `GamePhase` - outgoing / has property type / 1 refs
  - Evidence: `Gameplay/GameManager.cs:13 / GamePhase`
- `GameManager` -> `ItemData` - outgoing / has field type / 1 refs
  - Evidence: `Gameplay/GameManager.cs:39 / List<ItemData>`
- `GameManager` -> `PlayerRewardPoolSO` - outgoing / has field type / 1 refs
  - Evidence: `Gameplay/GameManager.cs:34 / PlayerRewardPoolSO`

### Internal Method Calls

- `GameManager.Start()` -> `GameManager.InitializeRun()` / 1 refs
  - Evidence: `Gameplay/GameManager.cs:63 / InitializeRun()`
- `GameManager.InitializeRun()` -> `GameManager.InitializeBattle()` / 1 refs
  - Evidence: `Gameplay/GameManager.cs:93 / InitializeBattle()`

### Evidence

- Likely flow - GameManager.InitializeRun() -> GameManager.InitializeBattle() / terminal
- Likely flow - GameManager.Start() -> GameManager.InitializeRun() -> GameManager.InitializeBattle() / terminal
- Internal call - GameManager.Start() -> GameManager.InitializeRun()
  - `Gameplay/GameManager.cs:63 / InitializeRun()`
- Internal call - GameManager.InitializeRun() -> GameManager.InitializeBattle()
  - `Gameplay/GameManager.cs:93 / InitializeBattle()`
- Outgoing creates - GameManager -> CardData / 9 refs
  - `Gameplay/GameManager.cs:37 / Dictionary<CardData.CardRarity, List<CardData>>`
- Outgoing calls member - GameManager -> CardData / 4 refs
  - `Gameplay/GameManager.cs:131 / pool.Contains(card)`
- Outgoing accepts parameter - GameManager -> CardData / 2 refs
  - `Gameplay/GameManager.cs:123 / CardData`

### Suggested AI Task

Use the Game context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## System: Inventory

Anchor: `systems/inventory.md`

### Role Estimate

Inventory appears to be an externally connected area around inventory. It contains 1 types, including 1 Unity-facing types.

### Stats

- Types: 1
- Internal relationships: 0
- External relationships: 8
- Entry candidates: 1
- Keywords: `inventory`

### Start Here

- `ItemInventoryManager.Awake()` - unity_lifecycle / Item/ItemInventoryManager.cs:10

### Core Types

- `ItemInventoryManager` - class / Unity / 8 out / 0 in

### Likely Method Flows

- `ItemInventoryManager.Awake()`
  - `ItemInventoryManager.Awake() / terminal`

### Internal Type Relationships

- None detected.

### External Touchpoints

- `ItemInventoryManager` -> `ItemData` - outgoing / calls member / 4 refs
  - Evidence: `Item/ItemInventoryManager.cs:29 / _items.Add(Instantiate(item))`
- `ItemInventoryManager` -> `ItemData` - outgoing / accepts parameter / 2 refs
  - Evidence: `Item/ItemInventoryManager.cs:22 / ItemData`
- `ItemInventoryManager` -> `ItemData` - outgoing / has field type / 1 refs
  - Evidence: `Item/ItemInventoryManager.cs:16 / List<ItemData>`
- `ItemInventoryManager` -> `ItemData` - outgoing / has property type / 1 refs
  - Evidence: `Item/ItemInventoryManager.cs:19 / IReadOnlyList<ItemData>`

### Internal Method Calls

- None detected.

### Evidence

- Likely flow - ItemInventoryManager.Awake() -> terminal
- Outgoing calls member - ItemInventoryManager -> ItemData / 4 refs
  - `Item/ItemInventoryManager.cs:29 / _items.Add(Instantiate(item))`
- Outgoing accepts parameter - ItemInventoryManager -> ItemData / 2 refs
  - `Item/ItemInventoryManager.cs:22 / ItemData`
- Outgoing has field type - ItemInventoryManager -> ItemData / 1 refs
  - `Item/ItemInventoryManager.cs:16 / List<ItemData>`

### Suggested AI Task

Use the Inventory context above to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

## Notes

- This context intentionally omits full source code. Use the listed files and methods as a reading map.
- If a system looks wrong, regenerate the graph after updating analyzer rules or changing system grouping.
- Cached AI walkthroughs are not included in this deterministic export.
