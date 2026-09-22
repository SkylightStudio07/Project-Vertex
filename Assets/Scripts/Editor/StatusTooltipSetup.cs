using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// 상태 칩 호버 확대(HoverScaleEffect) + 툴팁 패널(StatusTooltipView)을 구성한다.
// 1회성 배선 스크립트라 툴바 메뉴엔 안 올린다. 필요하면 Unity CLI로 직접 호출:
// unity command eval 'StatusTooltipSetup.Setup();'
public static class StatusTooltipSetup
{
    private const string ChipPrefabPath = "Assets/Data/UI/Prefabs/StatusChip.prefab";
    private const string FontPath = "Assets/Font/강원교육튼튼 SDF.asset";

    // 툴팁 패널 크기/위치. "아군 스프라이트 앞"에 고정 — 마우스를 따라다니지 않는다.
    // 이 씬(DevelopScene - Phase 3)의 파티 배치를 기준으로 잡은 값이라, 파티 위치가
    // 크게 바뀌면 이 오브젝트를 씬에서 직접 옮기면 된다(평범한 UI RectTransform이라 자유롭게 이동 가능).
    //
    // 반드시 Canvas의 "직속 자식"으로 둬야 한다 — BattleUI 밑에 넣으면 BattleUI 자체에 걸린
    // 로컬 스케일(약 0.72, Canvas의 0.01과 합쳐져 실질적으로 좌표 변화가 거의 안 보일 정도로
    // 뭉개짐) 때문에 anchoredPosition 숫자가 거의 의미가 없어진다. HPBackground/PartyViewer 등
    // 기존 HUD 요소들도 전부 Canvas 직속이라 이 값들은 그 좌표계 기준이다.
    // (Play 모드에서 실제 파티 위치를 Camera.WorldToScreenPoint + Canvas 기준
    // RectTransformUtility.ScreenPointToLocalPointInRectangle로 역산해서 잡은 값.
    // 검증은 capture_game_view --source screen으로 했다 — 기본값(source=camera)은
    // 이 프로젝트의 Screen Space-Camera 캔버스 UI를 제대로 못 잡아서 아무것도 안 보이는
    // 것처럼 나온다.)
    private static readonly Vector2 PanelAnchoredPos = new(-650f, 180f);
    private static readonly Vector2 PanelSize = new(460f, 160f);

    public static void Setup()
    {
        AddHoverScaleToChip();
        var panel = CreateOrLoadTooltipPanel();
        int raycasters = EnsureRaycastersForStatusLists();
        int strays = RemoveStrayChipsFromScene();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        Debug.Log($"[StatusTooltipSetup] 칩에 HoverScaleEffect 추가 완료.\n툴팁 패널: '{panel.name}' 구성 완료.\n" +
                  $"GraphicRaycaster 추가: {raycasters}개, 씬에 남아 있던 칩 제거: {strays}개");
    }

    // 상태 목록이 중첩 Canvas(예: HPBackground, overrideSorting) 안에 있으면 그 Canvas에
    // GraphicRaycaster가 따로 있어야 한다 — 부모 Canvas의 레이캐스터는 중첩 Canvas의 UI를 잡지 않는다.
    // HPBackground에 레이캐스터가 없어서 플레이어 쪽 칩에 마우스 이벤트가 전혀 안 들어갔다(호버 무반응).
    private static int EnsureRaycastersForStatusLists()
    {
        int added = 0;
        foreach (var list in Object.FindObjectsByType<StatusListView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var canvas = list.GetComponentInParent<Canvas>(true);
            if (canvas == null || canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() != null) continue;

            Undo.AddComponent<UnityEngine.UI.GraphicRaycaster>(canvas.gameObject);
            added++;
        }
        return added;
    }

