using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StartingWeapon", menuName = "Lobby/Starting Weapon")]
public class StartingWeaponData : ScriptableObject
{
    [Header("무기 정보")]
    [SerializeField] private string weaponId;
    [SerializeField] private string weaponName;

    [Header("시작 덱")]
    [SerializeField] private CardData attackCard;
    [SerializeField, Min(0)] private int attackCardCount;
    [SerializeField] private CardData defenseCard;
    [SerializeField, Min(0)] private int defenseCardCount;
    [SerializeField] private CardData reloadCard;
    [SerializeField, Min(0)] private int reloadCardCount;

    public string WeaponId => weaponId;
    public string WeaponName => weaponName;
    public CardData AttackCard => attackCard;
    public int AttackCardCount => attackCardCount;
    public CardData DefenseCard => defenseCard;
    public int DefenseCardCount => defenseCardCount;
    public CardData ReloadCard => reloadCard;
    public int ReloadCardCount => reloadCardCount;
    public int TotalCardCount => attackCardCount + defenseCardCount + reloadCardCount;

    public List<CardData> CreateRuntimeDeck()
    {
        List<CardData> deck = new(TotalCardCount);
        AddCardCopies(deck, attackCard, attackCardCount);
        AddCardCopies(deck, defenseCard, defenseCardCount);
        AddCardCopies(deck, reloadCard, reloadCardCount);
        return deck;
    }

    private static void AddCardCopies(List<CardData> deck, CardData card, int count)
    {
        if (card == null)
            return;

        for (int i = 0; i < Mathf.Max(0, count); i++)
            deck.Add(Instantiate(card));
    }

    private void OnValidate()
    {
        attackCardCount = Mathf.Max(0, attackCardCount);
        defenseCardCount = Mathf.Max(0, defenseCardCount);
        reloadCardCount = Mathf.Max(0, reloadCardCount);
    }
}
