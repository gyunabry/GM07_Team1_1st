using UnityEngine;

public class TitleSceneController : MonoBehaviour
{
    // [SerializeField] private GameObject titleOptionUI;

    private void Start()
    {
        AudioManager.Instance.PlayBGM(EBGMType.Title);
    }

    // 인게임 시작
    public void OnClickLoadGameBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        GameSceneManager.Instance.LoadScene(EScene.Game);
    }

    // 새 게임 시작
    public void OnClickNewGameBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        GameSceneManager.Instance.LoadScene(EScene.Game);
        /*
        if (data != null) //진행 중인 데이터가 있을 경우 경고 및 삭제 진행
        {
            GameSceneManager.Instance.LoadScene(EScene.Game);
        }
        else //없을 경우 로딩
        {
            GameSceneManager.Instance.LoadScene(EScene.Game);
        }
        */
    }
    /*
    // 옵션 UI
    public void OnClickOptionBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        titleOptionUI.SetActive(true);
    }
    */
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
