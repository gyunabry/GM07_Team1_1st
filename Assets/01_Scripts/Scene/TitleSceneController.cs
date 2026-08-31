using UnityEngine;
using UnityEngine.UI;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField] private GameObject titleOptionUI;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private GameObject warningSaveDeleteUI;

    private JsonSaveFileStore fileStore;

    private void Awake()
    {
        fileStore = new JsonSaveFileStore();
    }

    private void Start()
    {
        AudioManager.Instance.PlayBGM(EBGMType.Title);

        RefreshLoadButton();
    }

    // 저장 파일이 있을 때만 불러오기 버튼 활성화
    private void RefreshLoadButton()
    {
        if (loadGameButton != null)
        {
            loadGameButton.interactable = fileStore.Exists();
        }
    }

    // 인게임 시작
    public void OnClickLoadGameBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);

        if (!fileStore.TryLoad(out SaveGameData saveData))
        {
            Debug.LogWarning("저장 데이터를 불러올 수 없습니다.");

            if (loadGameButton != null)
            {
                loadGameButton.interactable = false;
            }

            return;
        }

        SaveLoadRequest.RequestLoad(saveData);

        GameSceneManager.Instance.LoadScene(EScene.Game);
    }

    // 새 게임 시작
    public void OnClickNewGameBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);

        // 불러오기 요청 제거
        SaveLoadRequest.Clear();

        // 기존 저장 파일을 삭제
        if (!fileStore.Delete())
        {
            Debug.LogError("기존 저장 파일을 삭제하지 못해 새 게임을 시작할 수 없습니다.");
            return;
        }

        GameSceneManager.Instance.LoadScene(EScene.Game);
    }

    public void OnClickResetGameBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);

        if (!fileStore.TryLoad(out SaveGameData saveData))
        {
            SaveLoadRequest.Clear();

            if (!fileStore.Delete())
            {
                Debug.LogError("기존 저장 파일을 삭제하지 못해 새 게임을 시작할 수 없습니다.");
                return;
            }

            GameSceneManager.Instance.LoadScene(EScene.Game);
        }
        else
        {
            warningSaveDeleteUI.SetActive(true);
            return;
        }
    }

    // 옵션 UI
    public void OnClickOptionBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        titleOptionUI.SetActive(true);
    }

    // 게임 끝내기
    public void OnClickExitBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
