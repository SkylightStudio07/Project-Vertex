# Map Generation

Map Generation appears to be an internally dense area around map, node, dialogue, config, connection. It contains 14 types, including 5 Unity-facing types.

## Stats

- Types: 14
- Internal relationships: 101
- External relationships: 27
- Entry candidates: 8
- Keywords: `map`, `node`, `dialogue`, `config`, `connection`, `floor`, `generator`, `guarantee`, `line`, `sprite`, `ui`, `weight`

## Start Here

- `MapUIController.Awake()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:9
- `MapManager.Awake()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapManager.cs:13
- `MapUIController.Start()` - unity_lifecycle / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:36
- `MapGenerator.Generate(MapConfig, int)` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:15
- `MapManager.InitializeMap()` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapManager.cs:26
- `MapNodeView.Setup(MapNode, Action<MapNode>)` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapNodeView.cs:45
- `MapUIController.BuildMap()` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:62
- `MapUIController.BuildLines(MapData, float)` - flow_candidate / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:119

## Core Types

- `MapNode` - class / 2 out / 40 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapNode.cs:9
- `MapUIController` - class / Unity / 33 out / 6 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:5
- `MapGenerator` - class / 36 out / 1 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:6
- `MapConfig` - class / Unity / 18 out / 4 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapConfig.cs:24
- `NodeType` - enum / 0 out / 16 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\NodeType.cs:1
- `MapData` - class / 5 out / 10 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapData.cs:6
- `MapNodeView` - class / Unity / 6 out / 8 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapNodeView.cs:17
- `MapManager` - class / Unity / 10 out / 0 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapManager.cs:6
- `NodeTypeWeight` - class / 1 out / 9 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapConfig.cs:5
- `FloorGuarantee` - class / 1 out / 7 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapConfig.cs:13
- `DialogueNodeData` - class / 1 out / 5 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueScriptData.cs:40
- `MapConnectionLine` - class / Unity / 0 out / 6 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapConnectionLine.cs:6
- `MapNodeState` - enum / 0 out / 2 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapNodeView.cs:8
- `MapNodeView+NodeSprite` - struct / 1 out / 1 in / H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapNodeView.cs:21

## Likely Method Flows

- `MapGenerator.Generate(MapConfig, int)`
  - `MapGenerator.Generate(MapConfig, int)`
  - `MapGenerator.PickColumns(int, int, HashSet<int>, System.Random)`
  - `MapGenerator.Shuffle(List<T>, System.Random) / terminal`
- `MapUIController.BuildMap()`
  - `MapUIController.BuildMap()`
  - `MapNodeView.Setup(MapNode, Action<MapNode>)`
  - `MapNodeView.GetSprite(NodeType) / terminal`
- `MapUIController.BuildLines(MapData, float)`
  - `MapUIController.BuildLines(MapData, float)`
  - `MapConnectionLine.Setup(Vector2, Vector2) / terminal`
- `MapManager.InitializeMap()`
  - `MapManager.InitializeMap()`
  - `MapGenerator.Generate(MapConfig) / terminal`
- `MapNodeView.Setup(MapNode, Action<MapNode>)`
  - `MapNodeView.Setup(MapNode, Action<MapNode>)`
  - `MapNodeView.GetSprite(NodeType) / terminal`
- `MapUIController.Awake()`
  - `MapUIController.Awake() / terminal`
- `MapManager.Awake()`
  - `MapManager.Awake() / terminal`
- `MapUIController.Start()`
  - `MapUIController.Start() / terminal`

## Internal Type Relationships

