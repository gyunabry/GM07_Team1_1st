using System;
using TMPro;
using UnityEngine;

public class SkillPointView : MonoBehaviour
{
    [SerializeField] SkillTreeManager skillTreeManager;
    [SerializeField] Player player;
    TextMeshProUGUI text;

    private void OnEnable()
    {
        skillTreeManager.OnSkillChange += SkillTreeManager_OnSkillChange;
        player.LevelUp += Player_LevelUp;
        text = GetComponent<TextMeshProUGUI>();
    }

    

    private void OnDisable()
    {
        skillTreeManager.OnSkillChange -= SkillTreeManager_OnSkillChange;
        player.LevelUp -= Player_LevelUp;
    }
    private void SkillTreeManager_OnSkillChange()
    {
        text.text = $"Skill Point : {player.skillPoint}";
    }
    private void Player_LevelUp()
    {
        text.text = $"Skill Point : {player.skillPoint}";
    }
}
