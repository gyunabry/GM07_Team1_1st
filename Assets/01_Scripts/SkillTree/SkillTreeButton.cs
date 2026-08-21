using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SkillTreeButton : MonoBehaviour
{
    [SerializeField] SkillDataSO skillData;
    Button button;
    [SerializeField] SkillTreeManager skillTreeManager;
    [SerializeField] SkillTreePopUp skillTreePopUp;
    [SerializeField] MonsterPoolManager monsterPoolManager;
    TextMeshProUGUI text;
    SkillRuntimeState state;
    SkillTreePopUp popUp;
    [SerializeField] Canvas canvas;

    private UnityAction clickAction;
    private Coroutine co;
    private void Awake()
    {
        clickAction = () => skillTreeManager.SkillTreeClick(skillData);
        button = GetComponent<Button>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        skillTreeManager.SkillUnlockCheck();
    }
    public void MouserEnter()
    {
        popUp = monsterPoolManager.GetPool<SkillTreePopUp>();
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
            monsterPoolManager.ReturnPool(popUp);
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
        button.image.color = Color.black;
        text.text = "Lock";
    }
    public void SkillUnlock()
    {
        button.image.color = Color.white;
        if (skillData.skillSprite != null)
        {
            button.image.sprite = skillData.skillSprite;
        }
    }
    public void SkillActivate()
    {
        button.image.color = Color.white;
        if (skillData.skillSprite != null)
        {
            button.image.sprite = skillData.skillSprite;
        }
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
