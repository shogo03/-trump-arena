using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class MemoryBurstScoreManager : MonoBehaviour
{
    [Header("得点表示（プレイヤー1～4）")]
    [SerializeField]
    private TextMeshProUGUI[] playerScoreTexts = new TextMeshProUGUI[4];

    [Header("スコア表示設定")]
    [SerializeField]
    

    private int[] scores = new int[4];
    private int playerCount = 1;

    // ゲーム開始時
    public void Initialize(int count)
    {
        playerCount = Mathf.Clamp(count, 1, 4);

        scores = new int[4];

        UpdateScoreUI();
    }

    // 得点を加算
    public void AddScore(int player)
    {
        if (player < 1 || player > playerCount)
            return;

        scores[player - 1]++;

        UpdateScoreUI();

        Debug.Log(
            $"プレイヤー{player}の得点：{scores[player - 1]}"
        );
    }

    // 指定プレイヤーのスコアを取得
    public int GetScore(int player)
    {
        if (player < 1 || player > playerCount)
            return 0;

        return scores[player - 1];
    }

    // 全プレイヤーのスコアを表示
    private void UpdateScoreUI()
    {
        if (playerScoreTexts == null)
            return;

        for (int i = 0; i < playerScoreTexts.Length; i++)
        {
            if (playerScoreTexts[i] == null)
                continue;

            if (i < playerCount)
            {
                playerScoreTexts[i].gameObject.SetActive(true);

                playerScoreTexts[i].text =
                    $"P{i + 1}：{scores[i]}点";
            }
            else
            {
                playerScoreTexts[i].gameObject.SetActive(false);
            }
        }
    }

    // ゲーム終了時に結果を保存
    public void SaveResult()
    {
        int maxScore = 0;

        for (int i = 0; i < playerCount; i++)
        {
            maxScore = Mathf.Max(maxScore, scores[i]);
        }

        List<int> winners = new List<int>();

        for (int i = 0; i < playerCount; i++)
        {
            if (scores[i] == maxScore)
            {
                winners.Add(i + 1);
            }
        }

        GameResultData.PlayerCount = playerCount;
        GameResultData.Scores = (int[])scores.Clone();
        GameResultData.Winners = winners.ToArray();
        GameResultData.IsSinglePlayer = playerCount == 1;

        Debug.Log("ゲーム結果を保存しました。");
    }
}