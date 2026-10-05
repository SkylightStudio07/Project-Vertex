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
    private Dictionary<WeaponData, int> _magazineBonuses = new();
    private bool _ammoPreview;

    // 무기고 영구 강화 단계. 전투 시작 때 BattleManager가 PlayerRecord로 연결한다. 비어 있으면 강화 없음(테스트 등).
    public System.Func<WeaponData, int> WeaponUpgradeLevel;
    private int UpgradeLevelOf(WeaponData weapon) => weapon != null && WeaponUpgradeLevel != null ? WeaponUpgradeLevel(weapon) : 0;

    // 최대 탄약 = 무기 기본 + 무기고 강화 + 이번 전투 탄창 확장
    public int MaxAmmo => CurrentWeapon != null
        ? CurrentWeapon.MaxAmmo + CurrentWeapon.MaxAmmoBonusAt(UpgradeLevelOf(CurrentWeapon)) + (_magazineBonuses.TryGetValue(CurrentWeapon, out int bonus) ? bonus : 0)
        : 0;

    // 탄약을 쓰는 공격의 타격마다 더하는 피해 (한방형 무기의 무기고 강화)
    public int ShotDamageBonus => CurrentWeapon != null ? CurrentWeapon.ShotDamageBonusAt(UpgradeLevelOf(CurrentWeapon)) : 0;

    public void IncreaseMagazine(WeaponData weapon, int amount)
    {
        if (weapon == null || amount <= 0) return;
        _magazineBonuses.TryGetValue(weapon, out int bonus);
        _magazineBonuses[weapon] = bonus + amount;
        if (CurrentWeapon == weapon) ReloadAmmo();
    }
    public bool FirstAmmoUseAvailable { get; private set; }
    public bool LastAmmoUseAvailable { get; private set; }

    // 미리보기는 탄약 관련 값만 변경한다. 전투원·더미에는 쓰지 않는다.
    internal BattleState CopyForAmmoPreview()
    {
        var copy = (BattleState)MemberwiseClone();
        copy._magazineBonuses = new Dictionary<WeaponData, int>(_magazineBonuses);
        copy._ammoPreview = true;
        return copy;
    }

    internal void PreviewWeaponReload(WeaponData weapon)
    {
        if (Player == null || weapon == null || weapon.ShootingCard == null) return;
        CurrentWeapon = weapon;
        ReloadAmmo();
    }

    public int ConsumeAmmo(int amount, CardContext context)
    {
        int spent = System.Math.Min(System.Math.Max(0, amount), System.Math.Max(0, Ammo));
        Ammo -= spent;
        bool cardUse = context?.Card != null && context.State == this &&
                       context.TriggeringPassive == null && Player != null && context.Source == Player;
        if (cardUse)
        {
            // 초탄·막탄은 그 판정을 쓰는 무기(WeaponData.UsesAmmoBoundary)에서만 성립한다
            bool boundary = CurrentWeapon != null && CurrentWeapon.UsesAmmoBoundary;
            bool first = boundary && spent > 0 && FirstAmmoUseAvailable;
            bool last = boundary && spent > 0 && Ammo == 0 && LastAmmoUseAvailable;
            bool firstConsumption = context.AmmoSpent == 0;
            context.RecordAmmoUse(spent, first, last);
            if (first) FirstAmmoUseAvailable = false;
            if (last) LastAmmoUseAvailable = false;
            if (spent > 0)
                AmmoStatusEvents.Visit(this, (status, behavior) => behavior.OnAmmoSpent(status, context, spent, first, firstConsumption, _ammoPreview));
            if (!_ammoPreview) Player.RemoveExpiredPassives();
        }
        return spent;
    }

    // 일반 탄약 획득. 총기를 들고 있으면 최대 탄약을 넘지 않는다(하드캡, 넘친 만큼은 버린다).
    // 총기가 없으면 상한이 없으므로 그대로 더한다. 이미 상한을 넘어 있으면 줄이지 않고 더하지도 않는다.
    public int GainAmmo(int amount)
    {
        if (amount <= 0) return 0;
        int before = Ammo;
        Ammo = CurrentWeapon == null ? Ammo + amount : System.Math.Max(Ammo, System.Math.Min(Ammo + amount, MaxAmmo));
        return Ammo - before;
    }

    // 일반 탄약 획득과 구분되는 재장전 진입점. 총기가 없으면 탄약을 변경하지 않는다.
    public bool ReloadAmmo()
    {
        if (Player == null || CurrentWeapon == null) return false;
        Ammo = MaxAmmo;
        FirstAmmoUseAvailable = true;
        LastAmmoUseAvailable = true;
        if (!_ammoPreview)
            AmmoStatusEvents.Visit(this, (status, behavior) => behavior.OnReload(status, new CardContext { State = this }));
        return true;
    }
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

        // 이미 든 총기로의 전환은 아무 일도 하지 않는다(재장전 없음). 재장전은 재장전 효과로만 한다.
        if (CurrentWeapon == weapon) return false;

        CurrentWeapon = weapon;
        ReplaceShootingCards(Hand);
        ReplaceShootingCards(DrawPile);
        ReplaceShootingCards(DiscardPile);
        ReplaceShootingCards(ExhaustPile);
        return ReloadAmmo();
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
    // 0이면 현재 의도만, 양수면 각 적의 향후 행동을 해당 개수만큼 추가 표시한다.
    public int EnemyIntentLookahead;
    public int TurnNumber;
    public bool PlayerLostHpThisTurn;
    public BattlePhase Phase;

    public List<CardData> Hand        = new();
    public List<CardData> DrawPile    = new();
    public List<CardData> DiscardPile = new();
    public List<CardData> ExhaustPile = new();
}
