using System.Collections.Generic;

public interface ICombatant
{
    int HP { get; }
    int MaxHP { get; }
    int Block { get; }
    bool IsDead { get; }
    List<IPassiveLogic> Passives { get; }

    void TakeDamage(DamageInfo info);
    void AddBlock(int amount);
    void ResetBlock();
    void RemoveExpiredPassives();
}
