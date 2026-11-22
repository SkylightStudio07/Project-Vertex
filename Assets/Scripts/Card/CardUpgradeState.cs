using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CardUpgradeState
{
    public string cardName;
    public int energyCost;
    public int ammoCost;
    // X 비용 표시. 체크하면 비용 칸에 숫자 대신 X가 보인다 — 표시 전용이며 비용 차감·소비 동작은 바꾸지 않는다.
    // 탄약 X는 AmmoConsumeAllDamageEffect처럼 효과가 직접 전량 소비하므로 ammoCost는 0으로 둔다.
    // 에너지 X는 아직 전량 소비 동작이 없다 (필요해지면 별도 구현).
    [Tooltip("비용 칸에 X로 표시 (표시 전용)")] public bool energyCostIsX;
    [Tooltip("비용 칸에 X로 표시 (표시 전용). 탄약 전량 소비 효과와 함께 쓰고 탄약 비용은 0으로 둔다")] public bool ammoCostIsX;
    // 이펙트는 SO 참조가 아니라 인라인 직렬화 — 인스펙터에서 타입 선택 후 수치 직접 입력
    [SerializeReference, SubclassPicker] public List<CardEffect> effects;
    public bool isExhaust;
    public bool isEthereal;
    public bool isInnate;
    public bool isRetain;

    public bool HasEffects => effects != null && effects.Count > 0;
}
