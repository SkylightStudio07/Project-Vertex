using UnityEngine;

// 설명문(string) 필드에 붙이면 인스펙터에서 사용 가능한 {인덱스.필드명} 토큰 목록과
// 치환 미리보기를 함께 표시한다. effectsPath는 같은 SO 안의 이펙트 리스트 프로퍼티 경로.
// 사용 예:
//   [SerializeField, EffectTokenHint("normalState.effects")] public string cardDescription;
public class EffectTokenHintAttribute : PropertyAttribute
{
    public readonly string EffectsPath;
    // 비어 있지 않으면 "이 필드로 복사" 버튼을 띄운다 (강화 설명 ← 기본 설명)
    public readonly string CopyFromPath;
    public EffectTokenHintAttribute(string effectsPath, string copyFromPath = null)
    {
        EffectsPath = effectsPath;
        CopyFromPath = copyFromPath;
    }
}
