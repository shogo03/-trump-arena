
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MemoryBurstManager : MonoBehaviour
{
    [Header("各マネージャー")]
    [SerializeField] private MemoryBurstCardManager cardManager;
    [SerializeField] private MemoryBurstTurnManager turnManager;
    [SerializeField] private MemoryBurstScoreManager scoreManager;
    [SerializeField] private MemoryBurstAbilityManager abilityManager;

    [Header("リザルト")]
    [SerializeField] private string resultSceneName = "MemoryBurstResult";

    private int playerCount;
    private bool turnFailed;
    private bool isGameOver;

    private void Start()
    {
        playerCount = GameSettings.TotalPlayerCount;

        Debug.Log($"プレイヤー人数：{playerCount}");

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
    }

    private void Update()
    {
        if (isGameOver)
            return;

        // カード判定中はタイマーを止める
        turnManager.Tick(cardManager.IsChecking);

        abilityManager.UpdateButtons();
    }

    // ペア成立
    private void OnPairMatched()
    {
        scoreManager.AddScore(turnManager.CurrentPlayer);

        // ペア成立後は同じプレイヤーのターンを継続
        turnManager.ResetTimer();
    }

    // ペア不成立
    private void OnPairFailed()
    {
        // このターンはこれ以上カードをめくれない
        turnFailed = true;
    }

    // 全カードのペア成立
    private void OnAllCardsMatched()
    {
        FinishGame();
    }

    // タイムアウト
    private void OnTurnTimeout()
    {
        if (isGameOver)
            return;

        Debug.Log($"プレイヤー{turnManager.CurrentPlayer}の時間切れ！");

        cardManager.HidePendingCards();

        turnFailed = false;
        turnManager.NextTurn();
    }

    // ターンが変わったとき
    private void OnTurnChanged(int player)
    {
        turnFailed = false;
        Debug.Log($"プレイヤー{player}のターンです。");
    }

    private void FinishGame()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        turnManager.SetGameOver();
        abilityManager.SetGameOver();

        scoreManager.SaveResult();

        Debug.Log("ゲーム終了！リザルトシーンへ移動します。");

        SceneManager.LoadScene(resultSceneName);
    }

    // 現在のプレイヤーがコントローラーを操作できるか
    public bool CanControl(int gamepadIndex)
    {
        if (cardManager.IsChecking || isGameOver || turnFailed)
            return false;

        // 人間プレイヤーの人数を超えていたらCPUのターン
        if (turnManager.CurrentPlayer > GameSettings.HumanPlayerCount)
            return false;

        if (gamepadIndex < 0 ||
            gamepadIndex >= Gamepad.all.Count)
            return false;

        return gamepadIndex == turnManager.CurrentPlayer - 1;
    }

    private void OnDestroy()
    {
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
    }
}
