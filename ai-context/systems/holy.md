# Holy

Holy appears to be an internally dense area around holy, place, gameplay, char, in. It contains 3 types, including 2 Unity-facing types.

## Stats

- Types: 3
- Internal relationships: 2
- External relationships: 0
- Entry candidates: 1
- Keywords: `holy`, `place`, `gameplay`, `char`, `in`, `selectable`

## Start Here

- `HolyPlaceManager.Awake()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\HolyPlace\HolyPlaceManager.cs:16

## Core Types

- `HolyPlaceData` - class / Unity / 1 out / 1 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\HolyPlace\HolyPlaceData.cs:6
- `HolyPlaceManager` - class / Unity / 1 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\HolyPlace\HolyPlaceManager.cs:7
- `SelectableCharDataInHolyPlace` - struct / 0 out / 1 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\HolyPlace\HolyPlaceData.cs:13

## Likely Method Flows

- `HolyPlaceManager.Awake()`
  - `HolyPlaceManager.Awake() / terminal`

## Internal Type Relationships

- `HolyPlaceData` -> `SelectableCharDataInHolyPlace` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\HolyPlace\HolyPlaceData.cs:8 / List<SelectableCharDataInHolyPlace>`
- `HolyPlaceManager` -> `HolyPlaceData` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\HolyPlace\HolyPlaceManager.cs:14 / HolyPlaceData`

## External Touchpoints

- None detected.

## Internal Method Calls

- None detected.

## Evidence

- Likely flow - HolyPlaceManager.Awake() -> 
- Internal has_field_type - HolyPlaceData -> SelectableCharDataInHolyPlace / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\HolyPlace\HolyPlaceData.cs:8 / List<SelectableCharDataInHolyPlace>`
- Internal has_field_type - HolyPlaceManager -> HolyPlaceData / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\HolyPlace\HolyPlaceManager.cs:14 / HolyPlaceData`

## Suggested AI Task

Use the Holy context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

