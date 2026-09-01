using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class LevelUpPopUp : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private CurrencySystem currencySystem;

    [Header("UI 연결")]
    [SerializeField] private GameObject levelUpPopUp;
    [SerializeField] private CanvasGroup levelUpCanvasGroup;
    [SerializeField] private CanvasGroup levelUpInfoCanvasGroup;

    private TextMeshProUGUI[] levelUpText;
    private int nowLevel = 0;
    private Coroutine co;

    private void OnEnable()
    {
        currencySystem.LevelUp += Instance_LevelUp;
        levelUpText = levelUpPopUp.GetComponentsInChildren<TextMeshProUGUI>();
        levelUpPopUp.SetActive(false);
    }
    private void OnDisable()
    {
        currencySystem.LevelUp -= Instance_LevelUp;
    }

    private void Instance_LevelUp()
    {
        //levelUpPopUp.SetActive(true);
        //co = StartCoroutine(DestroyPopUp());
        AudioManager.Instance.PlaySFX(ESFXType.PlayerLevelUp);
        PopUpOpen();

        for (int i = 1; i < 20; i += 2)
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
    //IEnumerator DestroyPopUp()
    //{
    //    yield return new WaitForSeconds(5f);
    //    levelUpPopUp.gameObject.SetActive(false);
    //    co = null;
    //}

    //DOTWeen 애니메이션
    private void PopUpOpen()
    {
        // 0. 상태/애니메이션 초기화
        levelUpCanvasGroup.DOKill();
        levelUpInfoCanvasGroup.DOKill();

        levelUpPopUp.SetActive(true);
        levelUpCanvasGroup.alpha = 0.0f;
        levelUpInfoCanvasGroup.alpha = 0.0f;

        // 1. 레벨업 시 팝업창 서서히 등장
        levelUpCanvasGroup.DOFade(1.0f, 0.3f);
        levelUpInfoCanvasGroup.DOFade(1.0f, 0.3f);

        Sequence sequence = DOTween.Sequence();

        // 2. 대기 후 서서히 사라짐
        sequence.AppendInterval(3.0f);
        sequence.OnComplete(() =>
        {
            levelUpCanvasGroup.DOFade(0.0f, 0.8f);
            levelUpInfoCanvasGroup.DOFade(0.0f, 0.8f).OnComplete(() =>
            {
                // 3. DOTween 재생 종료 후 비활성화
                PopUpClose();
            });
        });
    }

    private void PopUpClose()
    {
        levelUpCanvasGroup.alpha = 0.0f;
        levelUpInfoCanvasGroup.alpha = 0.0f;

        levelUpPopUp.SetActive(false);
    }
}
