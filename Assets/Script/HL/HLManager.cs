
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

    [Header("能力管理")]
    [SerializeField] private HighLowAbilityManager abilityManager;

    [Header("タイマー管理")]
    [SerializeField] private HighLowTimerManager timerManager;

    [Header("残り枚数表示")]
    [SerializeField] private TextMeshProUGUI remainingCardsText;

    [Header("UI管理")]
    [SerializeField] private HLUIManager uiManager;

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

    // 最後に反映した革命状態
    private bool lastRevolutionState;
    private bool revolutionStateInitialized = false;

    private void Start()
    {
        highButton.onClick.AddListener(OnHighButtonClicked);
        lowButton.onClick.AddListener(OnLowButtonClicked);

        StartGame();
    }

    private void Update()
    {
        // 革命状態を常に監視
        CheckRevolutionState();
    }

    private void OnDestroy()
    {
        if (highButton != null)
            highButton.onClick.RemoveListener(OnHighButtonClicked);

        if (lowButton != null)
            lowButton.onClick.RemoveListener(OnLowButtonClicked);
    }

    // =========================
    // ゲーム開始
    // =========================

    public void StartGame()
    {
        deck.CreateDeck();

        // ターン管理を先に初期化
        turnManager.Initialize();

        if (abilityManager != null)
            abilityManager.Initialize();

        scoreManager.Initialize();

        timerManager.Initialize();
        timerManager.StartTimer();

        ResetSelection();

        previousCard = null;

        // 革命状態を初期化
        revolutionStateInitialized = false;
        CheckRevolutionState();

        ShowFirstCard();
    }

    private void ShowFirstCard()
    {
        currentCard = deck.DrawCard();

        UpdateRemainingCards();

        if (currentCard == null)
            return;

        DisplayCard(currentCard);

        Debug.Log($"最初のカード：{currentCard.suit} {currentCard.number}");
    }

    // =========================
    // カード表示
    // =========================

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

        HighLowCardView cardView =
            currentCardObject.GetComponent<HighLowCardView>();

        if (cardView == null)
        {
            Debug.LogError("HighLowCardViewがPrefabにありません");
            return;
        }

        cardView.Setup(card.image, backImage);

        SetButtonsInteractable(false);

        cardView.Flip();

        StartCoroutine(WaitForFlip(cardView));
    }

    private IEnumerator WaitForFlip(HighLowCardView cardView)
    {
        while (cardView != null && cardView.IsFlipping)
        {
            yield return null;
        }

        if (currentCardObject != null &&
            currentCardObject.GetComponent<HighLowCardView>() == cardView)
        {
            RefreshPredictionButtons();
        }
    }

    // =========================
    // HIGH・LOWボタン
    // =========================

    public void RefreshPredictionButtons()
    {
        HighLowCardView cardView =
            currentCardObject != null
            ? currentCardObject.GetComponent<HighLowCardView>()
            : null;

        bool canSelect =
            cardView != null &&
            !cardView.IsFlipping &&
            currentPrediction == Prediction.None;

        SetButtonsInteractable(canSelect);
    }

    private void SetButtonsInteractable(bool value)
    {
        bool highAllowed = abilityManager == null ||
            !abilityManager.IsPredictionBlocked(
                turnManager.CurrentPlayer,
                Prediction.High
            );

        bool lowAllowed = abilityManager == null ||
            !abilityManager.IsPredictionBlocked(
                turnManager.CurrentPlayer,
                Prediction.Low
            );

        if (highButton != null)
            highButton.interactable = value && highAllowed;

        if (lowButton != null)
            lowButton.interactable = value && lowAllowed;
    }

    private void OnHighButtonClicked()
    {
        SelectPrediction(Prediction.High);
    }

    private void OnLowButtonClicked()
    {
        SelectPrediction(Prediction.Low);
    }

    private void SelectPrediction(Prediction prediction)
    {
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

        int predictingPlayer = turnManager.CurrentPlayer;

        // 妨害されている予想なら操作禁止
        if (abilityManager != null &&
            abilityManager.IsPredictionBlocked(
                predictingPlayer,
                prediction))
        {
            Debug.Log("この予想は妨害されています！");
            return;
        }

        currentPrediction = prediction;

        Debug.Log(
            $"PLAYER {predictingPlayer} の予想：{prediction}"
        );

        turnManager.SetPrediction(prediction);

        if (abilityManager != null)
        {
            abilityManager.OnPredictionMade(predictingPlayer);
            abilityManager.RefreshAbilityButtons();
        }

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

            RefreshPredictionButtons();
        }
    }

    // =========================
    // 次のカード
    // =========================

    private void ShowNextCard()
    {
        previousCard = currentCard;

        HighLowDeck.CardData nextCard = deck.DrawCard();

        UpdateRemainingCards();

        if (nextCard == null)
        {
            Debug.Log("デッキがなくなりました");

            if (highButton != null)
                highButton.interactable = false;

            if (lowButton != null)
                lowButton.interactable = false;

            return;
        }

        CheckPredictions(nextCard);

        currentCard = nextCard;

        DisplayCard(currentCard);

        Debug.Log(
            $"次のカード：{currentCard.suit} {currentCard.number}"
        );
    }

    // =========================
    // 正解判定
    // =========================


    private void CheckPredictions(HighLowDeck.CardData nextCard)
    {
        int oldNumber = previousCard.number;
        int newNumber = nextCard.number;

        Debug.Log($"前のカード：{oldNumber}");
        Debug.Log($"次のカード：{newNumber}");

        Prediction correctPrediction;

        // 同じ数字の場合
        if (newNumber == oldNumber)
        {
            correctPrediction = Prediction.High;

            Debug.Log("同じ数字！HIGHが正解！");
        }
        else
        {
            // 通常の正解判定
            correctPrediction =
                newNumber > oldNumber
                ? Prediction.High
                : Prediction.Low;
        }

        // 革命中ならHIGHとLOWを逆転
        if (abilityManager != null &&
            abilityManager.ShouldReverseHighLow)
        {
            correctPrediction =
                correctPrediction == Prediction.High
                ? Prediction.Low
                : Prediction.High;

            Debug.Log("【革命発動中】HIGHとLOWの判定が逆転！");
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

    // =========================
    // 革命背景管理
    // =========================

    private void CheckRevolutionState()
    {
        if (uiManager == null || abilityManager == null)
            return;

        bool currentRevolutionState =
            abilityManager.ShouldReverseHighLow;

        // 初回または革命状態が変化したときだけ更新
        if (!revolutionStateInitialized ||
            lastRevolutionState != currentRevolutionState)
        {
            lastRevolutionState = currentRevolutionState;
            revolutionStateInitialized = true;

            uiManager.SetRevolutionBackground(
                currentRevolutionState
            );

            if (currentRevolutionState)
            {
                Debug.Log("革命開始！背景を群青色に変更");
            }
            else
            {
                Debug.Log("革命終了！背景を通常色に戻しました");
            }
        }
    }

    // =========================
    // その他
    // =========================

    private void ResetSelection()
    {
        currentPrediction = Prediction.None;
        RefreshPredictionButtons();
    }

    private void UpdateRemainingCards()
    {
        if (remainingCardsText != null)
        {
            remainingCardsText.text =
                "残 : " + deck.RemainingCards + "枚";
        }
    }
}
