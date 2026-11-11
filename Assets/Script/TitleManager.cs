
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("シーン名")]
    [SerializeField] private string GameSelectSceneName = "GameSelect";



    // スタートボタン
    public void StartGame()
    {
        SceneManager.LoadScene(GameSelectSceneName);
    }

    // ゲーム終了
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("ゲームを終了します");
    }
}
