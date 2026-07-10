using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// [SerializeReference, SubclassPicker] 필드용 드로어.
// 첫 줄 오른쪽에 타입 드롭다운을 띄우고, 타입을 고르면 해당 서브클래스 인스턴스를 생성해 할당한다.
// 필드 내용(자식 프로퍼티)은 Unity 기본 폴드아웃 그대로 그린다.
[CustomPropertyDrawer(typeof(SubclassPickerAttribute))]
public class SubclassPickerDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUI.GetPropertyHeight(property, label, true);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // SerializeReference가 아닌 필드에 실수로 붙였으면 기본 그리기로 폴백
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        var dropdownRect = new Rect(
            position.x + EditorGUIUtility.labelWidth + 2f,
            position.y,
            position.width - EditorGUIUtility.labelWidth - 2f,
            EditorGUIUtility.singleLineHeight);

        Type currentType = property.managedReferenceValue?.GetType();
        string display = currentType != null
            ? ObjectNames.NicifyVariableName(currentType.Name)
            : "(없음)";

        if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(display), FocusType.Keyboard))
            ShowTypeMenu(property);

        // 폴드아웃 + 자식 필드 (amount, hitCount 등)
        EditorGUI.PropertyField(position, property, label, true);
    }

    private void ShowTypeMenu(SerializedProperty property)
    {
        // GenericMenu 콜백은 OnGUI 이후에 실행되므로 property를 직접 캡처하면 무효화될 수 있다.
        // serializedObject + propertyPath로 다시 찾아서 할당한다.
        var serializedObject = property.serializedObject;
        string path = property.propertyPath;

        var menu = new GenericMenu();

        menu.AddItem(new GUIContent("(없음)"), property.managedReferenceValue == null,
            () => Assign(serializedObject, path, null));

        foreach (var type in GetAssignableTypes(GetElementType()))
        {
            Type captured = type;
            bool selected = property.managedReferenceValue?.GetType() == captured;
            menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(captured.Name)), selected,
                () => Assign(serializedObject, path, Activator.CreateInstance(captured)));
        }

        menu.ShowAsContext();
    }

    private static void Assign(SerializedObject serializedObject, string path, object value)
    {
        var prop = serializedObject.FindProperty(path);
        if (prop == null) return;
        prop.managedReferenceValue = value;
        serializedObject.ApplyModifiedProperties();
    }

    // List<T>/T[] 필드면 원소 타입, 아니면 필드 타입 그대로
    private Type GetElementType()
    {
        Type fieldType = fieldInfo.FieldType;
        if (fieldType.IsArray) return fieldType.GetElementType();
        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            return fieldType.GetGenericArguments()[0];
        return fieldType;
    }

    private static IEnumerable<Type> GetAssignableTypes(Type baseType)
        => TypeCache.GetTypesDerivedFrom(baseType)
            .Where(t => !t.IsAbstract
                        && !t.IsGenericType
                        && !typeof(UnityEngine.Object).IsAssignableFrom(t) // SO/MB는 인라인 직렬화 불가
                        && t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => t.Name);
}
