using UnityEngine;
using System.Collections.Generic;

public class MemoryBurstScoreManager : MonoBehaviour
{
    private int[] scores = new int[4];

    public void Initialize()
    {
        scores = new int[4];
    }

    public void AddScore(int player)
    {
        if (player < 1 || player > 4)
            return;

        scores[player - 1]++;

        Debug.Log($"プレイヤー{player}の得点：{scores[player - 1]}");
    }

    public int GetScore(int player)
    {
        if (player < 1 || player > 4)
            return 0;

        return scores[player - 1];
    }

    public int[] GetAllScores()
    {
        return (int[])scores.Clone();
    }

    public int[] GetWinners(int playerCount)
    {
        int maxScore = 0;

        for (int i = 0; i < playerCount; i++)
        {
            maxScore = Mathf.Max(maxScore, scores[i]);
        }

        List<int> winners = new();

        for (int i = 0; i < playerCount; i++)
        {
            if (scores[i] == maxScore)
                winners.Add(i + 1);
        }

        return winners.ToArray();
    }
}