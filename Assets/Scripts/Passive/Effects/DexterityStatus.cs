// 민첩: 방어도 획득량에 스택만큼 가산. 영구 효과.
// 음수 스택도 허용 — 시베리안 로망스처럼 민첩 감소에 활용.
// 다른 상태와 달리 데미지 파이프라인 훅이 아니라 PlayerCombatant.AddBlock()이
// 직접 이 타입을 찾아서 적용한다 (IPassiveLogic에 방어도 훅이 없어서 택한 방식).
public class DexterityStatus : StatusEffectBase
{
    public DexterityStatus(int stacks) : base(stacks) { }

    public override void TickDown() { } // 영구

    // 음수 스택(민첩 감소)은 유효한 상태 — 정확히 0일 때만 정리.
    // 기본 판정(<= 0)을 쓰면 시베리안 로망스의 민첩 페널티가 생성 직후 제거되는 버그가 된다.
    public override bool IsExpired => Stacks == 0;
}
