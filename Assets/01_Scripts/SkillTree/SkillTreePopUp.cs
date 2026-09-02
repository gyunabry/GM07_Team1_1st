using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreePopUp : MonoBehaviour
{
    [SerializeField] GameObject descSprite;
    [SerializeField] GameObject descName;
    [SerializeField] GameObject descNeed;
    [SerializeField] GameObject descDesc;
    [SerializeField] GameObject descChange;
    [SerializeField] PoolManager poolManager;

    
    public void SetSprite(Sprite sprite)
    {
        Image image = descSprite.GetComponent<Image>();
        image.sprite = sprite;
    }
    public void SetName(string name)
    {
        TextMeshProUGUI text = descName.GetComponent<TextMeshProUGUI>();
        text.text = $"{name}";
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
    public void SetValue(string changeStat, float value)
    {
        TextMeshProUGUI text = descChange.GetComponent<TextMeshProUGUI>();
        if(value != 0)
        {
            text.text = $"{changeStat} | {value}";
        }
        else
        {
            text.text = $"{changeStat}";
        }
        
    }
    public void SetDesc(string desc)
    {
        TextMeshProUGUI text = descDesc.GetComponent<TextMeshProUGUI>();
        text.text = desc;
    }
}
