# Action Event Pipeline

Action Event Pipeline appears to be an externally connected area around effect, event, card, effects, choice. It contains 19 types, including 3 Unity-facing types.

## Stats

- Types: 19
- Internal relationships: 6
- External relationships: 94
- Entry candidates: 7
- Keywords: `effect`, `event`, `card`, `effects`, `choice`, `add`, `cooperator`, `dialogue`, `json`, `status`, `ammo`, `apply`

## Start Here

- `CharacterEventManager.Start()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CharacterEventManager.cs:14
- `CharacterEventManager.OnDisable()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CharacterEventManager.cs:20
- `CharacterEventManager.Update()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CharacterEventManager.cs:25
- `EventView.Awake()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:37
- `HitEffectSpawner.Spawn(GameObject, Transform, float)` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\HitEffectSpawner.cs:10
- `ApplyStatusEffect.CreatePassive()` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:39
- `EventView.BuildChoices()` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:105

## Core Types

- `StatusEffectBase` - class / 15 out / 16 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Passive\StatusEffectBase.cs:3
- `DamageEffect` - class / 15 out / 1 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\DamageEffect.cs:5
- `ApplyStatusEffect` - class / 11 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:4
- `EventView` - class / Unity / 8 out / 2 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:9
- `EventData` - class / Unity / 1 out / 3 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventData.cs:8
- `HealEffect` - class / 2 out / 2 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\HealEffect.cs:4
- `RankEventData` - class / 0 out / 4 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CoopCharData.cs:29
- `CharacterEventManager` - class / Unity / 3 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CharacterEventManager.cs:6
- `DialogueChoiceOption` - class / 1 out / 2 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueScriptData.cs:16
- `HitEffectSpawner` - class / 0 out / 3 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\HitEffectSpawner.cs:8
- `AddAmmoEffect` - class / 2 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\AddAmmoEffect.cs:4
- `AddCoopPointEffect` - class / 2 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\AddCoopPointEffect.cs:5
- `BlockEffect` - class / 2 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\BlockEffect.cs:4
- `DrawEffect` - class / 2 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\DrawEffect.cs:4
- `EventChoiceEffect` - class / 1 out / 1 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventData.cs:24
- `EventJsonData` - class / 1 out / 1 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventJsonData.cs:7
- `GainGoldEffect` - class / 2 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\GainGoldEffect.cs:4
- `LoseHPEffect` - class / 2 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\LoseHPEffect.cs:4
- `EventChoiceJson` - class / 0 out / 1 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventJsonData.cs:15

## Likely Method Flows

- `EventView.BuildChoices()`
  - `EventView.BuildChoices()`
  - `EventView.HideChoices() / terminal`
- `CharacterEventManager.Update()`
  - `CharacterEventManager.Update() / terminal`
- `CharacterEventManager.Start()`
  - `CharacterEventManager.Start() / terminal`
- `CharacterEventManager.OnDisable()`
  - `CharacterEventManager.OnDisable() / terminal`
- `EventView.Awake()`
  - `EventView.Awake() / terminal`
- `HitEffectSpawner.Spawn(GameObject, Transform, float)`
  - `HitEffectSpawner.Spawn(GameObject, Transform, float) / terminal`
- `ApplyStatusEffect.CreatePassive()`
  - `ApplyStatusEffect.CreatePassive() / terminal`

## Internal Type Relationships

- `EventView` -> `EventData` - internal / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:47 / EventData`
- `EventData` -> `EventChoiceEffect` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventData.cs:13 / List<EventChoiceEffect>`
- `EventJsonData` -> `EventChoiceJson` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventJsonData.cs:11 / EventChoiceJson[]`
- `EventView` -> `EventData` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:33 / EventData`
- `EventView` -> `EventJsonData` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:34 / EventJsonData`
- `ApplyStatusEffect` -> `StatusEffectBase` - internal / returns / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:39 / StatusEffectBase`

