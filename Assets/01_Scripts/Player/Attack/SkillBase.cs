using System.Collections;
using UnityEngine;

public abstract class SkillBase : ScriptableObject
{
    public abstract IEnumerator RunSkill(
        Player player, 
        AttackData ad,
        PoolManager poolManager,
        ParticleManager particleManager,
        LayerMask layer,
        IAttack attack);
}
