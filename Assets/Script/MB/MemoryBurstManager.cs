
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

    [Header("CPU管理")]
    [SerializeField] private MemoryBurstCPU cpu;

    [Header("TURN SKIPボタン")]
    [SerializeField] private Button turnSkipButton;

    [Header("リザルト")]
    [SerializeField] private string resultSceneName = "MemoryBurstResult";

    private int playerCount;
    private bool turnFailed;
    private bool isGameOver;

    private void Start()
    {
        playerCount = GameSettings.TotalPlayerCount;

        Debug.Log($"プレイヤー人数：{playerCount}");

        // TURN SKIPボタンの初期設定
        if (turnSkipButton != null)
        {
            turnSkipButton.gameObject.SetActive(false);
            turnSkipButton.onClick.AddListener(OnTurnSkipButtonClicked);
        }

        // 各マネージャーの初期化
        turnManager.Initialize(playerCount);
        scoreManager.Initialize(playerCount);
        cardManager.Initialize();

        abilityManager.Initialize(
            playerCount,
            turnManager,
            cardManager
        );

        // カードマネージャーのイベント登録
        cardManager.OnPairMatched += OnPairMatched;
        cardManager.OnPairFailed += OnPairFailed;
        cardManager.OnAllCardsMatched += OnAllCardsMatched;

        // ターンマネージャーのイベント登録
        turnManager.OnTurnTimeout += OnTurnTimeout;
        turnManager.OnTurnChanged += OnTurnChanged;

        cpu.Initialize(cardManager, turnManager);

        // 最初のプレイヤーがCPUだった場合にも対応
        if (turnManager.IsCurrentPlayerCPU)
        {
            cpu.StartTurn(turnManager.CurrentPlayer);
        }
    }

    private void Update()
    {
        if (isGameOver)
            return;

        // カード判定中はタイマーを止める
        turnManager.Tick(cardManager.IsChecking);

        abilityManager.UpdateButtons();
    }

    // =================================
    // ペア成立
    // =================================

    private void OnPairMatched()
    {
        scoreManager.AddScore(turnManager.CurrentPlayer);

        // ペア成立後は同じプレイヤーのターンを継続
        turnManager.ResetTimer();
    }

    // =================================
    // ペア不成立
    // =================================

    private void OnPairFailed()
    {
        turnFailed = true;

        if (turnManager.IsCurrentPlayerCPU)
        {
            HideTurnSkipButton();
            Debug.Log("CPUのペア不成立！自動でターンを終了します。");
            return;
        }

        ShowTurnSkipButton();
        Debug.Log("ペア不成立！TURN SKIPボタンを表示します。");
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

        Debug.Log($"プレイヤー{turnManager.CurrentPlayer}がターンを終了しました。");

        // ボタンを非表示
        HideTurnSkipButton();

        // ターン失敗状態を解除
        turnFailed = false;

        // 次のプレイヤーへ交代
        // 能力の使用回数は消費しない
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
        cpu.StopTurn();

        if (isGameOver)
            return;

        Debug.Log($"プレイヤー{turnManager.CurrentPlayer}の時間切れ！");

        HideTurnSkipButton();

        cardManager.HidePendingCards();

        turnFailed = false;
        turnManager.NextTurn();
    }

    // =================================
    // ターン変更
    // =================================

    private void OnTurnChanged(int player)
    {
        turnFailed = false;
        HideTurnSkipButton();

        cpu.StopTurn();

        Debug.Log($"プレイヤー{player}のターンです。");

        if (turnManager.IsCurrentPlayerCPU)
        {
            cpu.StartTurn(player);
        }
    }

    // =================================
    // ゲーム終了
    // =================================

    private void FinishGame()
    {
        cpu.StopTurn();

        if (isGameOver)
            return;

        isGameOver = true;

        HideTurnSkipButton();

        turnManager.SetGameOver();
        abilityManager.SetGameOver();

        scoreManager.SaveResult();

        Debug.Log("ゲーム終了！リザルトシーンへ移動します。");

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

        if (GameSettings.IsCPU[playerIndex])
            return false;

        int humanIndex = 0;

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
        if (turnSkipButton != null)
        {
            turnSkipButton.onClick.RemoveListener(OnTurnSkipButtonClicked);
        }

        if (cardManager != null)
        {
            cardManager.OnPairMatched -= OnPairMatched;
            cardManager.OnPairFailed -= OnPairFailed;
            cardManager.OnAllCardsMatched -= OnAllCardsMatched;
        }

        if (turnManager != null)
        {
            turnManager.OnTurnTimeout -= OnTurnTimeout;
            turnManager.OnTurnChanged -= OnTurnChanged;
        }

        if (cpu != null)
        {
            cpu.Shutdown();
        }
    }
}
