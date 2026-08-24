using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanelController : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("캐릭터 스탯")]
    [SerializeField] private TMP_Text attackPower;
    [SerializeField] private TMP_Text attackSpeed;
    [SerializeField] private TMP_Text attackRange;
    [SerializeField] private TMP_Text moveSpeed;
    [SerializeField] private TMP_Text magnetRange;
    [SerializeField] private TMP_Text inventoryCapacity;

    [Header("시설 한도")]
    [SerializeField] private TMP_Text maxProductionCount;
    [SerializeField] private TMP_Text maxSalesCounterCount;
    [SerializeField] private TMP_Text maxHunterCount;
    [SerializeField] private TMP_Text maxCarrierCount;

    private bool isOpen;

    private void Awake()
    {
        SetVisible(false);
    }

    public void ToggleUI()
    {
        SetVisible(!isOpen);
    }

    private void SetVisible(bool visible)
    {
        isOpen = visible;

        if (canvas != null) canvas.enabled = visible;
        if (graphicRaycaster != null) graphicRaycaster.enabled = visible;
    }
}
