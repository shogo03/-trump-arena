using UnityEngine;
using UnityEngine.UI;

public class BJTurnManager : MonoBehaviour
{
    public int currentTurn = 1;

    public Button[] player1Buttons;
    public Button[] player2Buttons;
    public Button[] player3Buttons;

    void Start()
    {
        UpdateButtonState();
    }

    public void ResetTurn()
    {
        currentTurn = 1;
        UpdateButtonState();
    }

    public int NextTurn(bool[] activePlayers, bool[] finishedPlayers)
    {
        currentTurn++;

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

        UpdateButtonState();

        return 4;
    }

    public int GetCurrentTurn()
    {
        return currentTurn;
    }


    // ボタンの有効・無効を切り替える
    public void UpdateButtonState()
    {
        SetButtons(player1Buttons, currentTurn == 1);
        SetButtons(player2Buttons, currentTurn == 2);
        SetButtons(player3Buttons, currentTurn == 3);
    }


    void SetButtons(Button[] buttons, bool enabled)
    {
        if (buttons == null)
            return;

        foreach (Button button in buttons)
        {
            if (button != null)
                button.interactable = enabled;
        }
    }
}