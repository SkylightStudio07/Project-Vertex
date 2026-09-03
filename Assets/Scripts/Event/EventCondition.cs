// 이벤트 로스터 엔트리의 등장 조건 기반 추상 클래스.
// CardEffect와 동일한 패턴 — [SerializeReference]로 EventRosterEntry.conditions에
// 인라인 다형성으로 저장된다. 여러 개면 전부 만족해야 등장(AND).
[System.Serializable]
public abstract class EventCondition
{
    public abstract bool IsMet();
}
