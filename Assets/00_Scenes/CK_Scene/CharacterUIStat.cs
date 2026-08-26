using TMPro;
using UnityEngine;

public class CharacterUIStat : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private CurrencySystem currencySystem;

    [Header("Ω∫≈» ≈ÿΩ∫∆Æ")]
    [SerializeField] TextMeshProUGUI text1;
    [SerializeField] TextMeshProUGUI text2;
    [SerializeField] TextMeshProUGUI text3;
    [SerializeField] TextMeshProUGUI text4;

    private void OnEnable()
    {
        currencySystem.LevelUp += Instance_LevelUp;
    }
    private void OnDisable()
    {
        currencySystem.LevelUp -= Instance_LevelUp;
    }

    private void Instance_LevelUp()
    {
        text1.text = $"{ player.attackDamage + player.baseAttackDamage}";
        text2.text = $"{ player.attackSpeed + player.baseAttackSpeed}";
        text3.text = $"{ player.attackDistance + player.baseAttackDistance}";
        text4.text = $"{ player.moveSpeed + player.navMeshAgent.speed}";
    }
}