using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    [SerializeField] private List<SkillDataSO> skillList = new List<SkillDataSO>();
    [SerializeField] private List<SkillRuntimeState> skillRuntimeStates = new List<SkillRuntimeState>();
    [SerializeField] private SkillEffectContext effectContext;

    public event Action OnSkillChange;

    private void Awake()
    {
        foreach(var skill in skillList)
        {
            SkillRuntimeState state = new SkillRuntimeState();
            state.skillID = skill.skillID;
            state.Locked = true;
            skillRuntimeStates.Add(state);
        }
        SkillUnlockCheck();
        SkillRefresh();
    }
    public void SkillTreeClick(SkillDataSO skill) //버튼 클릭시 호출하여 스킬 적용하는 메서드
    {
        foreach(var state in skillRuntimeStates)
        {
            if(state.skillID == skill.skillID)
            {
                if (state.Locked) return;
                
                if(state.skillLevel >= 0)
                {
                    if(!(skill.skillNeedSkillPoint == 0))
                    {
                        if (effectContext.player.skillPoint >= skill.skillNeedSkillPoint && !(skill.skillMaxLevel < state.skillLevel))
                        {
                            effectContext.player.skillPoint -= skill.skillNeedSkillPoint;
                            state.skillLevel++;
                            skill.effect.SkillEffect(effectContext, skill, state.skillLevel);
                        }
                    }
                    else if (!(skill.skillNeedMoney == 0))
                    {
                        if (effectContext.currencySystem.TrySpendMoney(skill.skillNeedMoney))
                        {
                            state.skillLevel++;
                        }
                    }
                }
            }
        }
        SkillRefresh();
        SkillUnlockCheck();
    }
    public void SkillUnlockCheck() //스킬 언락 기준 확인/스킬 트리 열때 실행
    {
        foreach(var skill in skillRuntimeStates)
        {
            if(skill.Locked == true)
            {
                foreach(var skillID in skillList)
                {
                    if(skillID.skillID == skill.skillID)
                    {
                        if (skillID.needSkill == null || skillID.needSkill.Length == 0)
                        {
                            skill.Locked = false;
                        }
                        else
                        {
                            int skillUnlocked = 0;
                            foreach(var lockSkill in skillRuntimeStates)
                            {
                                foreach(var s in skillID.needSkill)
                                {
                                    if(s == null)
                                    {
                                        break;
                                    }
                                    if(lockSkill.skillID == s.skillID)
                                    {
                                        if(lockSkill.skillLevel > 0)
                                        {
                                            skillUnlocked++;
                                        }
                                    }
                                }
                            }
                            if (skillUnlocked >= skillID.needSkill.Length)
                            {
                                skill.Locked = false;
                            }
                        }
                    }
                }
            }
        }
        OnSkillChange?.Invoke();
    }
    
    public void SkillRefresh() //모든 스킬 값 초기화 후 재적용
    {
        ResetEffect();
        foreach(var skill in skillRuntimeStates)
        {
            if (skill.Locked) continue;
            if (skill.skillLevel == 0) continue;
            foreach(var skillID in skillList)
            {
                if(skillID.skillID == skill.skillID)
                {
                    skillID.effect.SkillEffect(effectContext, skillID, skill.skillLevel);
                }
            }
        }
    }
    public void SkillTreeReset()//스킬트리 초기화
    {
        int getSkillPoint = 0;
        foreach (var state in skillRuntimeStates)
        {
            state.Locked = true;
            getSkillPoint += state.skillLevel;
            state.skillLevel = 0;
        }
        effectContext.player.skillPoint += getSkillPoint;
        SkillUnlockCheck();
        SkillRefresh();
    }
    public SkillRuntimeState GetState(SkillDataSO skill)
    {
        foreach(var state in skillRuntimeStates)
        {
            if (state.skillID == skill.skillID) return state;
        }
        Debug.Log("Null반환됨");
        return null;
    }
    public void ResetEffect()
    {
        effectContext.player.attackDamage = 0;
        effectContext.player.attackSpeed = 0;
        effectContext.player.attackDistance = 0;
        
         for (int i = 0; i < effectContext.playerAttack.upgrade.Length; i++)
         {
            if (effectContext.playerAttack.upgrade[i] != null)
            {
                effectContext.playerAttack.upgrade[i].damage = 0;
                effectContext.playerAttack.upgrade[i].attackSpeed = 0;
                effectContext.playerAttack.upgrade[i].distance = 0;
                effectContext.playerAttack.upgrade[i].projectileCount = 0;
            }
        }
        effectContext.player.navMeshAgent.speed = 3.5f;
    }
}
