// 민첩: 방어도 획득량에 스택만큼 가산. 영구 효과.
// 음수 스택도 허용 — 시베리안 로망스처럼 민첩 감소에 활용.
public class DexterityStatus : StatusEffectBase
{
    public DexterityStatus(int stacks) : base(stacks) { }

    public override void TickDown() { } // 영구
}
