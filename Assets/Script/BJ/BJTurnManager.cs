using UnityEngine;

public class BJTurnManager : MonoBehaviour
{
    // 現在のターン（1=P1、2=P2、3=P3）
    public int currentTurn = 1;

    // ターンを最初に戻す
    public void ResetTurn()
    {
        currentTurn = 1;
    }

    // 次のターンへ進む
    public int NextTurn(bool[] activePlayers, bool[] finishedPlayers)
    {
        currentTurn++;

        // 参加していないプレイヤーや終了したプレイヤーを飛ばす
        while (currentTurn <= 3)
        {
            int index = currentTurn - 1;

            if (activePlayers[index] && !finishedPlayers[index])
            {
                return currentTurn;
            }

            currentTurn++;
        }

        // 全員終了
        return 4;
    }

    public int GetCurrentTurn()
    {
        return currentTurn;
    }
}