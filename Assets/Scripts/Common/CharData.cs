using UnityEngine;

public class CharData : ScriptableObject
{
    [Header("캐릭터 기본 정보")]
    public string charID;
    public string charName;
    public string charDescription;

    [Header("캐릭터 이미지(임시 상태. 나중에 Addressable.Load로 불러오도록 변경)")]
    public Sprite charImage;   // 초상화. 성소 선택 버튼, 휴식 화면 등 아이콘/UI용
    public Sprite charIcon;
    public Sprite standingSprite; // 전투 화면 스탠딩(전신) 스프라이트. charImage와 별개 아트

    [Header("캐릭터 애니메이션 (스프라이트 시트 기반)")]
    [Tooltip("기본 대기(Idle) 루프 애니메이션 스프라이트 프레임 목록")]
    public Sprite[] idleFrames;
    [Tooltip("대기 애니메이션 초당 프레임 수 (기본 24 FPS)")]
    public float idleFrameRate = 24f;
    [Tooltip("기본 공격 애니메이션 스프라이트 프레임 목록 (비워둘 시 attackSequence 사용)")]
    public Sprite[] attackFrames;
    [Tooltip("공격 애니메이션 초당 프레임 수 (기본 24 FPS)")]
    public float attackFrameRate = 24f;

    [Header("레거시 애니메이션 클립")]
    public AnimationClip idleAnim;
    public AnimationClip attackAnim;
    public AnimationClip hitAnim;
    public AnimationClip deathAnim;

    [Header("공격 연출 (키프레임 홀드 방식 — PoseSequencePlayer 참고)")]
    public AttackKeyframe[] attackSequence;

    [Header("캐릭터 사운드")]
    public AudioClip charSoundEffect;

    [Header("패시브")]
    public IPassiveLogic passive;
}
