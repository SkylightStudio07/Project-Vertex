# Inventory

Inventory appears to be an externally connected area around inventory. It contains 1 types, including 1 Unity-facing types.

## Stats

- Types: 1
- Internal relationships: 0
- External relationships: 7
- Entry candidates: 1
- Keywords: `inventory`

## Start Here

- `ItemInventoryManager.Awake()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Item\ItemInventoryManager.cs:10

## Core Types

- `ItemInventoryManager` - class / Unity / 7 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Item\ItemInventoryManager.cs:7

## Likely Method Flows

- `ItemInventoryManager.Awake()`
  - `ItemInventoryManager.Awake() / terminal`

## Internal Type Relationships

- None detected.

## External Touchpoints

- `ItemInventoryManager` -> `ItemData` - outgoing / calls_member / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Item\ItemInventoryManager.cs:29 / _items.Add(item)`
- `ItemInventoryManager` -> `ItemData` - outgoing / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Item\ItemInventoryManager.cs:22 / ItemData`
- `ItemInventoryManager` -> `ItemData` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Item\ItemInventoryManager.cs:16 / List<ItemData>`
- `ItemInventoryManager` -> `ItemData` - outgoing / has_property_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Item\ItemInventoryManager.cs:19 / IReadOnlyList<ItemData>`

## Internal Method Calls

- None detected.

## Evidence

- Likely flow - ItemInventoryManager.Awake() -> 
- outgoing calls_member - ItemInventoryManager -> ItemData / 3 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Item\ItemInventoryManager.cs:29 / _items.Add(item)`
- outgoing accepts_parameter - ItemInventoryManager -> ItemData / 2 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Item\ItemInventoryManager.cs:22 / ItemData`
- outgoing has_field_type - ItemInventoryManager -> ItemData / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Item\ItemInventoryManager.cs:16 / List<ItemData>`

## Suggested AI Task

Use the Inventory context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

