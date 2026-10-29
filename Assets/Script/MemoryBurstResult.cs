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

        // 勝者を表示
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

        // 全プレイヤーの得点を表示
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < playerCount; i++)
        {
            sb.AppendLine(
                $"プレイヤー{i + 1}：{scores[i]}点"
            );
        }

        scoreText.text = sb.ToString();
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}