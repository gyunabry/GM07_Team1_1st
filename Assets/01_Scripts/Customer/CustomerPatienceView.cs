using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CustomerController))]
public sealed class CustomerPatienceView : MonoBehaviour
{
    private const float VerticalOffset = 2.2f;

    private CustomerController controller;
    private GameObject customerHudPrefab;
    private GameObject hudInstance;
    private Image orderIcon;
    private Image patienceGauge;
    private RectTransform patienceGaugeRect;
    private Vector2 patienceGaugeSize;
    private TextMeshProUGUI amountText;

    private void Awake()
    {
        controller = GetComponent<CustomerController>();
    }

    public void Configure(GameObject hudPrefab)
    {
        if (customerHudPrefab == hudPrefab && hudInstance != null)
        {
            return;
        }

        customerHudPrefab = hudPrefab;
        CreateView();
    }

    private void Update()
    {
        if (hudInstance == null || controller == null || !controller.Order.IsValid)
        {
            SetVisible(false);
            return;
        }

        bool isWaiting = !controller.IsPaymentCompleted && controller.RuntimeData.CurrentStateName != "Exit";
        if (!isWaiting)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);
        RefreshView();
    }

    private void CreateView()
    {
        if (hudInstance != null)
        {
            Destroy(hudInstance);
        }

        if (customerHudPrefab == null)
        {
            Debug.LogError("Customer HUD prefab is not assigned.", this);
            return;
        }

        hudInstance = Instantiate(customerHudPrefab, transform);
        hudInstance.name = customerHudPrefab.name;
        hudInstance.transform.localPosition = Vector3.up * VerticalOffset;

        orderIcon = FindComponent<Image>("Order_Icon");
        patienceGauge = FindComponent<Image>("Order_Slider");
        amountText = FindComponent<TextMeshProUGUI>("Amount_Text");

        if (orderIcon == null || patienceGauge == null || amountText == null)
        {
            Debug.LogError("UI_CustomerHUD is missing a required UI element.", hudInstance);
            Destroy(hudInstance);
            hudInstance = null;
            return;
        }

        patienceGaugeRect = patienceGauge.rectTransform;
        patienceGaugeSize = patienceGaugeRect.sizeDelta;
        // 프리팹의 게이지가 Z축으로 90도 회전되어 있어 로컬 X축이 화면상의 세로 축이다.
        patienceGaugeRect.pivot = new Vector2(0f, 0.5f);
        SetVisible(false);
    }

    private void RefreshView()
    {
        CustomerOrderItem requestedItem = controller.Order.Items[0];
        orderIcon.sprite = requestedItem.ItemId != null ? requestedItem.ItemId.Icon : null;
        orderIcon.enabled = orderIcon.sprite != null;

        amountText.text = requestedItem.Amount > 1 ? "x" + requestedItem.Amount : string.Empty;
        amountText.enabled = requestedItem.Amount > 1;

        patienceGaugeRect.sizeDelta = new Vector2(patienceGaugeSize.x * controller.PatienceNormalized, patienceGaugeSize.y);
    }

    private void SetVisible(bool visible)
    {
        if (hudInstance != null && hudInstance.activeSelf != visible)
        {
            hudInstance.SetActive(visible);
        }
    }

    private T FindComponent<T>(string objectName) where T : Component
    {
        Transform child = hudInstance.transform.Find(objectName);
        return child != null ? child.GetComponent<T>() : null;
    }
}
