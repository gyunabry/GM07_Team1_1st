using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackFireCircle", menuName = "ScriptableObjects/Skills/AttackFireCircle")]
public class AttackFireCircle : SkillBase
{
    public override IEnumerator RunSkill(Player player, AttackData ad, PoolManager poolManager, ParticleManager particleManager, LayerMask layer, IAttack attack)
    {
        ad.position = player.transform.position;

        AudioManager.Instance.PlaySFX(ESFXType.Active_FireCircle);
        attack.FireCircle(ad.attackDamage, ad, poolManager, layer);
        particleManager.GetParticle(1, player.transform.position, player.transform.rotation, 0, ad.distance, ad.attackSpeed);

        yield return null;
    }
}
