using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CardUpgradeState
{
    public string cardName;
    public int energyCost;
    public int ammoCost;
    public List<CardEffect> effects;
    public bool isExhaust;
    public bool isEthereal;
    public bool isInnate;
    public bool isRetain;

    public bool HasEffects => effects != null && effects.Count > 0;
}
