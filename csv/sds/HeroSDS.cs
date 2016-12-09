﻿public class HeroSDS : CsvBase, IHeroSDS
{
    public int hp;
    public int shield;
    public int power;
    public int cost;
    public bool canControl;
    public int attack;
    public int abilityType;
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
    public int GetAttack()
    {
        return attack;
    }
    public AbilityType GetAbilityType()
    {
        return (AbilityType)abilityType;
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

