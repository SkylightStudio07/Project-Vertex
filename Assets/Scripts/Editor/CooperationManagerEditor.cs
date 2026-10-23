using UnityEditor;
using UnityEngine;

// ============================================================
// filename   : CooperationManagerEditor.cs
// description: CooperationManager 인스펙터에 협력자 디버그 패널을 붙인다 (플레이 중에만 동작).
//              - 구출 플래그 해금/잠금(coop_rescued_{ID}) → 성소 후보에 올라오는 조건
//              - 성소 후보로 강제 노출(층과 무관), 이번 런에 즉시 합류
//              - 호감도 레벨 설정 (0~최대)
// ============================================================
[CustomEditor(typeof(CooperationManager))]
public class CooperationManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🛠 협력자 디버그", EditorStyles.boldLabel);

        var mgr = (CooperationManager)target;
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("플레이 중에만 동작합니다. 해금 · 합류 · 호감도 값은 플레이 모드에서 바꿔 주세요.", MessageType.Info);
            return;
        }

        var flags = BlessingAffinityManager.Instance;
        foreach (var data in mgr.AllCharData())
        {
            string id = data.charID;
            bool unlocked = flags != null && flags.HasFlag(HolyPlaceManager.RescueFlagOf(id));
            bool forced = HolyPlaceManager.DebugForcedCandidates.Contains(id);
            bool joined = mgr.IsJoinedInRun(id);
            int level = mgr.GetCoopLevel(id);
            int max = Mathf.Max(1, data.maxCoopLevel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"{data.charName}  ({id})", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                $"해금 {(unlocked ? "✔" : "✘")}   성소 강제 {(forced ? "✔" : "✘")}   런 합류 {(joined ? "✔" : "✘")}   호감도 Lv.{level} / {max}");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(unlocked ? "잠금으로" : "해금 (구출 완료)"))
                flags?.SetFlag(HolyPlaceManager.RescueFlagOf(id), !unlocked);
            if (GUILayout.Button(forced ? "성소 강제 해제" : "성소 후보로 강제"))
            {
                if (forced) HolyPlaceManager.DebugForcedCandidates.Remove(id);
                else HolyPlaceManager.DebugForcedCandidates.Add(id);
            }
            using (new EditorGUI.DisabledScope(joined))
                if (GUILayout.Button("이번 런에 합류"))
                    if (!mgr.DebugJoinNow(id)) Debug.LogWarning($"[협력자 디버그] {id} 합류 실패 — 동행 인원이 가득 찼거나 매니저가 없음");
            if (joined && GUILayout.Button("거점 귀환")) mgr.DismissFromRun(id);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("호감도 Lv.", GUILayout.Width(70));
            for (int lv = 0; lv <= max; lv++)
            {
                var prev = GUI.backgroundColor;
                if (lv == level) GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
                if (GUILayout.Button(lv.ToString(), GUILayout.Width(32))) mgr.DebugSetLevel(id, lv);
                GUI.backgroundColor = prev;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("성소 강제 후보 모두 해제")) HolyPlaceManager.DebugForcedCandidates.Clear();
        Repaint();
    }
}
