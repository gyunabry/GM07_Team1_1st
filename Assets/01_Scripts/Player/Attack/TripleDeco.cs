using UnityEngine;

public class TripleDeco : AttackDeco
{
    public TripleDeco(IAttack attack) : base(attack){}
    
    public override void Attack(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer)
    {
        data.projectileCount += 2;
        data.spreadAngle += 30;
        base.Attack(AttackDamage, data, poolManager, layer);
    }
}
