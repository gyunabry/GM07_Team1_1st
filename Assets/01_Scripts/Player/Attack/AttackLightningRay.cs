using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackLightningRay", menuName = "ScriptableObjects/Skills/AttackLightningRay")]
public class AttackLightningRay : SkillBase
{
    public override IEnumerator RunSkill(Player player, AttackData ad, PoolManager poolManager, ParticleManager particleManager, LayerMask layer, IAttack attack)
    {
        ad.position = player.transform.position;

        Collider[] enemy = Physics.OverlapSphere(player.transform.position, ad.distance, layer);
        Collider nearEnemy = null;
        float minDis = Mathf.Infinity;

        for(int i = 0; i < ad.projectileCount; i++)
        {
            if(enemy.Length > 0)
            {
                foreach(var that in enemy)
                {
                    float distance = (player.transform.position - that.transform.position).sqrMagnitude;
                    if(distance < minDis)
                    {
                        minDis = distance;
                        nearEnemy = that;
                    }
                }
            }
            if(nearEnemy != null)
            {
                Vector3 dir = (nearEnemy.transform.position - player.transform.position).normalized;
                Quaternion targetRota = Quaternion.LookRotation(dir);

                AudioManager.Instance.PlaySFX(ESFXType.Active_LightningRay);
                particleManager.GetParticle(3, player.transform.position, targetRota, ad.attackDamage, ad.distance, ad.attackSpeed);
            }
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
    }
}
