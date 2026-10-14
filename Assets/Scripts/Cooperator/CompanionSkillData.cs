using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public enum CompanionSkillCharge
{
    Turns,   // N턴마다 — 전투마다 처음부터
    Battles, // N전투마다 — 런 전체에 이어짐
}

// 동료 스킬. 동료(CoopCharData.skill)가 하나씩 가리키고, 전투 HUD의 지시 버튼에서 쓴다.
// 효과는 카드와 같은 CardEffect 목록이다. 인연 최대 레벨이면 강화 효과를 쓴다. 기획: Docs/기획/동료_스킬.md
[CreateAssetMenu(fileName = "NewCompanionSkill", menuName = "Game Asset/Companion Skill")]
public class CompanionSkillData : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    [Tooltip("카드 설명과 같은 토큰 문법 — {0.amount}는 0번 효과의 amount")]
    [TextArea] public string description;

    [Header("충전")]
    public CompanionSkillCharge chargeType = CompanionSkillCharge.Battles;
    [Min(1)] public int chargeAmount = 3;
    [Tooltip("처음(합류 직후 / 전투 시작)부터 쓸 수 있는지")]
    public bool startReady = true;

    [Header("사용")]
    [Tooltip("켜면 카드처럼 적 하나를 지정한다")]
    public bool selectEnemy;
    [SerializeReference, SubclassPicker] public List<CardEffect> effects = new();

    [Header("인연 강화 (최대 레벨) — 효과가 비어 있으면 강화 없음")]
    [SerializeReference, SubclassPicker] public List<CardEffect> enhancedEffects = new();
    [Tooltip("0이면 기본 충전 그대로")]
    [Min(0)] public int enhancedChargeAmount;
    [TextArea] public string enhancedDescription;

    [Header("컷인")]
    public Sprite cutinArt;
    public string cutinLine;
    public string enhancedCutinLine;

    public bool HasEnhancement => enhancedEffects != null && enhancedEffects.Count > 0;

    public IReadOnlyList<CardEffect> EffectsOf(bool enhanced) => enhanced && HasEnhancement ? enhancedEffects : effects;
    public int ChargeOf(bool enhanced) => enhanced && HasEnhancement && enhancedChargeAmount > 0 ? enhancedChargeAmount : chargeAmount;
    public string CutinLineOf(bool enhanced) => enhanced && HasEnhancement && !string.IsNullOrEmpty(enhancedCutinLine) ? enhancedCutinLine : cutinLine;

    public string GetDescription(bool enhanced)
    {
        bool useEnhanced = enhanced && HasEnhancement && !string.IsNullOrEmpty(enhancedDescription);
        string text = useEnhanced ? enhancedDescription : description;
        if (string.IsNullOrEmpty(text)) return string.Empty;
        var list = EffectsOf(enhanced);
        return Regex.Replace(text, @"\{(\d+)\.(\w+)\}", m =>
        {
            if (!int.TryParse(m.Groups[1].Value, out int i) || i >= list.Count || list[i] == null) return m.Value;
            var field = list[i].GetType().GetField(m.Groups[2].Value);
            object value = field?.GetValue(list[i]);
            return value != null ? value.ToString() : m.Value;
        });
    }
}
