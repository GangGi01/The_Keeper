using UnityEngine;

public class UIManager : MonoBehaviour
{
    public void GoToSingleScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("02_Single_Scene");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        // 에디터에서 테스트용 (플레이 모드 종료)
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서는 완전 종료
        Application.Quit();
#endif
    }
}
