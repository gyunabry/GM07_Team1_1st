using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackFlowerThorns", menuName = "ScriptableObjects/Skills/AttackFlowerThorns")]
public class AttackFlowerThorns : SkillBase
{
    public override IEnumerator RunSkill(Player player, AttackData ad, PoolManager poolManager, ParticleManager particleManager, LayerMask layer, IAttack attack)
    {
        for (int i = 0; i < ad.projectileCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * ad.distance;
            Vector3 randomPosi = new Vector3(player.transform.position.x + randomCircle.x,
                player.transform.position.y, player.transform.position.z + randomCircle.y);

            ad.position = randomPosi;
            AudioManager.Instance.PlaySFX(ESFXType.Active_FlowerThorns);
            attack.FlowerThorns(ad.attackDamage, ad, poolManager, layer);
            particleManager.GetParticle(4, randomPosi, player.transform.rotation, 0, ad.distance, ad.attackSpeed);
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
    }
}
