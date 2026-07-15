using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

// [EffectTokenHint] 필드용 드로어.
// 설명문 입력 필드 아래에 이펙트 리스트 기반의 사용 가능 토큰 목록과
// GetFullDescription() 치환 미리보기를 헬프박스로 표시한다.
// 토큰 문법을 확인하러 스크립트를 열어볼 필요 없이 인스펙터에서 바로 보고 작성하는 용도.
[CustomPropertyDrawer(typeof(EffectTokenHintAttribute))]
public class EffectTokenHintDrawer : PropertyDrawer
{
    private const float Spacing = 2f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float fieldHeight = EditorGUI.GetPropertyHeight(property, label, true);
        string hint = BuildHint(property);
        if (string.IsNullOrEmpty(hint)) return fieldHeight;

        // 헬프박스 높이는 내용 줄 수에 따라 달라지므로 실제 스타일로 계산한다.
        // (인스펙터 실폭은 GetPropertyHeight 시점에 모르기 때문에 currentViewWidth로 근사)
        float hintHeight = EditorStyles.helpBox.CalcHeight(
            new GUIContent(hint), EditorGUIUtility.currentViewWidth - 60f);
        return fieldHeight + Spacing + hintHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float fieldHeight = EditorGUI.GetPropertyHeight(property, label, true);
        var fieldRect = new Rect(position.x, position.y, position.width, fieldHeight);
        EditorGUI.PropertyField(fieldRect, property, label, true);

        string hint = BuildHint(property);
        if (string.IsNullOrEmpty(hint)) return;

        var boxRect = new Rect(position.x, position.y + fieldHeight + Spacing,
                               position.width, position.height - fieldHeight - Spacing);
        EditorGUI.HelpBox(boxRect, hint, MessageType.None);
    }

    private string BuildHint(SerializedProperty property)
    {
        if (attribute is not EffectTokenHintAttribute hintAttr) return null;

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
            FieldInfo[] fields = inst.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int f = 0; f < fields.Length; f++)
            {
                if (f > 0) sb.Append("  ");
                sb.Append($"{{{i}.{fields[f].Name}}}");
            }
        }

        // 미리보기는 ActiveEffects 기준(GetFullDescription)이라 isUpgraded 체크 시 강화 수치로 보인다.
        if (property.serializedObject.targetObject is CardData card)
        {
            string preview = card.GetFullDescription();
            if (!string.IsNullOrEmpty(preview))
                sb.Append($"\n\n미리보기: {preview}");
        }

        return sb.ToString();
    }
}
