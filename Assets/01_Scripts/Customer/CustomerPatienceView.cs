using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CustomerController))]
public sealed class CustomerPatienceView : MonoBehaviour
{
    private const float CanvasScale = 0.01f;
    private const float VerticalOffset = 2.2f;

    private CustomerController controller;
    private GameObject canvasRoot;
    private Image bubbleImage;
    private Image patienceFill;
    private RectTransform patienceFillRect;
    private Image itemIcon;
    private Text itemNameText;
    private Text amountText;
    private Camera targetCamera;

    private void Awake()
    {
        controller = GetComponent<CustomerController>();
        CreateView();
        SetVisible(false);
    }

    private void Update()
    {
        if (controller == null || !controller.Order.IsValid)
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

    private void LateUpdate()
    {
        if (canvasRoot == null || !canvasRoot.activeSelf)
        {
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            canvasRoot.transform.rotation = Quaternion.LookRotation(canvasRoot.transform.position - targetCamera.transform.position, targetCamera.transform.up);
        }
    }

    private void CreateView()
    {
        canvasRoot = new GameObject("PatienceCanvas", typeof(RectTransform), typeof(Canvas));
        canvasRoot.transform.SetParent(transform, false);
        canvasRoot.transform.localPosition = new Vector3(0f, VerticalOffset, 0f);
        canvasRoot.transform.localScale = Vector3.one * CanvasScale;

        Canvas canvas = canvasRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        RectTransform canvasRect = canvasRoot.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(90f, 70f);

        bubbleImage = CreateImage("SpeechBubble", canvasRoot.transform, new Vector2(90f, 64f), Vector2.zero, Color.white);
        bubbleImage.type = Image.Type.Sliced;

        patienceFill = CreateImage("PatienceFill", bubbleImage.transform, new Vector2(82f, 0f), new Vector2(0f, -28f), new Color(0.95f, 0.3f, 0.3f, 0.65f));
        patienceFillRect = patienceFill.rectTransform;
        patienceFillRect.pivot = new Vector2(0.5f, 0f);

        itemIcon = CreateImage("ItemIcon", bubbleImage.transform, new Vector2(92f, 78f), new Vector2(-3f, 4f), Color.white);
        itemIcon.preserveAspect = true;

        itemNameText = CreateText("ItemNameText", bubbleImage.transform, new Vector2(72f, 26f), new Vector2(0f, 5f));
        itemNameText.alignment = TextAnchor.MiddleCenter;
        itemNameText.color = Color.black;
        itemNameText.fontSize = 12;

        amountText = CreateText("AmountText", bubbleImage.transform, new Vector2(32f, 22f), new Vector2(24f, -16f));
        amountText.alignment = TextAnchor.MiddleCenter;
        amountText.color = Color.black;
        amountText.fontSize = 18;
    }

    private void RefreshView()
    {
        CustomerOrderItem requestedItem = controller.Order.Items[0];
        itemIcon.sprite = requestedItem.ItemId != null ? requestedItem.ItemId.Icon : null;
        itemIcon.enabled = itemIcon.sprite != null;

        itemNameText.text = itemIcon.enabled ? string.Empty : requestedItem.ItemId != null ? requestedItem.ItemId.ItemName : "?";
        itemNameText.enabled = !itemIcon.enabled;

        amountText.text = requestedItem.Amount > 1 ? "x" + requestedItem.Amount : string.Empty;
        amountText.enabled = requestedItem.Amount > 1;

        patienceFillRect.sizeDelta = new Vector2(82f, 56f * controller.PatienceNormalized);
        bubbleImage.color = Color.white;
        patienceFill.color = new Color(0.95f, 0.3f, 0.3f, 0.65f);
    }

    private void SetVisible(bool visible)
    {
        if (canvasRoot != null && canvasRoot.activeSelf != visible)
        {
            canvasRoot.SetActive(visible);
        }
    }

    private static Image CreateImage(string objectName, Transform parent, Vector2 size, Vector2 position, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = imageObject.GetComponent<Image>();
        image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Text CreateText(string objectName, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.raycastTarget = false;
        return text;
    }
}
