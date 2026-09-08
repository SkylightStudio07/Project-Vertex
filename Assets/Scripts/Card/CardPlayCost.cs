using System;

public struct CardPlayCost
{
    public int Energy;
    public int Ammo;
    public int Hp;

    public CardPlayCost(int energy, int ammo, int hp = 0)
    {
        Energy = energy;
        Ammo = ammo;
        Hp = hp;
    }

    public void ClampToNonNegative()
    {
        Energy = Math.Max(0, Energy);
        Ammo = Math.Max(0, Ammo);
        Hp = Math.Max(0, Hp);
    }
}
