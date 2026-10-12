using UnityEngine;

// 뼈대가 없는 한 장짜리 캐릭터 UI 이미지에 대기 호흡을 준다.
// 피벗(발 쪽)을 기준으로 세로로 아주 살짝 늘었다 줄어, 머리와 어깨가 오르내리는 것처럼 보인다.
// 리깅(RiggingV2Portrait)을 쓸 수 없는 스탠딩 일러스트용. 위치는 건드리지 않아 등장 연출 등과 충돌하지 않는다.
[RequireComponent(typeof(RectTransform))]
public class UIIdleBreath : MonoBehaviour
{
    [SerializeField, Range(0f, 0.03f)] private float stretch = 0.01f; // 세로 늘어남 비율
    [SerializeField, Min(0.1f)] private float period = 3.6f;          // 한 번 숨쉬는 데 걸리는 시간(초)

    private RectTransform _rect;
    private Vector3 _baseScale;

    private void Awake() => _rect = (RectTransform)transform;
    private void OnEnable() => _baseScale = _rect.localScale;
    private void OnDisable() => _rect.localScale = _baseScale;

    private void LateUpdate()
    {
        // 들숨은 천천히, 날숨은 조금 빠르게
        float phase = Time.time / period * Mathf.PI * 2f;
        float breath = (Mathf.Sin(phase) + 0.25f * Mathf.Sin(phase * 2f + 0.6f) + 1.25f) / 2.5f;
        _rect.localScale = new Vector3(_baseScale.x * (1f - stretch * 0.3f * breath), _baseScale.y * (1f + stretch * breath), _baseScale.z);
    }
}
