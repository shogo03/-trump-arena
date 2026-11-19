
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

    // CPUごとに別々の記憶を持つ
    // キー：プレイヤー番号
    // 値：カード本体とカード番号
    private readonly Dictionary<int, Dictionary<MemoryBurstCardView, int>>
        memories = new();

    private bool pairFailed;

    public void Initialize(
        MemoryBurstCardManager cards,
        MemoryBurstTurnManager turns)
    {
        cardManager = cards;
        turnManager = turns;

        cardManager.OnPairFailed += OnPairFailed;
        cardManager.OnCardsShuffled += ClearAllMemory;

        for (int i = 1; i <= 4; i++)
        {
            memories[i] = new Dictionary<MemoryBurstCardView, int>();
        }
    }

    public void StartTurn(int playerNumber)
    {
        StopTurn();

        if (!IsCPU(playerNumber))
            return;

        pairFailed = false;
        turnRoutine = StartCoroutine(PlayTurn(playerNumber));
    }

    public void StopTurn()
    {
        if (turnRoutine != null)
        {
            StopCoroutine(turnRoutine);
            turnRoutine = null;
        }
    }

    private IEnumerator PlayTurn(int playerNumber)
    {
        while (turnManager.CurrentPlayer == playerNumber &&
               !turnManager.IsGameOver)
        {
            // 思考時間
            yield return new WaitForSeconds(
                Random.Range(minThinkTime, maxThinkTime));

            if (!CanPlay(playerNumber))
                yield break;

            // 記憶を整理
            RemoveMatchedMemory(playerNumber);

            // 1枚目
            MemoryBurstCardView firstCard =
                ChooseFirstCard(playerNumber);

            if (firstCard == null ||
                !SelectCard(playerNumber, firstCard))
                yield break;

            // 2枚目を考える
            yield return new WaitForSeconds(
                Random.Range(minThinkTime, maxThinkTime));

            if (!CanPlay(playerNumber))
                yield break;

            MemoryBurstCardView secondCard =
                ChooseSecondCard(playerNumber, firstCard);

            if (secondCard == null ||
                !SelectCard(playerNumber, secondCard))
                yield break;

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
                if (turnManager.CurrentPlayer == playerNumber &&
                    !turnManager.IsGameOver)
                {
                    turnManager.EndTurn();
                }

                yield break;
            }

            // ペア成立なら同じCPUのターンを続ける
        }

        turnRoutine = null;
    }

    private bool CanPlay(int playerNumber)
    {
        return turnManager != null &&
               cardManager != null &&
               !turnManager.IsGameOver &&
               turnManager.CurrentPlayer == playerNumber &&
               !cardManager.IsChecking &&
               !pairFailed;
    }

    private bool IsCPU(int playerNumber)
    {
        int index = playerNumber - 1;

        return index >= 0 &&
               index < GameSettings.IsCPU.Length &&
               index < GameSettings.IsActive.Length &&
               GameSettings.IsActive[index] &&
               GameSettings.IsCPU[index];
    }

    private int GetDifficulty(int playerNumber)
    {
        int index = playerNumber - 1;

        if (index < 0 || index >= GameSettings.CPULevels.Length)
            return 2;

        return Mathf.Clamp(GameSettings.CPULevels[index], 1, 3);
    }

    private MemoryBurstCardView ChooseFirstCard(int playerNumber)
    {
        // 難易度に関係なく、覚えているペアがあれば優先
        var pair = FindKnownPair(playerNumber);

        if (pair != null && pair.Count >= 2)
        {
            Debug.Log($"CPU{playerNumber}：記憶したペアを発見！");
            return pair[0];
        }

        // ペアがなければランダム
        return GetRandomCard();
    }

    private MemoryBurstCardView ChooseSecondCard(
    int playerNumber,
    MemoryBurstCardView firstCard)
    {
        Dictionary<MemoryBurstCardView, int> memory =
            memories[playerNumber];

        // 1枚目と同じ番号のカードを探す
        foreach (var item in memory)
        {
            MemoryBurstCardView card = item.Key;

            if (card == null ||
                card == firstCard ||
                card.IsRevealed ||
                card.IsMatched)
                continue;

            if (item.Value == firstCard.Number)
            {
                Debug.Log($"CPU{playerNumber}：ペア候補を選択！");
                return card;
            }
        }

        // 覚えていなければランダム
        return GetRandomCard(firstCard);
    }

    private List<MemoryBurstCardView> FindKnownPair(int playerNumber)
    {
        var memory = memories[playerNumber];

        var available = memory
            .Where(x => x.Key != null &&
                        !x.Key.IsRevealed &&
                        !x.Key.IsMatched)
            .ToList();

        var groups = available.GroupBy(x => x.Value);

        foreach (var group in groups)
        {
            var pair = group.Select(x => x.Key).ToList();

            if (pair.Count >= 2)
                return pair;
        }

        return null;
    }

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
                continue;

            available.Add(card);
        }

        if (available.Count == 0)
            return null;

        return available[Random.Range(0, available.Count)];
    }

    private bool SelectCard(
        int playerNumber,
        MemoryBurstCardView card)
    {
        if (!cardManager.TrySelectCard(card))
            return false;

        // 自分が見たカードだけを記憶する
        memories[playerNumber][card] = card.Number;

        return true;
    }

    private void RemoveMatchedMemory(int playerNumber)
    {
        var memory = memories[playerNumber];

        var removeList = new List<MemoryBurstCardView>();

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

    private void OnPairFailed()
    {
        pairFailed = true;
    }

    private void ClearAllMemory()
    {
        foreach (var memory in memories.Values)
        {
            memory.Clear();
        }
    }

    public void Shutdown()
    {
        StopTurn();

        if (cardManager != null)
        {
            cardManager.OnPairFailed -= OnPairFailed;
            cardManager.OnCardsShuffled -= ClearAllMemory;
        }
    }

    private void OnDestroy()
    {
        Shutdown();
    }
}
