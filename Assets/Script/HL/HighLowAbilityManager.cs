
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
    [SerializeField] private int maxRevolutionUses = 2;

    [Header("革命設定")]
    [SerializeField] private int revolutionDuration = 10;

    // 妨害使用回数
    private int[] useCounts = new int[2];

    // 妨害対象
    private HLManager.Prediction[] blockedPredictions =
        new HLManager.Prediction[2];

    // 革命使用回数
    private int[] revolutionUseCounts = new int[2];

    // 各プレイヤーが現在のターンに能力を使ったか
    private bool[] abilityUsedThisTurn = new bool[2];

    // 革命状態
    private bool isRevolutionActive;
    private int revolutionPlayer;
    private int revolutionRemainingTurns;

    // ターン監視
    private int lastObservedPlayer;

    public int Player1Uses => useCounts[0];
    public int Player2Uses => useCounts[1];

    public int Player1RevolutionUses => revolutionUseCounts[0];
    public int Player2RevolutionUses => revolutionUseCounts[1];

    public bool IsRevolutionActive => isRevolutionActive;
    public int RevolutionRemainingTurns => revolutionRemainingTurns;

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

        if (currentPlayer != lastObservedPlayer)
        {
            lastObservedPlayer = currentPlayer;

            // 新しいターンなので能力使用権を回復
            if (currentPlayer >= 1 && currentPlayer <= 2)
                abilityUsedThisTurn[currentPlayer - 1] = false;

            // 革命の残りターンを減らす
            if (isRevolutionActive)
            {
                revolutionRemainingTurns--;

                Debug.Log($"革命残りターン：{revolutionRemainingTurns}");

                if (revolutionRemainingTurns <= 0)
                    EndRevolution();
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


    // =========================
    // 初期化
    // =========================

    public void Initialize()
    {
        useCounts[0] = 0;
        useCounts[1] = 0;

        revolutionUseCounts[0] = 0;
        revolutionUseCounts[1] = 0;

        abilityUsedThisTurn[0] = false;
        abilityUsedThisTurn[1] = false;

        blockedPredictions[0] = HLManager.Prediction.None;
        blockedPredictions[1] = HLManager.Prediction.None;

        isRevolutionActive = false;
        revolutionPlayer = 0;
        revolutionRemainingTurns = 0;

        if (turnManager != null)
            lastObservedPlayer = turnManager.CurrentPlayer;

        RefreshAbilityButtons();

        Debug.Log("能力を初期化しました。");
    }


    // =========================
    // 能力使用チェック
    // =========================

    private bool CanUseAbility(int player)
    {
        if (player < 1 || player > 2)
            return false;

        int index = player - 1;

        if (turnManager == null)
            return false;

        // 自分のターンでなければ不可
        if (turnManager.CurrentPlayer != player)
            return false;

        // このターンにすでに能力を使っていたら不可
        if (abilityUsedThisTurn[index])
            return false;

        // 予想済みなら不可
        if (turnManager.GetPrediction(player) != HLManager.Prediction.None)
            return false;

        return true;
    }


    private void ConsumeAbility(int player)
    {
        abilityUsedThisTurn[player - 1] = true;
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
        int index = currentPlayer - 1;

        if (!CanUseAbility(currentPlayer))
            return;

        if (useCounts[index] >= maxUses)
            return;

        int targetPlayer = currentPlayer == 1 ? 2 : 1;

        HLManager.Prediction randomPrediction =
            Random.value < 0.5f
            ? HLManager.Prediction.High
            : HLManager.Prediction.Low;

        blockedPredictions[targetPlayer - 1] = randomPrediction;

        useCounts[index]++;

        // このターンの能力使用権を消費
        ConsumeAbility(currentPlayer);

        Debug.Log($"PLAYER {currentPlayer} が妨害能力を使用！");
        Debug.Log($"PLAYER {targetPlayer} の {randomPrediction} を禁止！");
        Debug.Log($"妨害残り回数：{maxUses - useCounts[index]}");

        FindFirstObjectByType<HLManager>()?.RefreshPredictionButtons();
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

        if (blockedPredictions[index] == HLManager.Prediction.None)
            return;

        blockedPredictions[index] = HLManager.Prediction.None;

        RefreshAbilityButtons();

        FindFirstObjectByType<HLManager>()?.RefreshPredictionButtons();

        Debug.Log($"PLAYER {playerNumber} の妨害解除！");
    }


    // =========================
    // 革命能力
    // =========================

    public void UseRevolutionAbility()
    {
        if (turnManager == null)
            return;

        int currentPlayer = turnManager.CurrentPlayer;
        int index = currentPlayer - 1;

        if (!CanUseAbility(currentPlayer))
            return;

        if (revolutionUseCounts[index] >= maxRevolutionUses)
            return;

        // 革命中の場合
        if (isRevolutionActive)
        {
            // 発動した本人は解除できない
            if (currentPlayer == revolutionPlayer)
                return;

            revolutionUseCounts[index]++;

            ConsumeAbility(currentPlayer);

            Debug.Log($"PLAYER {currentPlayer} が革命を解除！");

            EndRevolution();

            return;
        }

        // 革命を発動
        revolutionUseCounts[index]++;

        isRevolutionActive = true;
        revolutionPlayer = currentPlayer;
        revolutionRemainingTurns = revolutionDuration;

        ConsumeAbility(currentPlayer);

        Debug.Log($"PLAYER {currentPlayer} が革命を発動！");
        Debug.Log("HIGHとLOWの判定が逆転！");
        Debug.Log($"革命残りターン：{revolutionRemainingTurns}");
        Debug.Log($"革命残り使用回数：{maxRevolutionUses - revolutionUseCounts[index]}");

        FindFirstObjectByType<HLManager>()?.RefreshPredictionButtons();
    }


    // =========================
    // 革命終了
    // =========================

    private void EndRevolution()
    {
        isRevolutionActive = false;
        revolutionPlayer = 0;
        revolutionRemainingTurns = 0;

        Debug.Log("革命終了！通常の判定に戻りました。");

        RefreshAbilityButtons();

        FindFirstObjectByType<HLManager>()?.RefreshPredictionButtons();
    }


    // =========================
    // ボタン更新
    // =========================

    public void RefreshAbilityButtons()
    {
        if (turnManager == null)
            return;

        int currentPlayer = turnManager.CurrentPlayer;

        bool validPlayer = currentPlayer >= 1 && currentPlayer <= 2;

        bool canUse = validPlayer &&
            CanUseAbility(currentPlayer);

        // 妨害ボタン
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

        // 革命ボタン
        if (player1RevolutionButton != null)
        {
            player1RevolutionButton.interactable =
                currentPlayer == 1 &&
                revolutionUseCounts[0] < maxRevolutionUses &&
                canUse &&
                (!isRevolutionActive || revolutionPlayer != 1);
        }

        if (player2RevolutionButton != null)
        {
            player2RevolutionButton.interactable =
                currentPlayer == 2 &&
                revolutionUseCounts[1] < maxRevolutionUses &&
                canUse &&
                (!isRevolutionActive || revolutionPlayer != 2);
        }
    }
}
