// 이벤트 선택지 효과 중 플레이어 입력이 필요한 것(카드 선택 등).
// EventView는 선택 즉시 이 효과를 실행하고, onDone이 불릴 때까지 결과 텍스트를 보류한다.
// 구현체는 끝나면 반드시 onDone을 불러야 한다 — 부르지 않으면 이벤트가 그 자리에서 멈춘다.
public interface IInteractiveEventEffect
{
    void Run(System.Action onDone);
}
