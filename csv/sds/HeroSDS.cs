public class HeroSDS : CsvBase, IHeroSDS
{
    public int hp;
    public int shield;
    public int power;
    public int cost;
    public bool canControl;
    public bool canMove;
    public int levelUp;
    public int attack;
    public int abilityType;
    public int abilityData;
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
    public bool GetCanMove()
    {
        return canMove;
    }
    public int GetLevelUp()
    {
        return levelUp;
    }
    public int GetAttack()
    {
        return attack;
    }
    public AbilityType GetAbilityType()
    {
        return (AbilityType)abilityType;
    }
    public int GetAbilityData()
    {
        return abilityData;
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

