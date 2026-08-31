using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class AttackBase : IAttack
{
    public void MagicArrow(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer)
    {
        Collider[] enemy = Physics.OverlapSphere(data.position, data.distance, layer);

        
        AttackPoint ap = poolManager.GetPool<AttackPoint>();
        if (!(enemy.Length == 0))
        {
            if (enemy[0] != null)
            {
                Enemy thisEnemy = enemy[0].gameObject.GetComponent<Enemy>();
                ap.enemy = thisEnemy;
            }
        }
        ap.attackDamage = AttackDamage;
        ap.transform.position = data.position;
        Quaternion baseRota = Quaternion.LookRotation(data.direction);
        baseRota.x = 0f;
        Quaternion side = Quaternion.Euler(0f, 0f, 0f);
        ap.transform.rotation = baseRota * side;
        ap.poolManager = poolManager;
        ap.layer = layer;
        
    }
    public void FireCircle(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer)
    {
        Collider[] enemy = Physics.OverlapSphere(data.position, data.distance, layer);

        if (enemy.Length > 0)
        {
            foreach (var that in enemy)
            {
                Enemy ene = that.GetComponent<Enemy>();
                AudioManager.Instance.PlaySFX(ESFXType.Hit_FireCircle);
                ene.TakeDamage(AttackDamage);
            }
        }
    }
    public void ChasingSickle(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer)
    {
        Collider[] enemy = Physics.OverlapSphere(data.position, data.distance, layer);

        if (enemy.Length > 0) 
        { 
            foreach(var that in enemy)
            {
                Vector3 thatDir = (that.transform.position - data.position).normalized;
                if(Vector3.Angle(data.forward, thatDir) < data.spreadAngle)
                {
                    Enemy ene = that.GetComponent<Enemy>();
                    AudioManager.Instance.PlaySFX(ESFXType.Hit_ChasingSickle);
                    ene.TakeDamage(AttackDamage);
                }
            }
        }
    }
    public void LightningRay(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer)
    {
        
    }
    public void FlowerThorns(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer)
    {
        Collider[] enemy = Physics.OverlapSphere(data.position, data.distance, layer);

        if (enemy.Length > 0)
        {
            foreach (var that in enemy)
            {
                Enemy ene = that.GetComponent<Enemy>();
                AudioManager.Instance.PlaySFX(ESFXType.Hit_FlowerThorns);
                ene.TakeDamage(AttackDamage);
            }
        }

    }
    public void Skill5(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer)
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
    public virtual void MagicArrow(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer)
    {
        this.attack.MagicArrow(AttackDamage, data, poolManager, layer);
    }
    public virtual void FireCircle(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer)
    {
        this.attack.FireCircle(AttackDamage, data, poolManager, layer);
    }
    public virtual void ChasingSickle(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer)
    {
        this.attack.ChasingSickle(AttackDamage, data, poolManager, layer);
    }
    public virtual void LightningRay(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer)
    {
        this.attack.LightningRay(AttackDamage, data, poolManager, layer);
    }
    public virtual void FlowerThorns(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer)
    {
        this.attack.FlowerThorns(AttackDamage, data, poolManager, layer);
    }
    public virtual void Skill5(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer)
    {
        this.attack.Skill5(AttackDamage, data, poolManager, layer);
    }
}
public class AttackData
{
    public float attackDamage = 5;
    public float attackSpeed = 0.5f;
    public Vector3 position;
    public Vector3 forward;
    public Vector3 direction;
    public float distance;
    public int projectileCount = 1;
    public float spreadAngle = 0f;
}
public interface IAttack
{
    public void MagicArrow(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer);
    public void FireCircle(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer);
    public void ChasingSickle(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer);
    public void LightningRay(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer);
    public void FlowerThorns(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer);
    public void Skill5(float AttackDamage, AttackData data, PoolManager poolManager, LayerMask layer);
}