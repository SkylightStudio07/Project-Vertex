using System.Collections.Generic;
using UnityEngine;

// 이벤트 선택지에서 전투를 시작하는 효과.
// EventView는 이 효과만 진행 버튼까지 보류했다가 실행한다 — 결과 텍스트를 읽는 동안 전투가 시작되면
// 이벤트 창 뒤에서 전투가 돌아가고, 진행 버튼이 전투 위에 맵을 띄워버리기 때문.
// 그래서 효과 목록 어디에 놓아도 항상 마지막에 실행된다.
[System.Serializable]
public class StartBattleEffect : CardEffect
{
    [Tooltip("이 전투에 등장할 적 구성. MapConfig의 적 풀에 등록하지 않으면 일반 전투에는 나오지 않는다.")]
    public EnemyEncounter encounter;

    [Tooltip("보상 확률표를 고르는 값. 이벤트 전투는 보통 Normal.")]
    public BattleType battleType = BattleType.Normal;

    [Tooltip("이 전투에서 승리하면 실행할 효과(예: 동료 합류). 패배하면 실행되지 않는다.")]
    [SerializeReference, SubclassPicker] public List<CardEffect> onVictoryEffects;

    public override void Execute(CardContext context)
    {
        if (GameManager.Instance == null) return;

        System.Action onVictory = null;
        if (onVictoryEffects != null && onVictoryEffects.Count > 0)
            onVictory = () => EffectRunner.ExecuteImmediate(onVictoryEffects, new CardContext());

        GameManager.Instance.StartEncounterBattle(encounter, battleType, onVictory);
    }
}
