
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
        return player1Prediction != HLManager.Prediction.None &&
               player2Prediction != HLManager.Prediction.None;
    }

    public void NextRound()
    {
        currentPlayer = 1;

        player1Prediction = HLManager.Prediction.None;
        player2Prediction = HLManager.Prediction.None;

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
        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        popupCoroutine = StartCoroutine(SwitchPopup());
    }

    private IEnumerator SwitchPopup()
    {
        // いったん非表示
        if (turnPopup != null)
            turnPopup.SetActive(false);

        // 少し待つ
        yield return new WaitForSeconds(switchDelay);

        if (turnPopup != null && popupText != null)
        {
            popupText.text = "PLAYER " + currentPlayer + " TURN";

            // プレイヤーごとに文字色を変更
            popupText.color = currentPlayer == 1
                ? Color.white
                : Color.red;

            // 再表示
            turnPopup.SetActive(true);
        }

        popupCoroutine = null;
    }
}
