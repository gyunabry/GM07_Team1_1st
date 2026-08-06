using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class AttackBase : IAttack
{
    public void Attack(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer)
    {
        for (int i = 0; i < data.projectileCount; i++)
        {
            AttackPoint ap = poolManager.GetPool<AttackPoint>();
            ap.attackDamage = AttackDamage;
            ap.transform.position = data.position;
            Quaternion baseRota = Quaternion.LookRotation(data.direction);
            baseRota.x = 0f;
            Quaternion side = Quaternion.Euler(0f, 0f, 0f);
            if (i == 0)
            {
                side = Quaternion.Euler(0f, -15f, 0f);
            }
            if (i == 1)
            {
                side = Quaternion.Euler(0f, 0f, 0f);
            }
            if (i == 2)
            {
                side = Quaternion.Euler(0f, 15f, 0f);
            }
            ap.transform.rotation = baseRota * side;
            ap.poolManager = poolManager;
            ap.layer = layer;
        }
    }
    public void Skill(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer)
    {
        Collider[] enemy = Physics.OverlapSphere(data.position, data.distance, layer);

        if (enemy.Length > 0)
        {
            foreach (var that in enemy)
            {
                Enemy ene = that.GetComponent<Enemy>();
                ene.TakeDamage(AttackDamage);
            }
        }
    }
    public void Skill2(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer)
    {

    }
    public void Skill3(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer)
    {

    }
}

public abstract class AttackDeco : IAttack
{
    protected IAttack attack;
    public AttackDeco(IAttack attack)
    {
        this.attack = attack;
    }
    public virtual void Attack(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer)
    {
        this.attack.Attack(AttackDamage, data, poolManager, layer);
    }
    public virtual void Skill(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer)
    {
        this.attack.Skill(AttackDamage, data, poolManager, layer);
    }
    public virtual void Skill2(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer)
    {
        this.attack.Skill2(AttackDamage, data, poolManager, layer);
    }
    public virtual void Skill3(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer)
    {
        this.attack.Skill3(AttackDamage, data, poolManager, layer);
    }
}
public class AttackData
{
    public int attackDamage = 5;
    public float attackSpeed = 0.5f;
    public Vector3 position;
    public Vector3 direction;
    public float distance;
    public int projectileCount = 1;
    public float spreadAngle = 0f;
}
public interface IAttack
{
    public void Attack(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer);
    public void Skill(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer);
    public void Skill2(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer);
    public void Skill3(int AttackDamage, AttackData data, MonsterPoolManager poolManager, LayerMask layer);
}