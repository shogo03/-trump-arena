
using UnityEngine;
using UnityEngine.UI;

public class HighLowAbilityManager : MonoBehaviour
{
    [Header("ターン管理")]
    [SerializeField] private HighLowTurnManager turnManager;

    [Header("妨害ボタン")]
    [SerializeField] private Button player1DisruptButton;
    [SerializeField] private Button player2DisruptButton;

    [Header("革命ボタン")]
    [SerializeField] private Button player1RevolutionButton;
    [SerializeField] private Button player2RevolutionButton;

    [Header("使用回数")]
    [SerializeField] private int maxUses = 2;

    [Header("革命設定")]
    [SerializeField] private int revolutionDuration = 5;

    // 妨害能力の使用回数
    private int[] useCounts = new int[2];

    // 妨害されている予想
    private HLManager.Prediction[] blockedPredictions =
        new HLManager.Prediction[2];

    // 革命の使用済み判定
    private bool[] revolutionUsed = new bool[2];

    // 革命の状態
    private bool isRevolutionActive = false;
    private int revolutionRemainingTurns = 0;

    // ターン監視
    private int lastObservedPlayer = 0;

    public int Player1Uses => useCounts[0];
    public int Player2Uses => useCounts[1];

    public bool IsRevolutionActive => isRevolutionActive;
    public int RevolutionRemainingTurns => revolutionRemainingTurns;

    // HLManagerから参照する
    public bool ShouldReverseHighLow => isRevolutionActive;

    private void Start()
    {
        if (player1DisruptButton != null)
            player1DisruptButton.onClick.AddListener(UseDisruptAbility);

        if (player2DisruptButton != null)
            player2DisruptButton.onClick.AddListener(UseDisruptAbility);

        if (player1RevolutionButton != null)
            player1RevolutionButton.onClick.AddListener(UseRevolutionAbility);

        if (player2RevolutionButton != null)
            player2RevolutionButton.onClick.AddListener(UseRevolutionAbility);

        if (turnManager != null)
            lastObservedPlayer = turnManager.CurrentPlayer;

        RefreshAbilityButtons();
    }

    private void Update()
    {
        if (turnManager == null)
            return;

        int currentPlayer = turnManager.CurrentPlayer;

        // プレイヤーが切り替わったか確認
        if (currentPlayer != lastObservedPlayer)
        {
            lastObservedPlayer = currentPlayer;

            // 革命中なら残りターンを減らす
            if (isRevolutionActive)
            {
                revolutionRemainingTurns--;

                Debug.Log(
                    $"革命残りターン：{revolutionRemainingTurns}"
                );

                if (revolutionRemainingTurns <= 0)
                {
                    EndRevolution();
                }
            }

            RefreshAbilityButtons();
        }
    }

    private void OnDestroy()
    {
        if (player1DisruptButton != null)
            player1DisruptButton.onClick.RemoveListener(UseDisruptAbility);

        if (player2DisruptButton != null)
            player2DisruptButton.onClick.RemoveListener(UseDisruptAbility);

        if (player1RevolutionButton != null)
            player1RevolutionButton.onClick.RemoveListener(UseRevolutionAbility);

        if (player2RevolutionButton != null)
            player2RevolutionButton.onClick.RemoveListener(UseRevolutionAbility);
    }

    // ゲーム開始時に初期化
    public void Initialize()
    {
        useCounts[0] = 0;
        useCounts[1] = 0;

        blockedPredictions[0] = HLManager.Prediction.None;
        blockedPredictions[1] = HLManager.Prediction.None;

        revolutionUsed[0] = false;
        revolutionUsed[1] = false;

        isRevolutionActive = false;
        revolutionRemainingTurns = 0;

        if (turnManager != null)
            lastObservedPlayer = turnManager.CurrentPlayer;

        RefreshAbilityButtons();
    }

    // =========================
    // 妨害能力
    // =========================

