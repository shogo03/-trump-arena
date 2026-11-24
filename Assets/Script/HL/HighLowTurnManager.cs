
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
    [SerializeField] private float popupDuration = 1.5f;

    private int currentPlayer = 1;
    private Coroutine popupCoroutine;

    private HLManager.Prediction player1Prediction;
    private HLManager.Prediction player2Prediction;

    public int CurrentPlayer => currentPlayer;

    public HLManager.Prediction Player1Prediction => player1Prediction;
    public HLManager.Prediction Player2Prediction => player2Prediction;

    public void Initialize()
    {
        currentPlayer = 1;

        player1Prediction = HLManager.Prediction.None;
        player2Prediction = HLManager.Prediction.None;

        UpdateTurnText();
        ShowTurnPopup();
    }

    public void SetPrediction(HLManager.Prediction prediction)
    {
        if (currentPlayer == 1)
        {
            player1Prediction = prediction;
            currentPlayer = 2;

            UpdateTurnText();
            ShowTurnPopup();
        }
        else
        {
            player2Prediction = prediction;
        }
    }

    public bool BothPlayersPredicted()
    {
        return player1Prediction !=HLManager.Prediction.None &&
               player2Prediction !=HLManager.Prediction.None;
    }

    public void NextRound()
    {
        currentPlayer = 1;

        player1Prediction =HLManager.Prediction.None;
        player2Prediction =HLManager.Prediction.None;

        UpdateTurnText();
        ShowTurnPopup();
    }

    private void UpdateTurnText()
    {
        if (playerTurnText != null)
            playerTurnText.text = "PLAYER " + currentPlayer + " TURN";
    }


    private void ShowTurnPopup()
    {
        if (turnPopup == null || popupText == null)
            return;

        popupText.text = "PLAYER " + currentPlayer + " TURN";

        // プレイヤーごとに文字色を変更
        if (currentPlayer == 1)
        {
            popupText.color = Color.white;
        }
        else
        {
            popupText.color = Color.red;
        }

        turnPopup.SetActive(true);

        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        popupCoroutine = StartCoroutine(HidePopup());
    }


    private IEnumerator HidePopup()
    {
        yield return new WaitForSeconds(popupDuration);

        if (turnPopup != null)
            turnPopup.SetActive(false);

        popupCoroutine = null;
    }
}
