using System;
using System.Collections.Generic;
using UnityEngine;

// 적 속성. 적 하나가 여러 개를 가질 수 있다(플래그). 카드는 특정 속성에 "특효"를 가진다.
// 기획: 노션 VERTEX 기획서 v2 > 7. 속성 & 특효
//   - 상성표(약점·저항 배율)는 없다. 특효가 있는 카드만 추가 효과를 받는다.
//   - 특효는 대상이 나열된 속성을 여러 개 가져도 한 번만 적용된다.
// 주의: 값은 에셋에 비트로 저장되므로 순서를 바꾸거나 중간에 끼워 넣지 말고 끝에 추가할 것.
[Flags]
public enum EnemyTrait
{
    None = 0,
    [InspectorName("인간")] Human = 1 << 0,
    [InspectorName("기계")] Machine = 1 << 1,
    [InspectorName("경장갑")] LightArmor = 1 << 2,
    [InspectorName("중장갑")] HeavyArmor = 1 << 3,
    [InspectorName("신비")] Mystic = 1 << 4,
    [InspectorName("차원")] Dimension = 1 << 5,
    [InspectorName("파도")] Wave = 1 << 6,
    [InspectorName("사이킥")] Psychic = 1 << 7,
    [InspectorName("호문클루스")] Homunculus = 1 << 8,
    [InspectorName("별")] Star = 1 << 9,
    [InspectorName("근원")] Origin = 1 << 10,
    [InspectorName("황금시대")] GoldenAge = 1 << 11,
}

public static class EnemyTraits
{
    private static readonly (EnemyTrait trait, string name)[] Names =
    {
        (EnemyTrait.Human, "인간"), (EnemyTrait.Machine, "기계"), (EnemyTrait.LightArmor, "경장갑"),
        (EnemyTrait.HeavyArmor, "중장갑"), (EnemyTrait.Mystic, "신비"), (EnemyTrait.Dimension, "차원"),
        (EnemyTrait.Wave, "파도"), (EnemyTrait.Psychic, "사이킥"), (EnemyTrait.Homunculus, "호문클루스"),
        (EnemyTrait.Star, "별"), (EnemyTrait.Origin, "근원"), (EnemyTrait.GoldenAge, "황금시대"),
    };

    public static EnemyTrait Of(ICombatant combatant) =>
        combatant is EnemyInstance enemy && enemy.Data != null ? enemy.Data.traits : EnemyTrait.None;

    // 하나라도 겹치면 true (특효 1회 적용 판정)
    public static bool HasAny(ICombatant combatant, EnemyTrait traits) =>
        traits != EnemyTrait.None && (Of(combatant) & traits) != 0;

    // "인간·신비"
    public static string Label(EnemyTrait traits)
    {
        var parts = new List<string>();
        foreach (var (trait, name) in Names)
            if ((traits & trait) != 0) parts.Add(name);
        return string.Join("·", parts);
    }
}
