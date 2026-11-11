using System.Collections.Generic;
using UnityEngine;

// 로비 화면들이 함께 쓰는 인물 · 적 목록. 동료 화면 · 거점 · 기록관 · 마테리얼 · 훈련장 · 카드 도감이
// 저마다 씬에 같은 목록을 들고 있던 것을 대체한다. 동료나 기록 대상 적을 추가할 때 이 에셋만 고친다.
// 훈련장 조우 목록은 TrainingRosterSO, 런 안의 합류 처리는 CooperationManager가 따로 맡는다.
[CreateAssetMenu(fileName = "LobbyRoster", menuName = "Game Asset/Lobby Roster")]
public class LobbyRosterSO : ScriptableObject
{
    [Tooltip("동료 전원 (이벤트 동료 포함). 동료 화면 · 거점 · 기록관 · 훈련장 동행 후보 · 카드 도감의 동료 카드 출처")]
    public List<CoopCharData> companions = new();

    [Tooltip("기록관 · 마테리얼에 기록이 남는 적. 격퇴 기록과 훈련장 해금은 이 목록을 기준으로 센다")]
    public List<EnemyData> recordEnemies = new();
}
