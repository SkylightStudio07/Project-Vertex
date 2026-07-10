# Cooperator

Cooperator appears to be an externally connected area around cooperator, dialogue. It contains 1 types, including 1 Unity-facing types.

## Stats

- Types: 1
- Internal relationships: 0
- External relationships: 8
- Entry candidates: 2
- Keywords: `cooperator`, `dialogue`

## Start Here

- `DialogueManager.Start()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:14
- `DialogueManager.LoadRelationshipEvent(string)` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:19

## Core Types

- `DialogueManager` - class / Unity / 6 out / 2 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:7

## Likely Method Flows

- `DialogueManager.LoadRelationshipEvent(string)`
  - `DialogueManager.LoadRelationshipEvent(string) / terminal`
- `DialogueManager.Start()`
  - `DialogueManager.Start() / terminal`

## Internal Type Relationships

- None detected.

## External Touchpoints

- `DialogueManager` -> `CooperationManager` - outgoing / calls_member / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:32 / cooperationManager.IsCoopLevelUP(charID)`
- `CharacterEventManager` -> `DialogueManager` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CharacterEventManager.cs:32 / dialogueManager.LoadRelationshipEvent("Cp_01")`
- `DialogueManager` -> `DialogueView` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:49 / dialogueView.Play(dialogueJson, () => isDialogueEnd?.Invoke())`
- `CharacterEventManager` -> `DialogueManager` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CharacterEventManager.cs:11 / DialogueManager`
- `DialogueManager` -> `CooperationManager` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:10 / CooperationManager`
- `DialogueManager` -> `DialogueView` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:9 / DialogueView`

## Internal Method Calls

- None detected.

## Evidence

- Likely flow - DialogueManager.LoadRelationshipEvent(string) -> 
- Likely flow - DialogueManager.Start() -> 
- outgoing calls_member - DialogueManager -> CooperationManager / 3 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:32 / cooperationManager.IsCoopLevelUP(charID)`
- incoming calls_member - CharacterEventManager -> DialogueManager / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\CharacterEventManager.cs:32 / dialogueManager.LoadRelationshipEvent("Cp_01")`
- outgoing calls_member - DialogueManager -> DialogueView / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:49 / dialogueView.Play(dialogueJson, () => isDialogueEnd?.Invoke())`

## Suggested AI Task

Use the Cooperator context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

