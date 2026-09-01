using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    private static GameSceneManager instance;

    #region 싱글톤
    public static GameSceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameSceneManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject(typeof(GameSceneManager).Name);
                    instance = obj.AddComponent<GameSceneManager>();
                }
            }
            return instance;
        }
    }
    #endregion
    private void Awake()
    {
        if (instance == null)
        {
            instance = this as GameSceneManager;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if(instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    public void LoadScene(EScene sceneType)
    {
        string sceneName = SceneNames.GetSceneName(sceneType);
        SceneManager.LoadScene(sceneName);
    }

    public bool SaveAndLoadScene(SaveGameService saveGameService, EScene sceneType)
    {
        if (saveGameService == null)
        {
            Debug.LogError("SaveGameService가 연결되지 않았습니다.");
            return false;
        }

        if (!saveGameService.SaveGame())
        {
            Debug.LogError("저장에 실패하여 씬 전환을 중지합니다.");
            return false;
        }

        LoadScene(sceneType);
        return true;
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
