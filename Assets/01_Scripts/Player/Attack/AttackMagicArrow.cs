using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackMagicArrow", menuName = "ScriptableObjects/Skills/AttackMagicArrow")]
public class AttackMagicArrow : SkillBase
{
    public override IEnumerator RunSkill(Player player, AttackData ad, PoolManager poolManager, ParticleManager particleManager, LayerMask layer, IAttack attack)
    {
        for(int i = 0; i < ad.projectileCount; i++)
        {
            Collider[] enemyIn = Physics.OverlapSphere(player.transform.position, ad.distance, layer);
            if (enemyIn == null || enemyIn.Length == 0) yield break;
            if (enemyIn.Length > 0)
            {
                int index = Random.Range(0, enemyIn.Length);
                Collider target = enemyIn[index];
                AudioManager.Instance.PlaySFX(ESFXType.Active_MagicArrow);
                attack.MagicArrow(ad.attackDamage, ad, poolManager, layer, target);

                if (i == ad.projectileCount - 1) break;
                yield return new WaitForSeconds(0.1f);

            }
        }
    }
}
