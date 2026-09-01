using UnityEngine;

[CreateAssetMenu(fileName = "SkillDataSO", menuName = "SkillData/SkillDataSO")]
public class SkillDataSO : ScriptableObject
{
    public string skillID;
    public string skillName;
    [TextArea]
    public string skillDesc; 
    public Sprite skillSprite;
    public int skillMaxLevel;
    public int skillNeedSkillPoint;
    public int skillNeedLevel;
    public int skillNeedMoney;
    public float[] value;
    public SkillDataSO[] needSkill;
    public SkillEffectSO effect;
    public string skillChangeStat;
}
[System.Serializable]
public class MultiValue
{
    public float[] value;
}
public abstract class SkillEffectSO : ScriptableObject
{
    public abstract void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel);
}
[System.Serializable]
public class SkillRuntimeState
{
    public string skillID;
    public int skillLevel;
    public bool Locked;
}