- `MapConfig` -> `NodeTypeWeight` - internal / creates / 7 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapConfig.cs:37 / NodeTypeWeight`
- `MapConfig` -> `FloorGuarantee` - internal / creates / 6 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapConfig.cs:63 / FloorGuarantee`
- `MapGenerator` -> `MapNode` - internal / creates / 6 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:69 / MapNode`
- `MapGenerator` -> `MapNode` - internal / accepts_parameter / 5 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:263 / List<MapNode>`
- `MapUIController` -> `MapNodeView` - internal / calls_member / 5 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:72 / nodeViews.Clear()`
- `MapGenerator` -> `MapNode` - internal / calls_member / 4 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:276 / sortedCurr.Sort((a, b) => a.column.CompareTo(b.column))`
- `MapGenerator` -> `NodeType` - internal / calls_member / 4 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:47 / guaranteeMap.ContainsKey(guarantee.floorIndex)`
- `MapGenerator` -> `NodeType` - internal / creates / 4 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:39 / Dictionary<int, List<NodeType>>`
- `MapGenerator` -> `MapConfig` - internal / accepts_parameter / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:9 / MapConfig`
- `MapUIController` -> `MapNode` - internal / accepts_parameter / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:173 / MapNode`
- `MapUIController` -> `MapConnectionLine` - internal / calls_member / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:73 / lineViews.Clear()`
- `MapManager` -> `MapNode` - internal / creates / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapManager.cs:51 / List<MapNode>`
- `MapUIController` -> `MapNode` - internal / uses_local_type / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:138 / MapNode`
- `MapNodeView` -> `MapNode` - internal / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapNodeView.cs:45 / MapNode`
- `MapData` -> `MapNode` - internal / creates / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapData.cs:20 / List<List<MapNode>>`
- `MapUIController` -> `MapConnectionLine` - internal / has_field_type / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:15 / MapConnectionLine`
- `MapUIController` -> `MapNodeView` - internal / has_field_type / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:14 / MapNodeView`
- `MapGenerator` -> `MapData` - internal / returns / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:9 / MapData`
- `MapGenerator` -> `NodeTypeWeight` - internal / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:393 / List<NodeTypeWeight>`
- `MapManager` -> `MapNode` - internal / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapManager.cs:37 / MapNode`
- `MapNodeView` -> `MapNodeState` - internal / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapNodeView.cs:56 / MapNodeState`
- `MapNodeView` -> `NodeType` - internal / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapNodeView.cs:77 / NodeType`
- `MapUIController` -> `MapData` - internal / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:119 / MapData`
- `MapData` -> `MapNode` - internal / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapData.cs:22 / floors.Add(new List<MapNode>())`
- `MapManager` -> `MapGenerator` - internal / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapManager.cs:28 / MapGenerator.Generate(mapConfig)`
- `MapManager` -> `MapNode` - internal / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapManager.cs:60 / result.Add(RunData.Instance.mapData.GetNode(nextFloor, nodeIndex))`
- `MapUIController` -> `MapData` - internal / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:138 / mapData.GetNode(nextFloor, nextIndex)`
- `MapUIController` -> `MapNode` - internal / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:164 / accessible.Contains(view.Data)`
- `MapGenerator` -> `MapData` - internal / creates / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:26 / MapData`
- `FloorGuarantee` -> `NodeType` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapConfig.cs:17 / NodeType`
- `MapConfig` -> `FloorGuarantee` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapConfig.cs:61 / List<FloorGuarantee>`
- `MapConfig` -> `NodeTypeWeight` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapConfig.cs:35 / List<NodeTypeWeight>`
- `MapData` -> `MapNode` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapData.cs:9 / List<List<MapNode>>`
- `MapManager` -> `MapConfig` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapManager.cs:11 / MapConfig`
- `MapNode` -> `NodeType` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapNode.cs:13 / NodeType`
- `MapNodeView` -> `MapNodeView+NodeSprite` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapNodeView.cs:27 / List<NodeSprite>`

## External Touchpoints

