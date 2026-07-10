using UnityEngine;

// [SerializeReference] 필드에 함께 붙이면 인스펙터에서 서브클래스 선택 드롭다운이 표시된다.
// 사용 예:
//   [SerializeReference, SubclassPicker] private List<CardEffect> cardEffects = new();
public class SubclassPickerAttribute : PropertyAttribute { }
