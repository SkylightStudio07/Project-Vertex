using UnityEngine;

public class CharData : ScriptableObject
{
    [Header("캐릭터 기본 정보")]
    public string CharID;
    public string CharName;
    public string CharDescription;

    [Header("캐릭터 이미지(임시 용임. 나중엔 Addressable.Load로 불러오도록 설정)")]
    public Sprite CharImage;
    public Sprite CharIcon;

    [Header("캐릭터 애니메이션")]
    public AnimationClip IdleAnim;
    public AnimationClip AttackAnim;
    public AnimationClip HitAnim;
    public AnimationClip DeathAnim;

    [Header("캐릭터 사운드")]
    public AudioClip CharSoundEffect;

    [Header("패시브")]
    public IPassiveLogic Passive;
}
