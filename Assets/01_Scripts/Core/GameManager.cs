using UnityEngine;

public class GameManager : MonoBehaviour
{

    private void Start()
    {
        StartGame();
    }

    private void StartGame() // 게임 시작했을때 시간 정상 작동
    {
        Time.timeScale = 1.0f;
        print("게임 시작");
    }

    public void PauseGame() // 게임을 일시 정지 외부에서 호출 할 수 있게 퍼블릭으로
    {
        Time.timeScale = 0.0f;
        print("게임 정지됨");
    }

    public void ResumeGame() // 게임 재개 외부 호출이 가능하게 퍼블릭으로 
    {
        Time.timeScale = 1.0f;
        print("게임 재개됨");
    }

    private void OnDestroy()
    {
        Time.timeScale = 1.0f; // 일시정지 상태에서 씬이 종료 되거나, 다른 씬으로 이동할 때 정지 상태가 남지 않게 복구
    }

}
