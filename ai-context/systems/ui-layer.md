# UI Layer

UI Layer appears to be an internally dense area around ui, char, coop, select, btn. It contains 4 types, including 4 Unity-facing types.

## Stats

- Types: 4
- Internal relationships: 9
- External relationships: 7
- Entry candidates: 4
- Keywords: `ui`, `char`, `coop`, `select`, `btn`, `cooperator`, `dialogue`, `fade`

## Start Here

- `SelectCoopCharBtn.Start()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:15
- `SelectCoopCharUI.Awake()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:17
- `SelectCoopCharUI.Init()` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:22
- `FadeController.StartFade()` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\FadeController.cs:35

## Core Types

- `SelectCoopCharUI` - class / Unity / 7 out / 6 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:6
- `SelectCoopCharBtn` - class / Unity / 7 out / 3 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:5
- `FadeController` - class / Unity / 0 out / 2 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\FadeController.cs:5
- `DialogueUI` - class / Unity / 0 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Cooperator\DialogueUI.cs:5

## Likely Method Flows

- `SelectCoopCharUI.Init()`
  - `SelectCoopCharUI.Init()`
  - `FadeController.FadeIn() / terminal`
- `SelectCoopCharBtn.Start()`
  - `SelectCoopCharBtn.Start() / terminal`
- `SelectCoopCharUI.Awake()`
  - `SelectCoopCharUI.Awake() / terminal`
- `FadeController.StartFade()`
  - `FadeController.StartFade() / terminal`

## Internal Type Relationships

- `SelectCoopCharBtn` -> `SelectCoopCharUI` - internal / unity_get_component / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:17 / SelectCoopCharUI`
- `SelectCoopCharBtn` -> `SelectCoopCharUI` - internal / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:73 / selectCoopCharUI.CloseUI()`
- `SelectCoopCharUI` -> `FadeController` - internal / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:24 / fadeController.FadeIn()`
- `SelectCoopCharUI` -> `SelectCoopCharBtn` - internal / creates / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:19 / List<SelectCoopCharBtn>`
- `SelectCoopCharBtn` -> `SelectCoopCharUI` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:9 / SelectCoopCharUI`
- `SelectCoopCharUI` -> `FadeController` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:10 / FadeController`
- `SelectCoopCharUI` -> `SelectCoopCharBtn` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:9 / List<SelectCoopCharBtn>`
- `SelectCoopCharUI` -> `SelectCoopCharBtn` - internal / unity_get_component / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:19 / SelectCoopCharBtn`

## External Touchpoints

- `MapUIController` -> `SelectCoopCharUI` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:241 / selectCoopCharUI.Init()`
- `SelectCoopCharBtn` -> `DialogueView` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:62 / dialogueView.Play(coopCharData.joinDialogueJson, FinishSelection)`
- `MapUIController` -> `SelectCoopCharUI` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:25 / SelectCoopCharUI`
- `SelectCoopCharUI` -> `DialogueView` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:11 / DialogueView`
- `SelectCoopCharUI` -> `DialogueView` - outgoing / has_property_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:13 / DialogueView`
- `SelectCoopCharBtn` -> `CoopCharData` - outgoing / uses_local_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:59 / CoopCharData`
- `SelectCoopCharBtn` -> `DialogueView` - outgoing / uses_local_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:58 / DialogueView`

## Internal Method Calls

- `SelectCoopCharUI.Init()` -> `FadeController.FadeIn()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:24 / fadeController.FadeIn()`
- `SelectCoopCharBtn.OnClickBtn()` -> `SelectCoopCharBtn.FinishSelection()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:66 / FinishSelection()`
- `SelectCoopCharBtn.FinishSelection()` -> `SelectCoopCharUI.CloseUI()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:73 / selectCoopCharUI.CloseUI()`

## Evidence

- Likely flow - SelectCoopCharUI.Init() -> FadeController.FadeIn() / terminal
- Likely flow - SelectCoopCharBtn.Start() -> 
- Internal call - SelectCoopCharUI.Init() -> FadeController.FadeIn()
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:24 / fadeController.FadeIn()`
- Internal call - SelectCoopCharBtn.OnClickBtn() -> SelectCoopCharBtn.FinishSelection()
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:66 / FinishSelection()`
- Internal call - SelectCoopCharBtn.FinishSelection() -> SelectCoopCharUI.CloseUI()
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:73 / selectCoopCharUI.CloseUI()`
- incoming calls_member - MapUIController -> SelectCoopCharUI / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:241 / selectCoopCharUI.Init()`
- outgoing calls_member - SelectCoopCharBtn -> DialogueView / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:62 / dialogueView.Play(coopCharData.joinDialogueJson, FinishSelection)`
- incoming has_field_type - MapUIController -> SelectCoopCharUI / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:25 / SelectCoopCharUI`
- Internal unity_get_component - SelectCoopCharBtn -> SelectCoopCharUI / 2 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:17 / SelectCoopCharUI`
- Internal calls_member - SelectCoopCharBtn -> SelectCoopCharUI / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:73 / selectCoopCharUI.CloseUI()`
- Internal calls_member - SelectCoopCharUI -> FadeController / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:24 / fadeController.FadeIn()`
- Internal creates - SelectCoopCharUI -> SelectCoopCharBtn / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:19 / List<SelectCoopCharBtn>`
- Internal has_field_type - SelectCoopCharBtn -> SelectCoopCharUI / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharBtn.cs:9 / SelectCoopCharUI`
- Internal has_field_type - SelectCoopCharUI -> FadeController / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:10 / FadeController`
- Internal has_field_type - SelectCoopCharUI -> SelectCoopCharBtn / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:9 / List<SelectCoopCharBtn>`
- Internal unity_get_component - SelectCoopCharUI -> SelectCoopCharBtn / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\UI\SelectCoopCharUI.cs:19 / SelectCoopCharBtn`

## Suggested AI Task

Use the UI Layer context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