- `MapConfig` -> `EnemyData` - outgoing / has_field_type / 3 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapConfig.cs:54 / List<EnemyData>`
- `DialogueView` -> `DialogueNodeData` - incoming / has_field_type / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:27 / Dictionary<string, DialogueNodeData>`
- `DialogueView` -> `DialogueNodeData` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:97 / _nodeMap.TryGetValue(nodeId, out _currentNode)`
- `EventView` -> `MapUIController` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:156 / mapUIController.OpenMap()`
- `MapUIController` -> `EventView` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:218 / eventView.Open(data)`
- `MapUIController` -> `RestView` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:229 / restView.Open()`
- `MapUIController` -> `SelectCoopCharUI` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:241 / selectCoopCharUI.Init()`
- `RestView` -> `MapUIController` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Rest\RestView.cs:78 / mapUIController.OpenMap()`
- `RewardsView` -> `MapUIController` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\Reward\RewardsView.cs:37 / mapUIController.OpenMap()`
- `DialogueView` -> `DialogueNodeData` - incoming / creates / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:61 / Dictionary<string, DialogueNodeData>`
- `DialogueNodeData` -> `DialogueLineData` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueScriptData.cs:43 / DialogueLineData[]`
- `DialogueScriptData` -> `DialogueNodeData` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueScriptData.cs:51 / DialogueNodeData[]`
- `EventView` -> `MapUIController` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Event\EventView.cs:30 / MapUIController`
- `MapNode` -> `EnemyData` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapNode.cs:23 / List<EnemyData>`
- `MapUIController` -> `EventData` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:20 / List<EventData>`
- `MapUIController` -> `EventView` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:19 / EventView`
- `MapUIController` -> `RestView` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:23 / RestView`
- `MapUIController` -> `SelectCoopCharUI` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:25 / SelectCoopCharUI`
- `RestView` -> `MapUIController` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Rest\RestView.cs:8 / MapUIController`
- `RewardsView` -> `MapUIController` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Gameplay\Reward\RewardsView.cs:10 / MapUIController`
- `RunData` -> `MapData` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\RunData.cs:14 / MapData`
- `RunData` -> `MapNode` - incoming / has_property_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\RunData.cs:24 / MapNode`
- `RunData` -> `NodeType` - incoming / has_property_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Card\RunData.cs:21 / NodeType`
- `MapGenerator` -> `EnemyData` - outgoing / uses_local_type / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:411 / List<EnemyData>`

## Internal Method Calls

- `MapGenerator.ConnectFloors(List<MapNode>, List<MapNode>, System.Random)` -> `MapGenerator.FindClosest(MapNode, List<MapNode>)` / 2 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:309 / FindClosest(curr, next)`
- `MapGenerator.Generate(MapConfig, int)` -> `MapGenerator.AssignEncounter(MapNode, MapConfig, System.Random)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:161 / AssignEncounter(node, config, rng)`
- `MapGenerator.Generate(MapConfig, int)` -> `MapGenerator.ConnectFloors(List<MapNode>, List<MapNode>, System.Random)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:176 / ConnectFloors(mapData.floors[f], mapData.floors[f + 1], rng)`
- `MapGenerator.Generate(MapConfig, int)` -> `MapGenerator.GetRandomNodeType(List<NodeTypeWeight>, System.Random)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:114 / GetRandomNodeType(config.nodeTypeWeights, rng)`
- `MapGenerator.Generate(MapConfig, int)` -> `MapGenerator.PickColumns(int, int, HashSet<int>, System.Random)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:139 / PickColumns(columnCount, nodeCount, prevCols, rng)`
- `MapManager.InitializeMap()` -> `MapGenerator.Generate(MapConfig)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapManager.cs:28 / MapGenerator.Generate(mapConfig)`
- `MapUIController.OpenMap()` -> `MapUIController.BuildMap()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:46 / BuildMap()`
- `MapUIController.OpenMap()` -> `MapUIController.RefreshNodeStates()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:47 / RefreshNodeStates()`
- `MapNodeView.Setup(MapNode, Action<MapNode>)` -> `MapNodeView.GetSprite(NodeType)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapNodeView.cs:48 / GetSprite(data.nodeType)`
- `MapUIController.ToggleMap()` -> `MapUIController.CloseMap()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:58 / CloseMap()`
- `MapUIController.ToggleMap()` -> `MapUIController.OpenMap()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:59 / OpenMap()`
- `MapUIController.BuildMap()` -> `MapNodeView.Setup(MapNode, Action<MapNode>)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:106 / view.Setup(node, OnNodeClicked)`
- `MapUIController.BuildMap()` -> `MapUIController.BuildLines(MapData, float)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:94 / BuildLines(mapData, xOffset)`
- `MapUIController.BuildMap()` -> `MapUIController.RefreshNodeStates()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:111 / RefreshNodeStates()`
- `MapUIController.BuildLines(MapData, float)` -> `MapConnectionLine.Setup(Vector2, Vector2)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:144 / line.Setup(from, to)`
- `MapUIController.BuildLines(MapData, float)` -> `MapData.GetNode(int, int)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:138 / mapData.GetNode(nextFloor, nextIndex)`
- `MapUIController.OnNodeClicked(MapNode)` -> `MapUIController.CloseMap()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:177 / CloseMap()`
- `MapUIController.OnNodeClicked(MapNode)` -> `MapUIController.OpenEvent(MapNode)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:187 / OpenEvent(node)`
- `MapUIController.OnNodeClicked(MapNode)` -> `MapUIController.OpenRest(MapNode)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:190 / OpenRest(node)`
- `MapUIController.OnNodeClicked(MapNode)` -> `MapUIController.OpenSanctuary()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:193 / OpenSanctuary()`
- `MapUIController.OnNodeClicked(MapNode)` -> `MapUIController.RefreshNodeStates()` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:176 / RefreshNodeStates()`
- `MapGenerator.PickColumns(int, int, HashSet<int>, System.Random)` -> `MapGenerator.Shuffle(List<T>, System.Random)` / 1 refs
  - Evidence: `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:248 / Shuffle(slots, rng)`

