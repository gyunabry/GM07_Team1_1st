using UnityEngine;

public class MainSceneController : MonoBehaviour
{
    private void Start()
    {
        AudioManager.Instance.PlayBGM(EBGMType.Atelier);
    }
}
