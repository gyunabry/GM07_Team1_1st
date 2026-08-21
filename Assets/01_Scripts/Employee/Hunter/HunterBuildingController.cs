using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlacedBuilding))]
public sealed class HunterBuildingController : MonoBehaviour
{
    [SerializeField] private HunterWorker hunterPrefab;
    [SerializeField] private Transform homePoint;
    [SerializeField] private Collider spawnArea;
    [SerializeField] private HuntingTransmitter transmitter;
    private readonly Dictionary<int,HunterWorker> workers=new(); private EmployeeManager manager; private PlacedBuilding building;
    void Awake(){building=GetComponent<PlacedBuilding>(); if(homePoint==null)homePoint=transform;}
    void OnEnable(){building.OnConstructionCompleted+=OnBuilt;}
    void Start(){manager=FindFirstObjectByType<EmployeeManager>(); manager.EmployeeHired+=Hire; manager.EmployeeRemoved+=Remove; if(building.IsComplete)Register();}
    void OnDisable(){if(building!=null)building.OnConstructionCompleted-=OnBuilt; if(manager!=null){manager.EmployeeHired-=Hire;manager.EmployeeRemoved-=Remove;}}
    void OnDestroy()
    {
        if(manager!=null)manager.TryUnregisterBuilding(building);
        ReturnAllWorkers();
    }
    void OnBuilt(PlacedBuilding b){Register();} void Register(){manager.TryRegisterBuilding(building); if(manager.TryGetEmployees(building,out var es))foreach(var e in es)Hire(e);}
    void Hire(EmployeeRuntimeData e){if(e==null||e.Role!=EmployeeRole.Hunter||e.AssignedBuilding!=building||workers.ContainsKey(e.EmployeeId))return; var w=PoolManager.Instance.GetPool(hunterPrefab);w.transform.position=homePoint.position;w.Initialize(manager,e,spawnArea,transmitter,homePoint);workers[e.EmployeeId]=w;}
    void Remove(EmployeeRuntimeData e){if(e!=null&&workers.TryGetValue(e.EmployeeId,out var w)){workers.Remove(e.EmployeeId);w.DepositCargoForBuildingSale();w.ResetForPool();PoolManager.Instance.ReturnPool(w);}}
    void ReturnAllWorkers()
    {
        foreach(HunterWorker worker in workers.Values)
        {
            if(worker==null)continue;
            worker.DepositCargoForBuildingSale();
            worker.ResetForPool();
            if(PoolManager.Instance!=null)PoolManager.Instance.ReturnPool(worker);
        }
        workers.Clear();
    }
}