## Evidence

- Likely flow - MapGenerator.Generate(MapConfig, int) -> MapGenerator.PickColumns(int, int, HashSet<int>, System.Random) -> MapGenerator.Shuffle(List<T>, System.Random) / terminal
- Likely flow - MapUIController.BuildMap() -> MapNodeView.Setup(MapNode, Action<MapNode>) -> MapNodeView.GetSprite(NodeType) / terminal
- Internal call - MapGenerator.ConnectFloors(List<MapNode>, List<MapNode>, System.Random) -> MapGenerator.FindClosest(MapNode, List<MapNode>)
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:309 / FindClosest(curr, next)`
- Internal call - MapGenerator.Generate(MapConfig, int) -> MapGenerator.AssignEncounter(MapNode, MapConfig, System.Random)
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:161 / AssignEncounter(node, config, rng)`
- Internal call - MapGenerator.Generate(MapConfig, int) -> MapGenerator.ConnectFloors(List<MapNode>, List<MapNode>, System.Random)
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:176 / ConnectFloors(mapData.floors[f], mapData.floors[f + 1], rng)`
- outgoing has_field_type - MapConfig -> EnemyData / 3 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapConfig.cs:54 / List<EnemyData>`
- incoming has_field_type - DialogueView -> DialogueNodeData / 2 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:27 / Dictionary<string, DialogueNodeData>`
- incoming calls_member - DialogueView -> DialogueNodeData / 1 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Dialogue\DialogueView.cs:97 / _nodeMap.TryGetValue(nodeId, out _currentNode)`
- Internal creates - MapConfig -> NodeTypeWeight / 7 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapConfig.cs:37 / NodeTypeWeight`
- Internal creates - MapConfig -> FloorGuarantee / 6 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapConfig.cs:63 / FloorGuarantee`
- Internal creates - MapGenerator -> MapNode / 6 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:69 / MapNode`
- Internal accepts_parameter - MapGenerator -> MapNode / 5 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:263 / List<MapNode>`
- Internal calls_member - MapUIController -> MapNodeView / 5 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapUIController.cs:72 / nodeViews.Clear()`
- Internal calls_member - MapGenerator -> MapNode / 4 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:276 / sortedCurr.Sort((a, b) => a.column.CompareTo(b.column))`
- Internal calls_member - MapGenerator -> NodeType / 4 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:47 / guaranteeMap.ContainsKey(guarantee.floorIndex)`
- Internal creates - MapGenerator -> NodeType / 4 refs
  - `H:\Unity\ProjectV\ProjectV\Assets\Scripts\Map\MapGenerator.cs:39 / Dictionary<int, List<NodeType>>`

## Suggested AI Task

Use the Map Generation context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

