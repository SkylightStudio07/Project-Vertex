using UnityEngine;

// 의도 하나의 설명 데이터. 새 의도의 문구는 표시 코드 대신 에셋에서 작성한다.
[CreateAssetMenu(fileName = "Intent", menuName = "Game Asset/Intent Definition")]
public class IntentDefinition : ScriptableObject
{
    public IntentType type;
    public Sprite icon;
    public string title;
    [TextArea] public string description;
    [Tooltip("피해 수치가 있을 때 설명 뒤에 붙일 문구. {damage}를 실제 수치로 바꿉니다.")]
    [TextArea] public string damageDescription;

    public string GetDescription(int? damage)
    {
        return damage.HasValue && !string.IsNullOrEmpty(damageDescription)
            ? description + damageDescription.Replace("{damage}", damage.Value.ToString())
            : description;
    }
}
