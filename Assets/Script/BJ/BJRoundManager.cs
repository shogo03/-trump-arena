using UnityEngine;

public class BJRoundManager : MonoBehaviour
{
    // 現在のラウンド
    public int currentRound = 1;

    // 最大ラウンド数
    public int maxRound = 10;

    // 最終ラウンドか確認
    public bool IsLastRound()
    {
        return currentRound >= maxRound;
    }

    // 次のラウンドへ進む
    public void NextRound()
    {
        currentRound++;
    }

    // 最初のラウンドに戻す
    public void ResetRound()
    {
        currentRound = 1;
    }

    // ラウンド表示用の文字列を作る
    public string GetRoundText()
    {
        return "ROUND " + currentRound + " / " + maxRound;
    }
}