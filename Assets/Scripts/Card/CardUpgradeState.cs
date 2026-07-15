using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CardUpgradeState
{
    public string cardName;
    public int energyCost;
    public int ammoCost;
    // 이펙트는 SO 참조가 아니라 인라인 직렬화 — 인스펙터에서 타입 선택 후 수치 직접 입력
    [SerializeReference, SubclassPicker] public List<CardEffect> effects;
    public bool isExhaust;
    public bool isEthereal;
    public bool isInnate;
    public bool isRetain;

    public bool HasEffects => effects != null && effects.Count > 0;
}
