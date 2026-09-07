using System.Collections.Generic;

public enum BattlePhase { PlayerTurn, EnemyTurn }

// 매니저와 독립된 전투 순수 데이터 컨테이너 (MonoBehaviour 아님)
// BattleManager가 소유·초기화하고, IPassiveLogic / CardEffect 에 파라미터로 전달한다.
public class BattleState
{
    public PlayerCombatant Player;
    public List<EnemyInstance> Enemies = new();

    // 상태 효과가 아닌 내부 장비 참조. 새 전투에서는 초기화된다.
    public WeaponData CurrentWeapon { get; private set; }
    // 교체 전 카드도 실행 중인 Context에서 참조할 수 있어 전투 수명 동안 보존한다.
    private readonly List<CardData> _runtimeCards = new();

    public CardData CreateCard(CardData template)
    {
        if (template == null) return null;
        var card = template.IsWeaponShootingCard && CurrentWeapon?.ShootingCard != null
            ? template.CreateWeaponVariant(CurrentWeapon.ShootingCard)
            : UnityEngine.Object.Instantiate(template);
        _runtimeCards.Add(card);
        return card;
    }

    public bool ChangePlayerWeapon(WeaponData weapon)
    {
        if (Player == null || weapon == null || weapon.ShootingCard == null)
            return false;

        bool changed = CurrentWeapon != weapon;
        if (!changed && !weapon.SetAmmoOnEquip) return false;
        if (changed)
        {
            CurrentWeapon = weapon;
            ReplaceShootingCards(Hand);
            ReplaceShootingCards(DrawPile);
            ReplaceShootingCards(DiscardPile);
            ReplaceShootingCards(ExhaustPile);
        }
        // 같은 무기로 재전환해도 현재 탄약을 지정값으로 덮어쓴다. 더하거나 상한을 적용하지 않는다.
        if (weapon.SetAmmoOnEquip) Ammo = weapon.AmmoOnEquip;
        return true;
    }

    private void ReplaceShootingCards(List<CardData> pile)
    {
        for (int i = 0; i < pile.Count; i++)
            if (pile[i] != null && pile[i].IsWeaponShootingCard)
                pile[i] = CreateCard(pile[i]);
    }

    public void ReleaseRuntimeCards()
    {
        foreach (var card in _runtimeCards)
        {
            if (card == null) continue;
            if (UnityEngine.Application.isPlaying) UnityEngine.Object.Destroy(card);
            else UnityEngine.Object.DestroyImmediate(card);
        }
        _runtimeCards.Clear();
    }

    public int Energy;
    public int MaxEnergy;
    public int Ammo;
    public int DrawCount;
    public int TurnNumber;
    public bool PlayerLostHpThisTurn;
    public BattlePhase Phase;

    public List<CardData> Hand        = new();
    public List<CardData> DrawPile    = new();
    public List<CardData> DiscardPile = new();
    public List<CardData> ExhaustPile = new();
}
