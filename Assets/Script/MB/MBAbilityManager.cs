using UnityEngine;

public class MemoryBurstAbilityManager : MonoBehaviour
{
    // =================================
    // 能力使用回数
    // =================================

    private const int MAX_SHUFFLE_USES = 1;
    private const int MAX_SKIP_USES = 1;
    private const int MAX_RANDOM_USES = 2;

    private int[] shuffleUseCount = new int[4];
    private bool[] skipUsed = new bool[4];
    private int[] randomUseCount = new int[4];

    // そのターンに能力を使用したか
    private bool[] abilityUsedThisTurn = new bool[4];

    // =================================
    // 能力発動予約
    // =================================

    private bool shufflePending;
    private bool randomPending;

    // =================================
    // Manager参照
    // =================================

    private MemoryBurstTurnManager turnManager;
    private MemoryBurstCardManager cardManager;

    private int activePlayerCount = 1;
    private bool isGameOver;

    // =================================
    // 初期化
    // =================================

    public void Initialize(
        int playerCount,
        MemoryBurstTurnManager turn,
        MemoryBurstCardManager card)
    {
        isGameOver = false;

        shufflePending = false;
        randomPending = false;

        shuffleUseCount = new int[4];
        skipUsed = new bool[4];
        randomUseCount = new int[4];
        abilityUsedThisTurn = new bool[4];

        activePlayerCount = Mathf.Clamp(playerCount, 1, 4);

        turnManager = turn;
        cardManager = card;

        if (turnManager == null || cardManager == null)
        {
            Debug.LogError(
                "AbilityManager：Managerが設定されていません！"
            );
            return;
        }

        // ターン変更イベント登録
        turnManager.OnTurnChanged -= OnTurnChanged;
        turnManager.OnTurnChanged += OnTurnChanged;

        Debug.Log("AbilityManager：初期化完了！");
    }

    // =================================
    // ターン変更
    // =================================

    private void OnTurnChanged(int player)
    {
        if (isGameOver)
            return;

        if (player < 1 || player > activePlayerCount)
            return;

        // 新しいターンなので能力使用状態をリセット
        abilityUsedThisTurn[player - 1] = false;

        // =================================
        // シャッフル能力の発動
        // =================================

        if (shufflePending)
        {
            shufflePending = false;

            Debug.Log(
                $"プレイヤー{player}のターン開始！シャッフル発動！"
            );

            if (!cardManager.ShuffleUnmatchedCards())
            {
                Debug.LogWarning(
                    "シャッフルできるカードがありませんでした。"
                );
            }
        }

        // =================================
        // ランダム公開能力の発動
        // =================================

        if (randomPending)
        {
            randomPending = false;

            Debug.Log(
                $"プレイヤー{player}のターン開始！ランダム公開発動！"
            );

            if (!cardManager.ForceRandomReveal())
            {
                Debug.LogWarning(
                    "ランダム公開できるカードがありませんでした。"
                );
            }
        }
    }

    // =================================
    // 能力使用可能判定
    // =================================

    public bool CanUseAbility(int player)
    {
        if (isGameOver)
            return false;

        if (turnManager == null || cardManager == null)
            return false;

        if (player < 1 || player > activePlayerCount)
            return false;

        // 自分のターン以外は使用不可
        if (turnManager.CurrentPlayer != player)
            return false;

        // 参加していないプレイヤーは使用不可
        if (GameSettings.IsActive != null)
        {
            int index = player - 1;

            if (index >= GameSettings.IsActive.Length ||
                !GameSettings.IsActive[index])
            {
                return false;
            }
        }

        // カード判定中・全カードペア完成時は使用不可
        if (cardManager.IsChecking || cardManager.AllCardsMatched)
            return false;

        // このターンに能力を使用済みなら使用不可
        if (abilityUsedThisTurn[player - 1])
            return false;

        return true;
    }

    // =================================
    // シャッフル能力
    // =================================

