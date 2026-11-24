
using System.Collections;
using UnityEngine;

public class MemoryBurstAbilityManager : MonoBehaviour
{
    private const int MAX_SHUFFLE_USES = 1;
    private const int MAX_SKIP_USES = 1;
    private const int MAX_RANDOM_USES = 2;

    private int[] shuffleUseCount = new int[4];
    private bool[] skipUsed = new bool[4];
    private int[] randomUseCount = new int[4];

    private bool[] abilityUsedThisTurn = new bool[4];

    private bool shufflePending;
    private bool randomPending;

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
            Debug.LogError("AbilityManager：Managerが設定されていません！");
            return;
        }

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

        abilityUsedThisTurn[player - 1] = false;

        if (shufflePending || randomPending)
        {
            StartCoroutine(ActivatePendingAbilities(player));
        }
    }

    // =================================
    // 予約能力の発動
    // =================================

    private IEnumerator ActivatePendingAbilities(int player)
    {
        // カード処理中なら終了を待つ
        while (cardManager.IsChecking && !isGameOver)
        {
            yield return null;
        }

        // シャッフル
        if (shufflePending && !isGameOver)
        {
            shufflePending = false;

            Debug.Log($"プレイヤー{player}のターン開始！シャッフル発動！");

            cardManager.ShuffleUnmatchedCards();

            // シャッフル演出が終わるまで待つ
            while (cardManager.IsChecking && !isGameOver)
            {
                yield return null;
            }
        }

        // ランダム公開
        if (randomPending && !isGameOver)
        {
            randomPending = false;

            Debug.Log($"プレイヤー{player}のターン開始！ランダム公開発動！");

            cardManager.ForceRandomReveal();
        }
    }

    // =================================
    // 使用可能判定
    // =================================

    public bool CanUseAbility(int player)
    {
        if (isGameOver)
            return false;

        if (turnManager == null || cardManager == null)
            return false;

        if (player < 1 || player > activePlayerCount)
            return false;

        if (turnManager.CurrentPlayer != player)
            return false;

        if (GameSettings.IsActive != null)
        {
            int index = player - 1;

            if (index >= GameSettings.IsActive.Length ||
                !GameSettings.IsActive[index])
                return false;
        }

        if (cardManager.IsChecking || cardManager.AllCardsMatched)
            return false;

        if (abilityUsedThisTurn[player - 1])
            return false;

        return true;
    }

    // =================================
    // シャッフル
    // =================================

    public void UseShuffle(int player)
    {
        if (!CanUseAbility(player))
            return;

        if (shuffleUseCount[player - 1] >= MAX_SHUFFLE_USES)
            return;

        if (shufflePending)
            return;

        shufflePending = true;

        shuffleUseCount[player - 1]++;
        abilityUsedThisTurn[player - 1] = true;

        Debug.Log(
            $"プレイヤー{player}がシャッフル能力を使用！" +
            $"残り回数：{GetShuffleRemaining(player)}回"
        );

        Debug.Log("次のターン開始時にシャッフルします！");
    }

    // =================================
    // スキップ
    // =================================

    public void UseSkip(int player)
    {
        if (!CanUseAbility(player))
            return;

        if (skipUsed[player - 1])
            return;

        skipUsed[player - 1] = true;
        abilityUsedThisTurn[player - 1] = true;

        turnManager.RequestSkipNextTurn();

        Debug.Log($"プレイヤー{player}がスキップ能力を使用！");
    }

    public void UseSkipForCurrentPlayer()
    {
        if (turnManager == null)
            return;

        UseSkip(turnManager.CurrentPlayer);
    }

    // =================================
    // ランダム公開
    // =================================

    public void UseRandom(int player)
    {
        if (!CanUseAbility(player))
            return;

        if (randomPending)
            return;

        if (randomUseCount[player - 1] >= MAX_RANDOM_USES)
            return;

        randomPending = true;

        randomUseCount[player - 1]++;
        abilityUsedThisTurn[player - 1] = true;

        Debug.Log(
            $"プレイヤー{player}がランダム公開を予約！" +
            $"残り回数：{GetRandomRemaining(player)}回"
        );

        Debug.Log("次のターン開始時にランダム公開します！");
    }

    // =================================
    // 残り使用回数
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
    // 使用可能判定
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

        StopAllCoroutines();

        Debug.Log("AbilityManager：ゲーム終了。能力を無効化しました。");
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
