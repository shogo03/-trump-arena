
using System.Collections.Generic;
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

        // 勝者表示
        if (playerCount == 1)
        {
            winnerText.text = "GAME CLEAR!";
        }
        else if (winners != null && winners.Length >= 2)
        {
            winnerText.text = "DRAW!";
        }
        else if (winners != null && winners.Length == 1)
        {
            winnerText.text = $"{GetPlayerName(winners[0])} WIN!";
        }
        else
        {
            winnerText.text = "GAME OVER";
        }

        // ランキング作成
        List<int> ranking = new List<int>();

        for (int i = 0; i < playerCount && i < scores.Length; i++)
        {
            ranking.Add(i);
        }

        ranking.Sort((a, b) =>
            scores[b].CompareTo(scores[a]));

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("FINAL RANKING");
        sb.AppendLine();

        int previousScore = -1;
        int rank = 0;

        for (int i = 0; i < ranking.Count; i++)
        {
            int playerIndex = ranking[i];
            int score = scores[playerIndex];

            // 同点なら同じ順位
            if (i == 0 || score != previousScore)
            {
                rank = i + 1;
            }

            previousScore = score;

            string rankText = rank switch
            {
                1 => "1st",
                2 => "2nd",
                3 => "3rd",
                _ => $"{rank}th"
            };

            sb.AppendLine(
                $"{rankText}  {GetPlayerName(playerIndex + 1)}      {score} pt"
            );

            if (GameSettings.IsCPU[playerIndex])
            {
                sb.AppendLine(
                    $"      {GetCPULevelName(playerIndex)}"
                );
            }
        }

        scoreText.text = sb.ToString();
    }

    // プレイヤー名
    private string GetPlayerName(int playerNumber)
    {
        int index = playerNumber - 1;

        if (index >= 0 &&
            index < GameSettings.IsCPU.Length &&
            GameSettings.IsCPU[index])
        {
            return $"CPU {playerNumber}";
        }

        return $"PLAYER {playerNumber}";
    }

    // CPU難易度
    private string GetCPULevelName(int index)
    {
        if (index < 0 || index >= GameSettings.CPULevels.Length)
            return "";

        return GameSettings.CPULevels[index] switch
        {
            1 => "EASY",
            2 => "NORMAL",
            3 => "HARD",
            _ => "UNKNOWN"
        };
    }

    // リトライ
    public void RetryGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // タイトルへ戻る
    public void ReturnToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}
