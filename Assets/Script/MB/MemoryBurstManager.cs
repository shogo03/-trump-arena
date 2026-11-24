
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MemoryBurstManager : MonoBehaviour
{
    [Header("各マネージャー")]
    [SerializeField] private MemoryBurstCardManager cardManager;
    [SerializeField] private MemoryBurstTurnManager turnManager;
    [SerializeField] private MemoryBurstScoreManager scoreManager;
    [SerializeField] private MemoryBurstAbilityManager abilityManager;
    [SerializeField] private MemoryBurstUIManager uiManager;

    [Header("CPU管理")]
    [SerializeField] private MemoryBurstCPU cpu;

    [Header("TURN SKIPボタン")]
    [SerializeField] private Button turnSkipButton;

    [Header("リザルト")]
    [SerializeField] private string resultSceneName = "MemoryBurstResult";

    private int playerCount;
    private bool turnFailed;
    private bool isGameOver;

    // =================================
    // 初期化
    // =================================

    private void Start()
    {
        playerCount = GameSettings.TotalPlayerCount;

        Debug.Log($"プレイヤー人数：{playerCount}");

        if (playerCount <= 0)
        {
            Debug.LogError("参加プレイヤーがいません！");
            return;
        }

        // TURN SKIPボタンの初期設定
        if (turnSkipButton != null)
        {
            turnSkipButton.gameObject.SetActive(false);
            turnSkipButton.onClick.AddListener(OnTurnSkipButtonClicked);
        }

        // =================================
        // 各マネージャーの初期化
        // =================================

        turnManager.Initialize(playerCount);
        scoreManager.Initialize(playerCount);
        cardManager.Initialize();

        // =================================
        // AbilityManager初期化
        // =================================

        abilityManager.Initialize(
            playerCount,
            turnManager,
            cardManager
        );

        // =================================
        // カードマネージャーのイベント登録
        // =================================

        cardManager.OnPairMatched += OnPairMatched;
        cardManager.OnPairFailed += OnPairFailed;
        cardManager.OnAllCardsMatched += OnAllCardsMatched;

        // =================================
        // ターンマネージャーのイベント登録
        // =================================

        turnManager.OnTurnTimeout += OnTurnTimeout;
        turnManager.OnTurnChanged += OnTurnChanged;

        // =================================
        // CPU初期化
        // =================================

        if (cpu != null)
        {
            cpu.Initialize(cardManager, turnManager);

            // CPUにAbilityManagerを接続
            cpu.SetAbilityManager(abilityManager);

            Debug.Log("CPUとAbilityManagerの接続完了！");
        }
        else
        {
            Debug.LogError(
                "MemoryBurstCPUがInspectorに設定されていません！");
        }

        // =================================
        // CPU設定を取得
        // =================================

        bool[] cpuPlayers = new bool[4];

        for (int i = 0; i < 4; i++)
        {
            cpuPlayers[i] =
                GameSettings.IsActive[i] &&
                GameSettings.IsCPU[i];
        }

        // =================================
        // UIManager初期化
        // =================================

        if (uiManager != null)
        {
            uiManager.Initialize(
                playerCount,
                abilityManager,
                turnManager,
                cpuPlayers
            );

            Debug.Log("UIManager初期化完了");
        }
        else
        {
            Debug.LogError(
                "MemoryBurstUIManagerがInspectorに設定されていません！");
        }

        // =================================
        // 最初のプレイヤーがCPUだった場合
        // =================================

        if (turnManager.IsCurrentPlayerCPU && cpu != null)
        {
            cpu.StartTurn(turnManager.CurrentPlayer);
        }
    }

    // =================================
    // Update
    // =================================

    private void Update()
    {
        if (isGameOver)
            return;

        // カード判定中はタイマーを止める
        turnManager.Tick(cardManager.IsChecking);

        // UI更新
        if (uiManager != null)
        {
            uiManager.RefreshUI();
        }
    }

    // =================================
    // ペア成立
    // =================================

    private void OnPairMatched()
    {
        scoreManager.AddScore(turnManager.CurrentPlayer);

        // ペア成立後は同じプレイヤーのターンを継続
        turnManager.ResetTimer();

        if (uiManager != null)
        {
            uiManager.RefreshUI();
        }
    }

    // =================================
    // ペア不成立
    // =================================

    private void OnPairFailed()
    {
        turnFailed = true;

        // CPUの場合は自動でターン終了
        if (turnManager.IsCurrentPlayerCPU)
        {
            HideTurnSkipButton();

            Debug.Log(
                "CPUのペア不成立！自動でターンを終了します。");

            return;
        }

        ShowTurnSkipButton();

        Debug.Log(
            "ペア不成立！TURN SKIPボタンを表示します。");
    }

    // =================================
    // TURN SKIPボタン表示
    // =================================

    private void ShowTurnSkipButton()
    {
        if (turnSkipButton != null && !isGameOver)
        {
            turnSkipButton.gameObject.SetActive(true);
        }
    }

    // =================================
    // TURN SKIPボタン非表示
    // =================================

    private void HideTurnSkipButton()
    {
        if (turnSkipButton != null)
        {
            turnSkipButton.gameObject.SetActive(false);
        }
    }

    // =================================
    // TURN SKIPボタンが押されたとき
    // =================================

    private void OnTurnSkipButtonClicked()
    {
        if (isGameOver || !turnFailed)
            return;

        if (cardManager.IsChecking)
            return;

        Debug.Log(
            $"プレイヤー{turnManager.CurrentPlayer}がターンを終了しました。");

        // ボタンを非表示
        HideTurnSkipButton();

        // ターン失敗状態を解除
        turnFailed = false;

        // 次のプレイヤーへ交代
        turnManager.EndTurn();
    }

    // =================================
    // 全カードのペア成立
    // =================================

    private void OnAllCardsMatched()
    {
        FinishGame();
    }

    // =================================
    // タイムアウト
    // =================================

    private void OnTurnTimeout()
    {
        if (cpu != null)
        {
            cpu.StopTurn();
        }

        if (isGameOver)
            return;

        Debug.Log(
            $"プレイヤー{turnManager.CurrentPlayer}の時間切れ！");

        HideTurnSkipButton();

        // 表向きカードを隠す
        cardManager.HidePendingCards();

        turnFailed = false;

        // 次のプレイヤーへ
        turnManager.NextTurn();
    }

    // =================================
    // ターン変更
    // =================================

    private void OnTurnChanged(int player)
    {
        turnFailed = false;

        HideTurnSkipButton();

        if (cpu != null)
        {
            cpu.StopTurn();
        }

        Debug.Log($"プレイヤー{player}のターンです。");

        // CPUのターンならCPUを開始
        if (turnManager.IsCurrentPlayerCPU && cpu != null)
        {
            cpu.StartTurn(player);
        }

        if (uiManager != null)
        {
            uiManager.RefreshUI();
        }
    }

    // =================================
    // ゲーム終了
    // =================================

    private void FinishGame()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (cpu != null)
        {
            cpu.StopTurn();
        }

        HideTurnSkipButton();

        // 各マネージャーのゲーム終了処理
        turnManager.SetGameOver();
        abilityManager.SetGameOver();

        if (uiManager != null)
        {
            uiManager.SetGameOver();
        }

        // リザルト保存
        scoreManager.SaveResult();

        Debug.Log(
            "ゲーム終了！リザルトシーンへ移動します。");

        SceneManager.LoadScene(resultSceneName);
    }

    // =================================
    // コントローラー操作確認
    // =================================

    public bool CanControl(int gamepadIndex)
    {
        if (cardManager.IsChecking || isGameOver || turnFailed)
            return false;

        if (gamepadIndex < 0 ||
            gamepadIndex >= Gamepad.all.Count)
            return false;

        int playerIndex = turnManager.CurrentPlayer - 1;

        if (playerIndex < 0 || playerIndex >= 4)
            return false;

        if (!GameSettings.IsActive[playerIndex])
            return false;

        // CPUはコントローラー操作不可
        if (GameSettings.IsCPU[playerIndex])
            return false;

        int humanIndex = 0;

        // 現在のプレイヤーより前にいる人間プレイヤーの数を調べる
        for (int i = 0; i < playerIndex; i++)
        {
            if (GameSettings.IsActive[i] &&
                !GameSettings.IsCPU[i])
            {
                humanIndex++;
            }
        }

        return gamepadIndex == humanIndex;
    }

    // =================================
    // 終了処理
    // =================================

    private void OnDestroy()
    {
        // TURN SKIPボタン
        if (turnSkipButton != null)
        {
            turnSkipButton.onClick.RemoveListener(
                OnTurnSkipButtonClicked);
        }

        // カードイベント解除
        if (cardManager != null)
        {
            cardManager.OnPairMatched -= OnPairMatched;
            cardManager.OnPairFailed -= OnPairFailed;
            cardManager.OnAllCardsMatched -= OnAllCardsMatched;
        }

        // ターンイベント解除
        if (turnManager != null)
        {
            turnManager.OnTurnTimeout -= OnTurnTimeout;
            turnManager.OnTurnChanged -= OnTurnChanged;
        }

        // CPU終了
        if (cpu != null)
        {
            cpu.Shutdown();
        }
    }
}
