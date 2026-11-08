using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponVisualSet
{
    [Tooltip("장착 대상 무기 ScriptableObject")]
    public WeaponData weapon;

    [Tooltip("해당 무기 장착 시 표시될 스탠딩 스프라이트")]
    public Sprite standingSprite;

    [Tooltip("해당 무기 장착 시 재생될 대기(Idle) 스프라이트 프레임 (비워둘 시 기본 idleFrames 사용 또는 standingSprite 단독 표시)")]
    public Sprite[] idleFrames;

    [Tooltip("대기 애니메이션 초당 프레임 수 (기본 24 FPS)")]
    public float idleFrameRate = 24f;

    [Tooltip("해당 무기 공격 시 재생될 스프라이트 시트 프레임 (비워둘 시 기본 attackFrames 또는 attackSequence 사용)")]
    public Sprite[] attackFrames;

    [Tooltip("공격 애니메이션 초당 프레임 수 (기본 24 FPS)")]
    public float attackFrameRate = 24f;

    [Tooltip("해당 무기 사용 시 재생될 공격 연출 시퀀스 (비워둘 시 기본 attackSequence 사용)")]
    public AttackKeyframe[] attackSequence;

    [Tooltip("채우면 attackFrames 대신 이 중 하나를 무작위로 재생한다 (같은 모션이 연달아 나오지 않게)")]
    public AttackClip[] attackClips;
}

// 공격 모션 한 벌. 시트 틀이 대기 시트보다 넓으면 scale로 재생 중에만 키워 캐릭터 크기를 맞춘다
[Serializable]
public class AttackClip
{
    public Sprite[] frames;
    public float frameRate = 24f;
    [Tooltip("재생 중 그림에 곱하는 배율 (그림 가운데 기준). 대기 시트와 같은 틀이면 1")]
    public float scale = 1f;
    [Tooltip("프레임별 표시 길이 배수 (1 = 1/frameRate초). 0이면 그 프레임은 건너뛴다. 비어 있거나 모자라면 나머지는 1")]
    public float[] holds;
    [Tooltip("이 번호의 프레임으로 넘어갈 때 앞 프레임 위에 서서히 겹쳐 나타난다. 포즈가 한 장 만에 크게 바뀌는 곳에 쓴다")]
    public int[] blendInto;
}

[CreateAssetMenu(fileName = "newPlayerCharData", menuName = "Game Asset/Player Character")]
public class PlayerCharData : CharData
{
    [Header("플레이어 캐릭터 기본 능력치")]
    public float CharMaxHp;
    public int StartingEnegy;
    public int StartingGold;
    public int DrawCountPerTurn;

    [Header("플레이어 기본 카드 풀")]
    public List<CardData> DefaultCardPool = new();

    [Header("플레이어 카드 전투 보상 카드 풀")]
    public List<CardData> BattleRewardCardPool = new();

    [Header("무기별 외형 매핑 (스탠딩 & 공격 모션)")]
    [Tooltip("장착 무기별 전용 스탠딩 스프라이트 및 공격 연출 목록입니다.")]
    public List<WeaponVisualSet> weaponVisuals = new();

    /// <summary>
    /// 현재 무기에 해당하는 대기(Idle) 프레임 배열과 FPS를 반환합니다.
    /// 전용 무기에 idleFrames가 없고 standingSprite만 등록되어 있다면, 정적 스프라이트 유지를 위해 null을 반환합니다.
    /// </summary>
    public Sprite[] GetIdleFrames(WeaponData weapon, out float frameRate)
    {
        frameRate = idleFrameRate > 0f ? idleFrameRate : 24f;

        if (weapon != null && weaponVisuals != null)
        {
            var match = weaponVisuals.Find(v => v != null && v.weapon == weapon);
            if (match != null)
            {
                if (match.idleFrames != null && match.idleFrames.Length > 0)
                {
                    frameRate = match.idleFrameRate > 0f ? match.idleFrameRate : 24f;
                    return match.idleFrames;
                }

                // 무기 전용 스탠딩 스프라이트가 별도로 등록되어 있으나 idleFrames는 없는 경우:
                // 기본 idleFrames를 덮어쓰지 않고 해당 무기의 정적 스프라이트를 유지하도록 null 반환
                if (match.standingSprite != null && match.standingSprite != standingSprite)
                {
                    return null;
                }
            }
        }

        return (idleFrames != null && idleFrames.Length > 0) ? idleFrames : null;
    }

