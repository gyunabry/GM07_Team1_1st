using UnityEngine;

[CreateAssetMenu(fileName = "SkillDataSO", menuName = "SkillData/SkillDataSO")]
public class SkillDataSO : ScriptableObject
{
    public int skillID;
    public string skillName;
    public Sprite skillSprite;
    public int skillMaxLevel;
    public int skillNeedSkillPoint;
    public int skillNeedLevel;
    public int skillNeedMoney;
    public float[] Value;
    public SkillDataSO needSkill;
    public SkillEffectSO effect;
}
public abstract class SkillEffectSO : ScriptableObject
{
    public abstract void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel);
}
[System.Serializable]
public class SkillRuntimeState
{
    public int skillID;
    public int skillLevel;
    public bool Locked;
}

