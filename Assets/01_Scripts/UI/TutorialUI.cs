using UnityEngine;
using UnityEngine.UI;
using static TutorialUI;

public class TutorialUI : MonoBehaviour
{
    [Header("버튼 연결")]
    [SerializeField] private Button prevBtn;
    [SerializeField] private Button nextBtn;

    [Header("튜토리얼 UI")]
    [SerializeField] private GameObject uI_Tutorial;
    [SerializeField] private Tutorials[] tutorials;

    [System.Serializable]
    public struct Tutorials
    {
        public int popupLevel;
        public GameObject levelPopupPanel;
        public GameObject[] pages;
    }

    private int currentPopupPanel;
    private int currentPage;
    private int totalPage;

    private void Start()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.LevelUp += LevelCheck;
        }
    }

    private void OnEnable()
    {
        if (uI_Tutorial != null)
        {
            uI_Tutorial.SetActive(false);
        }
    }
    private void OnDisable()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.LevelUp -= LevelCheck;
        }
    }

    //새 게임 시작 시 1번 튜토리얼 리스트가 열리는 기능
    private void NewGameCheck()
    {
        OpenTutorialPopup(0);
    }

    //popupLevel에 해당하는 레벨이 되면 해당 튜토리얼 리스트가 열리도록 하는 기능
    private void LevelCheck()
    {
        for (int i = 0; i < tutorials.Length; i++)
        {
            if (CurrencySystem.Instance.Level == tutorials[i].popupLevel)
            {
                OpenTutorialPopup(i);
                return;
            }
        }
    }

    private void OpenTutorialPopup(int level)
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_Open);

        uI_Tutorial.SetActive(true);

        for (int i = 0; i < tutorials.Length; i++)
        {
            tutorials[i].levelPopupPanel.SetActive(false);
        }
        tutorials[level].levelPopupPanel.SetActive(true);

        for (int i = 0; i < tutorials[level].pages.Length; i++)
        {
            tutorials[level].pages[i].SetActive(false);
        }
        tutorials[level].pages[0].SetActive(true);

        currentPopupPanel = level;
        currentPage = 0;
        totalPage = tutorials[level].pages.Length - 1;
        CheckPage();
    }

    //튜토리얼 다음으로 넘기기 기능
    public void OnClickNextPage()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        currentPage++;

        for (int i = 0; i < tutorials[currentPopupPanel].pages.Length; i++)
        {
            if (currentPage == i)
            {
                tutorials[currentPopupPanel].pages[i].SetActive(true);
            }
            else
            {
                tutorials[currentPopupPanel].pages[i].SetActive(false);
            }
        }
        CheckPage();
    }

    //튜토리얼 이전으로 넘기기 기능
    public void OnClickPrevPage()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        currentPage--;

        for (int i = 0; i < tutorials[currentPopupPanel].pages.Length; i++)
        {
            if (currentPage == i)
            {
                tutorials[currentPopupPanel].pages[i].SetActive(true);
            }
            else
            {
                tutorials[currentPopupPanel].pages[i].SetActive(false);
            }
        }
        CheckPage();
    }

    //이전or다음 페이지가 없다면 넘기기 버튼 비활성화
    private void CheckPage()
    {
        if (currentPage <= 0)
        {
            prevBtn.interactable = false;
        }
        else
        {
            prevBtn.interactable = true;
        }

        if (currentPage >= totalPage)
        {
            nextBtn.interactable = false;
        }
        else
        {
            nextBtn.interactable = true;
        }
    }

    // Option -> Help
    public void ClickHelpPopup()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);

        OpenTutorialPopup(0);
    }

    // 닫기
    public void CloseTutorialPopup()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_Close);

        uI_Tutorial.SetActive(false);
    }
}
