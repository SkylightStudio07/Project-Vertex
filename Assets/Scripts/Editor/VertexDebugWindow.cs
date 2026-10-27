using System.Linq;
using UnityEditor;
using UnityEngine;

// ============================================================
// filename   : VertexDebugWindow.cs
// description: 통합 디버그 창 (Tools ▸ Vertex ▸ 디버그 패널).
//              탭마다 플레이 중 바로 쓰는 도구를 모은다. 새 디버그 기능은 여기에 탭/섹션으로 추가할 것
//              (인스펙터마다 따로 만들지 않는다).
//              - 동료: 구출 플래그 해금 · 성소 후보 강제 · 이번 런에 즉시 합류
//              - 호감도: 협력자 호감도 레벨 · 마키나 · 토냐 · 로비 시설 담당 호감도 등급(0~4) · 이벤트 플래그 초기화
//              - 적 · 전투: 훈련 상대 전부 해금(격퇴 기록 무시), 조우 목록, 지금 바로 전투 시작
//              - 스크린샷: 게임뷰만 PNG로 저장 (F12 · 지연 촬영) → ScreenshotTool
//              저장된 기록(격퇴 수 · 호감도 파일)은 호감도 버튼을 빼고 건드리지 않는다.
// ============================================================
public class VertexDebugWindow : EditorWindow
{
    private static readonly string[] Tabs = { "동료", "호감도", "적 · 전투" };
    private int _tab;
    private Vector2 _scroll;
    private string _filter = "";
    private float _shotDelay = 3f;
    private EnemyEncounter[] _encounters;

    [MenuItem("Tools/Vertex/디버그 패널")]
    private static void Open() => GetWindow<VertexDebugWindow>("Vertex 디버그").Show();

    private void OnEnable() => _encounters = null;
    private void OnInspectorUpdate() { if (Application.isPlaying) Repaint(); }

    private void OnGUI()
    {
        DrawScreenshot();
        _tab = GUILayout.Toolbar(_tab, Tabs);
        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("플레이 중에 동작합니다. (훈련 상대 해금 토글만 미리 켜 둘 수 있어요.)", MessageType.Info);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        if (_tab == 0) DrawCompanions(); else if (_tab == 1) DrawAffinity(); else DrawEnemies();
        EditorGUILayout.EndScrollView();
    }

