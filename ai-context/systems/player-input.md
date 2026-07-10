# Player / Input

Player / Input appears to be an externally connected area around player, gameplay, char, combatant, hud. It contains 3 types, including 1 Unity-facing types.

## Stats

- Types: 3
- Internal relationships: 2
- External relationships: 19
- Entry candidates: 4
- Keywords: `player`, `gameplay`, `char`, `combatant`, `hud`

## Start Here

- `PlayerHUDView.Update()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:50
- `PlayerHUDView.OnDestroy()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:120
- `PlayerHUDView.SpawnHitEffect()` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:125
- `PlayerHUDView.SpawnBlockEffect()` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:131

## Core Types

- `PlayerCombatant` - class / 12 out / 4 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:8
- `PlayerHUDView` - class / Unity / 4 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:19
- `PlayerCharData` - class / 3 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Player\PlayerCharData.cs:5

## Likely Method Flows

- `PlayerHUDView.Update()`
  - `PlayerHUDView.Update()`
  - `PlayerHUDView.UpdateBlockedSubscription() / terminal`
- `PlayerHUDView.SpawnHitEffect()`
  - `PlayerHUDView.SpawnHitEffect() / terminal`
- `PlayerHUDView.SpawnBlockEffect()`
  - `PlayerHUDView.SpawnBlockEffect() / terminal`
- `PlayerHUDView.OnDestroy()`
  - `PlayerHUDView.OnDestroy() / terminal`

## Internal Type Relationships

- `PlayerHUDView` -> `PlayerCombatant` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:48 / PlayerCombatant`
- `PlayerHUDView` -> `PlayerCombatant` - internal / uses_local_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:110 / PlayerCombatant`

## External Touchpoints

- `PlayerCombatant` -> `IPassiveLogic` - outgoing / calls_member / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:57 / _passives.Add(passive)`
- `PlayerCombatant` -> `StatusEffectBase` - outgoing / calls_member / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:54 / existing.TryMerge(passive)`
- `PlayerHUDView` -> `HitEffectSpawner` - outgoing / calls_member / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:128 / HitEffectSpawner.Spawn(hitEffectPrefab, anchor)`
- `PlayerCharData` -> `CardData` - outgoing / has_field_type / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Player\PlayerCharData.cs:14 / List<CardData>`
- `PlayerCombatant` -> `StatusEffectBase` - outgoing / type_check / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:54 / StatusEffectBase`
- `PlayerCombatant` -> `BattleState` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:60 / BattleState`
- `PlayerCombatant` -> `DamageInfo` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:23 / DamageInfo`
- `PlayerCombatant` -> `StatusEffectBase` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:50 / StatusEffectBase`
- `BattleManager` -> `PlayerCombatant` - incoming / creates / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\BattleManager.cs:72 / PlayerCombatant`
- `BattleState` -> `PlayerCombatant` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\BattleState.cs:9 / PlayerCombatant`
- `PlayerCombatant` -> `IPassiveLogic` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:11 / List<IPassiveLogic>`
- `PlayerCombatant` -> `IPassiveLogic` - outgoing / has_property_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:17 / List<IPassiveLogic>`
- `PlayerCombatant` -> `ICombatant` - outgoing / implements / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:8 / ICombatant`
- `PlayerCharData` -> `CharData` - outgoing / inherits / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Player\PlayerCharData.cs:5 / CharData`

## Internal Method Calls

- `PlayerHUDView.Update()` -> `PlayerHUDView.SpawnHitEffect()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:61 / SpawnHitEffect()`
- `PlayerHUDView.Update()` -> `PlayerHUDView.UpdateBlockedSubscription()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:52 / UpdateBlockedSubscription()`
- `PlayerHUDView.HandleBlocked(int)` -> `PlayerHUDView.SpawnBlockEffect()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:118 / SpawnBlockEffect()`

## Evidence

- Likely flow - PlayerHUDView.Update() -> PlayerHUDView.UpdateBlockedSubscription() / terminal
- Likely flow - PlayerHUDView.SpawnHitEffect() -> 
- Internal call - PlayerHUDView.Update() -> PlayerHUDView.SpawnHitEffect()
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:61 / SpawnHitEffect()`
- Internal call - PlayerHUDView.Update() -> PlayerHUDView.UpdateBlockedSubscription()
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:52 / UpdateBlockedSubscription()`
- Internal call - PlayerHUDView.HandleBlocked(int) -> PlayerHUDView.SpawnBlockEffect()
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:118 / SpawnBlockEffect()`
- outgoing calls_member - PlayerCombatant -> IPassiveLogic / 2 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:57 / _passives.Add(passive)`
- outgoing calls_member - PlayerCombatant -> StatusEffectBase / 2 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:54 / existing.TryMerge(passive)`
- outgoing calls_member - PlayerHUDView -> HitEffectSpawner / 2 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:128 / HitEffectSpawner.Spawn(hitEffectPrefab, anchor)`
- Internal has_field_type - PlayerHUDView -> PlayerCombatant / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:48 / PlayerCombatant`
- Internal uses_local_type - PlayerHUDView -> PlayerCombatant / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:110 / PlayerCombatant`

## Suggested AI Task

Use the Player / Input context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

