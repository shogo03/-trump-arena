
using UnityEngine;
using UnityEngine.UI;

public class HighLowAbilityManager : MonoBehaviour
{
    [Header("ターン管理")]
    [SerializeField] private HighLowTurnManager turnManager;

    [Header("妨害ボタン")]
    [SerializeField] private Button player1DisruptButton;
    [SerializeField] private Button player2DisruptButton;

    [Header("使用回数")]
    [SerializeField] private int maxUses = 2;

    // プレイヤーごとの使用回数
    private int[] useCounts = new int[2];

    // 各プレイヤーが妨害されている予想
    private HLManager.Prediction[] blockedPredictions =
        new HLManager.Prediction[2];

    public int Player1Uses => useCounts[0];
    public int Player2Uses => useCounts[1];

    private void Start()
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

    // ゲーム開始時に初期化
    public void Initialize()
    {
        useCounts[0] = 0;
        useCounts[1] = 0;

        blockedPredictions[0] = HLManager.Prediction.None;
        blockedPredictions[1] = HLManager.Prediction.None;

        RefreshAbilityButtons();
    }

    // 妨害能力を使用
    public void UseDisruptAbility()
    {
        if (turnManager == null)
            return;

        int currentPlayer = turnManager.CurrentPlayer;

        if (currentPlayer < 1 || currentPlayer > 2)
            return;

        int index = currentPlayer - 1;

        // 使用回数の確認
        if (useCounts[index] >= maxUses)
            return;

        // すでに自分のターンで予想済みなら使用不可
        if (turnManager.GetPrediction(currentPlayer) !=
            HLManager.Prediction.None)
            return;

        // 相手プレイヤー
        int targetPlayer = currentPlayer == 1 ? 2 : 1;

        // HIGHかLOWをランダム決定
        HLManager.Prediction randomPrediction =
            Random.value < 0.5f
            ? HLManager.Prediction.High
            : HLManager.Prediction.Low;

        // 相手の妨害内容を保存
        blockedPredictions[targetPlayer - 1] = randomPrediction;

        // 使用回数を加算
        useCounts[index]++;

        Debug.Log($"PLAYER {currentPlayer} が妨害能力を使用！");
        Debug.Log(
            $"PLAYER {targetPlayer} の {randomPrediction} を禁止！"
        );

        RefreshAbilityButtons();

        FindFirstObjectByType<HLManager>()?
            .RefreshPredictionButtons();
    }

    // 妨害されているか確認
    public bool IsPredictionBlocked(
        int playerNumber,
        HLManager.Prediction prediction)
    {
        if (playerNumber < 1 || playerNumber > 2)
            return false;

        return blockedPredictions[playerNumber - 1] == prediction;
    }

    // プレイヤーが予想を終えたら妨害解除
    public void OnPredictionMade(int playerNumber)
    {
        if (playerNumber < 1 || playerNumber > 2)
            return;

        int index = playerNumber - 1;

        if (blockedPredictions[index] ==
            HLManager.Prediction.None)
            return;

        Debug.Log($"PLAYER {playerNumber} の妨害解除！");

        blockedPredictions[index] = HLManager.Prediction.None;

        RefreshAbilityButtons();

        FindFirstObjectByType<HLManager>()?
            .RefreshPredictionButtons();
    }

    // 能力ボタンの状態更新
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
                turnManager.GetPrediction(1) ==
                HLManager.Prediction.None;
        }

        if (player2DisruptButton != null)
        {
            player2DisruptButton.interactable =
                currentPlayer == 2 &&
                useCounts[1] < maxUses &&
                turnManager.GetPrediction(2) ==
                HLManager.Prediction.None;
        }
    }
}
