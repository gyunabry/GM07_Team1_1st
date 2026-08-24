using System;
using System.Collections.Generic;
using UnityEngine;

public class CarrierHouse : MonoBehaviour
{
    [Header("직원 설정")]
    [SerializeField] private GameObject carrierPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int maxCarrierCount = 3;
    [SerializeField] private int hireCost = 100;

    private PlacedBuilding placedBuilding;
    private EmployeeManager employeeManager;

    // 이 건물이 등록된 건물인지 기록
    private bool isRegisterd;

    public bool CanOperate => placedBuilding != null && placedBuilding.IsComplete;
    public bool IsRegisterd => 
        employeeManager != null &&
        placedBuilding != null &&
        employeeManager.TryGetEmployees(placedBuilding, out _);

    public int HiredCarrierCount
    {
        get
        {
            if (!employeeManager.TryGetEmployees(placedBuilding, out IReadOnlyList<EmployeeRuntimeData> employees))
            {
                return 0;
            }

            int count = 0;

            for (int i = 0; i < employees.Count; i++)
            {
                EmployeeRuntimeData employee = employees[i];

                if (employee != null && employee.Role == EmployeeRole.Carrier)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public int MaxCarrierCount => maxCarrierCount;
    public int HireCost => hireCost;

    public bool CanHire =>
        CanOperate &&
        IsRegisterd &&
        carrierPrefab != null &&
        HiredCarrierCount < maxCarrierCount;

    public event Action StateChanged;
    public event Action<GameObject> CarrierHired;

    private void Awake()
    {
        placedBuilding = GetComponent<PlacedBuilding>();
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void OnDestroy()
    {
        
    }

    // 외부 버튼에서 호출할 메서드
    public bool TryHireCarrier()
    {
        if (!CanOperate || employeeManager == null || carrierPrefab != null)
        {
            return false;
        }

        if (!IsRegisterd) return false;
        if (!CanHire) return false;
        if (hireCost > 0)
        {
            // 재화 소모 시도
            if (!CurrencySystem.Instance.TrySpendMoney(hireCost))
            {
                return false;
            }
        }

        // 실제 고용은 employeeManager에서 진행
        if (!employeeManager.TryHireAdditional(placedBuilding, out EmployeeRuntimeData employee))
        {
            StateChanged?.Invoke();
            return false;
        }

        StateChanged?.Invoke();
        return true;
    }
}
