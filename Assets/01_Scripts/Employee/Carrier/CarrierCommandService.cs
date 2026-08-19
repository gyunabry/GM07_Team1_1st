using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HUD가 사용하는 운반 직원 전역 명령 창구입니다.
/// 명령 UI와 운반 직원 건물의 수명주기를 분리하고, 전송기 연동 값도 한 곳에서 관리합니다.
/// </summary>
public sealed class CarrierCommandService : MonoBehaviour
{
    private readonly HashSet<CarrierEmployeeBuildingController> controllers = new();
    private ItemInventory materialStorage;
    private Transform materialStoragePoint;

    /// <summary>HUD의 + 버튼용 API입니다.</summary>
    public bool TryAssignCommand(CarrierCommandType type, ProductionBuilding targetBuilding)
    {
        foreach (CarrierEmployeeBuildingController controller in controllers)
        {
            if (controller != null && controller.TryAssignCommandInternal(type, targetBuilding))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>HUD의 - 버튼용 API입니다.</summary>
    public bool TryClearOneCommand(CarrierCommandType type, ProductionBuilding targetBuilding)
    {
        foreach (CarrierEmployeeBuildingController controller in controllers)
        {
            if (controller != null && controller.TryClearOneCommandInternal(type, targetBuilding))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>HUD 행에 표시할 해당 생산 건물·명령의 배정 인원 수입니다.</summary>
    public int GetCommandCount(CarrierCommandType type, ProductionBuilding targetBuilding)
    {
        int count = 0;
        foreach (CarrierEmployeeBuildingController controller in controllers)
        {
            if (controller != null)
            {
                count += controller.GetCommandCountInternal(type, targetBuilding);
            }
        }

        return count;
    }

    /// <summary>HUD의 + 버튼 활성화 여부에 사용할 대기 직원 수입니다.</summary>
    public int GetAvailableWorkerCount()
    {
        int count = 0;
        foreach (CarrierEmployeeBuildingController controller in controllers)
        {
            if (controller != null)
            {
                count += controller.GetAvailableWorkerCountInternal();
            }
        }

        return count;
    }

    /// <summary>
    /// 전송기 담당 시스템이 준비된 뒤 공용 재료 인벤토리와 작업 위치를 전달하는 API입니다.
    /// 서비스가 등록한 모든 운반 직원에게 즉시 적용합니다.
    /// </summary>
    public void ConfigureLogistics(ItemInventory sharedMaterialStorage, Transform sharedMaterialStoragePoint)
    {
        materialStorage = sharedMaterialStorage;
        materialStoragePoint = sharedMaterialStoragePoint;

        foreach (CarrierEmployeeBuildingController controller in controllers)
        {
            controller?.ConfigureLogisticsInternal(materialStorage, materialStoragePoint);
        }
    }

    internal void RegisterController(CarrierEmployeeBuildingController controller)
    {
        if (controller != null && controllers.Add(controller))
        {
            controller.ConfigureLogisticsInternal(materialStorage, materialStoragePoint);
        }
    }

    internal void UnregisterController(CarrierEmployeeBuildingController controller)
    {
        if (controller != null)
        {
            controllers.Remove(controller);
        }
    }
}
