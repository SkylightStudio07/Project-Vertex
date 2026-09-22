using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// ============================================================
// filename   : BlessingAffinityManagerEditor.cs
// description: BlessingAffinityManager 컴포넌트 전용 커스텀 인스펙터.
//              호감도 실시간 모니터링, 슬라이더, 원클릭 티어 점프,
//              호감도 가감(+0.5, +1.0, -1.0), 플래그 관리 및
//              전체 초기화 버튼을 제공합니다.
// ============================================================
[CustomEditor(typeof(BlessingAffinityManager))]
public class BlessingAffinityManagerEditor : Editor
{
    private string flagToAdd = "";

    public override void OnInspectorGUI()
    {
        var mgr = (BlessingAffinityManager)target;

        EditorGUILayout.Space(6);
        var titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("🔮 축복 호감도 & 플래그 매니저 (Affinity Debugger)", titleStyle);

        EditorGUILayout.HelpBox(
            "인스펙터에서 축복 캐릭터(마키나) 호감도 및 이벤트 플래그를 실시간으로 조작할 수 있습니다.\n" +
            "값 변경 시 즉시 PlayerPrefs 및 런타임에 동기화됩니다.",
            MessageType.Info);

        EditorGUILayout.Space(6);

        // Status Card
        float currentAff = mgr.GetAffinity("machina");
        int currentTier = mgr.GetAffinityTier("machina");
        string tierName = currentTier switch
        {
            0 => "Tier 0 (낯선 방랑자)",
            1 => "Tier 1 (희미한 온기)",
            2 => "Tier 2 (심층의 공명)",
            3 => "Tier 3 (침묵의 이해)",
            4 => "Tier 4 (영혼의 서약)",
            _ => $"Tier {currentTier}"
        };

        var boxStyle = new GUIStyle(EditorStyles.helpBox);
        boxStyle.padding = new RectOffset(10, 10, 8, 8);
        EditorGUILayout.BeginVertical(boxStyle);

        EditorGUILayout.LabelField("대상 캐릭터 : 마키나 (Machina)", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"현재 호감도 : {currentAff:F1} pt", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"현재 티어   : {tierName}", EditorStyles.boldLabel);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(8);

        // Quick Tier Jump Buttons
        EditorGUILayout.LabelField("⚡ 원클릭 티어 점프", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Tier 0\n(0.0)", GUILayout.Height(36)))
        {
            Undo.RecordObject(mgr, "Set Tier 0");
            mgr.SetTier0();
        }
        if (GUILayout.Button("Tier 1\n(2.0)", GUILayout.Height(36)))
        {
            Undo.RecordObject(mgr, "Set Tier 1");
            mgr.SetTier1();
        }
        if (GUILayout.Button("Tier 2\n(4.0)", GUILayout.Height(36)))
        {
            Undo.RecordObject(mgr, "Set Tier 2");
            mgr.SetTier2();
        }
        if (GUILayout.Button("Tier 3\n(6.0)", GUILayout.Height(36)))
        {
            Undo.RecordObject(mgr, "Set Tier 3");
            mgr.SetTier3();
        }
        if (GUILayout.Button("Tier 4\n(8.0)", GUILayout.Height(36)))
        {
            Undo.RecordObject(mgr, "Set Tier 4");
            mgr.SetTier4();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        // Delta Adjustment Buttons
        EditorGUILayout.LabelField("➕ 세부 호감도 가감", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+0.5 (조우)", GUILayout.Height(26)))
        {
            Undo.RecordObject(mgr, "Add 0.5 Affinity");
            mgr.AddAffinity("machina", 0.5f);
        }
        if (GUILayout.Button("+1.0 (4번 교감)", GUILayout.Height(26)))
        {
            Undo.RecordObject(mgr, "Add 1.0 Affinity");
            mgr.AddAffinity("machina", 1.0f);
        }
        if (GUILayout.Button("-1.0 (감소)", GUILayout.Height(26)))
        {
            Undo.RecordObject(mgr, "Subtract 1.0 Affinity");
            mgr.SetAffinity("machina", Mathf.Max(0f, currentAff - 1.0f));
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);

        // Standard Serialized Fields (Slider, Tier, etc.)
        serializedObject.Update();
        EditorGUILayout.LabelField("🎛️ 호감도 슬라이더 직접 조작", EditorStyles.boldLabel);
        SerializedProperty machinaAffProp = serializedObject.FindProperty("machinaAffinity");
        if (machinaAffProp != null)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(machinaAffProp, new GUIContent("마키나 호감도 (0 ~ 15)"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                mgr.SetAffinity("machina", machinaAffProp.floatValue);
            }
        }

        EditorGUILayout.Space(8);

        // Event Flags Management
        EditorGUILayout.LabelField("🚩 영구 이벤트 플래그 관리", EditorStyles.boldLabel);
        var flags = mgr.GetAllFlags();
        if (flags.Count == 0)
        {
            EditorGUILayout.HelpBox("현재 활성화된 이벤트 플래그가 없습니다.", MessageType.None);
        }
        else
        {
            foreach (var flag in flags)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"• {flag}");
                if (GUILayout.Button("제거", GUILayout.Width(50)))
                {
                    Undo.RecordObject(mgr, "Remove Flag");
                    mgr.SetFlag(flag, false);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.BeginHorizontal();
        flagToAdd = EditorGUILayout.TextField(flagToAdd);
        if (GUILayout.Button("플래그 추가", GUILayout.Width(80)))
        {
            if (!string.IsNullOrWhiteSpace(flagToAdd))
            {
                Undo.RecordObject(mgr, "Add Flag");
                mgr.SetFlag(flagToAdd.Trim(), true);
                flagToAdd = "";
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(14);

        // Reset Button
        GUI.backgroundColor = new Color(1f, 0.45f, 0.45f);
        if (GUILayout.Button("⚠️ [데이터 초기화] 모든 호감도 및 플래그 리셋", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("호감도 데이터 초기화", "정말로 모든 호감도(PlayerPrefs)와 이벤트 플래그를 초기화하시겠습니까?", "초기화", "취소"))
            {
                Undo.RecordObject(mgr, "Reset All Affinity");
                mgr.ResetAll();
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space(8);
    }
}
