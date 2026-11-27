
using System.Collections;
using TMPro;
using UnityEngine;

public class HighLowTurnManager : MonoBehaviour
{
    [Header("ターン表示")]
    [SerializeField] private TextMeshProUGUI playerTurnText;

    [Header("ポップアップ表示")]
    [SerializeField] private GameObject turnPopup;
    [SerializeField] private TextMeshProUGUI popupText;

    [Header("表示設定")]
    [SerializeField] private float switchDelay = 0.3f;

    [Header("カーソル管理")]
    [SerializeField] private HighLowCursorManager cursorManager;

    [Header("能力管理")]
    [SerializeField] private HighLowAbilityManager abilityManager;

    private int currentPlayer = 1;
    private Coroutine popupCoroutine;

    private HLManager.Prediction player1Prediction;
    private HLManager.Prediction player2Prediction;

    public int CurrentPlayer => currentPlayer;

    public HLManager.Prediction Player1Prediction => player1Prediction;
    public HLManager.Prediction Player2Prediction => player2Prediction;

    public HLManager.Prediction GetPrediction(int playerNumber)
    {
        if (playerNumber == 1)
            return player1Prediction;

        if (playerNumber == 2)
            return player2Prediction;

        return HLManager.Prediction.None;
    }

    public void Initialize()
    {
        currentPlayer = 1;

        player1Prediction = HLManager.Prediction.None;
        player2Prediction = HLManager.Prediction.None;

        if (cursorManager != null)
            cursorManager.SetCurrentTurn(1);

        UpdateTurnText();
        ShowTurnPopup();
        RefreshAbilityButtons();
    }

    public void SetPrediction(HLManager.Prediction prediction)
    {
        if (currentPlayer == 1)
        {
            player1Prediction = prediction;
            currentPlayer = 2;

            if (cursorManager != null)
                cursorManager.SetCurrentTurn(2);
        }
        else
        {
            player2Prediction = prediction;
        }

        UpdateTurnText();
        ShowTurnPopup();
        RefreshAbilityButtons();
    }

    public bool BothPlayersPredicted()
    {
        return player1Prediction != HLManager.Prediction.None &&
               player2Prediction != HLManager.Prediction.None;
    }

    public void NextRound()
    {
        currentPlayer = 1;

        player1Prediction = HLManager.Prediction.None;
        player2Prediction = HLManager.Prediction.None;

        if (cursorManager != null)
            cursorManager.SetCurrentTurn(1);

        UpdateTurnText();
        ShowTurnPopup();
        RefreshAbilityButtons();
    }

    private void UpdateTurnText()
    {
        if (playerTurnText != null)
            playerTurnText.text = "PLAYER " + currentPlayer + " TURN";
    }

    private void ShowTurnPopup()
    {
        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        popupCoroutine = StartCoroutine(SwitchPopup());
    }

    private IEnumerator SwitchPopup()
    {
        if (turnPopup != null)
            turnPopup.SetActive(false);

        yield return new WaitForSeconds(switchDelay);

        if (turnPopup != null && popupText != null)
        {
            popupText.text = "PLAYER " + currentPlayer + " TURN";

            popupText.color = currentPlayer == 1
                ? Color.white
                : Color.red;

            turnPopup.SetActive(true);
        }

        popupCoroutine = null;
    }

    private void RefreshAbilityButtons()
    {
        if (abilityManager != null)
            abilityManager.RefreshAbilityButtons();
    }
}
