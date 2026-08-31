using System;
using System.Collections.Generic;

public enum BuildingPlacementSource : byte
{
    Scene = 0,
    Player = 1
}

public enum SavedInventorySlot : byte
{
    Storage = 0,
    Input = 1,
    Output = 2
}

[Serializable]
public class SaveGameData
{
    public int schemaVersion = 1;

    public PlayerSaveData player = new();
    public ProgressionSaveData progression = new();
    public WorldSaveData world = new();
}

[Serializable]
public class PlayerSaveData
{
    public int level;
    public int experience;
    public int currentExperience;
    public int skillPoints;
    public int money;

    public InventorySaveData inventory = new();

    public List<EquippedAttackSaveData> equippedAttacks = new();
}

[Serializable]
public class ProgressionSaveData
{
    public List<SkillLevelSaveData> skills = new();

    public List<string> unlockedRecipeIds = new();
}

[Serializable]
public class WorldSaveData
{
    public int workshopExpansionStage;

    public List<NamedInventorySaveData> sharedInventories = new();

    public List<FacilitySaveData> facilities = new();
    public List<EmployeeSaveData> employees = new();
}

#region 플레이어 데이터
[Serializable]
public class InventorySaveData
{
    public List<ItemStackSaveData> items = new();
}

[Serializable]
public class ItemStackSaveData
{
    public string itemId;
    public int amount;
}

[Serializable]
public class NamedInventorySaveData
{
    public string inventoryId;
    public InventorySaveData inventory = new();
}

[Serializable]
public class EquippedAttackSaveData
{
    public byte slotIndex;
    public string attackId;
}

[Serializable]
public class SkillLevelSaveData
{
    public string skillId;
    public int level;
}
#endregion

#region 월드 데이터
[Serializable]
public class FacilitySaveData
{
    public string guid;

    public string buildingId;
    public string areaId;

    public int originCellX;
    public int originCellY;

    public short rotationIndex;

    public BuildingPlacementSource placementSource;

    public BuildingState buildingState;
    public float constructionProgress01;

    public List<BuildingInventorySaveData> inventories = new();

    // 생산시설이 아니면 null
    public ProductionSaveData production;

    public List<EmployeeSaveData> employees = new();
}

[Serializable]
public class BuildingInventorySaveData
{
    public SavedInventorySlot slot;
    public InventorySaveData inventory = new();
}

[Serializable] 
public class ProductionSaveData
{
    public string selectedRecipeId;
    public string activeRecipeId;

    public float activeProgress01;

    public int pendingOutputAmount;
}
#endregion

#region 직원 데이터
[Serializable]
public class EmployeeSaveData
{
    public InventorySaveData cargo = new();

    // 사냥 직원은 null
    public CarrierCommandSaveData command;
}

[Serializable]
public class CarrierCommandSaveData
{
    public CarrierCommandType commandType;
    public string targetBuildingGuid;
    public string assignedRecipeId;
}
#endregion