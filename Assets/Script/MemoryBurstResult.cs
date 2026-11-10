
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MemoryBurstResult : MonoBehaviour
{
    [Header("表示")]
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("シーン名")]
    [SerializeField] private string gameSceneName = "MemoryBurst";
    [SerializeField] private string titleSceneName = "Title";

    private void Start()
    {
        ShowResult();
    }

    private void ShowResult()
    {
        int playerCount = GameResultData.PlayerCount;
        int[] scores = GameResultData.Scores;
        int[] winners = GameResultData.Winners;

        if (playerCount <= 0 || scores == null)
        {
            winnerText.text = "結果データがありません";
            scoreText.text = "";
            return;
        }

        if (playerCount == 1)
        {
            winnerText.text = "ゲームクリア！";
        }
        else if (winners.Length >= 2)
        {
            winnerText.text = "引き分け！";
        }
        else if (winners.Length == 1)
        {
            winnerText.text = $"プレイヤー{winners[0]}の勝利！";
        }
        else
        {
            winnerText.text = "ゲーム終了";
        }

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < playerCount && i < scores.Length; i++)
        {
            sb.AppendLine($"プレイヤー{i + 1}：{scores[i]}点");
        }

        scoreText.text = sb.ToString();
    }

    // リトライ
    public void RetryGame()
    {
        Debug.Log("リトライボタンが押されました");
        SceneManager.LoadScene(gameSceneName);
    }

    // タイトルへ戻る
    public void ReturnToTitle()
    {
        Debug.Log("タイトルボタンが押されました");
        SceneManager.LoadScene(titleSceneName);
    }
}
