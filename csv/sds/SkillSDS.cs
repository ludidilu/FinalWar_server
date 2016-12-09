﻿public class SkillSDS : CsvBase, ISkillSDS
{
    public int skillTime;
    public int priority;
    public int skillTarget;
    public int targetNum;
    public int skillEffect;
    public int[] skillDatas;

    public SkillTime GetSkillTime()
    {
        return (SkillTime)skillTime;
    }
    public int GetPriority()
    {
        return priority;
    }
    public SkillTarget GetSkillTarget()
    {
        return (SkillTarget)skillTarget;
    }
    public int GetTargetNum()
    {
        return targetNum;
    }
    public SkillEffect GetSkillEffect()
    {
        return (SkillEffect)skillEffect;
    }
    public int[] GetSkillDatas()
    {
        return skillDatas;
    }
}