    public void UseShuffle(int player)
    {
        if (!CanUseAbility(player))
            return;

        // 使用回数確認
        if (shuffleUseCount[player - 1] >= MAX_SHUFFLE_USES)
            return;

        // すでにシャッフル予約中なら使用不可
        if (shufflePending)
            return;

        // シャッフル予約
        shufflePending = true;

        // 使用回数加算
        shuffleUseCount[player - 1]++;

        // このターンの能力使用済みにする
        abilityUsedThisTurn[player - 1] = true;

        Debug.Log(
            $"プレイヤー{player}がシャッフル能力を使用！" +
            $"残り回数：{GetShuffleRemaining(player)}回"
        );

        Debug.Log("次のターン開始時にシャッフルします！");
    }

    // =================================
    // スキップ能力
    // =================================

    public void UseSkip(int player)
    {
        if (!CanUseAbility(player))
            return;

        // 使用済み確認
        if (skipUsed[player - 1])
            return;

        skipUsed[player - 1] = true;

        // このターンの能力使用済みにする
        abilityUsedThisTurn[player - 1] = true;

        // 次のプレイヤーのターンをスキップ予約
        turnManager.RequestSkipNextTurn();

        Debug.Log(
            $"プレイヤー{player}がスキップ能力を使用！"
    );
    }

    // 現在のプレイヤーがスキップ能力を使用
    public void UseSkipForCurrentPlayer()
    {
        if (turnManager == null)
            return;

        UseSkip(turnManager.CurrentPlayer);
    }

    // =================================
    // ランダム公開能力
    // =================================

    public void UseRandom(int player)
    {
        if (!CanUseAbility(player))
            return;

        // すでに予約中なら使用不可
        if (randomPending)
            return;

        // 使用回数確認
        if (randomUseCount[player - 1] >= MAX_RANDOM_USES)
            return;

        // ランダム公開を予約
        randomPending = true;

        // 使用回数加算
        randomUseCount[player - 1]++;

        // このターンの能力使用済みにする
        abilityUsedThisTurn[player - 1] = true;

        Debug.Log(
            $"プレイヤー{player}がランダム公開を予約！" +
            $"残り回数：{GetRandomRemaining(player)}回"
    );

        Debug.Log("次のターン開始時にランダム公開します！");
    }

    // =================================
    // 使用回数取得
    // UIManagerから使用する
    // =================================

    public int GetShuffleRemaining(int player)
    {
        if (player < 1 || player > 4)
            return 0;

        return MAX_SHUFFLE_USES - shuffleUseCount[player - 1];
    }

    public int GetSkipRemaining(int player)
    {
        if (player < 1 || player > 4)
            return 0;

        return MAX_SKIP_USES - (skipUsed[player - 1] ? 1 : 0);
    }

    public int GetRandomRemaining(int player)
    {
        if (player < 1 || player > 4)
            return 0;

        return MAX_RANDOM_USES - randomUseCount[player - 1];
    }

    // =================================
    // 能力使用回数チェック
    // =================================

    public bool CanUseShuffle(int player)
    {
        if (!CanUseAbility(player))
            return false;

        return shuffleUseCount[player - 1] < MAX_SHUFFLE_USES
            && !shufflePending;
    }

    public bool CanUseSkip(int player)
    {
        if (!CanUseAbility(player))
            return false;

        return !skipUsed[player - 1];
    }

    public bool CanUseRandom(int player)
    {
        if (!CanUseAbility(player))
            return false;

        return randomUseCount[player - 1] < MAX_RANDOM_USES
            && !randomPending;
    }

    // =================================
    // ゲーム終了
    // =================================

    public void SetGameOver()
    {
        isGameOver = true;

        shufflePending = false;
        randomPending = false;

        Debug.Log(
            "AbilityManager：ゲーム終了。能力を無効化しました。"
        );
    }

    // =================================
    // イベント解除
    // =================================

    private void OnDestroy()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnChanged -= OnTurnChanged;
        }
    }
}