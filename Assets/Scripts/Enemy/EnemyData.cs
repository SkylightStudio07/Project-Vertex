using System.Collections.Generic;
using UnityEngine;

public enum EnemyActivityPatternType
{
    Sequential, // 순차 실행
    Random      // 랜덤 실행
}

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Asset/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int health;
    public Sprite enemyImage;

    [Header("스프라이트 연출")]
    [Tooltip("스프라이트 크기 배율 (기본값 1.0f). 적마다 0.9, 1.2 등으로 조절 가능합니다.")]
    public float spriteScale = 1.0f;

    [Tooltip("인텐트 UI 추가 Y 오프셋 (기본 0, 필요시 머리 위 위치 미세 조정)")]
    public float intentOffsetY = 0f;

    [Header("대기(Idle) 애니메이션")]
    [Tooltip("대기 스프라이트 시트 프레임 (비어있으면 enemyImage 단독 표시)")]
    public Sprite[] idleFrames;

    [Tooltip("대기 애니메이션 초당 프레임 수 (기본 12 FPS)")]
    public float idleFrameRate = 12f;

    [Header("행동 패턴")]
    [Tooltip("행동 패턴 타입(랜덤/순차)")]
    public EnemyActivityPatternType activityPatternType;

    [Tooltip("전투 시작 시 순서대로 1회씩만 실행하는 오프닝 행동. 모두 소진되면 activityPatterns로 넘어간다.")]
    public List<EnemyAction> openingActions = new();

    [Tooltip("오프닝 이후 반복하는 행동 풀. Sequential이면 순서 순환, Random이면 매 턴 랜덤 1개.")]
    public List<EnemyAction> activityPatterns = new();
}
