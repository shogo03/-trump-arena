
using UnityEngine;
using UnityEngine.UI;

public class HighLowAbilityManager : MonoBehaviour
{
    [Header("ターン管理")]
    [SerializeField] private HighLowTurnManager turnManager;

    [Header("妨害能力ボタン")]
    [SerializeField] private Button player1DisruptButton;
    [SerializeField] private Button player2DisruptButton;

    [Header("使用回数")]
    [SerializeField] private int maxUses = 2;

    private int[] useCounts = new int[2];

    // 妨害対象のプレイヤー
    private int blockedPlayer = 0;

    // 使用できなくする予想
    private HLManager.Prediction blockedPrediction =
        HLManager.Prediction.None;

    public int Player1Uses => useCounts[0];
    public int Player2Uses => useCounts[1];

    public void　Initialize()
    {
        if (player1DisruptButton != null)
            player1DisruptButton.onClick.AddListener(UseDisruptAbility);

        if (player2DisruptButton != null)
            player2DisruptButton.onClick.AddListener(UseDisruptAbility);

        RefreshAbilityButtons();
    }

    private void OnDestroy()
    {
        if (player1DisruptButton != null)
            player1DisruptButton.onClick.RemoveListener(UseDisruptAbility);

        if (player2DisruptButton != null)
            player2DisruptButton.onClick.RemoveListener(UseDisruptAbility);
    }

    // 妨害能力を使用
    public void UseDisruptAbility()
    {
        if (turnManager == null)
            return;

        int currentPlayer = turnManager.CurrentPlayer;

        // 使用回数の確認
        if (useCounts[currentPlayer - 1] >= maxUses)
            return;

        // すでに妨害効果が発動中なら使用できない
        if (blockedPlayer != 0)
            return;

        // 使用回数を加算
        useCounts[currentPlayer - 1]++;

        // 相手プレイヤーを妨害対象にする
        blockedPlayer = currentPlayer == 1 ? 2 : 1;

        // HIGH・LOWをランダムで決定
        blockedPrediction = Random.value < 0.5f
            ? HLManager.Prediction.High
            : HLManager.Prediction.Low;

        Debug.Log(
            $"PLAYER {currentPlayer} が妨害能力を使用！"
        );

        Debug.Log(
            $"PLAYER {blockedPlayer} の {blockedPrediction} を禁止！"
        );

        RefreshAbilityButtons();

        // HLManagerにボタン状態を更新させる
        FindHLManager()?.RefreshPredictionButtons();
    }

    // 指定プレイヤーの予想が禁止されているか
    public bool IsPredictionBlocked(
        int playerNumber,
        HLManager.Prediction prediction)
    {
        return blockedPlayer == playerNumber &&
               blockedPrediction == prediction;
    }

    // 相手が予想を終えたときに呼び出す
    public void OnPredictionMade(int playerNumber)
    {
        if (blockedPlayer != playerNumber)
            return;

        Debug.Log(
            $"PLAYER {playerNumber} の妨害効果が終了！"
        );

        blockedPlayer = 0;
        blockedPrediction = HLManager.Prediction.None;

        FindHLManager()?.RefreshPredictionButtons();
    }

    // 能力ボタンの有効・無効
    public void RefreshAbilityButtons()
    {
        if (turnManager == null)
            return;

        int currentPlayer = turnManager.CurrentPlayer;

        if (player1DisruptButton != null)
        {
            player1DisruptButton.interactable =
                currentPlayer == 1 &&
                useCounts[0] < maxUses &&
                blockedPlayer == 0;
        }

        if (player2DisruptButton != null)
        {
            player2DisruptButton.interactable =
                currentPlayer == 2 &&
                useCounts[1] < maxUses &&
                blockedPlayer == 0;
        }
    }

    // HLManagerを取得
    private HLManager FindHLManager()
    {
        return FindFirstObjectByType<HLManager>();
    }
}
