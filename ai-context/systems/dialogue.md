# Dialogue

Dialogue appears to be an externally connected area around dialogue, character, line. It contains 4 types, including 1 Unity-facing types.

## Stats

- Types: 4
- Internal relationships: 4
- External relationships: 19
- Entry candidates: 3
- Keywords: `dialogue`, `character`, `line`

## Start Here

- `DialogueView.Awake()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:33
- `DialogueView.Update()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:220
- `DialogueView.SetupCharacterSlots()` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:71

## Core Types

- `DialogueView` - class / Unity / 10 out / 8 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:12
- `DialogueLineData` - class / 1 out / 3 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueScriptData.cs:26
- `DialogueScriptData` - class / 2 out / 1 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueScriptData.cs:47
- `DialogueCharacterData` - class / 0 out / 2 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueScriptData.cs:8

## Likely Method Flows

- `DialogueView.Update()`
  - `DialogueView.Update()`
  - `DialogueView.OnAdvanceClicked()`
  - `DialogueView.ShowCurrentLine()`
  - `DialogueView.Finish() / terminal`
- `DialogueView.Awake()`
  - `DialogueView.Awake() / terminal`
- `DialogueView.SetupCharacterSlots()`
  - `DialogueView.SetupCharacterSlots() / terminal`

## Internal Type Relationships

- `DialogueView` -> `DialogueLineData` - internal / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:136 / DialogueLineData`
- `DialogueScriptData` -> `DialogueCharacterData` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueScriptData.cs:49 / DialogueCharacterData[]`
- `DialogueView` -> `DialogueScriptData` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:26 / DialogueScriptData`

## External Touchpoints

- `DialogueView` -> `DialogueNodeData` - outgoing / has_field_type / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:27 / Dictionary<string, DialogueNodeData>`
- `CharacterSlotView` -> `DialogueCharacterData` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\CharacterSlotView.cs:17 / DialogueCharacterData`
- `DialogueView` -> `DialogueChoiceOption` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:185 / DialogueChoiceOption`
- `DialogueManager` -> `DialogueView` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:49 / dialogueView.Play(dialogueJson, () => isDialogueEnd?.Invoke())`
- `DialogueView` -> `DialogueNodeData` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:97 / _nodeMap.TryGetValue(nodeId, out _currentNode)`
- `EventView` -> `DialogueView` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:82 / dialogueView.Play(_data.dialogueJson, ShowDescriptionAndChoices)`
- `SelectCoopCharBtn` -> `DialogueView` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:62 / dialogueView.Play(coopCharData.joinDialogueJson, FinishSelection)`
- `DialogueView` -> `CardContext` - outgoing / creates / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:193 / CardContext`
- `DialogueView` -> `DialogueNodeData` - outgoing / creates / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:61 / Dictionary<string, DialogueNodeData>`
- `DialogueLineData` -> `DialogueChoiceOption` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueScriptData.cs:36 / DialogueChoiceOption[]`
- `DialogueManager` -> `DialogueView` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueManager.cs:9 / DialogueView`
- `DialogueNodeData` -> `DialogueLineData` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueScriptData.cs:43 / DialogueLineData[]`
- `DialogueScriptData` -> `DialogueNodeData` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueScriptData.cs:51 / DialogueNodeData[]`
- `DialogueView` -> `CharacterSlotView` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:15 / CharacterSlotView[]`
- `EventView` -> `DialogueView` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:31 / DialogueView`
- `SelectCoopCharUI` -> `DialogueView` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:11 / DialogueView`
- `SelectCoopCharUI` -> `DialogueView` - incoming / has_property_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:13 / DialogueView`
- `SelectCoopCharBtn` -> `DialogueView` - incoming / uses_local_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:58 / DialogueView`

## Internal Method Calls

- `DialogueView.ShowCurrentLine()` -> `DialogueView.Finish()` / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:113 / Finish()`
- `DialogueView.Play(TextAsset, Action)` -> `DialogueView.GoToNode(string)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:68 / GoToNode(_script.startNode)`
- `DialogueView.Play(TextAsset, Action)` -> `DialogueView.SetupCharacterSlots()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:65 / SetupCharacterSlots()`
- `DialogueView.GoToNode(string)` -> `DialogueView.Finish()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:100 / Finish()`
- `DialogueView.GoToNode(string)` -> `DialogueView.ShowCurrentLine()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:104 / ShowCurrentLine()`
- `DialogueView.ShowCurrentLine()` -> `DialogueView.HideChoices()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:109 / HideChoices()`
- `DialogueView.ShowCurrentLine()` -> `DialogueView.ShowChoices(DialogueLineData)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:124 / ShowChoices(line)`
- `DialogueView.ShowCurrentLine()` -> `DialogueView.ShowLine(DialogueLineData)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:121 / ShowLine(line)`
- `DialogueView.ShowLine(DialogueLineData)` -> `DialogueView.GetCharacterName(string)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:139 / GetCharacterName(line.speaker)`
- `DialogueView.ShowLine(DialogueLineData)` -> `DialogueView.UpdateSpeakerHighlight(string, string)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:141 / UpdateSpeakerHighlight(line.speaker, line.emotion)`
- `DialogueView.ShowChoices(DialogueLineData)` -> `DialogueView.Finish()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:171 / Finish()`
- `DialogueView.ShowChoices(DialogueLineData)` -> `DialogueView.OnChoiceSelected(DialogueChoiceOption)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:180 / OnChoiceSelected(capturedOption)`
- `DialogueView.OnChoiceSelected(DialogueChoiceOption)` -> `DialogueView.Finish()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:203 / Finish()`
- `DialogueView.OnChoiceSelected(DialogueChoiceOption)` -> `DialogueView.GoToNode(string)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:207 / GoToNode(option.next)`
- `DialogueView.OnChoiceSelected(DialogueChoiceOption)` -> `DialogueView.HideChoices()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:187 / HideChoices()`
- `DialogueView.Update()` -> `DialogueView.OnAdvanceClicked()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:228 / OnAdvanceClicked()`
- `DialogueView.OnAdvanceClicked()` -> `DialogueView.ShowCurrentLine()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:234 / ShowCurrentLine()`

## Evidence

- Likely flow - DialogueView.Update() -> DialogueView.OnAdvanceClicked() -> DialogueView.ShowCurrentLine() -> DialogueView.Finish() / terminal
- Likely flow - DialogueView.Awake() -> 
- Internal call - DialogueView.ShowCurrentLine() -> DialogueView.Finish()
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:113 / Finish()`
- Internal call - DialogueView.Play(TextAsset, Action) -> DialogueView.GoToNode(string)
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:68 / GoToNode(_script.startNode)`
- Internal call - DialogueView.Play(TextAsset, Action) -> DialogueView.SetupCharacterSlots()
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:65 / SetupCharacterSlots()`
- outgoing has_field_type - DialogueView -> DialogueNodeData / 2 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:27 / Dictionary<string, DialogueNodeData>`
- incoming accepts_parameter - CharacterSlotView -> DialogueCharacterData / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\CharacterSlotView.cs:17 / DialogueCharacterData`
- outgoing accepts_parameter - DialogueView -> DialogueChoiceOption / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:185 / DialogueChoiceOption`
- Internal accepts_parameter - DialogueView -> DialogueLineData / 2 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:136 / DialogueLineData`
- Internal has_field_type - DialogueScriptData -> DialogueCharacterData / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueScriptData.cs:49 / DialogueCharacterData[]`
- Internal has_field_type - DialogueView -> DialogueScriptData / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:26 / DialogueScriptData`

## Suggested AI Task

Use the Dialogue context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

