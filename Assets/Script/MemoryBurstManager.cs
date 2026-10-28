
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MemoryBurstManager : MonoBehaviour
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
    [SerializeField] private int columns = 6;
    [SerializeField] private float flipBackDelay = 1f;
    [SerializeField] private float matchedDisplayTime = 1f;

    [Header("得点表示（プレイヤー1～4）")]
    [SerializeField]
    private TextMeshProUGUI[] playerScoreTexts =
        new TextMeshProUGUI[4];

    [Header("ターン表示")]
    [SerializeField] private TextMeshProUGUI turnText;

    private readonly List<MemoryBurstCardView> cards = new();

    private MemoryBurstCardView firstCard;
    private MemoryBurstCardView secondCard;

    private int currentPlayer = 1;
    private int playerCount = 1;
    private int[] scores = new int[4];

    private bool isChecking;
    private bool isGameOver;
    private int remainingCards;

    public int CurrentPlayer => currentPlayer;

    // 現在のプレイヤーに対応するコントローラーだけ操作可能
    public bool CanControl(int gamepadIndex)
    {
        if (isChecking || isGameOver)
            return false;

        if (gamepadIndex < 0 ||
            gamepadIndex >= Gamepad.all.Count)
            return false;

        return gamepadIndex == currentPlayer - 1;
    }

    private void Start()
    {
        int controllerCount = Gamepad.all.Count;
        playerCount = Mathf.Clamp(controllerCount, 1, 4);

        Debug.Log($"コントローラー接続数：{controllerCount}");
        Debug.Log($"プレイヤー人数：{playerCount}");

        CreateCards();
        UpdateUI();
    }

    private void CreateCards()
    {
        if (deck == null || cardPrefab == null ||
            cardArea == null || backImage == null)
        {
            Debug.LogError(
                "Deck、Card Prefab、Back Image、Card Areaを設定してください。",
                this
            );
            return;
        }

        if (cardCount < 2 || cardCount % 2 != 0)
        {
            Debug.LogError("カード枚数は2以上の偶数にしてください。", this);
            return;
        }

        deck.Initialize();

        foreach (Transform child in cardArea)
            Destroy(child.gameObject);

        cards.Clear();
        firstCard = null;
        secondCard = null;
        currentPlayer = 1;
        scores = new int[4];
        isChecking = false;
        isGameOver = false;
        remainingCards = 0;

        for (int i = 0; i < cardCount; i++)
        {
            Card card = deck.DrawCard();

            if (card == null)
            {
                Debug.LogError($"カードを取得できませんでした：{i}", this);
                break;
            }

            MemoryBurstCardView view =
                Instantiate(cardPrefab, cardArea);

            view.Setup(card, backImage, OnCardClicked);

            cards.Add(view);
            remainingCards++;
        }

        UpdateUI();
    }

    private void OnCardClicked(MemoryBurstCardView card)
    {
        if (isChecking || isGameOver || card == null)
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
        UpdateUI();

        if (firstCard.Number == secondCard.Number)
        {
            yield return new WaitForSeconds(matchedDisplayTime);

            firstCard.SetMatched();
            secondCard.SetMatched();

            scores[currentPlayer - 1]++;
            remainingCards -= 2;

            Debug.Log($"プレイヤー{currentPlayer}がペア成立！");

            firstCard = null;
            secondCard = null;

            if (remainingCards <= 0)
            {
                isGameOver = true;
                isChecking = false;
                UpdateUI();
                ShowResult();
                yield break;
            }

            // ペア成立時は同じプレイヤーのターンを継続
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

            // ペア不成立なら次のプレイヤーへ
            currentPlayer = currentPlayer % playerCount + 1;
        }

        isChecking = false;
        UpdateUI();
    }

    private void UpdateUI()
    {
        for (int i = 0; i < 4; i++)
        {
            if (playerScoreTexts == null ||
                i >= playerScoreTexts.Length ||
                playerScoreTexts[i] == null)
                continue;

            bool active = i < playerCount;
            playerScoreTexts[i].gameObject.SetActive(active);

            if (active)
                playerScoreTexts[i].text =
                    $"プレイヤー{i + 1}：{scores[i]}点";
        }

        if (turnText != null && !isGameOver)
        {
            turnText.text = isChecking
                ? "カード判定中..."
                : $"プレイヤー{currentPlayer}のターン";
        }
    }

    private void ShowResult()
    {
        if (turnText == null)
            return;

        int maxScore = -1;

        for (int i = 0; i < playerCount; i++)
            maxScore = Mathf.Max(maxScore, scores[i]);

        List<string> winners = new();

        for (int i = 0; i < playerCount; i++)
        {
            if (scores[i] == maxScore)
                winners.Add($"プレイヤー{i + 1}");
        }

        if (playerCount == 1)
            turnText.text = $"ゲームクリア！ 得点：{scores[0]}点";
        else if (winners.Count > 1)
            turnText.text = $"引き分け！\n{string.Join("・", winners)}";
        else
            turnText.text = $"{winners[0]}の勝利！";

        Debug.Log(turnText.text);
    }

}
