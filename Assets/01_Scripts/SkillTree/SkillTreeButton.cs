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
        popUp.transform.SetParent(transform);
        RectTransform rect = popUp.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0f, 170f);
        if(rect.transform.position.y > 1000f)
        {
            rect.anchoredPosition = new Vector2(-240f, 0f);
        }
        popUp.SetSprite(skillData.skillSprite);
        popUp.SetName(skillData.name, skillData.skillNeedLevel, skillData.skillMaxLevel);
        popUp.SetNeed(skillData.skillNeedSkillPoint, skillData.skillNeedMoney);
        popUp.SetDesc(skillData.skillDesc);
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
