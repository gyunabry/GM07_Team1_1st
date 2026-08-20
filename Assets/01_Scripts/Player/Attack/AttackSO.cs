using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "ScriptableObjects/AttackSO")]
public class AttackSO : ScriptableObject
{
    public string attackID;
    public float attackDamage = 5;
    public float attackSpeed = 0.5f;
    public Vector3 position;
    public Vector3 direction;
    public float distance;
    public int projectileCount = 1;
    public float spreadAngle = 0f;
    public Sprite sprite;
}
public class AttackUnlockData
{
    public string attackID;
    public bool unlock;
    public bool equip;
    public Sprite sprite;
}