## External Touchpoints

- `StatusEffectBase` -> `BattleState` - outgoing / accepts_parameter / 5 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Passive\StatusEffectBase.cs:25 / BattleState`
- `StatusEffectBase` -> `ICombatant` - outgoing / accepts_parameter / 4 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Passive\StatusEffectBase.cs:25 / ICombatant`
- `DamageEffect` -> `DamageCalculator` - outgoing / calls_member / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\DamageEffect.cs:23 / DamageCalculator.Resolve(new DamageInfo(amount, attacker), single, context.State)`
- `DamageEffect` -> `DamageInfo` - outgoing / creates / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\DamageEffect.cs:23 / DamageInfo`
- `DamageEffect` -> `CardContext` - outgoing / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\DamageEffect.cs:11 / CardContext`
- `StatusEffectBase` -> `DamageInfo` - outgoing / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Passive\StatusEffectBase.cs:28 / DamageInfo`
- `EnemyInstance` -> `StatusEffectBase` - incoming / calls_member / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Enemy\EnemyInstance.cs:89 / existing.TryMerge(passive)`
- `PlayerCombatant` -> `StatusEffectBase` - incoming / calls_member / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:54 / existing.TryMerge(passive)`
- `PlayerHUDView` -> `HitEffectSpawner` - incoming / calls_member / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerHUDView.cs:128 / HitEffectSpawner.Spawn(hitEffectPrefab, anchor)`
- `StatusEffectBase` -> `DamageInfo` - outgoing / returns / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Passive\StatusEffectBase.cs:28 / DamageInfo`
- `EnemyInstance` -> `StatusEffectBase` - incoming / type_check / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Enemy\EnemyInstance.cs:89 / StatusEffectBase`
- `PlayerCombatant` -> `StatusEffectBase` - incoming / type_check / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:54 / StatusEffectBase`
- `DamageEffect` -> `ICombatant` - outgoing / uses_local_type / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\DamageEffect.cs:15 / ICombatant`
- `AddAmmoEffect` -> `CardContext` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\AddAmmoEffect.cs:8 / CardContext`
- `AddCoopPointEffect` -> `CardContext` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\AddCoopPointEffect.cs:10 / CardContext`
- `ApplyStatusEffect` -> `CardContext` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:10 / CardContext`
- `ApplyStatusEffect` -> `EnemyInstance` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:33 / EnemyInstance`
- `BlockEffect` -> `CardContext` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\BlockEffect.cs:8 / CardContext`
- `DialogueView` -> `DialogueChoiceOption` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:185 / DialogueChoiceOption`
- `DrawEffect` -> `CardContext` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\DrawEffect.cs:8 / CardContext`
- `EnemyInstance` -> `StatusEffectBase` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Enemy\EnemyInstance.cs:85 / StatusEffectBase`
- `GainGoldEffect` -> `CardContext` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\GainGoldEffect.cs:8 / CardContext`
- `HealEffect` -> `CardContext` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\HealEffect.cs:8 / CardContext`
- `LoseHPEffect` -> `CardContext` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\LoseHPEffect.cs:8 / CardContext`
- `PlayerCombatant` -> `StatusEffectBase` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\PlayerCombatant.cs:50 / StatusEffectBase`
- `StatusEffectBase` -> `CardContext` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Passive\StatusEffectBase.cs:27 / CardContext`
- `ApplyStatusEffect` -> `EnemyInstance` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:36 / enemy.AddPassive(passive)`
- `CharacterEventManager` -> `DialogueManager` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CharacterEventManager.cs:32 / dialogueManager.LoadRelationshipEvent("Cp_01")`
- `DamageEffect` -> `EnemyInstance` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\DamageEffect.cs:38 / alive.Add(e)`
- `EnemyView` -> `HitEffectSpawner` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Enemy\EnemyView.cs:153 / HitEffectSpawner.Spawn(hitEffectPrefab, anchor)`
- `EventView` -> `DialogueView` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:82 / dialogueView.Play(_data.dialogueJson, ShowDescriptionAndChoices)`
- `EventView` -> `MapUIController` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:156 / mapUIController.OpenMap()`
- `MapUIController` -> `EventView` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:218 / eventView.Open(data)`
- `RestView` -> `HealEffect` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Rest\RestView.cs:60 / restHealEffect.Execute(new CardContext())`
- `ApplyStatusEffect` -> `PoisonStatus` - outgoing / creates / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:44 / PoisonStatus`
- `ApplyStatusEffect` -> `StrengthStatus` - outgoing / creates / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:43 / StrengthStatus`

