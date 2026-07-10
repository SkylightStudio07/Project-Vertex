# Cooperation

Cooperation appears to be an externally connected area around cooperation, cooperator. It contains 1 types, including 1 Unity-facing types.

## Stats

- Types: 1
- Internal relationships: 0
- External relationships: 30
- Entry candidates: 1
- Keywords: `cooperation`, `cooperator`

## Start Here

- `CooperationManager.Awake()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:27

## Core Types

- `CooperationManager` - class / Unity / 25 out / 5 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:19

## Likely Method Flows

- `CooperationManager.Awake()`
  - `CooperationManager.Awake() / terminal`

## Internal Type Relationships

- None detected.

## External Touchpoints

- `CooperationManager` -> `CoopCharState` - outgoing / calls_member / 11 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:74 / coopCharDict.TryGetValue(charID, out var charState)`
- `DialogueManager` -> `CooperationManager` - incoming / calls_member / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:32 / cooperationManager.IsCoopLevelUP(charID)`
- `CooperationManager` -> `CoopCharState` - outgoing / creates / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:24 / Dictionary<string, CoopCharState>`
- `CooperationManager` -> `CoopCharState` - outgoing / uses_local_type / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:199 / CoopCharState`
- `CooperationManager` -> `CoopCharData` - outgoing / creates / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:23 / List<CoopCharData>`
- `CooperationManager` -> `RankEventData` - outgoing / creates / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:43 / Dictionary<int, RankEventData>`
- `CharacterEventManager` -> `CooperationManager` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CharacterEventManager.cs:12 / CooperationManager`
- `CooperationManager` -> `CoopCharData` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:23 / List<CoopCharData>`
- `CooperationManager` -> `CoopCharState` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:24 / Dictionary<string, CoopCharState>`
- `DialogueManager` -> `CooperationManager` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:10 / CooperationManager`
- `CooperationManager` -> `CoopCharData` - outgoing / returns / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:104 / CoopCharData`
- `CooperationManager` -> `CoopCharState` - outgoing / returns / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:204 / List<CoopCharState>`
- `CooperationManager` -> `CardData` - outgoing / uses_local_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:187 / CardData`
- `CooperationManager` -> `CoopCharData` - outgoing / uses_local_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:178 / CoopCharData`
- `CooperationManager` -> `RankEventData` - outgoing / uses_local_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:43 / Dictionary<int, RankEventData>`

## Internal Method Calls

- `CooperationManager.SelectChar(string)` -> `CooperationManager.GetCoopCharData(string)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:178 / GetCoopCharData(charID)`
- `CooperationManager.SelectChar(string)` -> `CooperationManager.GetCoopLevel(string)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:179 / GetCoopLevel(charID)`
- `CooperationManager.Debug1()` -> `CooperationManager.AddCoopPoint(string, int)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:221 / AddCoopPoint("Cp_01", 10)`
- `CooperationManager.Debug2()` -> `CooperationManager.SettlePoint(string)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:227 / SettlePoint("Cp_01")`
- `CooperationManager.TestJoinCp01()` -> `CooperationManager.DebugJoin(string)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:252 / DebugJoin("Cp_01")`

## Evidence

- Likely flow - CooperationManager.Awake() -> 
- Internal call - CooperationManager.SelectChar(string) -> CooperationManager.GetCoopCharData(string)
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:178 / GetCoopCharData(charID)`
- Internal call - CooperationManager.SelectChar(string) -> CooperationManager.GetCoopLevel(string)
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:179 / GetCoopLevel(charID)`
- Internal call - CooperationManager.Debug1() -> CooperationManager.AddCoopPoint(string, int)
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:221 / AddCoopPoint("Cp_01", 10)`
- outgoing calls_member - CooperationManager -> CoopCharState / 11 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:74 / coopCharDict.TryGetValue(charID, out var charState)`
- incoming calls_member - DialogueManager -> CooperationManager / 3 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:32 / cooperationManager.IsCoopLevelUP(charID)`
- outgoing creates - CooperationManager -> CoopCharState / 3 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:24 / Dictionary<string, CoopCharState>`

## Suggested AI Task

Use the Cooperation context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

