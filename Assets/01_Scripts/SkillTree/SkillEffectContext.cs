using UnityEngine;

[System.Serializable]
public class SkillEffectContext //스킬트리에서 값을 받아올 클래스들 여기에 넣기
{
    public Player player;
    public CurrencySystem currencySystem;
    public PlayerAttack playerAttack;
    public ProductionMachine productionMachine;
    public EconomyModifierService economyModifierService;
    public EmployeeManager employeeManager;
    public CustomerSpawnManager customerSpawnManager;
    public ItemInventory itemInventory;
    // public ProductionSkillRegistry productionSkillRegistry;
}
