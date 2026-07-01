# Game

Game appears to be an externally connected area around game, gameplay. It contains 1 types, including 1 Unity-facing types.

## Stats

- Types: 1
- Internal relationships: 0
- External relationships: 24
- Entry candidates: 5
- Keywords: `game`, `gameplay`

## Start Here

- `GameManager.Awake()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:17
- `GameManager.Start()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:62
- `GameManager.Update()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:68
- `GameManager.InitializeRun()` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:76
- `GameManager.InitializeBattle()` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:102

## Core Types

- `GameManager` - class / Unity / 24 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:9

## Likely Method Flows

- `GameManager.InitializeRun()`
  - `GameManager.InitializeRun()`
  - `GameManager.InitializeBattle() / terminal`
- `GameManager.Start()`
  - `GameManager.Start()`
  - `GameManager.InitializeRun()`
  - `GameManager.InitializeBattle() / terminal`
- `GameManager.Awake()`
  - `GameManager.Awake() / terminal`
- `GameManager.Update()`
  - `GameManager.Update() / terminal`
- `GameManager.InitializeBattle()`
  - `GameManager.InitializeBattle() / terminal`

## Internal Type Relationships

- None detected.

## External Touchpoints

- `GameManager` -> `CardData` - outgoing / has_field_type / 4 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:35 / Dictionary<CardData.CardRarity, List<CardData>>`
- `GameManager` -> `CardData` - outgoing / calls_member / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:132 / pool.Contains(card)`
- `GameManager` -> `CardData` - outgoing / creates / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:35 / Dictionary<CardData.CardRarity, List<CardData>>`
- `GameManager` -> `CardData+CardRarity` - outgoing / creates / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:35 / Dictionary<CardData.CardRarity, List<CardData>>`
- `GameManager` -> `CardData+CardRarity` - outgoing / has_field_type / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:35 / Dictionary<CardData.CardRarity, List<CardData>>`
- `GameManager` -> `BattleType` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:151 / BattleType`
- `GameManager` -> `CardData` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:122 / CardData`
- `GameManager` -> `GamePhase` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:15 / GamePhase`
- `GameManager` -> `BattleReward` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:89 / BattleReward.SetItemRewardsPool(itemPool)`
- `GameManager` -> `CardData+CardRarity` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:126 / cardPools.TryGetValue(card.Rarity, out var pool)`
- `GameManager` -> `RewardProbabilityData` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:153 / rewardProbabilityTable.Find(t => t.Chapter == chapter && t.BattleType == battleType)`
- `GameManager` -> `EnemyData` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:30 / List<EnemyData>`
- `GameManager` -> `ItemData` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:40 / List<ItemData>`
- `GameManager` -> `RewardProbabilityData` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:42 / List<RewardProbabilityData>`
- `GameManager` -> `GamePhase` - outgoing / has_property_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:13 / GamePhase`
- `GameManager` -> `RewardProbabilityData` - outgoing / returns / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:151 / RewardProbabilityData`
- `GameManager` -> `BattleType` - outgoing / uses_local_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:104 / BattleType`

## Internal Method Calls

- `GameManager.Start()` -> `GameManager.InitializeRun()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:65 / InitializeRun()`
- `GameManager.InitializeRun()` -> `GameManager.InitializeBattle()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:97 / InitializeBattle()`

## Evidence

- Likely flow - GameManager.InitializeRun() -> GameManager.InitializeBattle() / terminal
- Likely flow - GameManager.Start() -> GameManager.InitializeRun() -> GameManager.InitializeBattle() / terminal
- Internal call - GameManager.Start() -> GameManager.InitializeRun()
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:65 / InitializeRun()`
- Internal call - GameManager.InitializeRun() -> GameManager.InitializeBattle()
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:97 / InitializeBattle()`
- outgoing has_field_type - GameManager -> CardData / 4 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:35 / Dictionary<CardData.CardRarity, List<CardData>>`
- outgoing calls_member - GameManager -> CardData / 2 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:132 / pool.Contains(card)`
- outgoing creates - GameManager -> CardData / 2 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\GameManager.cs:35 / Dictionary<CardData.CardRarity, List<CardData>>`

## Suggested AI Task

Use the Game context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

