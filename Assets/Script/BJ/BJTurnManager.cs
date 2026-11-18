using UnityEngine;
using UnityEngine.UI;

public class BJTurnManager : MonoBehaviour
{
    // 現在のターン（1=P1、2=P2、3=P3）
    public int currentTurn = 1;


    // P1のボタン

    public Button[] player1Buttons;

    // P2のボタン
    public Button[] player2Buttons;

    // P3のボタン
    public Button[] player3Buttons;


    // 初期化

    void Start()
    {
        UpdateButtonState();
    }


    // ターンを最初に戻す

    public void ResetTurn()
    {
        currentTurn = 1;
        UpdateButtonState();
    }


    // 次のターンへ進む

    public int NextTurn(bool[] activePlayers, bool[] finishedPlayers)
    {
        currentTurn++;

        // 参加していないプレイヤーや
        // 終了したプレイヤーを飛ばす
        while (currentTurn <= 3)
        {
            int index = currentTurn - 1;

            if (activePlayers[index] && !finishedPlayers[index])
            {
                UpdateButtonState();
                return currentTurn;
            }

            currentTurn++;
        }

        // 全員終了
        UpdateButtonState();

        return 4;
    }


    // 現在のターンを取得

    public int GetCurrentTurn()
    {
        return currentTurn;
    }


    // ボタンのON/OFF

    void UpdateButtonState()
    {
        // P1
        SetButtonsInteractable(player1Buttons, currentTurn == 1);

        // P2
        SetButtonsInteractable(player2Buttons, currentTurn == 2);

        // P3
        SetButtonsInteractable(player3Buttons, currentTurn == 3);
    }


    void SetButtonsInteractable(Button[] buttons, bool interactable)
    {
        if (buttons == null)
            return;

        foreach (Button button in buttons)
        {
            if (button != null)
                button.interactable = interactable;
        }
    }
}