using UnityEngine;
using UnityEngine.UI;

public class HighLowManager : MonoBehaviour
{
    [Header("デッキ")]
    [SerializeField] private HighLowDeck deck;

    [Header("カード表示")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardArea;

    [Header("HIGH・LOWボタン")]
    [SerializeField] private Button highButton;
    [SerializeField] private Button lowButton;

    public enum Prediction
    {
        None,
        High,
        Low
    }

    private Prediction currentPrediction = Prediction.None;

    public Prediction CurrentPrediction => currentPrediction;

    private GameObject currentCardObject;
    private HighLowDeck.CardData currentCard;

    private void Start()
    {
        highButton.onClick.AddListener(OnHighButtonClicked);
        lowButton.onClick.AddListener(OnLowButtonClicked);

        StartGame();
    }

    private void OnDestroy()
    {
        if (highButton != null)
            highButton.onClick.RemoveListener(OnHighButtonClicked);

        if (lowButton != null)
            lowButton.onClick.RemoveListener(OnLowButtonClicked);
    }

    public void StartGame()
    {
        deck.CreateDeck();

        ResetSelection();

        ShowFirstCard();
    }

    // 最初のカードを表示
    private void ShowFirstCard()
    {
        currentCard = deck.DrawCard();

        if (currentCard == null)
            return;

        DisplayCard(currentCard);

        Debug.Log($"最初のカード：{currentCard.suit} {currentCard.number}");
    }

    // カード表示
    private void DisplayCard(HighLowDeck.CardData card)
    {
        if (card == null)
        {
            Debug.LogError("カードデータがNullです");
            return;
        }

        if (card.image == null)
        {
            Debug.LogError($"カード画像が設定されていません：{card.suit} {card.number}");
            return;
        }

        if (currentCardObject != null)
        {
            Destroy(currentCardObject);
        }

        currentCardObject = Instantiate(cardPrefab, cardArea);

        RectTransform rect = currentCardObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(150f, 100f);
        rect.sizeDelta = new Vector2(170f, 310f);
        rect.localScale = Vector3.one;

        Image cardImage = currentCardObject.GetComponentInChildren<Image>(true);

        if (cardImage == null)
        {
            Debug.LogError("HLCardにImageコンポーネントが見つかりません");
            return;
        }

        cardImage.sprite = card.image;
        cardImage.color = Color.white;
        cardImage.enabled = true;
        cardImage.preserveAspect = true;

        Debug.Log($"カード表示成功：{card.suit} {card.number}");
    }

    // HIGHボタン
    private void OnHighButtonClicked()
    {
        SelectPrediction(Prediction.High);
    }

    // LOWボタン
    private void OnLowButtonClicked()
    {
        SelectPrediction(Prediction.Low);
    }

    // 選択処理
    private void SelectPrediction(Prediction prediction)
    {
        if (currentPrediction != Prediction.None)
            return;

        currentPrediction = prediction;

        highButton.interactable = false;
        lowButton.interactable = false;

        Debug.Log($"予想選択：{currentPrediction}");
    }

    // 選択リセット
    private void ResetSelection()
    {
        currentPrediction = Prediction.None;

        highButton.interactable = true;
        lowButton.interactable = true;
    }

    // 次のカードを引く
    public HighLowDeck.CardData DrawNextCard()
    {
        return deck.DrawCard();
    }

    // 現在のカード取得
    public HighLowDeck.CardData GetCurrentCard()
    {
        return currentCard;
    }

    // 現在のカードを更新
    public void SetCurrentCard(HighLowDeck.CardData card)
    {
        currentCard = card;
        DisplayCard(card);

        ResetSelection();
    }
}