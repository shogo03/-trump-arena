
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MemoryBurstCPU : MonoBehaviour
{
    [Header("思考時間")]
    [SerializeField] private float minThinkTime = 2f;
    [SerializeField] private float maxThinkTime = 3f;

    [Header("Manager")]
    private MemoryBurstCardManager cardManager;
    private MemoryBurstTurnManager turnManager;
    private MemoryBurstAbilityManager abilityManager;

    private Coroutine turnRoutine;

    // CPUごとに独立した記憶
    // キー：カード本体
    // 値：カード番号
    private readonly Dictionary<int, Dictionary<MemoryBurstCardView, int>>
        memories = new();

    private bool pairFailed;
    private bool initialized;

    // =================================
    // 初期化
    // =================================

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

        // イベント登録
        cardManager.OnPairFailed += OnPairFailed;
        cardManager.OnCardsShuffled += ClearAllMemory;
        cardManager.OnCardRevealed += OnAnyCardRevealed;

        // CPUごとの記憶領域を作成
        memories.Clear();

        for (int i = 1; i <= 4; i++)
        {
            memories[i] =
                new Dictionary<MemoryBurstCardView, int>();
        }

        initialized = true;

        Debug.Log("CPUを初期化しました。");
    }

    // AbilityManager設定
    public void SetAbilityManager(
        MemoryBurstAbilityManager manager)
    {
        abilityManager = manager;

        Debug.Log("CPU：AbilityManagerを接続しました。");
    }

    // =================================
    // CPUのターン開始
    // =================================

    public void StartTurn(int playerNumber)
    {
        StopTurn();

        if (!initialized || !IsCPU(playerNumber))
            return;

        pairFailed = false;

        Debug.Log($"CPU{playerNumber}のターン開始");

        turnRoutine = StartCoroutine(PlayTurn(playerNumber));
    }

    // =================================
    // CPUのターン停止
    // =================================

    public void StopTurn()
    {
        if (turnRoutine != null)
        {
            StopCoroutine(turnRoutine);
            turnRoutine = null;
        }
    }

    // =================================
    // CPUの行動
    // =================================

    private IEnumerator PlayTurn(int playerNumber)
    {
        // このターンに能力を判断したか
        bool abilityChecked = false;

        while (CanContinueTurn(playerNumber))
        {
            // 1枚目を選ぶ前の思考時間
            yield return new WaitForSeconds(
                Random.Range(minThinkTime, maxThinkTime));

            if (!CanPlay(playerNumber))
                break;

            // =================================
            // 能力使用判断
            // =================================

            if (!abilityChecked)
            {
                TryUseAbility(playerNumber);
                abilityChecked = true;
            }

            RemoveMatchedMemory(playerNumber);

            // =================================
            // 1枚目
            // =================================

            MemoryBurstCardView firstCard =
                ChooseFirstCard(playerNumber);

            if (firstCard == null)
                break;

            if (!SelectCard(playerNumber, firstCard))
                break;

            // =================================
            // 2枚目を選ぶ前の思考時間
            // =================================

            yield return new WaitForSeconds(
                Random.Range(minThinkTime, maxThinkTime));

            if (!CanPlay(playerNumber))
                break;

            // =================================
            // 2枚目
            // =================================

            MemoryBurstCardView secondCard =
                ChooseSecondCard(playerNumber, firstCard);

            if (secondCard == null)
                break;

            if (!SelectCard(playerNumber, secondCard))
                break;

            // =================================
            // ペア判定が終わるまで待機
            // =================================

            while (cardManager.IsChecking &&
                   turnManager.CurrentPlayer == playerNumber &&
                   !turnManager.IsGameOver)
            {
                yield return null;
            }

            // =================================
            // ペア不成立
            // =================================

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

            // =================================
            // ペア成立
            // =================================

            Debug.Log($"CPU{playerNumber}：ペア成立！");

            // ペア成立なら同じターンを続ける
        }

        turnRoutine = null;
    }

    // =================================
    // 能力使用判断
    // =================================

    private void TryUseAbility(int playerNumber)
    {
        if (abilityManager == null)
            return;

        // 難易度による能力使用確率
        float probability = GetDifficulty(playerNumber) switch
        {
            1 => 0.2f,
            2 => 0.5f,
            3 => 0.8f,
            _ => 0.5f
        };

        // 能力を使用するか抽選
        if (Random.value > probability)
        {
            Debug.Log($"CPU{playerNumber}：能力を使用しない");
            return;
        }

        // 使用可能な能力を取得
        List<int> availableAbilities = new();

        if (abilityManager.CanUseShuffle(playerNumber))
            availableAbilities.Add(0);

        if (abilityManager.CanUseSkip(playerNumber))
            availableAbilities.Add(1);

        if (abilityManager.CanUseRandom(playerNumber))
            availableAbilities.Add(2);

        // 使用可能な能力がない
        if (availableAbilities.Count == 0)
        {
            Debug.Log($"CPU{playerNumber}：使用可能な能力なし");
            return;
        }

        // 使用する能力をランダム選択
        int selectedAbility = availableAbilities[
            Random.Range(0, availableAbilities.Count)];

        switch (selectedAbility)
        {
            case 0:
                abilityManager.UseShuffle(playerNumber);

                Debug.Log(
                    $"CPU{playerNumber}：シャッフル能力を使用！");
                break;

            case 1:
                abilityManager.UseSkip(playerNumber);

                Debug.Log(
                    $"CPU{playerNumber}：スキップ能力を使用！");
                break;

            case 2:
                abilityManager.UseRandom(playerNumber);

                Debug.Log(
                    $"CPU{playerNumber}：ランダム公開能力を使用！");
                break;
        }
    }

    // =================================
    // ターン継続確認
    // =================================

    private bool CanContinueTurn(int playerNumber)
    {
        return turnManager != null &&
               !turnManager.IsGameOver &&
               turnManager.CurrentPlayer == playerNumber;
    }

    // =================================
    // カード選択可能か確認
    // =================================

    private bool CanPlay(int playerNumber)
    {
        return CanContinueTurn(playerNumber) &&
               cardManager != null &&
               !cardManager.IsChecking &&
               !cardManager.AllCardsMatched &&
               !pairFailed;
    }

    // =================================
    // CPU判定
    // =================================

    private bool IsCPU(int playerNumber)
    {
        int index = playerNumber - 1;

        return index >= 0 &&
               index < GameSettings.IsCPU.Length &&
               index < GameSettings.IsActive.Length &&
               GameSettings.IsActive[index] &&
               GameSettings.IsCPU[index];
    }

    // =================================
    // 難易度取得
    // =================================

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

    // =================================
    // 記憶を正しく使える確率
    // =================================

    private float GetMemoryAccuracy(int playerNumber)
    {
        return GetDifficulty(playerNumber) switch
        {
            1 => 0.3f,
            2 => 0.7f,
            3 => 1.0f,
            _ => 0.7f
        };
    }

    // =================================
    // 1枚目を選ぶ
    // =================================

    private MemoryBurstCardView ChooseFirstCard(
        int playerNumber)
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

        // ランダム選択
        return GetRandomCard();
    }

    // =================================
    // 2枚目を選ぶ
    // =================================

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

        // ランダム選択
        return GetRandomCard(firstCard);
    }

    // =================================
    // 記憶しているペアを探す
    // =================================

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

    // =================================
    // ランダムにカードを取得
    // =================================

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

    // =================================
    // カード選択
    // =================================

    private bool SelectCard(
        int playerNumber,
        MemoryBurstCardView card)
    {
        if (!CanPlay(playerNumber))
            return false;

        if (!cardManager.TrySelectCard(card))
            return false;

        return true;
    }

    // =================================
    // 全プレイヤーがめくったカードを記憶
    // =================================

    private void OnAnyCardRevealed(
        MemoryBurstCardView card)
    {
        if (card == null)
            return;

        for (int i = 0; i < 4; i++)
        {
            if (i >= GameSettings.IsActive.Length ||
                i >= GameSettings.IsCPU.Length)
            {
                continue;
            }

            if (!GameSettings.IsActive[i] ||
                !GameSettings.IsCPU[i])
            {
                continue;
            }

            int playerNumber = i + 1;

            memories[playerNumber][card] = card.Number;
        }

        Debug.Log($"カード記憶：番号 {card.Number}");
    }

    // =================================
    // ペア成立済みカードを記憶から削除
    // =================================

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

    // =================================
    // ペア不成立通知
    // =================================

    private void OnPairFailed()
    {
        pairFailed = true;
    }

    // =================================
    // シャッフル時に全CPUの記憶を消す
    // =================================

    private void ClearAllMemory()
    {
        foreach (var memory in memories.Values)
        {
            memory.Clear();
        }

        Debug.Log("CPU全員の記憶をリセットしました。");
    }

    // =================================
    // 終了処理
    // =================================

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
