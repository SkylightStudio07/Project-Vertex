# Char

Char appears to be an externally connected area around char, coop, cooperator. It contains 3 types, including 1 Unity-facing types.

## Stats

- Types: 3
- Internal relationships: 2
- External relationships: 34
- Entry candidates: 0
- Keywords: `char`, `coop`, `cooperator`

## Start Here

- None detected.

## Core Types

- `CoopCharState` - class / 2 out / 22 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:7
- `CoopCharData` - class / 5 out / 6 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CoopCharData.cs:5
- `CharData` - class / Unity / 1 out / 2 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Common\CharData.cs:3

## Likely Method Flows

- No internal method flow detected.

## Internal Type Relationships

- `CoopCharState` -> `CoopCharData` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:10 / CoopCharData`
- `CoopCharData` -> `CharData` - internal / inherits / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CoopCharData.cs:5 / CharData`

## External Touchpoints

- `CooperationManager` -> `CoopCharState` - incoming / calls_member / 11 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:74 / coopCharDict.TryGetValue(charID, out var charState)`
- `CooperationManager` -> `CoopCharState` - incoming / creates / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:24 / Dictionary<string, CoopCharState>`
- `CoopCharData` -> `CardData` - outgoing / has_field_type / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CoopCharData.cs:8 / List<CardData>`
- `PortraitSlot` -> `CoopCharState` - incoming / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Rest\PortraitSlot.cs:22 / CoopCharState`
- `PortraitSlot` -> `CoopCharState` - incoming / has_field_type / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Rest\PortraitSlot.cs:19 / CoopCharState`
- `CooperationManager` -> `CoopCharState` - incoming / uses_local_type / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:199 / CoopCharState`
- `CooperationManager` -> `CoopCharData` - incoming / creates / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:23 / List<CoopCharData>`
- `CharData` -> `IPassiveLogic` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Common\CharData.cs:24 / IPassiveLogic`
- `CoopCharData` -> `RankEventData` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CoopCharData.cs:23 / List<RankEventData>`
- `CoopCharState` -> `RankEventData` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:15 / Dictionary<int, RankEventData>`
- `CooperationManager` -> `CoopCharData` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:23 / List<CoopCharData>`
- `CooperationManager` -> `CoopCharState` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:24 / Dictionary<string, CoopCharState>`
- `PlayerCharData` -> `CharData` - incoming / inherits / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Player\PlayerCharData.cs:5 / CharData`
- `CooperationManager` -> `CoopCharData` - incoming / returns / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:104 / CoopCharData`
- `CooperationManager` -> `CoopCharState` - incoming / returns / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:204 / List<CoopCharState>`
- `CooperationManager` -> `CoopCharData` - incoming / uses_local_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:178 / CoopCharData`
- `SelectCoopCharBtn` -> `CoopCharData` - incoming / uses_local_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:59 / CoopCharData`

## Internal Method Calls

- None detected.

## Evidence

- incoming calls_member - CooperationManager -> CoopCharState / 11 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:74 / coopCharDict.TryGetValue(charID, out var charState)`
- incoming creates - CooperationManager -> CoopCharState / 3 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:24 / Dictionary<string, CoopCharState>`
- outgoing has_field_type - CoopCharData -> CardData / 3 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CoopCharData.cs:8 / List<CardData>`
- Internal has_field_type - CoopCharState -> CoopCharData / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CooperationManager.cs:10 / CoopCharData`
- Internal inherits - CoopCharData -> CharData / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CoopCharData.cs:5 / CharData`

## Suggested AI Task

Use the Char context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

