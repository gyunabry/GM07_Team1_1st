using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanelController : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("참조")]
    [SerializeField] private Player player;
    [SerializeField] private SkillTreeManager skillTreeManager;
    [SerializeField] private EmployeeManager employeeManager;

    [SerializeField] private BuildingDataSO productionBuilding;
    [SerializeField] private BuildingDataSO salesBuilding;
    [SerializeField] private BuildingDataSO hunterBuilding;
    [SerializeField] private BuildingDataSO carrierBuilding;

    [Header("캐릭터 스탯")]
    [SerializeField] private TMP_Text attackPower;
    [SerializeField] private TMP_Text attackSpeed;
    [SerializeField] private TMP_Text attackRange;
    [SerializeField] private TMP_Text moveSpeed;
    [SerializeField] private TMP_Text magnetRange;
    [SerializeField] private TMP_Text inventoryCapacity;

    [Header("시설 한도")]
    [SerializeField] private TMP_Text maxProductionCount;
    [SerializeField] private TMP_Text maxSalesCounterCount;

    [Header("직원 고용 한도")]
    [SerializeField] private TMP_Text maxHunterCount;
    [SerializeField] private TMP_Text maxCarrierCount;

    private PlayerInventory playerInventory;
    private PlayerItemCollector playerItemCollector;

    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (player != null)
        {
            playerInventory = player.GetComponent<PlayerInventory>();
            playerItemCollector = player.GetComponentInChildren<PlayerItemCollector>();
        }
        SetVisible(false);
    }

    private void Start()
    {
        RefreshAll();
    }

    private void OnEnable()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.LevelUp += HandleLevelUp;
        }

        if (skillTreeManager != null) 
        {
            // skillTreeManager.SkillEffectsRefreshed += HandleSkillEffectsRefreshed;
        }

        if (playerInventory?.Inventory != null)
        {
            playerInventory.Inventory.InventoryChanged += HandleInventoryChanged;
        }

        if (FacilityManager.Instance != null)
        {
            FacilityManager.Instance.FacilityInfoChanged += HandleFacilityInfoChanged;
        }
    }

    

    private void OnDisable()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.LevelUp -= HandleLevelUp;
        }

        if (skillTreeManager != null)
        {
            // skillTreeManager.SkillEffectsRefreshed -= HandleSkillEffectsRefreshed;
        }

        if (playerInventory?.Inventory != null)
        {
            playerInventory.Inventory.InventoryChanged -= HandleInventoryChanged;
        }

        if (FacilityManager.Instance != null)
        {
            FacilityManager.Instance.FacilityInfoChanged -= HandleFacilityInfoChanged;
        }
    }
    
    public void ToggleUI()
    {
        bool nextVisible = !isOpen;
        SetVisible(nextVisible);

        if (nextVisible)
        {
            RefreshAll();
        }
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        isOpen = visible;

        if (canvas != null) canvas.enabled = visible;
        if (graphicRaycaster != null) graphicRaycaster.enabled = visible;
    }

    public void RefreshAll()
    {
        RefreshCharacterStats();
        RefreshInventoryStat();
        RefreshFacilityInfoStats();
    }

    private void RefreshCharacterStats()
    {
        if (player == null) return;

        if (attackPower != null) attackPower.text = $"{player.attackDamage + player.baseAttackDamage}";
        if (attackSpeed != null) attackSpeed.text = $"{player.attackSpeed + player.baseAttackSpeed}";
        if (attackRange != null) attackRange.text = $"{player.attackDistance + player.baseAttackDistance}";
        if (moveSpeed != null) moveSpeed.text = $"{player.moveSpeed + player.navMeshAgent.speed}";
        if (magnetRange != null) magnetRange.text = $"{playerItemCollector.Range:0.##}";
    }

    private void RefreshInventoryStat()
    {
        if (inventoryCapacity == null) return;

        inventoryCapacity.text = $"{playerInventory.Inventory.Capacity}";
    }

    private void RefreshFacilityInfoStats()
    {
        if (FacilityManager.Instance == null) return;

        if (maxProductionCount != null)
        {
            int current = FacilityManager.Instance.GetPlacedCount(productionBuilding);
            int limit = FacilityManager.Instance.GetPlacementLimit(productionBuilding);

            maxProductionCount.text = $"{current} / {limit}";
        }

        if (maxSalesCounterCount != null)
        {
            int current = FacilityManager.Instance.GetPlacedCount(salesBuilding);
            int limit = FacilityManager.Instance.GetPlacementLimit(salesBuilding);

            maxSalesCounterCount.text = $"{current} / {limit}";
        }

        if (maxHunterCount != null)
        {
            int current = employeeManager.HunterEmployeeCount;
            int limit = employeeManager.GetHiringLimit(EmployeeRole.Hunter);

            maxHunterCount.text = $"{current} / {limit}";
        }

        if (maxCarrierCount != null)
        {
            int current = employeeManager.CarrierEmployeeCount;
            int limit = employeeManager.GetHiringLimit(EmployeeRole.Carrier);

            maxCarrierCount.text = $"{current} / {limit}";
        }
    }

    private void HandleLevelUp()
    {
        RefreshAll();
    }

    // 스킬트리에서 스킬을 찍었을 때 실행할 메서드
    private void HandleSkillEffectsRefreshed()
    {
        RefreshAll();
    }

    private void HandleInventoryChanged()
    {
        RefreshInventoryStat();
    }

    private void HandleFacilityInfoChanged(BuildingDataSO data)
    {
        RefreshFacilityInfoStats();
    }
}
