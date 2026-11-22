using System.Collections;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

// [EffectTokenHint] 필드용 드로어.
// 설명문 입력 필드 아래에 이펙트 리스트 기반의 사용 가능 토큰 목록과
// GetFullDescription() 치환 미리보기를 헬프박스로 표시한다.
// 토큰 문법을 확인하러 스크립트를 열어볼 필요 없이 인스펙터에서 바로 보고 작성하는 용도.
// CopyFromPath가 있으면(강화 설명) 그 필드 내용을 그대로 가져오는 버튼도 띄운다.
[CustomPropertyDrawer(typeof(EffectTokenHintAttribute))]
public class EffectTokenHintDrawer : PropertyDrawer
{
    private const float Spacing = 2f;

    private EffectTokenHintAttribute HintAttr => attribute as EffectTokenHintAttribute;
    private bool HasCopyButton => !string.IsNullOrEmpty(HintAttr?.CopyFromPath);
    private static float ButtonHeight => EditorGUIUtility.singleLineHeight;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUI.GetPropertyHeight(property, label, true);
        if (HasCopyButton) height += Spacing + ButtonHeight;
        string hint = BuildHint(property);
        if (string.IsNullOrEmpty(hint)) return height;

        // 헬프박스 높이는 내용 줄 수에 따라 달라지므로 실제 스타일로 계산한다.
        // (인스펙터 실폭은 GetPropertyHeight 시점에 모르기 때문에 currentViewWidth로 근사)
        float hintHeight = EditorStyles.helpBox.CalcHeight(
            new GUIContent(hint), EditorGUIUtility.currentViewWidth - 60f);
        return height + Spacing + hintHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float y = position.y;
        float fieldHeight = EditorGUI.GetPropertyHeight(property, label, true);
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, fieldHeight), property, label, true);
        y += fieldHeight;

        if (HasCopyButton)
        {
            y += Spacing;
            var source = property.serializedObject.FindProperty(HintAttr.CopyFromPath);
            var buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, y,
                                      position.width - EditorGUIUtility.labelWidth, ButtonHeight);
            using (new EditorGUI.DisabledScope(source == null || source.propertyType != SerializedPropertyType.String))
                if (GUI.Button(buttonRect, "기본 설명 그대로 가져오기"))
                    property.stringValue = source.stringValue;
            y += ButtonHeight;
        }

        string hint = BuildHint(property);
        if (string.IsNullOrEmpty(hint)) return;

        var boxRect = new Rect(position.x, y + Spacing, position.width, position.yMax - y - Spacing);
        EditorGUI.HelpBox(boxRect, hint, MessageType.None);
    }

    private string BuildHint(SerializedProperty property)
    {
        if (HintAttr is not { } hintAttr) return null;

        var effectsProp = property.serializedObject.FindProperty(hintAttr.EffectsPath);
        if (effectsProp == null || !effectsProp.isArray)
            return $"[EffectTokenHint] '{hintAttr.EffectsPath}' 경로에서 이펙트 리스트를 찾지 못함";

        var sb = new StringBuilder("사용 가능 토큰:");
        if (effectsProp.arraySize == 0)
            sb.Append(" (이펙트 없음 — 먼저 이펙트를 추가할 것)");

        for (int i = 0; i < effectsProp.arraySize; i++)
        {
            object inst = effectsProp.GetArrayElementAtIndex(i).managedReferenceValue;
            sb.Append('\n');
            if (inst == null)
            {
                sb.Append($"[{i}] (빈 슬롯)");
                continue;
            }

            sb.Append($"[{i}] {inst.GetType().Name}:  ");
            AppendTokens(sb, inst, i.ToString(), 0);
        }

        if (property.serializedObject.targetObject is CardData card)
        {
            // 강화 설명 필드는 강화 상태 기준, 기본 설명 필드는 현재 isUpgraded 기준(GetFullDescription)
            bool upgradedField = HasCopyButton;
            string preview = upgradedField ? card.GetDescription(true) : card.GetFullDescription();
            if (upgradedField && !card.HasUpgradedDescription)
                sb.Append("\n\n(비어 있음 — 강화 상태도 기본 설명을 사용)");
            if (!string.IsNullOrEmpty(preview))
                sb.Append($"\n\n미리보기: {preview}");
        }

        return sb.ToString();
    }

    private const int MaxNestDepth = 3;

    // 필드 토큰을 나열하고, 리스트 필드(effectsWhenMet·conditionalBonuses·onKillEffects 등)는
    // 원소마다 한 줄씩 내려가 {i.리스트.j.필드} 토큰을 보여준다.
    private static void AppendTokens(StringBuilder sb, object inst, string prefix, int depth)
    {
        FieldInfo[] fields = inst.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        var nested = new StringBuilder();
        for (int f = 0; f < fields.Length; f++)
        {
            if (f > 0) sb.Append("  ");
            string path = $"{prefix}.{CardData.FieldToTokenAlias(fields[f].Name)}";
            sb.Append($"{{{path}}}");

            if (depth >= MaxNestDepth || fields[f].GetValue(inst) is not IList list) continue;
            for (int j = 0; j < list.Count; j++)
            {
                object element = list[j];
                if (element == null || element is Object || element is string || element.GetType().IsPrimitive || element.GetType().IsEnum) continue;
                nested.Append('\n').Append(new string(' ', (depth + 1) * 4));
                nested.Append($"└ {path}.{j} {element.GetType().Name}:  ");
                AppendTokens(nested, element, $"{path}.{j}", depth + 1);
            }
        }
        sb.Append(nested);
    }
}