## Internal Method Calls

- `ApplyStatusEffect.Execute(CardContext)` -> `ApplyStatusEffect.ApplyTo(EnemyInstance)` / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:18 / ApplyTo(context.Target)`
- `EventView.Open(EventData)` -> `EventView.ShowDescriptionAndChoices()` / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:78 / ShowDescriptionAndChoices()`
- `ApplyStatusEffect.Execute(CardContext)` -> `ApplyStatusEffect.CreatePassive()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:27 / CreatePassive()`
- `DamageEffect.Execute(CardContext)` -> `DamageEffect.ResolveSingleTarget(CardContext)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\DamageEffect.cs:20 / ResolveSingleTarget(context)`
- `ApplyStatusEffect.ApplyTo(EnemyInstance)` -> `ApplyStatusEffect.CreatePassive()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:35 / CreatePassive()`
- `EventView.Open(EventData)` -> `EventView.HideChoices()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:69 / HideChoices()`
- `EventView.ShowDescriptionAndChoices()` -> `EventView.BuildChoices()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:95 / BuildChoices()`
- `EventView.BuildChoices()` -> `EventView.HideChoices()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:107 / HideChoices()`
- `EventView.BuildChoices()` -> `EventView.OnChoiceSelected(int)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:114 / OnChoiceSelected(index)`

## Evidence

- Likely flow - EventView.BuildChoices() -> EventView.HideChoices() / terminal
- Likely flow - CharacterEventManager.Update() -> 
- Internal call - ApplyStatusEffect.Execute(CardContext) -> ApplyStatusEffect.ApplyTo(EnemyInstance)
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:18 / ApplyTo(context.Target)`
- Internal call - EventView.Open(EventData) -> EventView.ShowDescriptionAndChoices()
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:78 / ShowDescriptionAndChoices()`
- Internal call - ApplyStatusEffect.Execute(CardContext) -> ApplyStatusEffect.CreatePassive()
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:27 / CreatePassive()`
- outgoing accepts_parameter - StatusEffectBase -> BattleState / 5 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Passive\StatusEffectBase.cs:25 / BattleState`
- outgoing accepts_parameter - StatusEffectBase -> ICombatant / 4 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Passive\StatusEffectBase.cs:25 / ICombatant`
- outgoing calls_member - DamageEffect -> DamageCalculator / 3 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\DamageEffect.cs:23 / DamageCalculator.Resolve(new DamageInfo(amount, attacker), single, context.State)`
- Internal accepts_parameter - EventView -> EventData / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:47 / EventData`
- Internal has_field_type - EventData -> EventChoiceEffect / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventData.cs:13 / List<EventChoiceEffect>`
- Internal has_field_type - EventJsonData -> EventChoiceJson / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventJsonData.cs:11 / EventChoiceJson[]`
- Internal has_field_type - EventView -> EventData / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:33 / EventData`
- Internal has_field_type - EventView -> EventJsonData / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:34 / EventJsonData`
- Internal returns - ApplyStatusEffect -> StatusEffectBase / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\Effects\ApplyStatusEffect.cs:39 / StatusEffectBase`

## Suggested AI Task

Use the Action Event Pipeline context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

