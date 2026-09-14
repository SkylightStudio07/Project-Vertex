using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// 버프/디버프 표시 UI(StatusChip 프리팹 + 플레이어/적 StatusListView)를 자동 구성한다.
// BlessingUISetup과 같은 방식으로, 이미 만들어진 것이 있으면 재사용하고 없으면 생성한다(멱등).
public static class StatusUISetup
{
    private const string ChipPrefabPath = "Assets/Data/UI/Prefabs/StatusChip.prefab";
    private const string EnemyPrefabPath = "Assets/Data/Enemy/Prefabs/EnemyView.prefab";
    private const string FontPath = "Assets/Font/강원교육튼튼 SDF.asset";

    private const float ChipHeight = 34f;
    private const float ChipMinWidth = 34f;
    private const float ChipLabelWidth = 72f;
    private const float ChipSpacing = 4f;

    // 한 줄 최대 칩 수. 34px 칩 + 4px 간격 기준 6개 = 224px로, 플레이어 HP 패널 폭(253px) 안에 들어간다.
    private const int DefaultMaxPerRow = 6;

    // 적 프리팹이 EnemyView.prefab 하나뿐이라 배선을 다시 할 일이 사실상 없어서
    // 툴바 메뉴엔 안 올린다. 필요하면 Unity CLI로 직접 호출:
    // unity command eval 'StatusUISetup.Setup();'
    public static void Setup()
    {
        var chipPrefab = CreateOrLoadChipPrefab();
        if (chipPrefab == null)
        {
            Debug.LogError("[StatusUISetup] StatusChip 프리팹 생성에 실패했습니다.");
            return;
        }

        string playerResult = SetupPlayer(chipPrefab);
        string enemyResult  = SetupEnemy(chipPrefab);

        // MarkSceneDirty만 하면 변경이 메모리에만 남아 에디터를 닫으면 사라진다. 실제로 저장까지 한다.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        // 모달 대화상자 대신 콘솔에 남긴다. CLI/배치로 반복 실행해도 멈추지 않는다.
        Debug.Log(
            $"[StatusUISetup] 칩 프리팹: {ChipPrefabPath}\n" +
            $"플레이어: {playerResult}\n" +
            $"적: {enemyResult}");
    }

    // ---------- 칩 프리팹 ----------

