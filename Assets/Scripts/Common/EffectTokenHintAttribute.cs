using UnityEngine;

// 설명문(string) 필드에 붙이면 인스펙터에서 사용 가능한 {인덱스.필드명} 토큰 목록과
// 치환 미리보기를 함께 표시한다. effectsPath는 같은 SO 안의 이펙트 리스트 프로퍼티 경로.
// 사용 예:
//   [SerializeField, EffectTokenHint("normalState.effects")] public string cardDescription;
public class EffectTokenHintAttribute : PropertyAttribute
{
    public readonly string EffectsPath;
    public EffectTokenHintAttribute(string effectsPath) => EffectsPath = effectsPath;
}
