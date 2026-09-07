using System.Collections.Generic;

public interface ICombatant
{
    int HP { get; }
    int MaxHP { get; }
    int Block { get; }
    bool IsDead { get; }
    List<IPassiveLogic> Passives { get; }
    StatusContainer Statuses { get; }

    void TakeDamage(DamageInfo info);
    void AddBlock(int amount);
    void Heal(int amount);
    void AddPassive(IPassiveLogic passive);
    void ResetBlock();
    void RemoveExpiredPassives();
}