    public void UseDisruptAbility()
    {
        if (turnManager == null)
            return;

        int currentPlayer = turnManager.CurrentPlayer;

        if (currentPlayer < 1 || currentPlayer > 2)
            return;

        int index = currentPlayer - 1;

        if (useCounts[index] >= maxUses)
            return;

        if (turnManager.GetPrediction(currentPlayer) !=
            HLManager.Prediction.None)
            return;

        int targetPlayer = currentPlayer == 1 ? 2 : 1;

        HLManager.Prediction randomPrediction =
            Random.value < 0.5f
            ? HLManager.Prediction.High
            : HLManager.Prediction.Low;

        blockedPredictions[targetPlayer - 1] = randomPrediction;

        useCounts[index]++;

        Debug.Log($"PLAYER {currentPlayer} が妨害能力を使用！");
        Debug.Log(
            $"PLAYER {targetPlayer} の {randomPrediction} を禁止！"
        );

        RefreshAbilityButtons();

        FindFirstObjectByType<HLManager>()?
            .RefreshPredictionButtons();
    }

    public bool IsPredictionBlocked(
        int playerNumber,
        HLManager.Prediction prediction)
    {
        if (playerNumber < 1 || playerNumber > 2)
            return false;

        return blockedPredictions[playerNumber - 1] == prediction;
    }

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

    // =========================
    // 革命能力
    // =========================

    public void UseRevolutionAbility()
    {
        if (turnManager == null)
            return;

        int currentPlayer = turnManager.CurrentPlayer;

        if (currentPlayer < 1 || currentPlayer > 2)
            return;

        int index = currentPlayer - 1;

        // すでに使用済みなら不可
        if (revolutionUsed[index])
            return;

        // すでに革命中なら不可
        if (isRevolutionActive)
            return;

        // 予想済みなら不可
        if (turnManager.GetPrediction(currentPlayer) !=
            HLManager.Prediction.None)
            return;

        // 使用済みにする
        revolutionUsed[index] = true;

        // 革命開始
        isRevolutionActive = true;
        revolutionRemainingTurns = revolutionDuration;

        Debug.Log($"PLAYER {currentPlayer} が革命を発動！");
        Debug.Log("HIGHとLOWの正解判定が逆転！");
        Debug.Log($"革命はあと{revolutionRemainingTurns}ターン！");

        RefreshAbilityButtons();

        FindFirstObjectByType<HLManager>()?
            .RefreshPredictionButtons();
    }

    // 革命解除
    private void EndRevolution()
    {
        isRevolutionActive = false;
        revolutionRemainingTurns = 0;

        Debug.Log("革命終了！通常のHIGH・LOW判定に戻りました。");

        RefreshAbilityButtons();

        FindFirstObjectByType<HLManager>()?
            .RefreshPredictionButtons();
    }

    // =========================
    // ボタン更新
    // =========================

    public void RefreshAbilityButtons()
    {
        if (turnManager == null)
            return;

        int currentPlayer = turnManager.CurrentPlayer;

        bool canUse =
            currentPlayer >= 1 &&
            currentPlayer <= 2 &&
            turnManager.GetPrediction(currentPlayer) ==
            HLManager.Prediction.None;

        if (player1DisruptButton != null)
        {
            player1DisruptButton.interactable =
                currentPlayer == 1 &&
                useCounts[0] < maxUses &&
                canUse;
        }

        if (player2DisruptButton != null)
        {
            player2DisruptButton.interactable =
                currentPlayer == 2 &&
                useCounts[1] < maxUses &&
                canUse;
        }

        if (player1RevolutionButton != null)
        {
            player1RevolutionButton.interactable =
                currentPlayer == 1 &&
                !revolutionUsed[0] &&
                !isRevolutionActive &&
                canUse;
        }

        if (player2RevolutionButton != null)
        {
            player2RevolutionButton.interactable =
                currentPlayer == 2 &&
                !revolutionUsed[1] &&
                !isRevolutionActive &&
                canUse;
        }
    }
}
