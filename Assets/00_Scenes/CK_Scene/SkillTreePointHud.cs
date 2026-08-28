using TMPro;
using UnityEngine;

public class SkillTreePointHud : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] SkillTreeManager skillTreeManager;
    TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        if(player == null)
        {
            player = FindAnyObjectByType<Player>();
        }
        if(skillTreeManager == null)
        {
            skillTreeManager = FindAnyObjectByType<SkillTreeManager>();
        }
    }
    private void OnEnable()
    {
        player.LevelUp += Player_LevelUp;
        skillTreeManager.OnSkillChange += SkillTreeManager_OnSkillChange;
        text.text = player.skillPoint.ToString();
    }
    private void OnDisable()
    {
        player.LevelUp -= Player_LevelUp;
        skillTreeManager.OnSkillChange -= SkillTreeManager_OnSkillChange;
    }
    private void Player_LevelUp()
    {
        text.text = player.skillPoint.ToString();
    }
    private void SkillTreeManager_OnSkillChange()
    {
        text.text = player.skillPoint.ToString();
    }
}
