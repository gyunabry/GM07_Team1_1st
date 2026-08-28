using UnityEngine;

public class _TestSystem : MonoBehaviour
{
    [SerializeField] private int testGold;
    [SerializeField] private int testEXP;

    public void OnClickGold()
    {
        CurrencySystem.Instance.GrantMoney(testGold);
    }

    public void OnClickEXP()
    {
        CurrencySystem.Instance.GrantExperience(testEXP);
    }
}