    /// <summary>
    /// 현재 무기에 해당하는 스탠딩 스프라이트를 반환합니다. 등록되지 않은 무기이거나 null일 경우 기본 standingSprite를 반환합니다.
    /// </summary>
    public Sprite GetStandingSprite(WeaponData weapon)
    {
        if (weapon != null && weaponVisuals != null)
        {
            var match = weaponVisuals.Find(v => v != null && v.weapon == weapon);
            if (match != null && match.standingSprite != null)
                return match.standingSprite;
        }
        return standingSprite;
    }

    /// <summary>
    /// 현재 무기에 해당하는 공격 스프라이트 프레임 배열과 FPS를 반환합니다.
    /// 전용 무기에 attackFrames가 등록되어 있다면 이를 반환하고,
    /// 없으면 기본 attackFrames를 반환합니다.
    /// </summary>
    public Sprite[] GetAttackFrames(WeaponData weapon, out float frameRate) => GetAttackFrames(weapon, out frameRate, out _);

    [NonSerialized] private int _lastClip = -1;

    // scale: 재생 중 그림에 곱할 배율 (attackClips의 틀 보정, 그 외 1)
    public Sprite[] GetAttackFrames(WeaponData weapon, out float frameRate, out float scale) => GetAttackFrames(weapon, out frameRate, out scale, out _);

    // clip: attackClips에서 고른 모션 (프레임별 길이 · 겹침 정보), 그 외 경로면 null
    public Sprite[] GetAttackFrames(WeaponData weapon, out float frameRate, out float scale, out AttackClip clip)
    {
        frameRate = attackFrameRate > 0f ? attackFrameRate : 24f;
        scale = 1f;
        clip = null;

        if (weapon != null && weaponVisuals != null)
        {
            var match = weaponVisuals.Find(v => v != null && v.weapon == weapon);
            clip = match != null ? PickClip(match.attackClips) : null;
            if (clip != null)
            {
                frameRate = clip.frameRate > 0f ? clip.frameRate : 24f;
                scale = clip.scale > 0f ? clip.scale : 1f;
                return clip.frames;
            }
            if (match != null && match.attackFrames != null && match.attackFrames.Length > 0)
            {
                frameRate = match.attackFrameRate > 0f ? match.attackFrameRate : 24f;
                return match.attackFrames;
            }
        }

        return (attackFrames != null && attackFrames.Length > 0) ? attackFrames : null;
    }

    /// <summary>
    /// 현재 무기에 해당하는 공격 연출 시퀀스를 반환합니다. 등록되지 않았거나 비어있을 경우 기본 attackSequence를 반환합니다.
    /// </summary>
    public AttackKeyframe[] GetAttackSequence(WeaponData weapon)
    {
        if (weapon != null && weaponVisuals != null)
        {
            var match = weaponVisuals.Find(v => v != null && v.weapon == weapon);
            if (match != null && match.attackSequence != null && match.attackSequence.Length > 0)
                return match.attackSequence;
        }
        return attackSequence;
    }

    // 프레임이 있는 모션 중 무작위 하나. 둘 이상이면 바로 전 모션은 빼고 고른다
    private AttackClip PickClip(AttackClip[] clips)
    {
        if (clips == null) return null;
        var valid = new List<int>();
        for (int i = 0; i < clips.Length; i++)
            if (clips[i] != null && clips[i].frames != null && clips[i].frames.Length > 0) valid.Add(i);
        if (valid.Count == 0) return null;
        if (valid.Count > 1) valid.Remove(_lastClip);
        _lastClip = valid[UnityEngine.Random.Range(0, valid.Count)];
        return clips[_lastClip];
    }
}
