// 민첩: 방어도 획득량에 스택만큼 가산. 영구 효과.
// 음수 스택도 허용 — 시베리안 로망스처럼 민첩 감소에 활용.
// 다른 상태와 달리 데미지 파이프라인 훅이 아니라 PlayerCombatant.AddBlock()이
// 직접 이 타입을 찾아서 적용한다 (IPassiveLogic에 방어도 훅이 없어서 택한 방식).
public class DexterityStatus : StatusEffectBase
{
    public DexterityStatus(int stacks) : base(stacks) { }

    public override void TickDown() { } // 영구
}
