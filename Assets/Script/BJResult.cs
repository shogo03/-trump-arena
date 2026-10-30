
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BJResult : MonoBehaviour
{
    // 勝敗を判定する
    public void CheckWinner(
        List<Card> playerHand,
        Text targetText,
        string playerName,
        int dealerTotal,
        bool playerBust,
        MoneyManager moneyManager)
    {
        int playerTotal = CalculateHandTotal(playerHand);

        int playerNumber = 0;

        if (playerName == "P1") playerNumber = 1;
        if (playerName == "P2") playerNumber = 2;
        if (playerName == "P3") playerNumber = 3;

        // プレイヤーがバーストした場合
        if (playerBust)
        {
            targetText.text = "負け！";

            if (moneyManager != null)
                moneyManager.Lose(playerNumber);
        }
        // ディーラーがバーストした場合
        else if (dealerTotal > 21)
        {
            targetText.text = "勝ち！";

            if (moneyManager != null)
                moneyManager.Win(playerNumber);
        }
        // プレイヤーの方が点数が高い場合
        else if (playerTotal > dealerTotal)
        {
            targetText.text = "勝ち！";

            if (moneyManager != null)
                moneyManager.Win(playerNumber);
        }
        // ディーラーの方が点数が高い場合
        else if (playerTotal < dealerTotal)
        {
            targetText.text = "負け！";

            if (moneyManager != null)
                moneyManager.Lose(playerNumber);
        }
        // 同じ点数の場合
        else
        {
            targetText.text = "引き分け！";

            if (moneyManager != null)
                moneyManager.Draw(playerNumber);
        }
    }

    // 手札の点数を計算する
    int CalculateHandTotal(List<Card> playerHand)
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in playerHand)
        {
            if (card.number == 1)
            {
                total += 11;
                aceCount++;
            }
            else if (card.number >= 11)
            {
                total += 10;
            }
            else
            {
                total += card.number;
            }
        }

        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }
}
