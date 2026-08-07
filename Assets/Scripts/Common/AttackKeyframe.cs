using UnityEngine;

// 공격 연출 키프레임 하나. 배열로 이어 붙여서 순서대로 재생 — 개수 제한 없음.
// 림버스 컴퍼니식 "몇 개 포즈를 딱딱 홀드"하는 제한 애니메이션 연출을 목표로 함.
[System.Serializable]
public class AttackKeyframe
{
    public Sprite pose;            // 비워두면 직전 프레임 스프라이트를 그대로 유지
    public Vector2 positionOffset; // 대기 포즈 기준 오프셋
    public Vector2 scale = Vector2.one;
    public float rotation;
    public float moveDuration = 0f;   // 이 포즈로 넘어가는 데 걸리는 시간(0 = 즉시 스냅)
    public float holdDuration = 0.1f; // 이 포즈에 도달한 뒤 유지하는 시간
}
