using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public sealed partial class CardAuthoringWindow : EditorWindow
{
    [SerializeField] private CardData card;
    [SerializeField] private int page;
    [SerializeField] private int upgradeMode;
    [SerializeField] private Vector2 scroll;
    private SerializedObject serializedCard;
    private readonly AdvancedDropdownState dropdownState = new();
    private TypeDropdown dropdown;
    private string normalPreview, upgradePreview, tokenHint;
    private bool previewDirty = true;
    private bool presentation;

    [MenuItem("Tools/Cards/Card Authoring")]
    public static void Open()
    {
        var window = GetWindow<CardAuthoringWindow>("카드 제작");
        window.minSize = new Vector2(820, 650);
        if (Selection.activeObject is CardData selected && window.SelectCard(selected)) window.workspacePage = 0;
        window.Show();
    }

    [MenuItem("Assets/카드 제작 창에서 열기", true)]
    private static bool CanOpenSelected() => Selection.activeObject is CardData;

    [MenuItem("Assets/카드 제작 창에서 열기")]
    private static void OpenSelected() => Open();

    private void OnEnable()
    {
        minSize = new Vector2(820, 650);
        Undo.undoRedoPerformed += OnUndo;
        EditorApplication.projectChanged += OnUndo;
        InitializeCatalog();
        if (card != null) BindCard(card, ownsDraft, false);
        else { ownsDraft = false; hasUnsavedChanges = false; }
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndo;
        EditorApplication.projectChanged -= OnUndo;
        serializedCard?.Dispose();
        serializedCard = null;
    }

    private void OnUndo() { previewDirty = true; catalogDirty = true; Repaint(); }

    private bool SelectCard(CardData selected)
    {
        if (selected == card) return true;
        if (!TryLeaveDraft()) return false;
        BindCard(selected, false);
        return true;
    }

    private void BindCard(CardData selected, bool draft, bool resetFolder = true)
    {
        serializedCard?.Dispose();
        card = selected;
        ownsDraft = draft;
        serializedCard = card != null ? new SerializedObject(card) : null;
        hasUnsavedChanges = ownsDraft && card != null;
        saveChangesMessage = "작성 중인 카드가 아직 SO로 저장되지 않았습니다.";
        if (resetFolder) ChooseInitialFolder();
        previewDirty = true;
        scroll = Vector2.zero;
        Repaint();
    }

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            var selected = (CardData)EditorGUILayout.ObjectField(card, typeof(CardData), false, GUILayout.MinWidth(170));
            if (selected != card && SelectCard(selected)) workspacePage = 0;
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("새 카드", EditorStyles.toolbarButton, GUILayout.Width(70))) CreateCard(false);
                using (new EditorGUI.DisabledScope(card == null))
                {
                    if (GUILayout.Button("복제", EditorStyles.toolbarButton, GUILayout.Width(50))) CreateCard(true);
                    if (GUILayout.Button("저장", EditorStyles.toolbarButton, GUILayout.Width(50))) SaveCard();
                }
            }
        }
        workspacePage = GUILayout.Toolbar(workspacePage, new[] { "카드 제작", "Player 전체 카드 · 보상 풀" });
        if (workspacePage == 1) { DrawCatalog(); return; }
        if (card == null)
        {
            EditorGUILayout.HelpBox("새 카드는 임시 초안으로 시작합니다. 효과·강화·설명을 모두 작성한 뒤 저장을 누르면 SO가 생성됩니다.\n기존 CardData를 드래그하거나 전체 카드 목록에서 열 수도 있습니다.", MessageType.Info);
            return;
        }
        if (serializedCard == null) BindCard(card, ownsDraft, false);
        if (serializedCard.UpdateIfRequiredOrScript()) previewDirty = true;
        EditorGUILayout.LabelField(ownsDraft ? "미저장 초안 · 저장할 때 SO 생성" : AssetDatabase.GetAssetPath(card) + (EditorUtility.IsDirty(card) ? "  • 변경됨" : ""), EditorStyles.miniLabel);
        EditorGUILayout.LabelField("Ctrl+Z로 되돌리기 · 기존 SO는 직접 편집하며 저장 시 경로·파일명을 반영합니다.", EditorStyles.miniLabel);
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            EditorGUILayout.HelpBox("플레이 모드에서는 카드 제작을 잠급니다. 플레이를 종료한 뒤 편집하세요.", MessageType.Info);

        using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
        {
            DrawSaveSettings();
            DrawCurrentPool();
            page = GUILayout.Toolbar(page, new[] { "1  기본 정보·이미지", "2  기본 효과", "3  강화", "4  설명·미리보기" });
            using (var view = new EditorGUILayout.ScrollViewScope(scroll))
            {
                scroll = view.scrollPosition;
                EditorGUI.BeginChangeCheck();
                switch (page)
                {
                    case 0: DrawIdentity(); break;
                    case 1:
                        DrawState(serializedCard.FindProperty("normalState"));
                        DrawNode(serializedCard.FindProperty("normalState.effects"), typeof(List<CardEffect>), "실행 순서대로 효과 구성");
                        break;
                    case 2: DrawUpgrade(); break;
                    case 3: DrawDescription(); break;
                }
                if (EditorGUI.EndChangeCheck()) previewDirty = true;
            }
        }
        if (serializedCard.ApplyModifiedProperties()) previewDirty = true;
    }

    private void DrawIdentity()
    {
        Title("카드 분류");
        Field("normalState.cardName", "카드 이름");
        Field("cardType", "종류");
        Field("cardRarity", "희귀도");
        Field("cardOwner", "소유자");
        Field("useMode", "사용 방식");
        Field("isWeaponShootingCard", "무기에 따라 교체되는 사격 카드");
        Title("이미지 드래그 앤 드롭");
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawSprite("cardImage", "카드 일러스트");
            DrawSprite("CardBackground", "카드 배경");
        }
        presentation = EditorGUILayout.Foldout(presentation, "연출·획득 시 강화 상태", true);
        if (presentation)
        {
            Field("useAnimation", "사용 애니메이션");
            Field("useSFX", "사용 효과음");
            Field("isUpgraded", "획득 시부터 강화된 카드");
        }
        if (serializedCard.FindProperty("isUpgraded").boolValue)
            EditorGUILayout.HelpBox("이 원본은 획득 시점부터 강화 상태입니다.", MessageType.Warning);
    }

    private void DrawSprite(string path, string label)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            var property = serializedCard.FindProperty(path);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            Rect rect = GUILayoutUtility.GetRect(120, 140, GUILayout.ExpandWidth(true));
            var sprite = property.objectReferenceValue as Sprite;
            GUI.Box(rect, GUIContent.none);
            if (sprite != null)
            {
                var preview = AssetPreview.GetAssetPreview(sprite);
                if (preview != null) GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);
                else { GUI.Label(rect, sprite.name, EditorStyles.centeredGreyMiniLabel); Repaint(); }
            }
            else GUI.Label(rect, "Sprite / 이미지 파일을 여기에 놓기", EditorStyles.centeredGreyMiniLabel);
            var evt = Event.current;
            if (rect.Contains(evt.mousePosition) && (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform))
            {
                var sprites = new List<Sprite>();
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is Sprite direct) sprites.Add(direct);
                    else if (obj is Texture2D)
                        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(obj)))
                            if (asset is Sprite candidate) sprites.Add(candidate);
                }
                DragAndDrop.visualMode = sprites.Count > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    if (sprites.Count == 1) property.objectReferenceValue = sprites[0];
                    else if (sprites.Count > 1)
                    {
                        var menu = new GenericMenu();
                        var owner = card;
                        for (int i = 0; i < sprites.Count; i++)
                        {
                            var chosen = sprites[i];
                            menu.AddItem(new GUIContent($"{i}: {chosen.name}"), false, () =>
                            {
                                if (owner == null) return;
                                using var so = new SerializedObject(owner);
                                so.FindProperty(path).objectReferenceValue = chosen;
                                so.ApplyModifiedProperties();
                                OnUndo();
                            });
                        }
                        menu.ShowAsContext();
                    }
                    else ShowNotification(new GUIContent("Texture Import Settings에서 Sprite (2D and UI)로 설정하세요."));
                    previewDirty = true;
                }
                evt.Use();
            }
            EditorGUILayout.PropertyField(property, GUIContent.none);
            EditorGUILayout.LabelField("여러 Sprite가 있는 이미지는 선택 메뉴가 열립니다.", EditorStyles.miniLabel);
        }
    }

    private void DrawState(SerializedProperty state)
    {
        Title("이름·비용·키워드");
        EditorGUILayout.PropertyField(state.FindPropertyRelative("cardName"), new GUIContent("이름"));
        EditorGUILayout.PropertyField(state.FindPropertyRelative("energyCost"), new GUIContent("에너지 비용"));
        EditorGUILayout.PropertyField(state.FindPropertyRelative("ammoCost"), new GUIContent("탄약 비용"));
        using (new EditorGUILayout.HorizontalScope())
        {
            Toggle(state, "isExhaust", "소멸"); Toggle(state, "isEthereal", "휘발성");
            Toggle(state, "isInnate", "선천성"); Toggle(state, "isRetain", "보존");
        }
        if (serializedCard.FindProperty("cardType").enumValueIndex == (int)CardData.CardType.Power)
            EditorGUILayout.HelpBox("Power 카드는 전투 규칙상 소멸 체크와 무관하게 사용 후 소멸합니다.", MessageType.Info);
    }

    private void DrawUpgrade()
    {
        EditorGUILayout.HelpBox("기본 구성을 완성한 뒤 한 번 복사하고 강화 값을 수정하세요. 두 상태는 독립 저장되며, 이후 기본 값 변경은 자동 반영되지 않습니다.", MessageType.Info);
        if (GUILayout.Button("기본 상태 전체를 강화에 복사 (이름 +)"))
        {
            if (EditorUtility.DisplayDialog("강화 상태 복사", "현재 강화 이름·비용·키워드·효과를 기본 상태의 복사본으로 교체합니다. Ctrl+Z로 되돌릴 수 있습니다.", "복사", "취소"))
            {
                serializedCard.ApplyModifiedProperties();
                CardAuthoringUtility.CopyNormalToUpgrade(card);
                serializedCard.Update();
                previewDirty = true;
            }
        }
        upgradeMode = GUILayout.Toolbar(upgradeMode, new[] { "수치·키워드 빠른 수정", "효과 구성 직접 편집" });
        var upgraded = serializedCard.FindProperty("upgradedState");
        DrawState(upgraded);
        if (upgradeMode == 1)
        {
            DrawNode(upgraded.FindPropertyRelative("effects"), typeof(List<CardEffect>), "강화 효과 (추가·교체·삭제 가능)");
            return;
        }
        var normal = serializedCard.FindProperty("normalState.effects");
        var effects = upgraded.FindPropertyRelative("effects");
        if (!CardAuthoringUtility.SameShape(normal, effects))
        {
            EditorGUILayout.HelpBox("일반/강화 효과의 종류 또는 중첩 구성이 다릅니다. 기본 상태를 복사하거나 '효과 구성 직접 편집'에서 수정하세요.", MessageType.Warning);
            return;
        }
        Title("효과 수치 비교 — 왼쪽 기본 / 오른쪽 강화");
        if (effects.arraySize == 0) EditorGUILayout.HelpBox("기본 효과를 먼저 추가한 뒤 강화에 복사하세요.", MessageType.Info);
        DrawQuick(normal, effects, "효과");
        EditorGUILayout.HelpBox("대상·조건 종류·참조 에셋을 바꾸려면 '효과 구성 직접 편집'을 사용하세요. 수치는 최종 값이며 증감량이 아닙니다.", MessageType.None);
    }

    private void DrawQuick(SerializedProperty normal, SerializedProperty upgraded, string label)
    {
        if (normal.propertyType == SerializedPropertyType.ManagedReference && normal.managedReferenceValue != null)
            Title(label + " · " + CardAuthoringUtility.TypeLabel(normal.managedReferenceValue.GetType()));
        if (normal.type == nameof(EffectValue) &&
            normal.FindPropertyRelative("source").enumValueIndex == 0 &&
            upgraded.FindPropertyRelative("source").enumValueIndex == 0) return;
        if (normal.isArray && normal.propertyType != SerializedPropertyType.String)
        {
            for (int i = 0; i < normal.arraySize; i++)
                DrawQuick(normal.GetArrayElementAtIndex(i), upgraded.GetArrayElementAtIndex(i), $"{label} [{i}]");
            return;
        }
        if (normal.propertyType == SerializedPropertyType.Integer || normal.propertyType == SerializedPropertyType.Float || normal.propertyType == SerializedPropertyType.Boolean)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.MinWidth(200));
                using (new EditorGUI.DisabledScope(true)) EditorGUILayout.PropertyField(normal, GUIContent.none, GUILayout.Width(90));
                EditorGUILayout.LabelField("→", GUILayout.Width(18));
                bool different = !SerializedProperty.DataEquals(normal, upgraded);
                var tint = GUI.backgroundColor;
                if (different) GUI.backgroundColor = new Color(0.6f, 1f, 0.65f);
                EditorGUILayout.PropertyField(upgraded, GUIContent.none, GUILayout.Width(100));
                GUI.backgroundColor = tint;
            }
            return;
        }
        foreach (var child in CardAuthoringUtility.Children(normal))
            DrawQuick(child, upgraded.FindPropertyRelative(child.name), label + " / " + Label(child.name));
    }

    private void DrawNode(SerializedProperty property, Type declaredType, string label = null, int depth = 0)
    {
        if (depth > 16) { EditorGUILayout.HelpBox("중첩 편집 한도(16)를 초과했습니다. 구성을 단순화하세요.", MessageType.Warning); return; }
        label ??= Label(property.name);
        if (property.isArray && property.propertyType != SerializedPropertyType.String && declaredType != null && declaredType.IsGenericType)
        {
            var elementType = declaredType.GetGenericArguments()[0];
            Title(label);
            for (int i = 0; i < property.arraySize; i++)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"[{i}]", EditorStyles.boldLabel, GUILayout.Width(40));
                        GUILayout.FlexibleSpace();
                        using (new EditorGUI.DisabledScope(i == 0))
                            if (GUILayout.Button("↑", GUILayout.Width(28))) { property.MoveArrayElement(i, i - 1); CommitAndExit(); }
                        using (new EditorGUI.DisabledScope(i == property.arraySize - 1))
                            if (GUILayout.Button("↓", GUILayout.Width(28))) { property.MoveArrayElement(i, i + 1); CommitAndExit(); }
                        if (GUILayout.Button("삭제", GUILayout.Width(44))) { property.DeleteArrayElementAtIndex(i); CommitAndExit(); }
                    }
                    DrawNode(property.GetArrayElementAtIndex(i), elementType, "효과 / 조건", depth + 1);
                }
            }
            var rect = GUILayoutUtility.GetRect(new GUIContent("+ 효과 / 조건 추가"), GUI.skin.button);
            if (GUI.Button(rect, "+ 효과 / 조건 추가")) ShowPicker(rect, property, elementType, true);
            EditorGUILayout.LabelField("실행 순서는 위 → 아래. 순서를 바꾸면 설명 토큰의 인덱스도 확인하세요.", EditorStyles.miniLabel);
            return;
        }
        if (property.propertyType == SerializedPropertyType.ManagedReference)
        {
            Type actual = property.managedReferenceValue?.GetType();
            Rect rect = EditorGUILayout.GetControlRect();
            if (EditorGUI.DropdownButton(rect, new GUIContent(actual == null ? label + " 선택…" : CardAuthoringUtility.TypeLabel(actual)), FocusType.Keyboard))
                ShowPicker(rect, property, declaredType, false);
            if (actual == null) return;
            foreach (var child in CardAuthoringUtility.Children(property))
                DrawNode(child, FieldType(actual, child.name), null, depth + 1);
            return;
        }
        if (declaredType == typeof(EffectTargetSelector))
        {
            var mode = property.FindPropertyRelative("mode");
            mode.enumValueIndex = EditorGUILayout.Popup(label, mode.enumValueIndex, new[] { "자신", "선택한 대상", "모든 적", "무작위 적" });
            return;
        }
        if (property.propertyType == SerializedPropertyType.Generic && property.hasVisibleChildren)
        {
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                foreach (var child in CardAuthoringUtility.Children(property)) DrawNode(child, FieldType(declaredType, child.name), null, depth + 1);
                EditorGUI.indentLevel--;
            }
            return;
        }
        EditorGUILayout.PropertyField(property, new GUIContent(label, property.tooltip), true);
    }

    private void ShowPicker(Rect rect, SerializedProperty property, Type baseType, bool append)
    {
        if (baseType == null) return;
        serializedCard.ApplyModifiedProperties();
        string path = property.propertyPath;
        var owner = card;
        dropdown = new TypeDropdown(dropdownState, baseType, chosen =>
        {
            if (owner == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            using var so = new SerializedObject(owner);
            var target = so.FindProperty(path);
            if (target == null) return;
            if (append) { target.arraySize++; target = target.GetArrayElementAtIndex(target.arraySize - 1); }
            else if (target.managedReferenceValue != null && target.managedReferenceValue.GetType() != chosen &&
                     !EditorUtility.DisplayDialog("효과 종류 변경", "기존 효과의 설정을 새 종류의 기본 값으로 교체합니다.", "교체", "취소")) return;
            else if (target.managedReferenceValue?.GetType() == chosen) return;
            target.managedReferenceValue = Activator.CreateInstance(chosen);
            target.isExpanded = true;
            so.ApplyModifiedProperties();
            OnUndo();
        });
        dropdown.Show(rect);
    }

    private void DrawDescription()
    {
        Title("카드 설명 — 일반·강화 공용, 직접 입력");
        var description = serializedCard.FindProperty("cardDescription");
        description.stringValue = EditorGUILayout.TextArea(description.stringValue, GUILayout.MinHeight(90));
        EditorGUILayout.HelpBox("예: 피해 {0.amount}을 {0.hitCount}회 줍니다.\n숫자는 각 상태의 효과 값으로 치환됩니다. 효과가 바뀌어도 설명은 자동 작성되지 않습니다.", MessageType.None);
        if (serializedCard.ApplyModifiedProperties()) previewDirty = true;
        if (previewDirty)
        {
            normalPreview = CardAuthoringUtility.Preview(card, false);
            upgradePreview = CardAuthoringUtility.Preview(card, true);
            var sb = new StringBuilder();
            AppendTokens(serializedCard.FindProperty("normalState.effects"), "기본", sb);
            AppendTokens(serializedCard.FindProperty("upgradedState.effects"), "강화", sb);
            tokenHint = sb.ToString();
            previewDirty = false;
        }
        Title("기본 설명 미리보기");
        EditorGUILayout.HelpBox(normalPreview ?? "", MessageType.None);
        Title("강화 설명 미리보기");
        EditorGUILayout.HelpBox(upgradePreview ?? "", MessageType.None);
        if (System.Text.RegularExpressions.Regex.IsMatch((normalPreview ?? "") + upgradePreview, @"\{\d+\.[\w.]+\}"))
            EditorGUILayout.HelpBox("치환되지 않은 토큰이 있습니다. 효과 인덱스·필드명을 확인하세요.", MessageType.Warning);
        Title("사용 가능한 토큰 (선택해서 복사)");
        float tokenHeight = Mathf.Max(140, EditorStyles.textArea.CalcHeight(new GUIContent(tokenHint), position.width - 50));
        EditorGUILayout.SelectableLabel(tokenHint, EditorStyles.textArea, GUILayout.Height(tokenHeight));
        EditorGUILayout.HelpBox("미리보기는 현재 입력한 기본 수치입니다. 전투 중 보정은 포함하지 않습니다. SO 저장 후 상단에서 보상 풀에 등록할 수 있습니다. 시작 덱·동료 해금 연결은 별도로 설정하세요.", MessageType.Info);
    }

    private static void AppendTokens(SerializedProperty effects, string title, StringBuilder sb)
    {
        sb.AppendLine(title);
        for (int i = 0; i < effects.arraySize; i++)
        {
            var effect = effects.GetArrayElementAtIndex(i);
            if (effect.managedReferenceValue == null) { sb.AppendLine($"[{i}] 빈 효과 — 종류를 선택하세요."); continue; }
            AppendLeafTokens(effect, i.ToString(), sb, 0);
        }
    }

    private static void AppendLeafTokens(SerializedProperty parent, string prefix, StringBuilder sb, int depth)
    {
        if (depth > 16) return;
        foreach (var child in CardAuthoringUtility.Children(parent))
        {
            if (child.isArray && child.propertyType != SerializedPropertyType.String) continue;
            string path = prefix + "." + child.name;
            if (child.propertyType == SerializedPropertyType.Generic || child.propertyType == SerializedPropertyType.ManagedReference)
                AppendLeafTokens(child, path, sb, depth + 1);
            else if (child.propertyType == SerializedPropertyType.Integer || child.propertyType == SerializedPropertyType.Float)
                sb.Append('{').Append(path).AppendLine("}");
        }
    }

    private void CommitAndExit() { serializedCard.ApplyModifiedProperties(); previewDirty = true; GUIUtility.ExitGUI(); }
    private void Field(string path, string label) => EditorGUILayout.PropertyField(serializedCard.FindProperty(path), new GUIContent(label), true);
    private static void Title(string label) { EditorGUILayout.Space(8); EditorGUILayout.LabelField(label, EditorStyles.boldLabel); }
    private static void Toggle(SerializedProperty state, string path, string label)
    {
        var property = state.FindPropertyRelative(path);
        property.boolValue = GUILayout.Toggle(property.boolValue, label, "Button");
    }
    private static Type FieldType(Type owner, string field) => owner?.GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.FieldType;
    private static string Label(string field) => field switch
    {
        "amount" => "수치 (amount)", "count" => "장수 / 횟수 (count)", "hitCount" => "타격 횟수",
        "targets" => "효과 대상", "target" => "대상", "scaling" => "동적 수치 보정 (고급)",
        "piercing" => "방어도 관통", "environmentalDamage" => "환경 피해", "hitInterval" => "타격 간격 (초)",
        "condition" => "발동 조건", "effectsWhenMet" => "조건 충족 시 효과", "effects" => "효과 목록",
        "weapon" => "변경할 무기", "status" => "상태 에셋", "source" => "수치 원천",
        "multiplier" => "배수", "flatBonus" => "고정 보너스", _ => ObjectNames.NicifyVariableName(field)
    };

    private sealed class TypeDropdown : AdvancedDropdown
    {
        private readonly Type baseType;
        private readonly Action<Type> selected;
        internal TypeDropdown(AdvancedDropdownState state, Type baseType, Action<Type> selected) : base(state)
        {
            this.baseType = baseType;
            this.selected = selected;
            minimumSize = new Vector2(420, 350);
        }
        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("효과 / 조건 검색");
            foreach (var type in CardAuthoringUtility.TypesFor(baseType))
                root.AddChild(new TypeItem(type));
            return root;
        }
        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            // AddChild recomputes IDs in Unity 6. Keep the payload on the item itself;
            // search results also reuse these items, so selection needs no ID lookup.
            if (item is TypeItem typeItem) selected(typeItem.Type);
        }

        private sealed class TypeItem : AdvancedDropdownItem
        {
            internal Type Type { get; }

            internal TypeItem(Type type) : base(CardAuthoringUtility.TypeLabel(type))
            {
                Type = type;
            }
        }
    }
}
