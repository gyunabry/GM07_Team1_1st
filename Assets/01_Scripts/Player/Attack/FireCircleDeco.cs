using UnityEngine;

public class FireCircleDeco : AttackDeco
{
    public FireCircleDeco(IAttack attack) : base(attack) { }

    public override void FireCircle(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer)
    {
        
        base.FireCircle(AttackDamage, data, poolManager, layer);
    }
}