    // 칩은 StatusListView가 런타임에만 생성한다. 에디트 모드 씬에 칩이 있으면 테스트하다 저장돼 버린 것이고,
    // StatusListView가 추적하지 않는 칩이라 절대 정리되지 않고 엉뚱한 상태(화상 등)를 계속 보여준다.
    private static int RemoveStrayChipsFromScene()
    {
        if (Application.isPlaying) return 0; // 플레이 중엔 진짜 칩이라 건드리면 안 된다

        var strays = Object.FindObjectsByType<StatusChipView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var chip in strays)
            Undo.DestroyObjectImmediate(chip.gameObject);
        return strays.Length;
    }

    // ---------- 칩 호버 확대 ----------

    private static void AddHoverScaleToChip()
    {
        var root = PrefabUtility.LoadPrefabContents(ChipPrefabPath);
        try
        {
            if (root.GetComponent<HoverScaleEffect>() == null)
            {
                var hover = root.AddComponent<HoverScaleEffect>();
                var so = new SerializedObject(hover);
                // 칩이 34px로 작아서 카드(1.2배)보다 좀 더 확대해야 눈에 띈다.
                so.FindProperty("hoverScaleMultiplier").floatValue = 1.4f;
                so.FindProperty("animationDuration").floatValue = 0.1f;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            PrefabUtility.SaveAsPrefabAsset(root, ChipPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // ---------- 툴팁 패널 ----------

    private static GameObject CreateOrLoadTooltipPanel()
    {
        var existing = Object.FindFirstObjectByType<StatusTooltipView>(FindObjectsInactive.Include);
        if (existing != null) return existing.gameObject;

        // Canvas의 "직속" 자식으로 붙인다 — 위 PanelAnchoredPos 주석 참고.
        // BattleUI 밑에 넣으면 좌표가 거의 의미 없어지는 버그를 실측 중 겪었다.
        Transform parent = GameObject.Find("/Canvas")?.transform;
        if (parent == null)
        {
            Debug.LogError("[StatusTooltipSetup] Canvas를 씬에서 찾지 못함.");
            return null;
        }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        var root = new GameObject("StatusTooltipView",
            typeof(RectTransform), typeof(Image), typeof(StatusTooltipView));
        root.transform.SetParent(parent, false);
        var rootRect = (RectTransform)root.transform;
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot     = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = PanelAnchoredPos;
        rootRect.sizeDelta = PanelSize;

        var background = root.GetComponent<Image>();
        background.color = new Color(0.06f, 0.08f, 0.10f, 0.92f);
        background.raycastTarget = false; // 툴팁 자체는 마우스 입력을 가로채면 안 된다(칩 이벤트만 반응)

        // 아이콘 — 좌상단.
        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(root.transform, false);
        var iconRect = (RectTransform)iconGo.transform;
        iconRect.anchorMin = new Vector2(0f, 1f);
        iconRect.anchorMax = new Vector2(0f, 1f);
        iconRect.pivot     = new Vector2(0f, 1f);
        iconRect.anchoredPosition = new Vector2(14f, -14f);
        iconRect.sizeDelta = new Vector2(40f, 40f);
        var iconImage = iconGo.GetComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        // 제목 — 아이콘 오른쪽.
        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(root.transform, false);
        var titleRect = (RectTransform)titleGo.transform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot     = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(64f, -14f);
        titleRect.sizeDelta = new Vector2(-78f, 40f); // 오른쪽 여백만큼 폭에서 뺌
        var titleText = titleGo.GetComponent<TextMeshProUGUI>();
        ConfigureText(titleText, font, 22f, TextAlignmentOptions.MidlineLeft, Color.white);
        titleText.fontStyle = FontStyles.Bold;

        // 설명 — 아래쪽 전체 폭.
        var descGo = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI));
        descGo.transform.SetParent(root.transform, false);
        var descRect = (RectTransform)descGo.transform;
        descRect.anchorMin = new Vector2(0f, 0f);
        descRect.anchorMax = new Vector2(1f, 1f);
        descRect.offsetMin = new Vector2(14f, 12f);
        descRect.offsetMax = new Vector2(-14f, -60f); // 위쪽은 아이콘/제목 영역 아래부터
        var descText = descGo.GetComponent<TextMeshProUGUI>();
        ConfigureText(descText, font, 17f, TextAlignmentOptions.TopLeft, new Color(0.88f, 0.88f, 0.90f));
        descText.enableWordWrapping = true;

        var so2 = new SerializedObject(root.GetComponent<StatusTooltipView>());
        so2.FindProperty("iconImage").objectReferenceValue = iconImage;
        so2.FindProperty("titleText").objectReferenceValue = titleText;
        so2.FindProperty("descriptionText").objectReferenceValue = descText;
        so2.ApplyModifiedPropertiesWithoutUndo();

        // 여기서 SetActive(false)로 꺼서 저장하면 안 된다 — Unity는 씬 로드 시점에
        // 이미 비활성인 오브젝트는 Awake()를 호출하지 않는다. Awake가 Instance를 설정하기 전에
        // 꺼져버리면 Instance가 영영 null로 남고, Show()는 그 Instance로만 켤 수 있으니
        // 아무도 다시 켜줄 수 없는 순환 문제가 생긴다(실제로 겪은 버그).
        // 그래서 씬엔 "활성" 상태로 저장해두고, StatusTooltipView.Awake() 자신이
        // Instance를 먼저 설정한 뒤 스스로를 끄게 한다.
        return root;
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
