using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductProgressWorldUI : MonoBehaviour
{
    [Header("캔버스")]
    [SerializeField] private Canvas canvas;

    [Header("UI")]
    [SerializeField] private Image outputIcon;
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text outputCountText;
    [SerializeField] private BilboardUI bilboardUI;

    [Header("갱신 속도")]
    [SerializeField] private float refreshInterval = 0.1f;

    [Header("색상")]
    [SerializeField] private Color defaultColor = Color.green;
    [SerializeField] private Color warningColor = Color.orange;
    [SerializeField] private Color errorColor = Color.red;

    private ProductionBuilding productionBuilding;
    private ItemInventory outputInventory;

    private bool isProducing;
    private bool hasStarted;

    private float nextRefreshTime;

    private void Awake()
    {
        productionBuilding = GetComponentInParent<ProductionBuilding>();
        
        if (productionBuilding != null)
        {
            outputInventory = productionBuilding.OutputInventory;
        }

        SetVisible(false);
    }

    private void OnEnable()
    {
        if (productionBuilding != null)
        {
            productionBuilding.StateChanged += HandleStateChanged;
            productionBuilding.ProductionStarted += HandleProductionStarted;
            productionBuilding.ProgressChanged += HandleProgressChanged;
        }

        if (outputInventory != null)
        {
            outputInventory.InventoryChanged += HandleInventoryChanged;
        }

        if (hasStarted)
        {
            ApplyState(productionBuilding.State);
        }
    }

    private void OnDisable()
    {
        if (productionBuilding == null) return;

        productionBuilding.StateChanged -= HandleStateChanged;
        productionBuilding.ProductionStarted -= HandleProductionStarted;
        productionBuilding.ProgressChanged -= HandleProgressChanged;
    }

    private void Start()
    {
        hasStarted = true;

        if (productionBuilding == null) return;

        ApplyState(productionBuilding.State);
        RefreshInventoryCount();
    }

    private void HandleStateChanged(ProductionState state)
    {
        ApplyState(state);
    }

    private void ApplyState(ProductionState state)
    {
        isProducing = state == ProductionState.Producing;

        //bool shoudShow = isProducing 
        //    || state == ProductionState.WaitingForOutputSpace 
        //    || outputInventory.TotalAmount > 0;

        // 생산 중이거나 출력물 인벤토리에 아이템이 있을 때 활성화
        // 0901 수정 : 항상 보이도록 수정
        SetVisible(true);
        SetColor();

        if (isProducing)
        {
            SetProgress(productionBuilding.Progress);
            nextRefreshTime = 0f;
        }
    }

    private void HandleProductionStarted(RecipeDataSO recipe)
    {
        isProducing = true;

        if (outputIcon != null)
        {
            outputIcon.sprite = recipe.Output.Icon;
            outputIcon.enabled = true;
        }

        SetVisible(true);
        SetProgress(0f);

        // 갱신 시간 조정
        nextRefreshTime = Time.unscaledTime + refreshInterval;
    }

    private void HandleProgressChanged(float progress)
    {
        if (!isProducing || Time.unscaledTime < nextRefreshTime) return;

        SetProgress(progress);

        nextRefreshTime = Time.unscaledTime + refreshInterval;
    }

    private void HandleInventoryChanged()
    {
        RefreshInventoryCount();
        ApplyState(productionBuilding.State);
    }

    private void RefreshInventoryCount()
    {
        if (outputInventory == null) return;

        if (outputCountText != null)
        {
            outputCountText.text = $"{outputInventory.TotalAmount} / {outputInventory.Capacity}";
        }
    }

    private void SetProgress(float progress)
    {
        float value = Mathf.Clamp01(progress);

        if (!Mathf.Approximately(progressFill.fillAmount, value))
        {
            progressFill.fillAmount = value;
        }
    }

    private void SetColor()
    {
        if (productionBuilding.State == ProductionState.WaitingForMaterials)
        {
            if (progressFill != null)
            {
                progressFill.color = warningColor;
                return;
            }
        }

        if (productionBuilding.State == ProductionState.WaitingForOutputSpace)
        {
            if (progressFill != null)
            {
                progressFill.color = errorColor;
                return;
            }
        }

        if (progressFill != null)
        {
            progressFill.color = defaultColor;
        }
    }

    private void SetVisible(bool visible)
    {
        canvas.enabled = visible;
        bilboardUI.enabled = visible;
    }
}
