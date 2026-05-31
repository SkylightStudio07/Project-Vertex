using UnityEngine;

public class CharData : ScriptableObject
{
    [Header("캐릭터 기본 정보")]
    public string charID;
    public string charName;
    public string charDescription;

    [Header("캐릭터 이미지(임시 용임. 나중엔 Addressable.Load로 불러오도록 설정)")]
    public Sprite charImage;
    public Sprite charIcon;

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
