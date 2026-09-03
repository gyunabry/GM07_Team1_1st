using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class SaveGameService : MonoBehaviour
{
    private const int SupportedSchemaVersion = 1;
    private const string SalesCounterInventoryId = "salesCouunter";

    [Header("데이터베이스")]
    [SerializeField] private BuildingDatabaseSO buildingDatabase;
    [SerializeField] private ItemDatabaseSO itemDatabase;
    [SerializeField] private RecipeDatabaseSO recipeDatabase;

    [Header("플레이어")]
    [SerializeField] private CurrencySystem currencySystem;
    [SerializeField] private Player player;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private SkillTreeManager skillTreeManager;
    [SerializeField] private RecipeUnlockManager recipeUnlockManager;

    [Header("게임 월드")]
    [SerializeField] private WorkshopExpansionManager expansionManager;
    [SerializeField] private PlacementSystem placementSystem;
    [SerializeField] private EmployeeManager employeeManager;
    [SerializeField] private CounterInventory counterInventory;
    [SerializeField] private HuntingFieldManager huntingFieldManager;
    [Tooltip("텔레포트 UI 참조 연결 복구")]
    [SerializeField] private TeleportUI teleportUI;

    private JsonSaveFileStore fileStore;

    private class PendingProductionRestore
    {
        public ProductionBuilding building;
        public ProductionSaveData savedData;
    }

    private class PendingEmployeeRestore
    {
        public PlacedBuilding building;
        public EmployeeRuntimeData employee;
        public EmployeeSaveData saved;
    }

    private void Awake()
    {
        fileStore = new JsonSaveFileStore();
    }

    private void Start()
    {
        if (!SaveLoadRequest.TryConsumeSaveData(out SaveGameData saveData))
        {
            Debug.Log("새 게임 상태로 게임을 시작합니다.");
            return;
        }

        if (!ApplySaveData(saveData))
        {
            Debug.LogError("전달받은 저장 데이터 적용에 실패했습니다.");
            return;
        }

        Debug.Log("저장된 게임 상태로 게임을 시작합니다.");
    }

    #region 저장 서비스
    public bool SaveGame()
    {
        if (fileStore == null)
        {
            fileStore = new JsonSaveFileStore();
        }

        if (!ValidateRuntimeReferences())
        {
            Debug.LogError("저장에 필요한 시스템 참조가 연결되지 않았습니다.");
            return false;
        }

        if (!TryCreateSaveData(out SaveGameData saveData))
        {
            Debug.LogError("저장 데이터를 만들지 못했습니다.");
            return false;
        }

        bool success = fileStore.Save(saveData);

        if (success)
        {
            Debug.Log($"게임 저장 완료: {fileStore.SavePath}");
        }

        return success;
    }

    public bool LoadGame()
    {
        if (fileStore == null)
        {
            fileStore = new JsonSaveFileStore();
        }

        if (!ValidateRuntimeReferences())
        {
            Debug.LogError("불러오기에 필요한 시스템 참조가 연결되지 않았습니다.");
            return false;
        }

        if (!fileStore.TryLoad(out SaveGameData saveData))
        {
            Debug.LogError("저장 데이터를 불러오지 못했습니다.");
            return false;
        }

        return ApplySaveData(saveData);
    }

    public bool ApplySaveData(SaveGameData data)
    {
        if (!ValidateRuntimeReferences())
        {
            Debug.LogError("저장 데이터 검증에 실패했습니다.");
            return false;
        }

        Dictionary<string, PlacedBuilding> buildingsByGuid = new();
        List<PendingEmployeeRestore> pendingEmployees = new();

        bool allSuccessed = true;

        // 플레이어 상태 복구
        currencySystem.RestoreState(
            data.player.money,
            data.player.experience,
            data.player.currentExperience,
            data.player.level);

        player.RestoreProgress(
            data.player.level,
            data.player.skillPoints);

        if (!RestoreInventory(
            data.player.inventory,
            playerInventory.Inventory))
        {
            allSuccessed = false;
        }

        // 사냥터 복구
        List<string> savedHuntingFieldIds = data.progression != null
            ? data.progression.unlockedHuntingFieldIds
            : null;

        huntingFieldManager.RestoreUnlockedIds(savedHuntingFieldIds);

        // 확장 복구
        placementSystem.ClearBuildingsOnLoad();

        if (!expansionManager.RestoreStage(data.world.workshopExpansionStage))
        {
            Debug.LogError($"공방 확장 단계 복구 실패: {data.world.workshopExpansionStage}");
            return false;
        }

        // 판매대 인벤토리 복구
        if (!RestoreSharedInventory(data.world.sharedInventories))
        {
            allSuccessed = false;
        }

        List<PendingProductionRestore> pendingProductions = new();

        // 시설 복구
        if (data.world.facilities != null)
        {
            foreach (FacilitySaveData savedFacility in data.world.facilities)
            {
                if (savedFacility == null)
                {
                    allSuccessed = false;
                    continue;
                }

                if (!buildingDatabase.TryGetById(savedFacility.buildingId, out BuildingDataSO buildingData))
                {
                    Debug.LogWarning($"시설 데이터를 찾지 못했습니다: {savedFacility.buildingId}");
                    allSuccessed = false;
                    continue;
                }

                if (!placementSystem.TryGetBuildableArea(savedFacility.areaId, out BuildableArea area))
                {
                    Debug.LogWarning($"배치 영역을 찾지 못했습니다: {savedFacility.areaId}");
                    allSuccessed = false;
                    continue;
                }

                if (!placementSystem.TryRestoreBuilding(savedFacility, buildingData, area, out PlacedBuilding restoredBuilding))
                {
                    Debug.LogWarning($"시설 복구에 실패했습니다: {savedFacility.buildingId}, GUID: {savedFacility.guid}");
                    allSuccessed = false;
                    continue;
                }

                if (!RestoreBuildingInventories(savedFacility, restoredBuilding))
                {
                    allSuccessed = false;
                }

                if (savedFacility.production != null 
                    && restoredBuilding.TryGetComponent(out ProductionBuilding productionBuilding))
                {
                    pendingProductions.Add(new PendingProductionRestore
                    {
                        building = productionBuilding,
                        savedData = savedFacility.production
                    });
                }

                // 직원 복구
                buildingsByGuid[restoredBuilding.PersistentId] = restoredBuilding;

                if (!RestoreEmployeeEntries(restoredBuilding, savedFacility, pendingEmployees))
                {
                    allSuccessed = false;
                }
            }
        }

        if (!ReconnectRuntimePortal())
        {
            allSuccessed = false;
        }

        // 스킬 복구
        skillTreeManager.RestoreLevels(data.progression.skills);

        // 스킬 효과가 반영된 생산시간으로 생산 진행도 복구
        foreach (PendingProductionRestore pending in pendingProductions)
        {
            if (!RestoreProduction(pending))
            {
                allSuccessed = false;
            }
        }

        // 장착한 공격 스킬 복구
        playerAttack.RestoreEquippedAttacks(data.player.equippedAttacks);

        if (allSuccessed)
        {
            Debug.Log("게임 데이터 복구 완료");
        }
        else
        {
            Debug.LogWarning("일부 데이터의 복구에 실패했습니다.");
        }

        StartCoroutine(RestoreEmployeeState(pendingEmployees, buildingsByGuid));

        return allSuccessed;
    }
    #endregion

    private bool TryCreateSaveData(out SaveGameData data)
    {
        data = new SaveGameData
        {
            schemaVersion = SupportedSchemaVersion
        };

        // 플레이어 데이터 저장
        data.player.level = currencySystem.Level;
        data.player.experience = currencySystem.Experience;
        data.player.currentExperience = currencySystem.CurrentExperience;
        data.player.skillPoints = player.skillPoint;
        data.player.money = currencySystem.Money;
        data.player.inventory = CaptureInventory(playerInventory.Inventory);
        data.player.equippedAttacks = CaptureEquippedAttacks();

        // 스킬 데이터 저장
        data.progression.skills.Clear();

        foreach (SkillRuntimeState runtimeState in skillTreeManager.RuntimeState)
        {
            if (runtimeState == null || string.IsNullOrWhiteSpace(runtimeState.skillID) || runtimeState.skillLevel <= 0)
            {
                continue;
            }

            data.progression.skills.Add(new SkillLevelSaveData
            {
                skillId = runtimeState.skillID,
                level = runtimeState.skillLevel
            });
        }

        // 해금 레시피 저장
        data.progression.unlockedRecipeIds.Clear();

        foreach (RecipeDataSO recipe in recipeUnlockManager.UnlockedRecipes)
        {
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.RecipeId))
            {
                continue;
            }

            data.progression.unlockedRecipeIds.Add(recipe.RecipeId);
        }

        // 월드 진행 데이터 저장
        data.world.workshopExpansionStage = expansionManager.CurrentStage;

        data.progression.unlockedHuntingFieldIds.Clear();
        data.progression.unlockedHuntingFieldIds = huntingFieldManager.CaptureUnlockedIds();

        // 판매대 인벤토리 저장
        data.world.sharedInventories.Add(new NamedInventorySaveData
        {
            inventoryId = SalesCounterInventoryId,
            inventory = CaptureInventory(counterInventory.Inventory)
        });

        // 시설 데이터 저장
        data.world.facilities.Clear();

        PlacedBuilding[] buildings = placementSystem.GetPlacedBuildings();

        foreach (PlacedBuilding building in buildings)
        {
            if (!TryCaptureFacility(building, out FacilitySaveData facilityData))
            {
                data = null;
                return false;
            }

            data.world.facilities.Add(facilityData);
        }

        return true;
    }

    #region 인벤토리 저장/복구
    private InventorySaveData CaptureInventory(ItemInventory inventory)
    {
        InventorySaveData result = new();

        if (inventory == null)
        {
            Debug.LogWarning("저장할 인벤토리가 null입니다.");
            return result;
        }

        foreach (InventoryEntry entry in inventory.Entries)
        {
            if (entry == null || entry.IsEmpty || entry.Item == null || entry.Amount <= 0)
            {
                continue;
            }

            string itemId = entry.Item.ItemId;

            if (string.IsNullOrWhiteSpace(itemId))
            {
                Debug.LogWarning($"ItemId가 누락되었습니다 : {entry.Item.name}");
                continue;
            }

            result.items.Add(new ItemStackSaveData
            {
                itemId = itemId,
                amount = entry.Amount
            });
        }

        return result;
    }

    private bool RestoreInventory(InventorySaveData saved, ItemInventory target)
    {
        if (target == null)
        {
            Debug.LogWarning("복구 대상 인벤토리가 null입니다.");
            return false;
        }

        if (itemDatabase == null)
        {
            Debug.LogWarning("SaveGameService에 ItemDatabase가 연결되지 않았습니다.");
            return false;
        }

        if (saved == null)
        {
            Debug.LogWarning("복구할 InventorySaveData가 null입니다.");
            return false;
        }

        List<ItemAmount> restoredItems = new();

        if (saved.items != null)
        {
            foreach (ItemStackSaveData savedItem in saved.items)
            {
                if (savedItem == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(savedItem.itemId))
                {
                    Debug.LogWarning("itemId가 비어있는 저장 항목을 건너뜁니다");
                    continue;
                }

                if (savedItem.amount <= 0)
                {
                    Debug.LogWarning($"수량 부족으로 해당 항목을 건너뜁니다. ID: {savedItem.itemId}, 수량 : {savedItem.amount}");
                    continue;
                }

                if (!itemDatabase.TryGetById(savedItem.itemId, out ItemDataSO itemData))
                {
                    Debug.LogWarning($"ItemDatabase에서 아이템을 찾지 못 했습니다. {savedItem.itemId}");
                    continue;
                }

                restoredItems.Add(new ItemAmount(itemData, savedItem.amount));
            }
        }

        target.RestoreInventory(restoredItems);

        return true;
    }
    #endregion

    #region 스킬 저장
    private List<EquippedAttackSaveData> CaptureEquippedAttacks()
    {
        List<EquippedAttackSaveData> result = new();

        if (playerAttack == null || playerAttack.slots == null)
        {
            return result;
        }

        for (int i = 0; i < playerAttack.slots.Length; i++)
        {
            AttackSlotData slot = playerAttack.slots[i];

            if (slot == null || string.IsNullOrWhiteSpace(slot.equipAttackID))
            {
                continue;
            }

            result.Add(new EquippedAttackSaveData
            {
                slotIndex = (byte)i,
                attackId = slot.equipAttackID,
            });
        }

        return result;
    }
    #endregion

    #region 시설 데이터 저장
    private bool TryCaptureFacility(PlacedBuilding building, out FacilitySaveData result)
    {
        result = null;

        if (building == null || building.Data == null || building.AssignedArea == null)
        {
            Debug.LogWarning("시설의 배치 데이터가 올바르지 않습니다.", building);
            return false;
        }

        if (!Guid.TryParseExact(building.PersistentId, "N", out _))
        {
            Debug.LogWarning($"시설의 GUID가 올바르지 않습니다: {building.BuildingName}", building);
            return false;
        }

        if (string.IsNullOrWhiteSpace(building.Data.BuildingId))
        {
            Debug.LogWarning($"시설 ID가 비어있습니다: {building.BuildingName}", building);
            return false;
        }

        if (string.IsNullOrWhiteSpace(building.AssignedArea.AreaId))
        {
            Debug.LogWarning($"시설의 Area ID가 비어있습니다: {building.BuildingName}", building);
            return false;
        }

        if (building.RotationIndex < 0 || building.RotationIndex > 3)
        {
            Debug.LogWarning($"시설의 회전 인덱스가 올바르지 않습니다: " +
                $"{building.BuildingName} {building.RotationIndex}", building);
            return false;
        }

        result = new FacilitySaveData
        {
            guid = building.PersistentId,
            buildingId = building.Data.BuildingId,
            areaId = building.AssignedArea.AreaId,

            originCellX = building.OriginCell.x,
            originCellY = building.OriginCell.y,

            rotationIndex = building.RotationIndex,

            placementSource = building.PlacementSource,

            buildingState = building.State,

            constructionProgress01 = Mathf.Clamp01(building.ConstructionProgress),

            inventories = new List<BuildingInventorySaveData>()
        };

        CaptureBuildingInventories(building, result);
        CaptureProductionState(building, result);

        if (!CaptureEmployees(building, result))
        {
            return false;
        }

        return true;
    }

    // 시설 인벤토리 저장
    private void CaptureBuildingInventories(PlacedBuilding building, FacilitySaveData target)
    {
        // 생산 시설
        if (building.TryGetComponent(out ProductionBuilding production))
        {
            AddBuildingInventory(target, SavedInventorySlot.Input, production.InputInventory);
            AddBuildingInventory(target, SavedInventorySlot.Output, production.OutputInventory);

            return;
        }

        // 일반 전송기
        if (building.TryGetComponent(out Transmitter transmitter))
        {
            AddBuildingInventory(target, SavedInventorySlot.Storage, transmitter.Inventory);

            return;
        }

        // 통합 전송기
        if (building.TryGetComponent(out IntegratedTransmitter integratedTransmitter))
        {
            AddBuildingInventory(target, SavedInventorySlot.Storage, integratedTransmitter.Inventory);

            return;
        }
    }

    private void AddBuildingInventory(FacilitySaveData target, SavedInventorySlot slot, ItemInventory inventory)
    {
        target.inventories.Add(new BuildingInventorySaveData
        {
            slot = slot,
            inventory = CaptureInventory(inventory)
        });
    }

    private void CaptureProductionState(PlacedBuilding building, FacilitySaveData target) 
    {
        if (!building.TryGetComponent(out ProductionBuilding production))
        {
            target.production = null;
            return;
        }

        target.production = new ProductionSaveData
        {
            selectedRecipeId = production.SelectedRecipe != null ? production.SelectedRecipe.RecipeId : null,
            activeRecipeId = production.ActiveRecipe != null ? production.ActiveRecipe.RecipeId : null,
            activeProgress01 = Mathf.Clamp01(production.Progress),
            pendingOutputAmount = Mathf.Max(0, production.PendingOutputAmount)
        };
    }

    // 포탈 재연결 메서드
    private bool ReconnectRuntimePortal()
    {
        if (placementSystem == null)
        {
            Debug.LogWarning("Placement System이 없습니다.");
            return false;
        }

        if (teleportUI == null)
        {
            Debug.LogWarning("Teleport UI가 연결되지 않았습니다.");
            return false;
        }

        PlacedBuilding[] buildings = placementSystem.GetPlacedBuildings();

        Portal workshopPortal = null;
        List<Portal> fieldPortals = new();

        foreach (PlacedBuilding building in buildings)
        {
            if (building == null || building.AssignedArea == null)
            {
                continue;
            }

            if (!building.TryGetComponent(out Portal portal))
            {
                continue;
            }

            switch (building.AssignedArea.AreaType)
            {
                case AreaType.Workshop:
                    workshopPortal = portal;
                    break;

                case AreaType.HuntingField:
                    fieldPortals.Add(portal);
                    break;
            }
        }

        if (workshopPortal == null)
        {
            Debug.LogWarning("복원된 공방 포탈을 찾지 못했습니다.");
            return false;
        }

        if (fieldPortals.Count == 0)
        {
            Debug.LogWarning("복원된 사냥터 포탈을 찾지 못했습니다.");
            return false;
        }

        Portal[] destination = fieldPortals.ToArray();

        workshopPortal.SetWorkshopPortal(teleportUI, destination, playerAttack);

        foreach (Portal fieldPortal in fieldPortals)
        {
            fieldPortal.SetHuntingFieldPortal(workshopPortal, playerAttack);
        }

        Debug.Log($"포탈 연결 완료. 공방: {workshopPortal.name}, 사냥터: {fieldPortals.Count}");

        return true;
    }
    #endregion

    #region 시설 인벤토리 복구
    private bool RestoreBuildingInventories(FacilitySaveData saved, PlacedBuilding building)
    {
        bool success = true;

        if (building.TryGetComponent(out ProductionBuilding production))
        {
            success &= RestoreInventory(
                FindBuildingInventory(saved, SavedInventorySlot.Input), 
                production.InputInventory);
            success &= RestoreInventory(
                FindBuildingInventory(saved, SavedInventorySlot.Output),
                production.OutputInventory);

            return success;
        }

        ItemInventory storageInventory = null;

        if (building.TryGetComponent(out Transmitter transmitter))
        {
            storageInventory = transmitter.Inventory;
        }
        else if (building.TryGetComponent(out IntegratedTransmitter integratedTransmitter))
        {
            storageInventory = integratedTransmitter.Inventory;
        }
        
        if (storageInventory == null)
        {
            return true;
        }

        return RestoreInventory(
            FindBuildingInventory(saved, SavedInventorySlot.Storage),
            storageInventory);
    }

    private InventorySaveData FindBuildingInventory(FacilitySaveData saved, SavedInventorySlot slot)
    {
        if (saved?.inventories != null)
        {
            foreach (BuildingInventorySaveData inventory in saved.inventories)
            {
                if (inventory != null && inventory.slot == slot)
                {
                    return inventory.inventory ?? new InventorySaveData();
                }
            }
        }

        return new InventorySaveData();
    }
    #endregion

    #region 공유 인벤토리 복구
    private bool RestoreSharedInventory(IReadOnlyList<NamedInventorySaveData> savedInventories)
    {
        InventorySaveData salesInventoryData = new();

        if (savedInventories != null)
        {
            foreach (NamedInventorySaveData savedInventory in savedInventories)
            {
                if (savedInventory == null) continue;

                if (string.Equals(savedInventory.inventoryId, SalesCounterInventoryId, StringComparison.Ordinal))
                {
                    salesInventoryData = savedInventory.inventory;
                    break;
                }
            }
        }

        if (salesInventoryData == null)
        {
            Debug.LogWarning("판매대 인벤토리 저장 데이터가 없습니다.");
            salesInventoryData = new InventorySaveData();
        }

        return RestoreInventory(salesInventoryData, counterInventory.Inventory);
    }
    #endregion

    #region 생산 상태 복구
    private bool RestoreProduction(PendingProductionRestore pending)
    {
        if (pending == null || pending.building == null || pending.savedData == null)
        {
            return false;
        }

        if (!TryRestoreRecipe(
            pending.savedData.selectedRecipeId,
            out RecipeDataSO selectedRecipe))
        {
            return false;
        }

        if (!TryRestoreRecipe(
            pending.savedData.activeRecipeId,
            out RecipeDataSO activeRecipeId))
        {
            return false;
        }

        // 현재 시설이 해당 레시피를 처리할 수 있는지 검사
        if (selectedRecipe != null && !pending.building.CanProcess(selectedRecipe))
        {
            Debug.LogWarning($"시설이 현재 레시피를 처리할 수 없습니다: {selectedRecipe.RecipeId}", pending.building);
            return false;
        }

        pending.building.RestoreProduction(
            selectedRecipe,
            activeRecipeId,
            pending.savedData.activeProgress01,
            pending.savedData.pendingOutputAmount);

        return true;
    }

    private bool TryRestoreRecipe(string recipeId, out RecipeDataSO result)
    {
        result = null;

        // 레시피 ID가 null이라면 레시피가 선택되지 않은 상태
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            return true;
        }

        if (recipeDatabase.TryGetById(recipeId, out result))
        {
            return true;
        }

        Debug.LogWarning($"생산 레시피를 찾지 못했습니다: {recipeId}");

        return false;
    }
    #endregion

    #region 직원 상태 저장/복구
    private bool CaptureEmployees(PlacedBuilding building, FacilitySaveData target)
    {
        target.employees.Clear();

        if (building.Data.BuildingTag != BuildingTag.Employee)
        {
            return true;
        }

        if (!employeeManager.TryGetEmployees(building, out var employess))
        {
            return true;
        }

        building.TryGetComponent(out HunterBuildingController hunterController);
        building.TryGetComponent(out CarrierEmployeeBuildingController carrierController);

        foreach (EmployeeRuntimeData employee in employess)
        {
            EmployeeSaveData saved = new();

            if (hunterController != null)
            {
                if (!hunterController.TryGetWorker(employee.EmployeeId, out HunterWorker worker))
                {
                    return false;
                }

                // 사냥 직원 인벤토리 정보 저장
                saved.cargo = worker.CaptureCargo();
            }
            else if (carrierController != null)
            {
                if (!carrierController.TryGetWorker(employee.EmployeeId, out CarrierWorker worker))
                {
                    return false;
                }

                // 운반 직원 인벤토리 정보 저장
                saved.cargo = CaptureInventory(worker.CargoInventory);

                // 운반 직원이 배정된 상태이고 현재 명령이 유효할 때
                if (worker.HasCommand && worker.CurrentCommand.IsValid)
                {
                    CarrierCommand command = worker.CurrentCommand;
                    PlacedBuilding targetBuilding = command.TargetBuilding.GetComponent<PlacedBuilding>();

                    // 운반 직원에게 내려진 명령 저장
                    saved.command = new CarrierCommandSaveData
                    {
                        commandType = command.Type,
                        targetBuildingGuid = targetBuilding.PersistentId,
                        assignedRecipeId = command.AssignedRecipe.RecipeId
                    };
                }
            }

            // 타겟 시설에 직원 데이터 추가
            target.employees.Add(saved);
        }

        return true;
    }

    private bool RestoreEmployeeEntries(
        PlacedBuilding building, 
        FacilitySaveData savedFacility, 
        List<PendingEmployeeRestore> pending)
    {
        if (building.Data.BuildingTag != BuildingTag.Employee || !building.IsComplete)
        {
            return true;
        }

        if (!employeeManager.TryRegisterBuildingOnLoad(building))
        {
            return false;
        }

        if (savedFacility.employees == null)
        {
            return true;
        }

        bool success = true;

        foreach (EmployeeSaveData savedEmployee in savedFacility.employees)
        {
            if (savedEmployee == null ||
                !employeeManager.TryRestoreEmployee(building, out EmployeeRuntimeData employee))
            {
                success = false;
                continue;
            }

            pending.Add(new PendingEmployeeRestore
            {
                building = building,
                employee = employee,
                saved = savedEmployee
            });
        }

        return success;
    }

    private IEnumerator RestoreEmployeeState(
        List<PendingEmployeeRestore> pending, 
        Dictionary<string, PlacedBuilding> buildingsByGuid)
    {
        yield return null;

        foreach (PendingEmployeeRestore entry in pending)
        {
            if (entry == null ||
                entry.building == null ||
                entry.employee == null ||
                entry.saved == null)
            {
                continue;
            }

            if (entry.building.TryGetComponent(out HunterBuildingController hunterController))
            {
                if (hunterController.TryGetWorker(entry.employee.EmployeeId, out HunterWorker hunter))
                {
                    hunter.RestoreCargo(entry.saved.cargo, itemDatabase);
                }

                continue;
            }

            if (!entry.building.TryGetComponent(out CarrierEmployeeBuildingController carrierController) ||
                !carrierController.TryGetWorker(entry.employee.EmployeeId, out CarrierWorker carrier))
            {
                continue;
            }

            RestoreInventory(entry.saved.cargo ?? new InventorySaveData(),
                carrier.CargoInventory);

            ProductionBuilding commandTarget = null;

            if (entry.saved.command != null &&
                buildingsByGuid.TryGetValue(
                    entry.saved.command.targetBuildingGuid,
                    out PlacedBuilding targetBuilding))
            {
                targetBuilding.TryGetComponent(out commandTarget);
            }

            carrier.ResumeAfterLoad(entry.saved.command, commandTarget);
        }
    }
    #endregion

    public void OnClickSave()
    {
        if (!SaveGame())
        {
            Debug.LogWarning("저장 버튼 처리에 실패했습니다.");
        }
    }

    public void OnClickLoad()
    {
        if (!LoadGame())
        {
            Debug.LogWarning("불러오기 버튼 처리에 실패했습니다.");
        }
    }

    public void OnClickDeleteSave()
    {
        if (fileStore == null)
        {
            fileStore = new JsonSaveFileStore();
        }

        if (!fileStore.Delete())
        {
            Debug.LogWarning("저장 파일 삭제에 실패했습니다.");
            return;
        }

        Debug.Log($"저장 파일 삭제 완료: {fileStore.SavePath}");
    }

    #region 유틸
    private bool ValidateRuntimeReferences()
    {
        bool valid = true;

        if (buildingDatabase == null)
        {
            Debug.LogError("BuildingDatabase가 연결되지 않았습니다.");
            valid = false;
        }

        if (itemDatabase == null)
        {
            Debug.LogError("itemDatabase가 연결되지 않았습니다.");
            valid = false;
        }

        if (recipeDatabase == null)
        {
            Debug.LogError("recipeDatabase가 연결되지 않았습니다.");
            valid = false;
        }

        if (currencySystem == null)
        {
            Debug.LogError("currencySystem이 연결되지 않았습니다.");
            valid = false;
        }

        if (player == null)
        {
            Debug.LogError("player가 연결되지 않았습니다.");
            valid = false;
        }

        if (playerInventory == null)
        {
            Debug.LogError("playerInventory가 연결되지 않았습니다.");
            valid = false;
        }

        if (playerAttack == null)
        {
            Debug.LogError("playerAttack이 연결되지 않았습니다.");
            valid = false;
        }

        if (skillTreeManager == null)
        {
            Debug.LogError("skillTreeManager가 연결되지 않았습니다.");
            valid = false;
        }

        if (recipeUnlockManager == null)
        {
            Debug.LogError("recipeUnlockManager가 연결되지 않았습니다.");
            valid = false;
        }

        if (expansionManager == null)
        {
            Debug.LogError("expansionManager가 연결되지 않았습니다.");
            valid = false;
        }

        if (placementSystem == null)
        {
            Debug.LogError("placementSystem이 연결되지 않았습니다.");
            valid = false;
        }

        if (counterInventory == null)
        {
            Debug.LogError("counterInventory가 연결되지 않았습니다.");
            valid = false;
        }

        if (huntingFieldManager == null)
        {
            Debug.LogError("huntingFieldManager가 연결되지 않았습니다.");
            valid = false;
        }

        return valid;
    }
    #endregion
}

