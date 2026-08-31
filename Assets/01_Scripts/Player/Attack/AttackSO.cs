using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "ScriptableObjects/AttackSO")]
public class AttackSO : ScriptableObject
{
    public string attackID;
    public float attackDamage;
    public float attackSpeed;
    public Vector3 position;
    public Vector3 direction;
    public float distance;
    public int projectileCount;
    public float spreadAngle;
    public Sprite sprite;
    public SkillBase skillBase;
    public AttackData CalculateAttackData(Player player, PlayerAttackUpgrade upgrade)
    {
        AttackData ad = new AttackData();
        ad.position = player.transform.position;
        ad.forward = player.transform.forward;

        ad.attackDamage = (player.attackDamage + player.baseAttackDamage) * (attackDamage + upgrade.damage);
        ad.attackSpeed = (attackSpeed + upgrade.attackSpeed + player.baseAttackSpeed) + player.attackSpeed;
        ad.distance = distance + upgrade.distance + player.baseAttackDistance + player.attackDistance;
        ad.projectileCount = projectileCount + upgrade.projectileCount;
        ad.spreadAngle = spreadAngle;
        return ad;
    }
}
public class AttackUnlockData
{
    public string attackID;
    public bool unlock;
    public bool equip;
    public Sprite sprite;
}