    private static StatusChipView CreateOrLoadChipPrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ChipPrefabPath);
        if (existing != null) return existing.GetComponent<StatusChipView>();

        string dir = Path.GetDirectoryName(ChipPrefabPath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();
        }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        var root = new GameObject("StatusChip",
            typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(StatusChipView));
        var rootRect = (RectTransform)root.transform;
        rootRect.sizeDelta = new Vector2(ChipMinWidth, ChipHeight);

        var background = root.GetComponent<Image>();
        background.color = new Color(0.30f, 0.30f, 0.35f, 0.85f);

        var layout = root.GetComponent<LayoutElement>();
        layout.minWidth  = ChipMinWidth;
        layout.minHeight = ChipHeight;
        layout.preferredHeight = ChipHeight;
        // 아이콘이 없는 상태(신규 추가 등)에서 이름 텍스트로 대체 표시될 때를 위한 폭.
        // 컨테이너가 GridLayoutGroup이면 cellSize가 우선해 이 값은 쓰이지 않는다.
        layout.preferredWidth = ChipLabelWidth;

        // 아이콘 — 에셋에 Icon이 연결된 상태에서만 켜진다.
        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(root.transform, false);
        var iconRect = (RectTransform)iconGo.transform;
        StretchFull(iconRect, 3f);
        var iconImage = iconGo.GetComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        // 이름 텍스트 — 아이콘이 없을 때의 대체 표시. 현재 Status 에셋 대부분이 여기에 해당한다.
        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(root.transform, false);
        StretchFull((RectTransform)labelGo.transform, 2f);
        var label = labelGo.GetComponent<TextMeshProUGUI>();
        ConfigureText(label, font, 13f, TextAlignmentOptions.Center, Color.white);
        label.enableAutoSizing = true;
        label.fontSizeMin = 8f;
        label.fontSizeMax = 13f;

        // 스택 수치 — 우하단에 겹쳐 표시.
        var stackGo = new GameObject("Stacks", typeof(RectTransform), typeof(TextMeshProUGUI));
        stackGo.transform.SetParent(root.transform, false);
        var stackRect = (RectTransform)stackGo.transform;
        stackRect.anchorMin = new Vector2(1f, 0f);
        stackRect.anchorMax = new Vector2(1f, 0f);
        stackRect.pivot     = new Vector2(1f, 0f);
        stackRect.anchoredPosition = new Vector2(-1f, 1f);
        stackRect.sizeDelta = new Vector2(20f, 16f);
        var stack = stackGo.GetComponent<TextMeshProUGUI>();
        ConfigureText(stack, font, 14f, TextAlignmentOptions.BottomRight, Color.white);
        stack.fontStyle = FontStyles.Bold;
        stack.outlineWidth = 0.25f;
        stack.outlineColor = Color.black;

        var so = new SerializedObject(root.GetComponent<StatusChipView>());
        so.FindProperty("background").objectReferenceValue = background;
        so.FindProperty("iconImage").objectReferenceValue  = iconImage;
        so.FindProperty("labelText").objectReferenceValue  = label;
        so.FindProperty("stackText").objectReferenceValue  = stack;
        so.ApplyModifiedPropertiesWithoutUndo();

        var saved = PrefabUtility.SaveAsPrefabAsset(root, ChipPrefabPath);
        Object.DestroyImmediate(root);

        return saved != null ? saved.GetComponent<StatusChipView>() : null;
    }

    // ---------- 플레이어 ----------

    private static string SetupPlayer(StatusChipView chipPrefab)
    {
        var hud = Object.FindFirstObjectByType<PlayerHUDView>(FindObjectsInactive.Include);
        if (hud == null) return "PlayerHUDView를 씬에서 찾지 못했습니다.";

        // PlayerHUDView 오브젝트 자체는 화면 위쪽의 빈 앵커라, 그 아래에 붙이면
        // 실제 HP 패널(HPBackground)과 멀리 떨어진 곳에 칩이 뜬다.
        // hpText의 부모가 곧 HP 패널이므로 그쪽에 붙인다 (HPBarFixup과 같은 방식).
        var hudSo = new SerializedObject(hud);
        var hpTextProp = hudSo.FindProperty("hpText");
        Transform host = hpTextProp?.objectReferenceValue is Component hpText && hpText.transform.parent != null
            ? hpText.transform.parent
            : hud.transform;

        Undo.RegisterFullObjectHierarchyUndo(host.gameObject, "Setup Status UI (Player)");

        // HP 게이지 슬롯(패널 로컬 x -99.5~51.5, y -21) 바로 아래에서 왼쪽 정렬로 시작한다.
        // 6열 = 224px이고 패널 폭이 253px이라 왼쪽 끝을 게이지와 맞추면 한 줄이 정확히 들어간다.
        var listView = FindOrCreateListView(host, "StatusList", chipPrefab,
            anchorMin: new Vector2(0.5f, 0.5f),
            anchorMax: new Vector2(0.5f, 0.5f),
            pivot:     new Vector2(0f, 1f),
            anchoredPosition: new Vector2(-99.5f, -32f));

        var so = new SerializedObject(hud);
        so.FindProperty("statusList").objectReferenceValue = listView;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(hud);

        return $"'{host.name}' 아래에 StatusList 구성 완료.";
    }

    // ---------- 적 ----------

    private static string SetupEnemy(StatusChipView chipPrefab)
    {
        var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
        if (enemyPrefab == null) return $"적 프리팹을 찾지 못했습니다: {EnemyPrefabPath}";

        var root = PrefabUtility.LoadPrefabContents(EnemyPrefabPath);
        try
        {
            var view = root.GetComponent<EnemyView>();
            if (view == null) return "EnemyView 컴포넌트가 프리팹 루트에 없습니다.";

            var listView = FindOrCreateListView(root.transform, "StatusList", chipPrefab,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                pivot:     new Vector2(0.5f, 1f),
                anchoredPosition: new Vector2(0f, -40f)); // HP 바 아래.

            var so = new SerializedObject(view);
            so.FindProperty("statusList").objectReferenceValue = listView;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefabPath);
            return $"'{enemyPrefab.name}' 프리팹에 StatusList 구성 완료.";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // ---------- 공통 ----------

    // StatusListView 루트와, GridLayoutGroup을 가진 자식 Container를 만든다.
    // Container를 자식으로 두는 이유는 StatusListView가 "비었을 때 숨기기"로 Container를 껐다 켜기 때문이다.
    // 루트 자신을 끄면 Update()가 멈춰서 다시 켜지지 못한다.
    private static StatusListView FindOrCreateListView(
        Transform parent, string name, StatusChipView chipPrefab,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition)
    {
        var existing = parent.Find(name);
        GameObject rootGo = existing != null
            ? existing.gameObject
            : new GameObject(name, typeof(RectTransform), typeof(StatusListView));

        if (existing == null) rootGo.transform.SetParent(parent, false);

        var listView = rootGo.GetComponent<StatusListView>();
        if (listView == null) listView = rootGo.AddComponent<StatusListView>();

        var rootRect = (RectTransform)rootGo.transform;
        rootRect.anchorMin = anchorMin;
        rootRect.anchorMax = anchorMax;
        rootRect.pivot     = pivot;
        rootRect.anchoredPosition = anchoredPosition;
        rootRect.sizeDelta = new Vector2(240f, ChipHeight);

        var containerTrans = rootGo.transform.Find("Container");
        GameObject containerGo = containerTrans != null
            ? containerTrans.gameObject
            : new GameObject("Container",
                typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));

        if (containerTrans == null) containerGo.transform.SetParent(rootGo.transform, false);

        var containerRect = (RectTransform)containerGo.transform;
        containerRect.anchorMin = new Vector2(0f, 1f);
        containerRect.anchorMax = new Vector2(0f, 1f);
        containerRect.pivot     = new Vector2(0f, 1f);
        containerRect.anchoredPosition = Vector2.zero;

        // 칩이 6개를 넘으면 다음 줄로 내려가야 해서 Grid를 쓴다(HorizontalLayoutGroup은 줄바꿈이 없다).
        // 아이콘 기반이라 칩 폭이 균일해 고정 셀 크기가 잘 맞는다.
        // 이전 버전이 만든 HorizontalLayoutGroup이 남아 있으면 제거한다 — 둘이 공존하면 서로 배치를 덮어쓴다.
        var legacy = containerGo.GetComponent<HorizontalLayoutGroup>();
        if (legacy != null) Object.DestroyImmediate(legacy, true);

        var grid = containerGo.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = containerGo.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(ChipMinWidth, ChipHeight);
        grid.spacing  = new Vector2(ChipSpacing, ChipSpacing);
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis   = GridLayoutGroup.Axis.Horizontal;
        grid.constraint  = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = DefaultMaxPerRow;

        // 칩 개수에 따라 가로 폭이 자동으로 늘어나게 한다.
        var fitter = containerGo.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = containerGo.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        var so = new SerializedObject(listView);
        so.FindProperty("chipPrefab").objectReferenceValue = chipPrefab;
        so.FindProperty("container").objectReferenceValue  = containerRect;
        so.FindProperty("maxPerRow").intValue = DefaultMaxPerRow;
        so.ApplyModifiedPropertiesWithoutUndo();

        return listView;
    }

    private static void StretchFull(RectTransform rect, float padding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding, padding);
        rect.offsetMax = new Vector2(-padding, -padding);
    }

    private static void ConfigureText(
        TextMeshProUGUI text, TMP_FontAsset font, float size, TextAlignmentOptions alignment, Color color)
    {
        if (font != null) text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
    }
}
