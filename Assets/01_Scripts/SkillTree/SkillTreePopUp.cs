using TMPro;
using UnityEngine;

public class SkillTreePopUp : MonoBehaviour
{
    [SerializeField] GameObject descSprite;
    [SerializeField] GameObject descName;
    [SerializeField] GameObject descNeed;
    [SerializeField] GameObject descDesc;
    [SerializeField] MonsterPoolManager monsterPoolManager;

    public void MouseExit()
    {
        monsterPoolManager.ReturnPool(this);
    }
    public void SetSprite(Sprite sprite)
    {
        Sprite descSprite = this.descSprite.GetComponent<Sprite>(); 
        descSprite = sprite;
    }
    public void SetName(string name, int level)
    {
        TextMeshProUGUI text = descName.GetComponent<TextMeshProUGUI>();
        text.text = $"{name}({level})";
    }
    public void SetNeed(float needPoint, float needMoney)
    {
        TextMeshProUGUI text = descNeed.GetComponent<TextMeshProUGUI>();
        if (needPoint > 0)
        {
            text.text = $"Need Point : {needPoint}";
        }
        else if (needMoney > 0) 
        { 
            text.text = $"Need Money : {needMoney}";
        }
    }
    public void SetDesc(string desc)
    {
        TextMeshProUGUI text = descDesc.GetComponent<TextMeshProUGUI>();
        text.text = desc;
    }
}
