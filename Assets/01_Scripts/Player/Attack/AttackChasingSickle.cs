using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackChasingSickle", menuName = "ScriptableObjects/Skills/AttackChasingSickle")]
public class AttackChasingSickle : SkillBase
{
    public override IEnumerator RunSkill(Player player, AttackData ad, PoolManager poolManager, ParticleManager particleManager, LayerMask layer, IAttack attack)
    {
        ad.position = player.transform.position;
        ad.forward = player.transform.forward;

        AudioManager.Instance.PlaySFX(ESFXType.Active_ChasingSickle);
        attack.ChasingSickle(ad.attackDamage, ad, poolManager, layer);
        particleManager.GetParticle(2, player.transform.position, new Quaternion(player.transform.rotation.x, player.transform.rotation.y + 0.4f, player.transform.rotation.z, player.transform.rotation.w) , 0, ad.distance, ad.attackSpeed);

        yield return null;
    }
}
