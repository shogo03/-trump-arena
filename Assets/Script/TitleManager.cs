
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("シーン名")]
    [SerializeField] private string playerSelectSceneName = "PlayerSelect";



    // スタートボタン
    public void StartGame()
    {
        SceneManager.LoadScene(playerSelectSceneName);
    }

    // ゲーム終了
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("ゲームを終了します");
    }
}
