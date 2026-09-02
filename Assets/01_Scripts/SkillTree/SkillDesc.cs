using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillDesc : MonoBehaviour
{
    [SerializeField] GameObject descDamage;
    [SerializeField] GameObject descAttackSpeed;
    [SerializeField] GameObject descDistance;
    [SerializeField] GameObject descProjectile;
    [SerializeField] PoolManager poolManager;
    public void SetDamage(float damage)
    {
        TextMeshProUGUI text = descDamage.GetComponent<TextMeshProUGUI>();
        text.text = $"공격력 : {damage}";
    }
    public void SetSpeed(float attackSpeed)
    {
        TextMeshProUGUI text = descAttackSpeed.GetComponent<TextMeshProUGUI>();
        text.text = $"공격 속도 : {attackSpeed}";
    }
    public void SetDistance(float distance)
    {
        TextMeshProUGUI text = descDistance.GetComponent<TextMeshProUGUI>();
        text.text = $"공격 범위 : {distance}";
    }
    public void SetProjectile(int projectile)
    {
        TextMeshProUGUI text = descProjectile.GetComponent<TextMeshProUGUI>();
        text.text = $"투사체 개수 : {projectile}";
    }
}