    // ── 스크린샷 (게임뷰만, UI 포함) ──
    private void DrawScreenshot()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("📷 스크린샷 (F12)", GUILayout.Height(24))) ScreenshotTool.Capture();
            _shotDelay = EditorGUILayout.FloatField(_shotDelay, GUILayout.Width(40));
            if (GUILayout.Button(ScreenshotTool.Pending ? $"{ScreenshotTool.Remaining:0.0}초 뒤…" : "초 뒤에 찍기", GUILayout.Height(24), GUILayout.Width(100)))
                ScreenshotTool.CaptureAfter(_shotDelay);
        }
        if (GUILayout.Button("폴더 열기", GUILayout.Height(24), GUILayout.Width(70))) ScreenshotTool.Reveal();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            var prev = GUI.backgroundColor;
            if (GameViewRecorder.IsRecording) GUI.backgroundColor = new Color(1f, 0.45f, 0.45f);
            string label = GameViewRecorder.IsRecording ? $"■ 녹화 정지 (F10)   {GameViewRecorder.Elapsed:0.0}초" : "● 녹화 시작 (F10)";
            if (GUILayout.Button(label, GUILayout.Height(24))) ScreenshotTool.ToggleRecording();
            GUI.backgroundColor = prev;
        }
        if (GUILayout.Button("폴더 열기", GUILayout.Height(24), GUILayout.Width(70))) ScreenshotTool.RevealRecordings();
        EditorGUILayout.EndHorizontal();
    }

    // ── 동료 ──
    private void DrawCompanions()
    {
        var mgr = CooperationManager.Instance;
        if (!Application.isPlaying || mgr == null) { EditorGUILayout.LabelField("협력자 매니저가 없습니다 (전투 씬을 플레이해 주세요)."); return; }

        var flags = BlessingAffinityManager.Instance;
        foreach (var data in mgr.AllCharData())
        {
            string id = data.charID;
            bool unlocked = flags != null && flags.HasFlag(HolyPlaceManager.RescueFlagOf(id));
            bool forced = HolyPlaceManager.DebugForcedCandidates.Contains(id);
            bool joined = mgr.IsJoinedInRun(id);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"{data.charName}  ({id})", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"해금 {(unlocked ? "✔" : "✘")}   성소 강제 {(forced ? "✔" : "✘")}   런 합류 {(joined ? "✔" : "✘")}   호감도 Lv.{mgr.GetCoopLevel(id)} (호감도 탭에서 변경)");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(unlocked ? "잠금으로" : "해금 (구출 완료)")) flags?.SetFlag(HolyPlaceManager.RescueFlagOf(id), !unlocked);
            if (GUILayout.Button(forced ? "성소 강제 해제" : "성소 후보로 강제"))
            {
                if (forced) HolyPlaceManager.DebugForcedCandidates.Remove(id);
                else HolyPlaceManager.DebugForcedCandidates.Add(id);
            }
            using (new EditorGUI.DisabledScope(joined))
                if (GUILayout.Button("이번 런에 합류") && !mgr.DebugJoinNow(id))
                    Debug.LogWarning($"[디버그] {id} 합류 실패 — 동행 인원이 가득 찼거나 매니저가 없음");
            if (joined && GUILayout.Button("거점 귀환")) mgr.DismissFromRun(id);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        if (GUILayout.Button("성소 강제 후보 모두 해제")) HolyPlaceManager.DebugForcedCandidates.Clear();
    }

    // ── 호감도 ──
    // 협력자는 레벨(0~최대), 마키나 · 상점 주인 · 로비 담당은 BlessingAffinityManager의 호감도 값(등급 0~4 = 0 / 2 / 4 / 6 / 8)
    private static readonly float[] TierValue = { 0f, 2f, 4f, 6f, 8f };

    private void DrawAffinity()
    {
        if (!Application.isPlaying) { EditorGUILayout.LabelField("플레이 중에 바꿀 수 있습니다."); return; }

        var aff = BlessingAffinityManager.Instance;
        EditorGUILayout.LabelField("NPC 호감도 (등급 버튼 = 그 등급의 하한값으로 설정)", EditorStyles.boldLabel);
        if (aff == null) EditorGUILayout.HelpBox("BlessingAffinityManager가 없습니다.", MessageType.Warning);
        else
        {
            DrawNpc(aff, "machina", "마키나 (축복)");
            foreach (var g in AssetDatabase.FindAssets("t:ShopkeeperData"))
            {
                var d = AssetDatabase.LoadAssetAtPath<ShopkeeperData>(AssetDatabase.GUIDToAssetPath(g));
                if (d != null) DrawNpc(aff, d.entityId, $"{d.entityName} (상점 주인)");
            }
            foreach (var g in AssetDatabase.FindAssets("t:LobbyKeeperData"))
            {
                var d = AssetDatabase.LoadAssetAtPath<LobbyKeeperData>(AssetDatabase.GUIDToAssetPath(g));
                if (d != null) DrawNpc(aff, d.entityId, $"{d.displayName} (로비)");
            }
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("협력자 호감도 레벨", EditorStyles.boldLabel);
        var mgr = CooperationManager.Instance;
        if (mgr == null) { EditorGUILayout.LabelField("협력자 매니저가 없습니다."); return; }
        foreach (var data in mgr.AllCharData())
        {
            string id = data.charID;
            int level = mgr.GetCoopLevel(id);
            int max = Mathf.Max(1, data.maxCoopLevel);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"{data.charName}  Lv.{level}/{max}", GUILayout.Width(180));
            for (int lv = 0; lv <= max; lv++)
            {
                var prev = GUI.backgroundColor;
                if (lv == level) GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
                if (GUILayout.Button(lv.ToString(), GUILayout.Width(32))) mgr.DebugSetLevel(id, lv);
                GUI.backgroundColor = prev;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private static void DrawNpc(BlessingAffinityManager aff, string id, string label)
    {
        if (string.IsNullOrEmpty(id)) return;
        float value = aff.GetAffinity(id);
        int tier = aff.GetAffinityTier(id);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"{label}   {id}   호감도 {value:0.0}  ·  등급 {tier}", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("등급", GUILayout.Width(36));
        for (int t = 0; t < TierValue.Length; t++)
        {
            var prev = GUI.backgroundColor;
            if (t == tier) GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
            if (GUILayout.Button(t.ToString(), GUILayout.Width(32))) aff.SetAffinity(id, TierValue[t]);
            GUI.backgroundColor = prev;
        }
        if (GUILayout.Button("+0.5", GUILayout.Width(46))) aff.AddAffinity(id, 0.5f);
        if (GUILayout.Button("−0.5", GUILayout.Width(46))) aff.SetAffinity(id, Mathf.Max(0f, value - 0.5f));
        if (GUILayout.Button("이벤트 플래그 초기화")) ResetFlags(aff, id);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    // 이 NPC의 1회성 이벤트(등급업 · 첫 만남 등)를 다시 볼 수 있게 "<id>_..." 플래그를 지운다
    private static void ResetFlags(BlessingAffinityManager aff, string id)
    {
        int n = 0;
        foreach (var f in aff.GetAllFlags())
            if (f.StartsWith(id + "_")) { aff.SetFlag(f, false); n++; }
        Debug.Log($"[디버그] {id} 이벤트 플래그 {n}개 제거");
    }

    // ── 적 · 전투 ──
    private void DrawEnemies()
    {
        bool unlockAll = EditorPrefs.GetBool(TrainingView.DebugUnlockPref, false);
        bool next = EditorGUILayout.ToggleLeft("훈련장: 모든 훈련 상대 해금 (격퇴 기록 무시)", unlockAll);
        if (next != unlockAll)
        {
            EditorPrefs.SetBool(TrainingView.DebugUnlockPref, next);
            TrainingView.DebugUnlockAll = next;
        }
        EditorGUILayout.Space(4);

        if (_encounters == null)
            _encounters = AssetDatabase.FindAssets("t:EnemyEncounter")
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyEncounter>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(e => e != null && e.enemies != null && e.enemies.Count > 0)
                .OrderBy(e => e.chapter).ThenByDescending(e => (int)e.encounterType).ThenBy(e => e.name).ToArray();

        EditorGUILayout.BeginHorizontal();
        _filter = EditorGUILayout.TextField("검색", _filter);
        if (GUILayout.Button("새로고침", GUILayout.Width(70))) _encounters = null;
        EditorGUILayout.EndHorizontal();

        bool canStart = Application.isPlaying && GameManager.Instance != null && BattleManager.Instance != null;
        if (!canStart) EditorGUILayout.HelpBox("전투 씬을 플레이 중일 때 '전투 시작'이 켜집니다.", MessageType.None);

        if (_encounters == null) return;
        foreach (var enc in _encounters)
        {
            string names = string.Join(", ", enc.enemies.Where(e => e != null).Select(e => e.enemyName));
            string label = $"{enc.name}  ·  {names}";
            if (!string.IsNullOrEmpty(_filter) && label.IndexOf(_filter, System.StringComparison.OrdinalIgnoreCase) < 0) continue;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"[{enc.chapter}막 {TypeName(enc.encounterType)}]", GUILayout.Width(80));
            EditorGUILayout.LabelField(label);
            using (new EditorGUI.DisabledScope(!canStart))
                if (GUILayout.Button("전투 시작", GUILayout.Width(80)))
                    GameManager.Instance.StartEncounterBattle(enc, (BattleType)(int)enc.encounterType);
            if (GUILayout.Button("선택", GUILayout.Width(50))) Selection.activeObject = enc;
            EditorGUILayout.EndHorizontal();
        }
    }

    private static string TypeName(EnemyEncounterType t) => t switch
    {
        EnemyEncounterType.Boss => "보스", EnemyEncounterType.Elite => "엘리트", _ => "일반",
    };
}
