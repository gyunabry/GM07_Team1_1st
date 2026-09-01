using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SkillTreeButton : MonoBehaviour
{
    [SerializeField] SkillDataSO skillData;
    private Button button;
    [SerializeField] SkillTreeManager skillTreeManager;
    [SerializeField] SkillTreePopUp skillTreePopUp;
    [SerializeField] PoolManager poolManager;
    private TextMeshProUGUI text;
    private SkillRuntimeState state;
    private SkillTreePopUp popUp;
    [SerializeField] Canvas canvas;

    private UnityAction clickAction;
    private Coroutine co;
    private RectTransform[] lockObject;
    private void Awake()
    {
        clickAction = () => skillTreeManager.SkillTreeClick(skillData);
        button = GetComponent<Button>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        skillTreeManager.SkillUnlockCheck();
        lockObject = GetComponentsInChildren<RectTransform>();
        if(poolManager == null)
        {
            poolManager = FindAnyObjectByType<PoolManager>();
        }
    }
    public void MouserEnter()
    {
        popUp = poolManager.GetPool<SkillTreePopUp>();
        Canvas rootCanvas = canvas != null ? canvas : GetComponentInParent<Canvas>();
        if(rootCanvas != null)
        {
            popUp.transform.SetParent(rootCanvas.transform, false);
        }
        popUp.transform.SetAsLastSibling();
        popUp.transform.localScale = Vector3.one;
        
        popUp.SetSprite(skillData.skillSprite);
        popUp.SetName(skillData.skillName);
        popUp.SetNeed(skillData.skillNeedSkillPoint, skillData.skillNeedMoney);
        popUp.SetDesc(skillData.skillDesc);
        if (skillData.value.Length != 0)
        {
            popUp.SetValue(skillData.skillChangeStat, skillData.value[0]);
        }
        else
        {
            popUp.SetValue(skillData.skillChangeStat, 0);
        }

            RectTransform popUpRect = popUp.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(popUpRect);

        RectTransform buttonRect = GetComponent<RectTransform>();


        float buttonHalfHeight = (buttonRect.rect.height * buttonRect.lossyScale.y) * 0.5f;
        float halfWidth = (popUpRect.rect.width * popUpRect.lossyScale.x) * 0.5f;
        float halfHeight = (popUpRect.rect.height * popUpRect.lossyScale.y) * 0.5f;

        float margin = 15f;
        float dynamicOffsetY = buttonHalfHeight + halfHeight + margin;
        float padding = 80f;
        Vector3 targetPos = transform.position + new Vector3(0f, dynamicOffsetY, 0f);

        if (targetPos.y + halfHeight > Screen.height)
        {
            targetPos = transform.position - new Vector3(0f, dynamicOffsetY, 0f);
        }

        float minX = halfWidth + padding;
        float maxX = Screen.width - halfWidth - padding;
        float minY = halfHeight + padding;
        float maxY = Screen.height - halfHeight - padding;

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        popUp.transform.position = targetPos;
    }
    public void MouseExit()
    {
        if(popUp != null)
        {
            poolManager.ReturnPool(popUp);
        }
    }
    private void OnEnable()
    {
        co = StartCoroutine(StartButton());
    }
    private void OnDisable()
    {
        skillTreeManager.OnSkillChange -= SkillTreeManager_OnSkillChange;
        button.onClick.RemoveListener(clickAction);
    }
    private void SkillTreeManager_OnSkillChange()
    {
        if (state.Locked == true)
        {
            SkillLock();
        }
        else if(state.Locked == false && state.skillLevel == 0)
        {
            SkillUnlock();
        }
        else if(state.Locked == false && state.skillLevel > 0)
        {
            SkillActivate();
        }
    }
    public void SkillLock()
    {
        lockObject[6].gameObject.SetActive(true);
    }
    public void SkillUnlock()
    {
        lockObject[6].gameObject.SetActive(false);
        text.text = $"({state.skillLevel}/{skillData.skillMaxLevel})";
    }
    public void SkillActivate()
    {
        lockObject[6].gameObject.SetActive(false);
        text.text = $"({state.skillLevel}/{skillData.skillMaxLevel})";
    }

    IEnumerator StartButton()
    {
        yield return new WaitForSeconds(0.05f);
        button.onClick.AddListener(clickAction);
        state = skillTreeManager.GetState(skillData);
        skillTreeManager.OnSkillChange += SkillTreeManager_OnSkillChange;
        skillTreeManager.SkillUnlockCheck();
        co = null;
    }
}
