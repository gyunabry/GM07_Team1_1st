using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    private static GameSceneManager instance;
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
    public void OnClickTitleScene()
    {
        SceneManager.LoadScene(0);
    }
    public void OnClickLoadScene()
    {
        SceneManager.LoadScene(1);
    }
}
