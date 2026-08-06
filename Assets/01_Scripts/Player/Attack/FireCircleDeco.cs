using UnityEngine;

public class FireCircleDeco : AttackDeco
{
    public FireCircleDeco(IAttack attack) : base(attack) { }

    public override void Skill(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer)
    {
        
        base.Skill(AttackDamage, data, poolManager, layer);
    }
}
