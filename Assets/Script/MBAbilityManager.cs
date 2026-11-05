
using UnityEngine;
using UnityEngine.UI;

public class MemoryBurstAbilityManager : MonoBehaviour
{
    [Header("プレイヤー1の能力")]
    [SerializeField] private Button shuffleButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button randomButton;

    [Header("プレイヤー2の能力")]
    [SerializeField] private Button shuffleButton2;
    [SerializeField] private Button skipButton2;
    [SerializeField] private Button randomButton2;

    // 能力のゲーム中使用状況
    private bool[] shuffleUsed = new bool[4];
    private bool[] skipUsed = new bool[4];

    // ランダム公開能力は各プレイヤー2回まで
    private int[] randomUseCount = new int[4];
    private const int MAX_RANDOM_USES = 2;

    // そのターンに能力を使用したか
    private bool[] abilityUsedThisTurn = new bool[4];

    private MemoryBurstTurnManager turnManager;
    private MemoryBurstCardManager cardManager;

    // ランダム公開が予約されているプレイヤー
    private int pendingRandomPlayer = 0;

    // ゲーム終了状態
    private bool isGameOver = false;

    // =================================
    // 初期化
    // =================================

    public void Initialize(
        int playerCount,
        MemoryBurstTurnManager turn,
        MemoryBurstCardManager card)
    {
        pendingRandomPlayer = 0;
        isGameOver = false;

        shuffleUsed = new bool[4];
        skipUsed = new bool[4];
        randomUseCount = new int[4];
        abilityUsedThisTurn = new bool[4];

        turnManager = turn;
        cardManager = card;

        if (turnManager == null || cardManager == null)
        {
            Debug.LogError("AbilityManager：Managerが設定されていません！");
            return;
        }

        // ボタンのイベント登録
        if (shuffleButton != null)
            shuffleButton.onClick.AddListener(ShufflePlayer1);

        if (shuffleButton2 != null)
            shuffleButton2.onClick.AddListener(ShufflePlayer2);

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipPlayer1);

        if (skipButton2 != null)
            skipButton2.onClick.AddListener(SkipPlayer2);

        if (randomButton != null)
            randomButton.onClick.AddListener(RandomPlayer1);

        if (randomButton2 != null)
            randomButton2.onClick.AddListener(RandomPlayer2);

        // ターン変更イベント登録
        turnManager.OnTurnChanged += OnTurnChanged;

        UpdateButtons();

