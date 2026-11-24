
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("シャッフル演出")]
    [SerializeField] private float gatherDuration = 0.4f;
    [SerializeField] private float scatterDuration = 0.5f;
    [SerializeField] private float centerWaitTime = 0.2f;

    private readonly List<MemoryBurstCardView> cards = new();

    private MemoryBurstCardView firstCard;
    private MemoryBurstCardView secondCard;

    private bool isChecking;
    private bool isShuffling;
    private int remainingCards;

    private LayoutGroup layoutGroup;

    public bool IsChecking => isChecking || isShuffling;
    public bool AllCardsMatched => remainingCards <= 0;

    public event Action OnPairMatched;
    public event Action OnPairFailed;
    public event Action OnAllCardsMatched;

    public event Action OnCardsShuffled;
    public event Action<MemoryBurstCardView> OnCardRevealed;

    public IReadOnlyList<MemoryBurstCardView> Cards => cards;

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

        layoutGroup = cardArea.GetComponent<LayoutGroup>();

        deck.Initialize();

        foreach (Transform child in cardArea)
            Destroy(child.gameObject);

        cards.Clear();

        firstCard = null;
        secondCard = null;

        isChecking = false;
        isShuffling = false;
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
        if (IsChecking || card == null)
            return;

        if (firstCard == card || secondCard == card)
            return;

        if (card.IsRevealed || card.IsMatched)
            return;

        card.Reveal();

        OnCardRevealed?.Invoke(card);

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
    // ランダム公開能力
    // =================================

    public bool ForceRandomReveal()
    {
        if (IsChecking || AllCardsMatched)
            return false;

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

        if (targets.Count == 0)
        {
            Debug.Log("公開できるカードがありません。");
            return false;
        }

        int randomIndex = UnityEngine.Random.Range(0, targets.Count);

        MemoryBurstCardView selectedCard = targets[randomIndex];

        Debug.Log("ランダムに選ばれたカードを強制公開！");

        OnCardClicked(selectedCard);

        // 選ばれたカードを発光
        selectedCard.PlayGlow();

        return true;
    }

    // =================================
    // タイムアウト時
    // =================================

    public void HidePendingCards()
    {
        if (isShuffling)
            return;

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
        if (IsChecking || AllCardsMatched)
            return false;

        List<Transform> currentOrder = new();

        foreach (Transform child in cardArea)
        {
            currentOrder.Add(child);
        }

        List<int> shufflePositions = new();
        List<Transform> shuffleCards = new();

        for (int i = 0; i < currentOrder.Count; i++)
        {
            MemoryBurstCardView card =
                currentOrder[i].GetComponent<MemoryBurstCardView>();

            if (card == null)
                continue;

            if (card.IsMatched || card.IsRevealed)
                continue;

            shufflePositions.Add(i);
            shuffleCards.Add(currentOrder[i]);
        }

        if (shuffleCards.Count < 2)
        {
            Debug.Log("シャッフルできるカードがありません。");
            return false;
        }

        StartCoroutine(ShuffleAnimation(shufflePositions, shuffleCards));

        return true;
    }

    // =================================
    // シャッフル演出
    // =================================

    private IEnumerator ShuffleAnimation(
        List<int> shufflePositions,
        List<Transform> shuffleCards)
    {
        isShuffling = true;

        RectTransform areaRect = cardArea as RectTransform;

        if (areaRect == null)
        {
            Debug.LogError("Card AreaにはRectTransformが必要です。");
            isShuffling = false;
            yield break;
        }

        // レイアウトを停止
        if (layoutGroup != null)
            layoutGroup.enabled = false;

        // Fisher-Yatesでシャッフル
        for (int i = shuffleCards.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            Transform temp = shuffleCards[i];
            shuffleCards[i] = shuffleCards[randomIndex];
            shuffleCards[randomIndex] = temp;
        }

        // シャッフル対象の位置だけを入れ替える
        List<Transform> allChildren = new();

        foreach (Transform child in cardArea)
            allChildren.Add(child);

        for (int i = 0; i < shufflePositions.Count; i++)
        {
            allChildren[shufflePositions[i]] = shuffleCards[i];
        }

        for (int i = 0; i < allChildren.Count; i++)
        {
            allChildren[i].SetSiblingIndex(i);
        }

        // レイアウトを一度更新して移動先を取得
        if (layoutGroup != null)
        {
            layoutGroup.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(areaRect);
            layoutGroup.enabled = false;
        }

        Vector2[] targetPositions = new Vector2[shuffleCards.Count];

        Vector2 centerPosition = Vector2.zero;

        for (int i = 0; i < shuffleCards.Count; i++)
        {
            RectTransform rect =
                shuffleCards[i] as RectTransform;

            targetPositions[i] = rect.anchoredPosition;
            centerPosition += targetPositions[i];
        }

        centerPosition /= shuffleCards.Count;

        // =================================
        // 中央に集める
        // =================================

        Vector2[] startPositions = new Vector2[shuffleCards.Count];

        for (int i = 0; i < shuffleCards.Count; i++)
        {
            RectTransform rect =
                shuffleCards[i] as RectTransform;

            startPositions[i] = rect.anchoredPosition;
        }

        float elapsed = 0f;

        while (elapsed < gatherDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / gatherDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < shuffleCards.Count; i++)
            {
                RectTransform rect =
                    shuffleCards[i] as RectTransform;

                rect.anchoredPosition =
                    Vector2.Lerp(startPositions[i], centerPosition, t);
            }

            yield return null;
        }

        for (int i = 0; i < shuffleCards.Count; i++)
        {
            RectTransform rect =
                shuffleCards[i] as RectTransform;

            rect.anchoredPosition = centerPosition;
        }

        // 中央で少し待つ
        yield return new WaitForSeconds(centerWaitTime);

        // =================================
        // 散らばる
        // =================================

        elapsed = 0f;

        while (elapsed < scatterDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / scatterDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < shuffleCards.Count; i++)
            {
                RectTransform rect =
                    shuffleCards[i] as RectTransform;

                rect.anchoredPosition =
                    Vector2.Lerp(centerPosition, targetPositions[i], t);
            }

            yield return null;
        }

        for (int i = 0; i < shuffleCards.Count; i++)
        {
            RectTransform rect =
                shuffleCards[i] as RectTransform;

            rect.anchoredPosition = targetPositions[i];
        }

        // レイアウトを復帰
        if (layoutGroup != null)
        {
            layoutGroup.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(areaRect);
        }

        isShuffling = false;

        OnCardsShuffled?.Invoke();

        Debug.Log($"未成立カード{shuffleCards.Count}枚をシャッフルしました！");
    }

    // =================================
    // CPU用カード選択
    // =================================

    public bool TrySelectCard(MemoryBurstCardView card)
    {
        if (IsChecking || card == null)
            return false;

        if (firstCard == card || secondCard == card)
            return false;

        if (card.IsRevealed || card.IsMatched)
            return false;

        OnCardClicked(card);

        return true;
    }
}
