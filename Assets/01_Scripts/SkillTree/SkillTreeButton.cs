using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SkillTreeButton : MonoBehaviour
{
    [SerializeField] SkillDataSO skillData;
    Button button;
    [SerializeField] SkillTreeManager skillTreeManager;
    TextMeshProUGUI text;
    SkillRuntimeState state;

    private UnityAction clickAction;
    private Coroutine co;
    private void Awake()
    {
        clickAction = () => skillTreeManager.SkillTreeClick(skillData);
        button = GetComponent<Button>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        skillTreeManager.SkillUnlockCheck();
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
