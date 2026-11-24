using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class HighLowTurnManager : MonoBehaviour
{
    [Header("プレイヤー表示")]
    [SerializeField] private TextMeshProUGUI playerTurnText;

    //現在のプレイヤー
    private int currentPlayer = 1;

    //それぞれの予想
    private HLManager.Prediction player1Prediction;
    private HLManager.Prediction player2Prediction;

    public int CurrentPlayer => currentPlayer;

    public HLManager.Prediction Player1Prediction => player1Prediction;
    public HLManager.Prediction Player2Prediction => player2Prediction;


    //初期化
    public void Initialize()
    {
        currentPlayer = 1;

        player1Prediction = HLManager.Prediction.None;
        player2Prediction = HLManager.Prediction.None;

        UpdateTurnText();
    }

  
    //予想を登録
    public void SetPrediction(HLManager.Prediction prediction)
    {
        if(currentPlayer == 1)
        {
            player1Prediction = prediction;
            currentPlayer = 2;
        }

        else
        {
            player2Prediction = prediction;
        }

        UpdateTurnText();
    }

    //二人とも予想したか
    public bool BothPlayersPredicted()
    {
        return player1Prediction != HLManager.Prediction.None &&
               player2Prediction != HLManager.Prediction.None;
    }
   
    //次のラウンドへ
    public void NextRound()
    {
        currentPlayer = 1;

        player1Prediction = HLManager.Prediction.None;
        player2Prediction = HLManager.Prediction.None;

        UpdateTurnText();
    }

    //ターン表示
    private void UpdateTurnText()
    {
        if(playerTurnText != null)
        {
            playerTurnText.text = "PLAYER " + currentPlayer + " TURN";
        }
    }
}
