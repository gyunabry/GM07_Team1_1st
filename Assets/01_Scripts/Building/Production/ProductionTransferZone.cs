using System.Collections;
using UnityEngine;

// 재료 납품과 완성품 수령 담당

public class ProductionTransferZone : MonoBehaviour
{
    [SerializeField] private ProductionBuilding productionBuilding;
    [SerializeField] private float transferInterval = 0.1f;
    [SerializeField] private LayerMask playerLayer;

    private PlayerInventory currentPlayer;
    private Coroutine transferCoroutine;
    private WaitForSeconds transferWait;

    private IntegratedTransmitter integratedTransmitter;
    private bool isPlayerInteracting;

    private void Awake()
    {
        if (productionBuilding == null)
        {
            productionBuilding = GetComponentInParent<ProductionBuilding>();
        }

        if (productionBuilding == null)
        {
            enabled = false;
            return;
        }

        transferWait = new WaitForSeconds(transferInterval);
    }

    private void OnDisable()
    {
        StopTransfer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerLayer(other.gameObject.layer))
        {
            return;
        }

        PlayerInventory playerInventory = other.GetComponentInParent<PlayerInventory>();
        if (playerInventory == null) return;

        currentPlayer = playerInventory;
        SetPlayerInteracting(true);
        if (transferCoroutine == null)
        {
            transferCoroutine = StartCoroutine(TransferCo());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerLayer(other.gameObject.layer))
        {
            return;
        }

        SetPlayerInteracting(false);
        StopTransfer();
    }

    private IEnumerator TransferCo()
    {
        while (currentPlayer != null)
        {
            TransferCycle(currentPlayer);

            yield return transferWait;
        }

        transferCoroutine = null;
        currentPlayer = null;
    }

    // 한 번의 전송에서 납품 및 수령 처리
    private void TransferCycle(PlayerInventory player)
    {
        if (player == null || 
            productionBuilding == null || 
            !productionBuilding.CanOperate)
        {
            return;
        }

        if (isPlayerInteracting)
        {
            TryTransferFromTransmitter();
        }

        TryTransferOutput(player);
    }

    private int TryTransferInput(ItemInventory sourceInventory)
    {
        RecipeDataSO recipe = productionBuilding.SelectedRecipe;

        if (recipe == null || recipe.Input == null) return 0;

        // 플레이어의 인벤토리에서 Input Inventory로 레시피에 해당하는 재료를 1개 이동
        return sourceInventory.TransferTo(
            productionBuilding.InputInventory, 
            recipe.Input, 
            1
        );
    }

    private int TryTransferOutput(PlayerInventory player)
    {
        ItemDataSO outputItem = FindFirstOutputItem();

        if (outputItem == null) return 0;

        // 산출물 인벤토리에서 플레이어 인벤토리로 산출물을 1개 이동
        return productionBuilding.OutputInventory.TransferTo(
            player.Inventory,
            outputItem,
            1
        );
    }

    private int TryTransferFromTransmitter()
    {
        RecipeDataSO recipe = productionBuilding.SelectedRecipe;

        if (recipe == null || recipe.Input == null)
        {
            return 0;
        }

        if (integratedTransmitter == null)
        {
            integratedTransmitter = FindAnyObjectByType<IntegratedTransmitter>();
        }

        if (integratedTransmitter == null || !integratedTransmitter.CanOperate)
        {
            return 0;
        }

        return integratedTransmitter.Inventory.TransferTo(
            productionBuilding.InputInventory,
            recipe.Input,
            1);
    }

    private ItemDataSO FindFirstOutputItem()
    {
        foreach (InventoryEntry entry in productionBuilding.OutputInventory.Entries)
        {
            if (entry != null && !entry.IsEmpty)
            {
                return entry.Item;
            }
        }

        return null;
    }

    private bool IsPlayerLayer(int layer)
    {
        return (playerLayer.value & (1 << layer)) != 0;
    }

    private void StopTransfer()
    {
        if (transferCoroutine != null)
        {
            StopCoroutine(transferCoroutine);
            transferCoroutine = null;
        }

        currentPlayer = null;
    }

    private void SetPlayerInteracting(bool interacting)
    {
        isPlayerInteracting = interacting;
    }
}
