using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class LevelUpPopUp : MonoBehaviour
{
    [SerializeField] private RectTransform levelUpPopUp;
    [SerializeField] private Player player;
    [SerializeField] private CurrencySystem currencySystem;

    private GameObject a;

    private TextMeshProUGUI[] levelUpText;
    private int nowLevel = 0;
    private Coroutine co;
    

    private void OnEnable()
    {
        currencySystem.LevelUp += Instance_LevelUp;
        levelUpText = levelUpPopUp.GetComponentsInChildren<TextMeshProUGUI>();
        Debug.Log(levelUpText.Length);
        levelUpPopUp.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        currencySystem.LevelUp -= Instance_LevelUp;
    }

    private void Instance_LevelUp()
    {
        levelUpPopUp.gameObject.SetActive(true);
        co = StartCoroutine(DestroyPopUp());
        for(int i = 1; i < 20; i += 2)
        {
            switch (i)
            {
                case 1:
                    levelUpText[i].gameObject.SetActive(false);
                    if (player.levelUpStats[nowLevel].attackDamage != 0)
                    {
                        levelUpText[i].gameObject.SetActive(true);
                        levelUpText[i].text = $"공격력";
                        levelUpText[i + 1].text = $"+{player.levelUpStats[nowLevel].attackDamage}";
                    }
                    break;
                case 3:
                    levelUpText[i].gameObject.SetActive(false);
                    if (player.levelUpStats[nowLevel].attackSpeed != 0)
                    {
                        levelUpText[i].gameObject.SetActive(true);
                        levelUpText[i].text = $"공격속도";
                        levelUpText[i + 1].text = $"+{player.levelUpStats[nowLevel].attackSpeed}";
                    }
                    break;
                case 5:
                    levelUpText[i].gameObject.SetActive(false);
                    if (player.levelUpStats[nowLevel].attackDistance != 0)
                    {
                        levelUpText[i].gameObject.SetActive(true);
                        levelUpText[i].text = $"공격범위";
                        levelUpText[i + 1].text = $"+{player.levelUpStats[nowLevel].attackDistance}";
                    }
                    break;
                case 7:
                    levelUpText[i].gameObject.SetActive(false);
                    if (player.levelUpStats[nowLevel].moveSpeed != 0)
                    {
                        levelUpText[i].gameObject.SetActive(true);
                        levelUpText[i].text = $"이동속도";
                        levelUpText[i + 1].text = $"+{player.levelUpStats[nowLevel].moveSpeed}";
                    }
                    break;
                case 9:
                    levelUpText[i].gameObject.SetActive(false);
                    break;
                case 11:
                    levelUpText[i].gameObject.SetActive(false);
                    break;
                case 13:
                    levelUpText[i].gameObject.SetActive(false);
                    break;
                case 15:
                    levelUpText[i].gameObject.SetActive(false);
                    break;
                case 17:
                    levelUpText[i].gameObject.SetActive(false);
                    break;
                case 19:
                    levelUpText[i].gameObject.SetActive(false);
                    break;
                default: break;
            }
        }
        nowLevel++;
    }
    IEnumerator DestroyPopUp()
    {
        yield return new WaitForSeconds(5f);
        levelUpPopUp.gameObject.SetActive(false);
        co = null;
    }
}
