using UnityEngine;

/*
 [생산 건물 프로세스]

1. 플레이어가 UI를 통해 레시피 선택
2. 플레이어 인벤토리에서 생산 건물에 해당 레시피의 Input 재료 자동 납품
3. 레시피 시간에 맞춰 가공 시작
4. 시간이 모두 지나면 건물 인벤토리에 아이템 추가 및 시각화

- 만약 생산 중 레시피를 변경한다면 현재 작업 진행 중인 아이템 가공 완료 후 변경(예약 기능)
- 
 */

public class ProductionBuilding : MonoBehaviour
{
    [SerializeField] private RecipeDataSO recipe;
    [SerializeField] private ItemInventory inputInventory = new();
    [SerializeField] private ItemInventory outputInventory = new();

    private ProductionMachine machine;

    public RecipeDataSO Recipe => recipe;
    public ItemInventory InputInventory => inputInventory;
    public ItemInventory OutputInventory => outputInventory;
    public ProductionState State => machine.state;
    public float Progress => machine.Progress;
}
