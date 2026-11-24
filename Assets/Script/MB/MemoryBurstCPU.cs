
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MemoryBurstCPU : MonoBehaviour
{
    [Header("思考時間")]
    [SerializeField] private float minThinkTime = 2f;
    [SerializeField] private float maxThinkTime = 3f;

    private MemoryBurstCardManager cardManager;
    private MemoryBurstTurnManager turnManager;

    private Coroutine turnRoutine;

    // CPUごとに独立した記憶
    // キー：カード本体
    // 値：カード番号
    private readonly Dictionary<int, Dictionary<MemoryBurstCardView, int>>
        memories = new();

    private bool pairFailed;
    private bool initialized;

    // 初期化
    public void Initialize(
        MemoryBurstCardManager cards,
        MemoryBurstTurnManager turns)
    {
        if (initialized)
            Shutdown();

        cardManager = cards;
        turnManager = turns;

        if (cardManager == null || turnManager == null)
        {
            Debug.LogError("CPUの初期化に失敗しました。");
            return;
        }

        cardManager.OnPairFailed += OnPairFailed;
        cardManager.OnCardsShuffled += ClearAllMemory;
        cardManager.OnCardRevealed += OnAnyCardRevealed;

        for (int i = 1; i <= 4; i++)
        {
            memories[i] =
                new Dictionary<MemoryBurstCardView, int>();
        }

        initialized = true;

        Debug.Log("CPUを初期化しました。");
    }

    // CPUのターン開始
    public void StartTurn(int playerNumber)
    {
        StopTurn();

        if (!initialized || !IsCPU(playerNumber))
            return;

        pairFailed = false;

        Debug.Log($"CPU{playerNumber}のターン開始");

        turnRoutine = StartCoroutine(PlayTurn(playerNumber));
    }

    // CPUのターン停止
    public void StopTurn()
    {
        if (turnRoutine != null)
        {
            StopCoroutine(turnRoutine);
            turnRoutine = null;
        }
    }

    // CPUの行動
    private IEnumerator PlayTurn(int playerNumber)
    {
        while (CanContinueTurn(playerNumber))
        {
            // 1枚目を選ぶ前の思考時間
            yield return new WaitForSeconds(
                Random.Range(minThinkTime, maxThinkTime));

            if (!CanPlay(playerNumber))
                break;

            RemoveMatchedMemory(playerNumber);

            // 1枚目
            MemoryBurstCardView firstCard =
                ChooseFirstCard(playerNumber);

            if (firstCard == null)
                break;

            if (!SelectCard(playerNumber, firstCard))
                break;

            // 2枚目を選ぶ前の思考時間
            yield return new WaitForSeconds(
                Random.Range(minThinkTime, maxThinkTime));

            if (!CanPlay(playerNumber))
                break;

            // 2枚目
            MemoryBurstCardView secondCard =
                ChooseSecondCard(playerNumber, firstCard);

            if (secondCard == null)
                break;

            if (!SelectCard(playerNumber, secondCard))
                break;

            // ペア判定が終わるまで待つ
            while (cardManager.IsChecking &&
                   turnManager.CurrentPlayer == playerNumber &&
                   !turnManager.IsGameOver)
            {
                yield return null;
            }

            // ペア不成立ならターン終了
            if (pairFailed)
            {
                Debug.Log($"CPU{playerNumber}：ペア不成立");

                if (turnManager.CurrentPlayer == playerNumber &&
                    !turnManager.IsGameOver)
                {
                    turnManager.EndTurn();
                }

                turnRoutine = null;
                yield break;
            }

            // ペア成立なら同じCPUのターンを続ける
            Debug.Log($"CPU{playerNumber}：ペア成立！");
        }

        turnRoutine = null;
    }

    // ターン継続確認
    private bool CanContinueTurn(int playerNumber)
    {
        return turnManager != null &&
               !turnManager.IsGameOver &&
               turnManager.CurrentPlayer == playerNumber;
    }

    // カード選択可能か確認
    private bool CanPlay(int playerNumber)
    {
        return CanContinueTurn(playerNumber) &&
               cardManager != null &&
               !cardManager.IsChecking &&
               !pairFailed;
    }

    // CPU判定
    private bool IsCPU(int playerNumber)
    {
        int index = playerNumber - 1;

        return index >= 0 &&
               index < GameSettings.IsCPU.Length &&
               index < GameSettings.IsActive.Length &&
               GameSettings.IsActive[index] &&
               GameSettings.IsCPU[index];
    }

    // 難易度取得
    private int GetDifficulty(int playerNumber)
    {
        int index = playerNumber - 1;

        if (index < 0 ||
            index >= GameSettings.CPULevels.Length)
        {
            return 2;
        }

        return Mathf.Clamp(
            GameSettings.CPULevels[index], 1, 3);
    }

    // 記憶を正しく使える確率
    private float GetMemoryAccuracy(int playerNumber)
    {
        return GetDifficulty(playerNumber) switch
        {
            1 => 0.3f, // Easy
            2 => 0.7f, // Normal
            3 => 1.0f, // Hard
            _ => 0.7f
        };
    }

    // 1枚目を選ぶ
    private MemoryBurstCardView ChooseFirstCard(int playerNumber)
    {
        float accuracy = GetMemoryAccuracy(playerNumber);

        // 記憶を使うか抽選
        if (Random.value <= accuracy)
        {
            List<MemoryBurstCardView> knownPair =
                FindKnownPair(playerNumber);

            if (knownPair != null && knownPair.Count >= 2)
            {
                Debug.Log(
                    $"CPU{playerNumber}：記憶したペアを発見");

                return knownPair[0];
            }
        }

        // 記憶を使わない場合はランダム
        return GetRandomCard();
    }

    // 2枚目を選ぶ
    private MemoryBurstCardView ChooseSecondCard(
        int playerNumber,
        MemoryBurstCardView firstCard)
    {
        float accuracy = GetMemoryAccuracy(playerNumber);

        // 記憶を使うか抽選
        if (Random.value <= accuracy)
        {
            Dictionary<MemoryBurstCardView, int> memory =
                memories[playerNumber];

            foreach (var item in memory)
            {
                MemoryBurstCardView card = item.Key;

                if (card == null ||
                    card == firstCard ||
                    card.IsRevealed ||
                    card.IsMatched)
                {
                    continue;
                }

                // 1枚目と同じ番号なら選択
                if (item.Value == firstCard.Number)
                {
                    Debug.Log(
                        $"CPU{playerNumber}：記憶から2枚目を選択");

                    return card;
                }
            }
        }

        // 記憶を外した場合はランダム
        return GetRandomCard(firstCard);
    }

    // 記憶しているペアを探す
    private List<MemoryBurstCardView> FindKnownPair(
        int playerNumber)
    {
        Dictionary<MemoryBurstCardView, int> memory =
            memories[playerNumber];

        var available = memory
            .Where(x =>
                x.Key != null &&
                !x.Key.IsRevealed &&
                !x.Key.IsMatched)
            .ToList();

        var groups = available.GroupBy(x => x.Value);

        foreach (var group in groups)
        {
            List<MemoryBurstCardView> pair =
                group.Select(x => x.Key).ToList();

            if (pair.Count >= 2)
                return pair;
        }

        return null;
    }

    // ランダムにカードを取得
    private MemoryBurstCardView GetRandomCard(
        MemoryBurstCardView exclude = null)
    {
        List<MemoryBurstCardView> available = new();

        foreach (var card in cardManager.Cards)
        {
            if (card == null ||
                card == exclude ||
                card.IsRevealed ||
                card.IsMatched)
            {
                continue;
            }

            available.Add(card);
        }

        if (available.Count == 0)
            return null;

        return available[
            Random.Range(0, available.Count)];
    }

    // カードを選択して記憶する
    private bool SelectCard(
        int playerNumber,
        MemoryBurstCardView card)
    {
        if (!cardManager.TrySelectCard(card))
            return false;

        // カードをめくった通知で記憶するため、
        // ここでは追加の記憶処理をしない
        return true;
    }

    // 全プレイヤーがめくったカードを記憶
    private void OnAnyCardRevealed(
        MemoryBurstCardView card)
    {
        if (card == null)
            return;

        for (int i = 0; i < 4; i++)
        {
            if (!GameSettings.IsActive[i] ||
                !GameSettings.IsCPU[i])
            {
                continue;
            }

            int playerNumber = i + 1;

            // CPUごとに独立して記憶
            memories[playerNumber][card] = card.Number;
        }

        Debug.Log(
            $"カード記憶：番号 {card.Number}");
    }

    // ペア成立済みカードを記憶から削除
    private void RemoveMatchedMemory(int playerNumber)
    {
        Dictionary<MemoryBurstCardView, int> memory =
            memories[playerNumber];

        List<MemoryBurstCardView> removeList = new();

        foreach (var item in memory)
        {
            if (item.Key == null || item.Key.IsMatched)
                removeList.Add(item.Key);
        }

        foreach (var card in removeList)
        {
            memory.Remove(card);
        }
    }

    // ペア不成立通知
    private void OnPairFailed()
    {
        pairFailed = true;
    }

    // シャッフル時に全CPUの記憶を消す
    private void ClearAllMemory()
    {
        foreach (var memory in memories.Values)
        {
            memory.Clear();
        }

        Debug.Log("CPU全員の記憶をリセットしました。");
    }

    // 終了処理
    public void Shutdown()
    {
        StopTurn();

        if (cardManager != null)
        {
            cardManager.OnPairFailed -= OnPairFailed;
            cardManager.OnCardsShuffled -= ClearAllMemory;
            cardManager.OnCardRevealed -= OnAnyCardRevealed;
        }

        initialized = false;
    }

    private void OnDestroy()
    {
        Shutdown();
    }
}
