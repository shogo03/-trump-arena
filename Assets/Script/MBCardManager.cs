
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryBurstCardManager : MonoBehaviour
{
    [Header("山札")]
    [SerializeField] private MemoryBurstDeck deck;

    [Header("カード")]
    [SerializeField] private MemoryBurstCardView cardPrefab;
    [SerializeField] private Sprite backImage;

    [Header("カードを並べる場所")]
    [SerializeField] private Transform cardArea;

    [Header("設定")]
    [SerializeField] private int cardCount = 24;
    [SerializeField] private float flipBackDelay = 1f;
    [SerializeField] private float matchedDisplayTime = 1f;

    private readonly List<MemoryBurstCardView> cards = new();

    private MemoryBurstCardView firstCard;
    private MemoryBurstCardView secondCard;

    private bool isChecking;
    private int remainingCards;

    public bool IsChecking => isChecking;
    public bool AllCardsMatched => remainingCards <= 0;

    public event Action OnPairMatched;
    public event Action OnPairFailed;
    public event Action OnAllCardsMatched;

    // =================================
    // 初期化
    // =================================

    public void Initialize()
    {
        if (deck == null || cardPrefab == null ||
            cardArea == null || backImage == null)
        {
            Debug.LogError("Deck、Card Prefab、Back Image、Card Areaを設定してください。");
            return;
        }

        if (cardCount < 2 || cardCount % 2 != 0)
        {
            Debug.LogError("カード枚数は2以上の偶数にしてください。");
            return;
        }

        deck.Initialize();

        foreach (Transform child in cardArea)
            Destroy(child.gameObject);

        cards.Clear();
        firstCard = null;
        secondCard = null;
        isChecking = false;
        remainingCards = 0;

        for (int i = 0; i < cardCount; i++)
        {
            Card card = deck.DrawCard();

            if (card == null)
            {
                Debug.LogError($"カードを取得できませんでした：{i}");
                break;
            }

            MemoryBurstCardView view = Instantiate(cardPrefab, cardArea);
            view.Setup(card, backImage, OnCardClicked);

            cards.Add(view);
            remainingCards++;
        }

        Debug.Log($"{cards.Count}枚のカードを配置しました。");
    }

    // =================================
    // カードをめくる
    // =================================

    private void OnCardClicked(MemoryBurstCardView card)
    {
        if (isChecking || card == null)
            return;

        if (firstCard == card || secondCard == card)
            return;

        if (card.IsRevealed || card.IsMatched)
            return;

        card.Reveal();

        if (firstCard == null)
        {
            firstCard = card;
            return;
        }

        secondCard = card;
        StartCoroutine(CheckPair());
    }

    // =================================
    // ペア判定
    // =================================

    private IEnumerator CheckPair()
    {
        isChecking = true;

        if (firstCard.Number == secondCard.Number)
        {
            yield return new WaitForSeconds(matchedDisplayTime);

            firstCard.SetMatched();
            secondCard.SetMatched();

            remainingCards -= 2;

            Debug.Log("ペア成立！");

            firstCard = null;
            secondCard = null;

            OnPairMatched?.Invoke();

            if (remainingCards <= 0)
            {
                isChecking = false;
                OnAllCardsMatched?.Invoke();
                yield break;
            }
        }
        else
        {
            yield return new WaitForSeconds(flipBackDelay);

            if (firstCard != null)
                firstCard.FlipBack();

            if (secondCard != null)
                secondCard.FlipBack();

            firstCard = null;
            secondCard = null;

            OnPairFailed?.Invoke();
        }

        isChecking = false;
    }

    // =================================
    // 強制ランダム公開能力
    // =================================

    public bool ForceRandomReveal()
    {
        if (isChecking || AllCardsMatched)
            return false;

        // 裏向きでペア成立していないカードを取得
        List<MemoryBurstCardView> targets = new();

        foreach (MemoryBurstCardView card in cards)
        {
            if (card != null &&
                !card.IsRevealed &&
                !card.IsMatched)
            {
                targets.Add(card);
            }
        }

        // 対象カードがなければ失敗
        if (targets.Count == 0)
        {
            Debug.Log("公開できるカードがありません。");
            return false;
        }

        // ランダムに1枚選択
        int randomIndex = UnityEngine.Random.Range(0, targets.Count);

        MemoryBurstCardView selectedCard = targets[randomIndex];

        Debug.Log("ランダムに選ばれたカードを強制公開！");

        // 通常のカード選択処理に通す
        OnCardClicked(selectedCard);

        return true;
    }

    // =================================
    // タイムアウト時のカード処理
    // =================================

    public void HidePendingCards()
    {
        if (firstCard != null)
            firstCard.FlipBack();

        if (secondCard != null)
            secondCard.FlipBack();

        firstCard = null;
        secondCard = null;
    }

    // =================================
    // シャッフル能力
    // =================================


    public bool ShuffleUnmatchedCards()
    {
        if (isChecking) return false;

        // 現在のカードエリアの並び順を取得
        List<Transform> currentOrder = new List<Transform>();

        foreach (Transform child in cardArea)
        {
            currentOrder.Add(child);
        }

        // シャッフル対象の位置を記録
        List<int> shufflePositions = new List<int>();

        // シャッフル対象のカードを取得
        List<Transform> shuffleCards = new List<Transform>();

        for (int i = 0; i < currentOrder.Count; i++)
        {
            MemoryBurstCardView card =
                currentOrder[i].GetComponent<MemoryBurstCardView>();

            if (card == null)
                continue;

            // ペア成立済み・表向きのカードは動かさない
            if (card.IsMatched || card.IsRevealed)
                continue;

            shufflePositions.Add(i);
            shuffleCards.Add(currentOrder[i]);
        }

        // シャッフルできるカードが2枚未満なら終了
        if (shuffleCards.Count < 2)
        {
            Debug.Log("シャッフルできるカードがありません。");
            return false;
        }

        // Fisher-Yatesアルゴリズムでカードをシャッフル
        for (int i = shuffleCards.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            Transform temp = shuffleCards[i];
            shuffleCards[i] = shuffleCards[randomIndex];
            shuffleCards[randomIndex] = temp;
        }

        // シャッフル対象の位置だけを入れ替える
        for (int i = 0; i < shufflePositions.Count; i++)
        {
            currentOrder[shufflePositions[i]] = shuffleCards[i];
        }

        // 新しい並び順を反映
        for (int i = 0; i < currentOrder.Count; i++)
        {
            currentOrder[i].SetSiblingIndex(i);
        }

        Debug.Log($"未成立カード{shuffleCards.Count}枚をシャッフルしました！");

        return true;
    }

}
