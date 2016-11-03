public class HeroSDS : CsvBase, IHeroSDS
{
    public int hp;
    public int shield;
    public int power;
    public int cost;
    public bool canControl;
    public bool threat;
    public int attack;
    public int shoot;
    public int[] skills;
    public int[] auras;

    public int GetID()
    {
        return ID;
    }
    public int GetHp()
    {
        return hp;
    }
    public int GetShield()
    {
        return shield;
    }
    public int GetCost()
    {
        return cost;
    }
    public bool GetCanControl()
    {
        return canControl;
    }
    public bool GetThreat()
    {
        return threat;
    }
    public int GetAttack()
    {
        return attack;
    }
    public int GetShoot()
    {
        return shoot;
    }
    public int[] GetSkills()
    {
        return skills;
    }
    public int[] GetAuras()
    {
        return auras;
    }
}

