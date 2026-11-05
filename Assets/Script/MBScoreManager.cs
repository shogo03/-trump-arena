
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class MemoryBurstScoreManager : MonoBehaviour
{
    [Header("得点表示（プレイヤー1～4）")]
    [SerializeField]
    private TextMeshProUGUI[] playerScoreTexts = new TextMeshProUGUI[4];

    private int[] scores = new int[4];
    private int playerCount = 1;

    public void Initialize(int count)
    {
        playerCount = count;
        scores = new int[4];

        UpdateScoreUI();
    }

    public void AddScore(int player)
    {
        if (player < 1 || player > playerCount)
            return;

        scores[player - 1]++;
        UpdateScoreUI();

        Debug.Log($"プレイヤー{player}の得点：{scores[player - 1]}");
    }

    public void SaveResult()
    {
        int maxScore = 0;

        for (int i = 0; i < playerCount; i++)
            maxScore = Mathf.Max(maxScore, scores[i]);

        List<int> winners = new();

        for (int i = 0; i < playerCount; i++)
        {
            if (scores[i] == maxScore)
                winners.Add(i + 1);
        }

        GameResultData.PlayerCount = playerCount;
        GameResultData.Scores = (int[])scores.Clone();
        GameResultData.Winners = winners.ToArray();
        GameResultData.IsSinglePlayer = playerCount == 1;
    }

    private void UpdateScoreUI()
    {
        if (playerScoreTexts == null)
            return;

        for (int i = 0; i < playerScoreTexts.Length; i++)
        {
            if (playerScoreTexts[i] != null)
            {
                playerScoreTexts[i].text =
                    $"PLAYER {i + 1}：{scores[i]}点";
            }
        }
    }
}
