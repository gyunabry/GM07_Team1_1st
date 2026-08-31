using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackMagicArrow", menuName = "ScriptableObjects/Skills/AttackMagicArrow")]
public class AttackMagicArrow : SkillBase
{
    public override IEnumerator RunSkill(Player player, AttackData ad, PoolManager poolManager, ParticleManager particleManager, LayerMask layer, IAttack attack)
    {
        Collider[] enemyIn = Physics.OverlapSphere(player.transform.position, ad.distance, layer);
        if (enemyIn == null || enemyIn.Length == 0) yield break;

        ad.direction = (enemyIn[0].transform.position - player.transform.position).normalized;

        for(int i = 0; i < ad.projectileCount; i++)
        {
            if(enemyIn.Length == 0)
            {
                enemyIn = Physics.OverlapSphere(player.transform.position, ad.distance, layer);
            }

            if(enemyIn.Length > 0)
            {
                AudioManager.Instance.PlaySFX(ESFXType.Active_MagicArrow);
                attack.MagicArrow(ad.attackDamage, ad, poolManager, layer);

                if (i == ad.projectileCount - 1) break;
                yield return new WaitForSeconds(0.1f);

            }
        }
    }
}