        Debug.Log("AbilityManager：初期化完了！");
    }

    // =================================
    // ターン変更
    // =================================

    private void OnTurnChanged(int player)
    {
        if (isGameOver) return;

        if (player < 1 || player > abilityUsedThisTurn.Length)
            return;

        // 新しいターンなので能力使用状態をリセット
        abilityUsedThisTurn[player - 1] = false;

        // このプレイヤーのターン開始時にランダム公開
        if (pendingRandomPlayer == player)
        {
            pendingRandomPlayer = 0;

            Debug.Log(
                $"プレイヤー{player}のターン開始！" +
                "ランダム公開を発動！"
            );

            if (!cardManager.ForceRandomReveal())
            {
                Debug.LogWarning("ランダム公開できるカードがありませんでした。");
            }
        }

        UpdateButtons();
    }

    // =================================
    // ボタン状態の更新
    // =================================

    public void UpdateButtons()
    {
        if (isGameOver) return;

        UpdatePlayerButtons(
            1, shuffleButton, skipButton, randomButton
        );

        UpdatePlayerButtons(
            2, shuffleButton2, skipButton2, randomButton2
        );
    }

    private void UpdatePlayerButtons(
        int player,
        Button shuffle,
        Button skip,
        Button random)
    {
        bool canUse = CanUseAbility(player);

        if (shuffle != null)
        {
            shuffle.interactable =
                canUse && !shuffleUsed[player - 1];
        }

        if (skip != null)
        {
            skip.interactable =
                canUse && !skipUsed[player - 1];
        }

        if (random != null)
        {
            random.interactable =
                canUse &&
                randomUseCount[player - 1] < MAX_RANDOM_USES &&
                pendingRandomPlayer == 0;
        }
    }

    // =================================
    // 能力使用可能判定
    // =================================

    private bool CanUseAbility(int player)
    {
        if (isGameOver) return false;

        if (turnManager == null || cardManager == null)
            return false;

        if (player < 1 || player > abilityUsedThisTurn.Length)
            return false;

        // 自分のターン以外は使用不可
        if (turnManager.CurrentPlayer != player)
            return false;

        // カード判定中・ゲーム終了時は使用不可
        if (cardManager.IsChecking || cardManager.AllCardsMatched)
            return false;

        // このターンに能力を使用済みなら不可
        if (abilityUsedThisTurn[player - 1])
            return false;

        return true;
    }

    // =================================
    // シャッフル能力
    // =================================

    private void ShufflePlayer1()
    {
        UseShuffle(1);
    }

    private void ShufflePlayer2()
    {
        UseShuffle(2);
    }

    private void UseShuffle(int player)
    {
        if (!CanUseAbility(player))
            return;

        if (shuffleUsed[player - 1])
            return;

        if (!cardManager.ShuffleUnmatchedCards())
            return;

        // 使用済みにする
        shuffleUsed[player - 1] = true;
        abilityUsedThisTurn[player - 1] = true;

        Debug.Log($"プレイヤー{player}がシャッフル能力を使用！");

        UpdateButtons();
    }

    // =================================
    // スキップ能力
    // =================================

    private void SkipPlayer1()
    {
        UseSkip(1);
    }

    private void SkipPlayer2()
    {
        UseSkip(2);
    }

    private void UseSkip(int player)
    {
        if (!CanUseAbility(player))
            return;

        if (skipUsed[player - 1])
            return;

        // 使用済みにする
        skipUsed[player - 1] = true;
        abilityUsedThisTurn[player - 1] = true;

        Debug.Log($"プレイヤー{player}が相手のターンをスキップ！");

        // 相手のターンへ移動
        turnManager.NextTurn();

        // 相手のターンを飛ばして使用者に戻す
        turnManager.NextTurn();

        UpdateButtons();
    }

    // =================================
    // 強制ランダム公開能力
    // =================================

    private void RandomPlayer1()
    {
        UseRandom(1);
    }

    private void RandomPlayer2()
    {
        UseRandom(2);
    }

    private void UseRandom(int player)
    {
        if (!CanUseAbility(player))
            return;

        // すでに予約中なら使用できない
        if (pendingRandomPlayer != 0)
            return;

        // 使用回数の確認
        if (randomUseCount[player - 1] >= MAX_RANDOM_USES)
            return;

        // 相手のターンに発動するよう予約
        int opponent = player == 1 ? 2 : 1;
        pendingRandomPlayer = opponent;

        // 使用回数とターン内使用状況を更新
        randomUseCount[player - 1]++;
        abilityUsedThisTurn[player - 1] = true;

        Debug.Log(
            $"プレイヤー{player}がランダム公開を予約！" +
            $"プレイヤー{opponent}のターン開始時に発動！" +
            $"残り回数：{MAX_RANDOM_USES - randomUseCount[player - 1]}回"
        );

        UpdateButtons();
    }

    // =================================
    // ゲーム終了
    // =================================

    public void SetGameOver()
    {
        isGameOver = true;
        pendingRandomPlayer = 0;

        if (shuffleButton != null)
            shuffleButton.interactable = false;

        if (shuffleButton2 != null)
            shuffleButton2.interactable = false;

        if (skipButton != null)
            skipButton.interactable = false;

        if (skipButton2 != null)
            skipButton2.interactable = false;

        if (randomButton != null)
            randomButton.interactable = false;

        if (randomButton2 != null)
            randomButton2.interactable = false;

        Debug.Log("AbilityManager：ゲーム終了。能力を無効化しました。");
    }

    // =================================
    // イベント解除
    // =================================

    private void OnDestroy()
    {
        if (turnManager != null)
            turnManager.OnTurnChanged -= OnTurnChanged;

        if (shuffleButton != null)
            shuffleButton.onClick.RemoveListener(ShufflePlayer1);

        if (shuffleButton2 != null)
            shuffleButton2.onClick.RemoveListener(ShufflePlayer2);

        if (skipButton != null)
            skipButton.onClick.RemoveListener(SkipPlayer1);

        if (skipButton2 != null)
            skipButton2.onClick.RemoveListener(SkipPlayer2);

        if (randomButton != null)
            randomButton.onClick.RemoveListener(RandomPlayer1);

        if (randomButton2 != null)
            randomButton2.onClick.RemoveListener(RandomPlayer2);
    }
}
