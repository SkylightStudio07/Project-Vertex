// 현재 총기의 최대 탄약으로 재장전한다. 일반 탄약 획득(AddAmmoEffect)과는 별도 동작이다.
[System.Serializable]
public class ReloadAmmoEffect : CardEffect
{
    [UnityEngine.SerializeReference, SubclassPicker]
    public System.Collections.Generic.List<CardEffect> effectsWhenEmpty = new();
    public override void Execute(CardContext context)
    {
        if (context?.State == null) return;
        bool empty = context.State.Ammo == 0;
        if (context.State.ReloadAmmo() && empty) EffectRunner.ExecuteImmediate(effectsWhenEmpty, context);
    }
}
