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

    [Header("캐릭터 애니메이션")]
    public AnimationClip idleAnim;
    public AnimationClip attackAnim;
    public AnimationClip hitAnim;
    public AnimationClip deathAnim;

    [Header("캐릭터 사운드")]
    public AudioClip charSoundEffect;

    [Header("패시브")]
    public IPassiveLogic passive;
}
