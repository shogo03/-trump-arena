
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HLManager : MonoBehaviour
{
    [Header("デッキ")]
    [SerializeField] private HighLowDeck deck;

    [Header("カード表示")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardArea;
    [SerializeField] private Sprite backImage;

    [Header("HIGH・LOWボタン")]
    [SerializeField] private Button highButton;
    [SerializeField] private Button lowButton;

    [Header("ターン管理")]
    [SerializeField] private HighLowTurnManager turnManager;

    [Header("スコア管理")]
    [SerializeField] private HighLowScoreManager scoreManager;

    [Header("スキル管理")]
    [SerializeField] private HighLowAbilityManager abilityManager;

    [SerializeField] private HighLowTimerManager timerManager;

    [Header("残り枚数表示")]
    [SerializeField] private TextMeshProUGUI remainingCardsText;

    public enum Prediction
    {
        None,
        High,
        Low
    }

    private Prediction currentPrediction = Prediction.None;

    private GameObject currentCardObject;

    private HighLowDeck.CardData currentCard;
    private HighLowDeck.CardData previousCard;

    public Prediction CurrentPrediction => currentPrediction;

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

        turnManager.Initialize();
        scoreManager.Initialize();

        timerManager.Initialize();
        timerManager.StartTimer();

        ResetSelection();

        previousCard = null;

        ShowFirstCard();
    }

    // 最初のカード
    private void ShowFirstCard()
    {
        currentCard = deck.DrawCard();

        currentCard = deck.DrawCard();
        UpdateRemainingCards();

        if (currentCard == null)
            return;

        DisplayCard(currentCard);

        Debug.Log($"最初のカード：{currentCard.suit} {currentCard.number}");
    }

    // カード表示・めくるアニメーション
    private void DisplayCard(HighLowDeck.CardData card)
    {
        if (card == null || card.image == null)
        {
            Debug.LogError("カードデータまたは画像がありません");
            return;
        }

        if (backImage == null)
        {
            Debug.LogError("カード裏面画像が設定されていません");
            return;
        }

        if (currentCardObject != null)
            Destroy(currentCardObject);

        currentCardObject = Instantiate(cardPrefab, cardArea);

        RectTransform rect =
            currentCardObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 100f);
        rect.sizeDelta = new Vector2(210f, 350f);
        rect.localScale = Vector3.one;

        // アニメーション用スクリプトを取得
        HighLowCardView cardView =
            currentCardObject.GetComponent<HighLowCardView>();

        if (cardView == null)
        {
            Debug.LogError("HighLowCardViewがPrefabにありません");
            return;
        }

        // 裏面を表示してからめくる
        cardView.Setup(card.image, backImage);

        // アニメーション中はボタンを無効化
        SetButtonsInteractable(false);

        cardView.Flip();

        StartCoroutine(WaitForFlip(cardView));
    }

    // アニメーション終了まで待つ
    private IEnumerator WaitForFlip(HighLowCardView cardView)
    {
        while (cardView != null && cardView.IsFlipping)
        {
            yield return null;
        }

        // 現在表示中のカードか確認
        if (currentCardObject != null &&
            currentCardObject.GetComponent<HighLowCardView>() == cardView)
        {
            SetButtonsInteractable(true);
        }
    }

    // ボタンの有効・無効
    private void SetButtonsInteractable(bool value)
    {
        if (highButton != null)
            highButton.interactable = value;

        if (lowButton != null)
            lowButton.interactable = value;
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

    // 予想選択
    private void SelectPrediction(Prediction prediction)
    {
        // アニメーション中は操作禁止
        HighLowCardView cardView =
            currentCardObject != null
            ? currentCardObject.GetComponent<HighLowCardView>()
            : null;

        if (cardView == null || cardView.IsFlipping)
            return;

        if (currentPrediction != Prediction.None)
            return;

        if (turnManager.BothPlayersPredicted())
            return;

        currentPrediction = prediction;

        Debug.Log(
            $"PLAYER {turnManager.CurrentPlayer} の予想：{prediction}"
        );

        turnManager.SetPrediction(prediction);

        if (turnManager.BothPlayersPredicted())
        {
            Debug.Log("2人の予想が完了しました");

            Debug.Log($"P1：{turnManager.Player1Prediction}");
            Debug.Log($"P2：{turnManager.Player2Prediction}");

            ShowNextCard();

            turnManager.NextRound();

            ResetSelection();
        }
        else
        {
            currentPrediction = Prediction.None;
        }
    }

    // 次のカードを表示
    private void ShowNextCard()
    {
        previousCard = currentCard;

        HighLowDeck.CardData nextCard = deck.DrawCard();

        currentCard = deck.DrawCard();
        UpdateRemainingCards();

        if (nextCard == null)
        {
            Debug.Log("デッキがなくなりました");

            highButton.interactable = false;
            lowButton.interactable = false;
            return;
        }

        // 2人の予想を判定して加点
        CheckPredictions(nextCard);

        currentCard = nextCard;

        DisplayCard(currentCard);

        Debug.Log(
            $"次のカード：{currentCard.suit} {currentCard.number}"
        );
    }

    // 正解判定
    private void CheckPredictions(HighLowDeck.CardData nextCard)
    {
        int oldNumber = previousCard.number;
        int newNumber = nextCard.number;

        Debug.Log($"前のカード：{oldNumber}");
        Debug.Log($"次のカード：{newNumber}");

        if (newNumber == oldNumber)
        {
            Debug.Log("同じ数字！今回は両者ポイントなし");
            return;
        }

        Prediction correctPrediction;

        if (newNumber > oldNumber)
        {
            correctPrediction = Prediction.High;
        }
        else
        {
            correctPrediction = Prediction.Low;
        }

        Debug.Log($"正解は：{correctPrediction}");

        // Player1の判定
        if (turnManager.Player1Prediction == correctPrediction)
        {
            scoreManager.AddPlayer1Score(1);
            Debug.Log("Player1 正解！ +1ポイント");
        }
        else
        {
            Debug.Log("Player1 不正解");
        }

        // Player2の判定
        if (turnManager.Player2Prediction == correctPrediction)
        {
            scoreManager.AddPlayer2Score(1);
            Debug.Log("Player2 正解！ +1ポイント");
        }
        else
        {
            Debug.Log("Player2 不正解");
        }
    }

    // 選択リセット
    private void ResetSelection()
    {
        currentPrediction = Prediction.None;

        // カードのアニメーション中ならボタンを有効にしない
        HighLowCardView cardView =
            currentCardObject != null
            ? currentCardObject.GetComponent<HighLowCardView>()
            : null;

        bool canSelect = cardView != null && !cardView.IsFlipping;

        SetButtonsInteractable(canSelect);
    }

    private void UpdateRemainingCards()
    {
        if (remainingCardsText != null)
        {
            remainingCardsText.text = "残 : " + deck.RemainingCards + "枚";
        }
    }
}
