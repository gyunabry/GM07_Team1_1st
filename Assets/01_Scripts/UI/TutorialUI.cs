using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("버튼 연결")]
    [SerializeField] private Button prevBtn;
    [SerializeField] private Button nextBtn;

    [Header("튜토리얼 UI")]
    [SerializeField] private GameObject uI_Tutorial;
    [SerializeField] private Tutorials[] tutorials;

    private readonly HashSet<int> seenTutorialLevels = new();
    private bool isLoadedGame;

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

    private void Awake()
    {
        isLoadedGame = SaveLoadRequest.HasPending;
    }

    private void Start()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.LevelUp += LevelCheck;
        }

        if (!isLoadedGame)
        {
            LevelCheck();
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
        if (CurrencySystem.Instance == null || uI_Tutorial.activeSelf)
        {
            return;
        }

        int currentLevel = CurrencySystem.Instance.Level;

        for (int i = 0; i < tutorials.Length; i++)
        {
            int popupLevel = tutorials[i].popupLevel;

            // 아직 보지 않은 가이드 오픈
            if (popupLevel <= currentLevel &&
                !seenTutorialLevels.Contains(popupLevel))
            {
                OpenTutorialPopup(i);
                return;
            }
        }
    }

    private void OpenTutorialPopup(int tutorialIndex)
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_Open);

        seenTutorialLevels.Add(tutorials[tutorialIndex].popupLevel);

        uI_Tutorial.SetActive(true);

        for (int i = 0; i < tutorials.Length; i++)
        {
            tutorials[i].levelPopupPanel.SetActive(false);
        }
        tutorials[tutorialIndex].levelPopupPanel.SetActive(true);

        for (int i = 0; i < tutorials[tutorialIndex].pages.Length; i++)
        {
            tutorials[tutorialIndex].pages[i].SetActive(false);
        }
        tutorials[tutorialIndex].pages[0].SetActive(true);

        currentPopupPanel = tutorialIndex;
        currentPage = 0;
        totalPage = tutorials[tutorialIndex].pages.Length - 1;
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

        LevelCheck();
    }

    #region 저장 및 복구
    public List<int> CaptureSeenTutorialLevels()
    {
        return new List<int>(seenTutorialLevels);
    }

    public void RestoreSeenTutorialLevels(List<int> savedLevels)
    {
        seenTutorialLevels.Clear();

        if (savedLevels != null)
        {
            foreach (int level in savedLevels)
            {
                seenTutorialLevels.Add(level);
            }
        }

        LevelCheck();
    }

    #endregion
}
