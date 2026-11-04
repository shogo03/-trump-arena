
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MemoryBurstCardManager : MonoBehaviour
{
    [Header("カード設定")]
    [SerializeField] private MemoryBurstDeck deck;
    [SerializeField] private MemoryBurstCardView cardPrefab;
    [SerializeField] private Sprite backImage;
    [SerializeField] private Transform cardArea;

    [SerializeField] private int cardCount = 24;
    [SerializeField] private float flipBackDelay = 1f;
    [SerializeField] private float matchedDisplayTime = 1f;

    private readonly List<MemoryBurstCardView> cards = new();

    private MemoryBurstCardView firstCard;
    private MemoryBurstCardView secondCard;

    private bool isChecking;
    private bool turnFailed;
    private int remainingCards;

    public bool IsChecking => isChecking;
    public int RemainingCards => remainingCards;

    public event Action<int> OnPairMatched;
    public event Action OnPairFailed;
    public event Action OnGameCompleted;

    public void Initialize()
    {
        if (deck == null || cardPrefab == null ||
            cardArea == null || backImage == null)
        {
            Debug.LogError("カードの設定が不足しています。");
            return;
        }

        deck.Initialize();

        foreach (Transform child in cardArea)
            Destroy(child.gameObject);

        cards.Clear();

        firstCard = null;
        secondCard = null;

        isChecking = false;
        turnFailed = false;
        remainingCards = 0;

        for (int i = 0; i < cardCount; i++)
        {
            Card card = deck.DrawCard();

            if (card == null)
            {
                Debug.LogError("カードを取得できませんでした。");
                break;
            }

            MemoryBurstCardView view =
                Instantiate(cardPrefab, cardArea);

            view.Setup(card, backImage, OnCardClicked);

            cards.Add(view);
            remainingCards++;
        }
    }

    private void OnCardClicked(MemoryBurstCardView card)
    {
        if (isChecking || turnFailed || card == null)
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

    private IEnumerator CheckPair()
    {
        isChecking = true;

        if (firstCard.Number == secondCard.Number)
        {
            yield return new WaitForSeconds(matchedDisplayTime);

            firstCard.SetMatched();
            secondCard.SetMatched();

            remainingCards -= 2;

            firstCard = null;
            secondCard = null;

            isChecking = false;

            OnPairMatched?.Invoke(remainingCards);

            if (remainingCards <= 0)
            {
                OnGameCompleted?.Invoke();
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

            turnFailed = true;
            isChecking = false;

            OnPairFailed?.Invoke();
        }
    }

    // タイムアウト時に呼び出す
    public void EndCurrentTurn()
    {
        if (firstCard != null)
        {
            firstCard.FlipBack();
            firstCard = null;
        }

        if (secondCard != null)
        {
            secondCard.FlipBack();
            secondCard = null;
        }

        turnFailed = false;
    }

    // 次のプレイヤーがカードを選べるようにする
    public void ResetTurn()
    {
        turnFailed = false;
    }

    // 裏向きカードのシャッフル
    public bool ShuffleCards()
    {
        if (isChecking || firstCard != null)
            return false;

        List<MemoryBurstCardView> targets =
            cards.FindAll(card =>
                card != null &&
                !card.IsMatched &&
                !card.IsRevealed
            );

        if (targets.Count < 2)
            return false;

        List<Transform> order = new();

        for (int i = 0; i < cardArea.childCount; i++)
            order.Add(cardArea.GetChild(i));

        List<int> slots = new();

        foreach (var card in targets)
            slots.Add(card.transform.GetSiblingIndex());

        for (int i = targets.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (targets[i], targets[j]) =
                (targets[j], targets[i]);
        }

        for (int i = 0; i < slots.Count; i++)
            order[slots[i]] = targets[i].transform;

        for (int i = 0; i < order.Count; i++)
            order[i].SetSiblingIndex(i);

        return true;
    }
}
