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
        Vector2 localPoint;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        popUp = monsterPoolManager.GetPool<SkillTreePopUp>();
        popUp.transform.SetParent(canvas.transform);
        RectTransform rect = popUp.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
            new Vector2(mousePosition.x, mousePosition.y + 20f),
            canvas.worldCamera,
            out localPoint
            );
        rect.anchoredPosition = localPoint;
        popUp.SetSprite(skillData.skillSprite);
        popUp.SetName(skillData.name, skillData.skillNeedLevel);
        popUp.SetNeed(skillData.skillNeedSkillPoint, skillData.skillNeedMoney);
        popUp.SetDesc(skillData.skillDesc);
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
        text.text = "UnLock";
    }
    public void SkillActivate()
    {
        button.image.color = Color.green;
        text.text = $"Upgrade : {state.skillLevel}";
    }

    IEnumerator StartButton()
    {
        yield return new WaitForSeconds(0.05f);
        state = skillTreeManager.GetState(skillData);
        skillTreeManager.OnSkillChange += SkillTreeManager_OnSkillChange;
        button.onClick.AddListener(clickAction);
        skillTreeManager.SkillUnlockCheck();
        co = null;
    }
}
