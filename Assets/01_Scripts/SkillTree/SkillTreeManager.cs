using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    [SerializeField] private List<SkillDataSO> skillList = new List<SkillDataSO>();
    [SerializeField] private List<SkillRuntimeState> skillRuntimeStates = new List<SkillRuntimeState>();
    [SerializeField] private SkillEffectContext effectContext;

    public IReadOnlyList<SkillRuntimeState> RuntimeState => skillRuntimeStates;

    public event Action OnSkillChange;

    private void Awake()
    {
        // 중복 생성 방지
        skillRuntimeStates.Clear();

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
                        if (effectContext.player.skillPoint >= skill.skillNeedSkillPoint && !(skill.skillMaxLevel <= state.skillLevel))
                        {
                            effectContext.player.skillPoint -= skill.skillNeedSkillPoint;
                            state.skillLevel++;
                        }
                    }
                    else if (!(skill.skillNeedMoney == 0))
                    {
                        if(!(skill.skillMaxLevel <= state.skillLevel))
                        {
                            if (effectContext.currencySystem.TrySpendMoney(skill.skillNeedMoney))
                            {
                                state.skillLevel++;
                            }
                        }
                    }
                    else if(!(skill.skillMaxLevel <= state.skillLevel))
                    {
                        state.skillLevel++;
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
                    skillID.effect.SkillEffect(effectContext, skillID, skill.skillLevel - 1);
                }
            }
        }

        CommitEffects();
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
        effectContext.player.attackDamage = 10;
        effectContext.player.attackSpeed = 1;
        effectContext.player.attackDistance = 2.5f;
        
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
        effectContext.player.navMeshAgent.speed = 3f;

        // 플레이어 인벤토리 수용량 초기화
        PlayerInventory playerInventory = effectContext.player.GetComponent<PlayerInventory>();
        if (playerInventory != null) 
        {
            playerInventory.Inventory.SetBonusCapacity(0);
        }

        // 플레이어 아이템 획득 범위 초기화
        PlayerItemCollector itemCollector = effectContext.player.GetComponent<PlayerItemCollector>();
        if (itemCollector != null)
        {
            itemCollector.ResetRangeBonus();
        }

        BeginEffectsRebuild();
    }

    private void BeginEffectsRebuild()
    {
        ProductionSkillRegistry.BeginRebuild();
        StorageSkillRegistry.BeginRebuild();
        RewardSkillRegistry.BeginRebuild();
    }

    private void CommitEffects()
    {
        ProductionSkillRegistry.Commit();
        StorageSkillRegistry.Commit();
        RewardSkillRegistry.Commit();
    }

    public void RestoreLevels(IReadOnlyList<SkillLevelSaveData> savedSkills)
    {
        // 현재 스킬 상태를 초기화
        foreach (SkillRuntimeState runtimeState in skillRuntimeStates)
        {
            if (runtimeState == null) continue;

            runtimeState.skillLevel = 0;
            runtimeState.Locked = true;
        }

        if (savedSkills != null)
        {
            foreach (SkillLevelSaveData savedSkill in savedSkills)
            {
                if (savedSkill == null || string.IsNullOrWhiteSpace(savedSkill.skillId))
                {
                    continue;
                }

                SkillDataSO matchedSkillData = null;
                SkillRuntimeState matchedRuntimeState = null;

                // 저장된 ID와 일치하는 원본 SkillDataSO를 검색
                foreach (SkillDataSO skillData in skillList)
                {
                    if (skillData == null)
                    {
                        continue;
                    }

                    if (string.Equals(skillData.skillID, savedSkill.skillId, StringComparison.Ordinal))
                    {
                        matchedSkillData = skillData;
                        break;
                    }
                }

                if (matchedSkillData == null)
                {
                    Debug.LogWarning($"저장된 스킬 ID를 찾을 수 없습니다: {savedSkill.skillId}");
                    continue;
                } 

                foreach (SkillRuntimeState runtimeState in skillRuntimeStates)
                {
                    if (runtimeState == null) continue;

                    if (string.Equals(runtimeState.skillID, savedSkill.skillId, StringComparison.Ordinal))
                    {
                        matchedRuntimeState = runtimeState;
                        break;
                    }
                }

                if (matchedRuntimeState == null)
                {
                    Debug.LogWarning($"스킬 런타임 상태를 찾을 수 없습니다: {savedSkill.skillId}");
                    continue;
                }

                // 실제 스킬의 최대 레벨을 넘지 못 하도록 제한
                int maxLevel = Mathf.Max(0, matchedSkillData.skillMaxLevel);

                matchedRuntimeState.skillLevel = Mathf.Clamp(savedSkill.level, 0, maxLevel);
            }
        }

        // 저장된 레벨을 기준으로 스킬 조건을 다시 계산
        SkillUnlockCheck();

        // 복구된 레벨을 기준으로 스킬트리 효과를 재적용
        SkillRefresh();
    }
}
