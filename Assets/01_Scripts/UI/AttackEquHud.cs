using UnityEngine;
using UnityEngine.UI;

public class AttackEquHud : MonoBehaviour
{
    // Button button;
    [SerializeField] PlayerAttack playerAttack;
    [SerializeField] PoolManager poolManager;
    [SerializeField] GameObject layoutWidth;

    [SerializeField] AttackEquHud closeButton1;
    [SerializeField] AttackEquHud closeButton2;

    [SerializeField] private Image skillIcon;
    [SerializeField] private Image plusIcon; // 스킬이 장착된 상태가 아닐 때 활성화

    public string equAttackID;
    private float damage;
    private float attackSpeed;
    private float distance;
    private float projectile;
    public bool isEquip;
    public int slotIndex;

    private SkillDesc popUp;
    private void Awake()
    {
        // button = GetComponent<Button>();
        layoutWidth.SetActive(false);
    }

    private void Start()
    {
        RefreshSlot();
    }

    private void OnDisable()
    {
        if (layoutWidth != null) CloseSelector();
    }

    public void OnOffButton()
    {
        //if (layoutWidth.activeSelf == true)
        //{
        //    AttackEquPrefab[] childButton = GetComponentsInChildren<AttackEquPrefab>();
        //    foreach(var button in childButton)
        //    {
        //        poolManager.ReturnPool(button);
        //    }
        //    layoutWidth.SetActive(false);
        //}
        //else
        //{
        //    layoutWidth.SetActive(true);
        //    if(closeButton1.layoutWidth.activeSelf == true)
        //    {
        //        closeButton1.OnOffButton();
        //    }
        //    if (closeButton2.layoutWidth.activeSelf == true)
        //    {
        //        closeButton2.OnOffButton();
        //    }
        //    SelectAttack();
        //}

        if (layoutWidth.activeSelf)
        {
            CloseSelector();
            return;
        }

        closeButton1?.CloseSelector();
        closeButton2?.CloseSelector();

        layoutWidth.SetActive(true);

        AudioManager.Instance.PlaySFX(ESFXType.UI_Open);

        SelectAttack();
    }

    public void SelectAttack()
    {
        foreach(var attackData in playerAttack.attackUnlockDatas)
        {
            //if(attackData.unlock == true)
            //{
            //    AttackEquPrefab prefabButton = poolManager.GetPool<AttackEquPrefab>();
            //    Button prefabImage = prefabButton.GetComponent<Button>();
            //    prefabButton.attackEquHud = this;
            //    prefabImage.image.sprite = attackData.sprite;
            //    prefabButton.transform.SetParent(layoutWidth.transform);
            //    prefabButton.equID = equAttackID;
            //    prefabButton.slotIndex = slotIndex;
            //    prefabButton.playerAttack = playerAttack;
            //    prefabButton.attackID = attackData.attackID;
            //}


            AttackEquPrefab prefabButton = poolManager.GetPool<AttackEquPrefab>();

            if (prefabButton == null) continue;

            prefabButton.transform.SetParent(layoutWidth.transform);

            prefabButton.Bind(this, attackData.attackID, attackData.sprite, attackData.unlock);
        }
    }

    public void EquipSlot(string id)
    {
        playerAttack.StartAndStopAttackCo(slotIndex, id, this);
    }

    public void EquipRefresh(string id)
    {
        //equAttackID = id;
        //isEquip = false;
        //foreach(var playerAttackSlot in playerAttack.slots)
        //{
        //    if(playerAttackSlot.equipAttackID == id)
        //    {
        //        isEquip = true;
        //    }
        //}

        //if(isEquip == false)
        //{
        //    button.image.sprite = null;
        //}
        //else
        //{
        //    foreach(var unlockData in playerAttack.attackUnlockDatas)
        //    {
        //        if(unlockData.attackID == id)
        //        {
        //            button.image.sprite = unlockData.sprite;
        //        }
        //    }
        //}

        RefreshSlot();
    }

    public void RefreshSlot()
    {
        if (playerAttack == null ||
            playerAttack.slots == null || 
            slotIndex < 0 || 
            slotIndex >= playerAttack.slots.Length)
        {
            SetEmptySlot();
            return;
        }

        string equippedId = playerAttack.slots[slotIndex].equipAttackID;

        equAttackID = equippedId;
        isEquip = !string.IsNullOrEmpty(equippedId);

        Sprite equippedSprite = null;

        if (isEquip)
        {
            foreach (var unlockData in playerAttack.attackUnlockDatas)
            {
                if (unlockData.attackID == equippedId)
                {
                    equippedSprite = unlockData.sprite;
                    break;
                }
            }
        }

        // 스킬을 장착하면 스킬 아이콘을 활성화하고 플러스 아이콘은 비활성화
        if (skillIcon != null)
        {
            skillIcon.sprite = equippedSprite;
            skillIcon.gameObject.SetActive(isEquip);
        }

        if (plusIcon != null)
        {
            plusIcon.gameObject.SetActive(!isEquip);
        }
    }

    private void SetEmptySlot()
    {
        equAttackID = null;
        isEquip = false;

        if (skillIcon != null)
        {
            skillIcon.sprite = null;
            skillIcon.gameObject.SetActive(false);
        }

        if (plusIcon != null)
        {
            plusIcon.gameObject.SetActive(true);
        }
    }

    public void CloseSelector()
    {
        if (layoutWidth == null) return;

        AttackEquPrefab[] childButtons = layoutWidth.GetComponentsInChildren<AttackEquPrefab>(true);

        foreach (var button in childButtons)
        {
            button.ResetState();
            poolManager.ReturnPool(button);
        }

        layoutWidth.SetActive(false);
    }
    public void MouserEnter()
    {
        if (equAttackID == null) return;
        popUp = poolManager.GetPool<SkillDesc>();
        popUp.transform.SetParent(transform);
        RectTransform rect = popUp.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0f, -170f);
        float[] value = new float[4];
        value = playerAttack.ReturnSkillValue(equAttackID);
        popUp.SetDamage(value[0]);
        popUp.SetSpeed(value[1]);
        popUp.SetDistance(value[2]);
        popUp.SetProjectile((int)value[3]);
    }
    public void MouseExit()
    {
        if (popUp != null)
        {
            poolManager.ReturnPool(popUp);
        }
    }
}
