using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "ScriptableObjects/AttackSO")]
public class AttackSO : ScriptableObject
{
    public int attackCode;
    public float attackDamage = 5;
    public float attackSpeed = 0.5f;
    public Vector3 position;
    public Vector3 direction;
    public float distance;
    public int projectileCount = 1;
    public float spreadAngle = 0f;
}
