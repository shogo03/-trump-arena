
using TMPro;
using UnityEngine;

public class HighLowScoreManager : MonoBehaviour
{
    [Header("スコア表示")]
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] private TextMeshProUGUI player2ScoreText;

    private int player1Score = 0;
    private int player2Score = 0;

    public int Player1Score => player1Score;
    public int Player2Score => player2Score;

    // 初期化
    public void Initialize()
    {
        player1Score = 0;
        player2Score = 0;

        UpdateScoreUI();
    }

    // Player1に加点
    public void AddPlayer1Score(int point)
    {
        player1Score += point;
        UpdateScoreUI();

        Debug.Log($"Player1のスコア：{player1Score}");
    }

    // Player2に加点
    public void AddPlayer2Score(int point)
    {
        player2Score += point;
        UpdateScoreUI();

        Debug.Log($"Player2のスコア：{player2Score}");
    }

    // スコア表示更新
    private void UpdateScoreUI()
    {
        if (player1ScoreText != null)
            player1ScoreText.text = "P1 : " + player1Score;

        if (player2ScoreText != null)
            player2ScoreText.text = "P2 : " + player2Score;
    }
}
