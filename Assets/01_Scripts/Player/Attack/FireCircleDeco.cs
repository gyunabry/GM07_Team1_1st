using UnityEngine;

public class FireCircleDeco : AttackDeco
{
    public FireCircleDeco(IAttack attack) : base(attack) { }

    public override void FireCircle(float AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer)
    {
        
        base.FireCircle(AttackDamage, data, poolManager, layer);
    }
}
