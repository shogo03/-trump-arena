
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BJdraw : MonoBehaviour
{
    public Deck deck;
    public GameObject cardPrefab;
    public Transform handArea;
    public Transform handArea2;
    public Transform handArea3;
    public Text pointText;
    public Text pointText2;
    public Text pointText3;

    public Text resultText;
    public Text resultText2;
    public Text resultText3;

    public MoneyManager moneyManager;

    private List<Card> hand = new List<Card>();
    private List<Card> hand2 = new List<Card>();
    private List<Card> hand3 = new List<Card>();

    public Transform dealerHandArea;
    public Text dealerPointText;
    private List<Image> dealerCardImages = new List<Image>();

    private List<Card> dealerHand = new List<Card>();
    public Vector2 cardSize = new Vector2(60, 80);

    private bool isBust = false;
    private bool isBust2 = false;
    private bool isBust3 = false;

    public void DrawCard()
    {
        if (isBust)
        {
            Debug.Log("P1はバーストしているため引けません");
            return;
        }

        if (isStand)
        {
            Debug.Log("STAND中はカードを引けません");
            return;
        }

        if (deck == null)
        {
            Debug.LogError("Deckが未設定！");
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("Card Prefabが未設定！");
            return;
        }

        if (handArea == null)
        {
            Debug.LogError("Hand Areaが未設定！");
            return;
        }

        Card card = deck.DrawCard();

        GameObject obj = Instantiate(cardPrefab, handArea);

        Image image = obj.GetComponent<Image>();

        if (image == null)
        {
            Debug.LogError("Card PrefabにImageがない！");
            return;
        }

        image.sprite = card.image;
        image.enabled = true;

        hand.Add(card);

        RectTransform rect = obj.GetComponent<RectTransform>();

        // カードを小さくする
        rect.sizeDelta = cardSize;

        // カードを横に並べる
        rect.anchoredPosition = new Vector2(
            (hand.Count - 1) * 35,
            0
        );
        UpdatePoint();
    }

    public void DrawCard2()
    {
        if (isBust2)
        {
            Debug.Log("P2はバーストしているため引けません");
            return;
        }

        if (isStand2)
        {
            Debug.Log("2人目はSTAND中です");
            return;
        }

        DrawForPlayer(hand2, handArea2);
        UpdatePoint2();
    }

    public void DrawCard3()
    {
        if (isBust3)
        {
            Debug.Log("P3はバーストしているため引けません");
            return;
        }

        if (isStand3)
        {
            Debug.Log("3人目はSTAND中です");
            return;
        }

        DrawForPlayer(hand3, handArea3);
        UpdatePoint3();
    }

    void DrawForPlayer(List<Card> playerHand, Transform area)
    {
        if (deck == null || cardPrefab == null || area == null)
        {
            Debug.LogError("Deck、Card Prefab、Hand Areaの設定を確認してね！");
            return;
        }

        Card card = deck.DrawCard();

        GameObject obj = Instantiate(cardPrefab, area);

        Image image = obj.GetComponent<Image>();

        if (image == null)
        {
            Debug.LogError("Card PrefabにImageがない！");
            Destroy(obj);
            return;
        }

        image.sprite = card.image;
        image.enabled = true;

        playerHand.Add(card);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = cardSize;
        rect.anchoredPosition = new Vector2((playerHand.Count - 1) * 35, 0);
    }
    void UpdatePoint()
    {
        CalculatePoint(hand, pointText);
    }

    void UpdatePoint2()
    {
        CalculatePoint(hand2, pointText2);
    }

    void UpdatePoint3()
    {
        CalculatePoint(hand3, pointText3);
    }

    void CalculatePoint(List<Card> playerHand, Text targetText)
    {
        if (targetText == null) return;

        int total = 0;
        int aceCount = 0;

        foreach (Card card in playerHand)
        {
            if (card.number == 1)
            {
                total += 11;
                aceCount++;
            }
            else if (card.number >= 11)
            {
                total += 10;
            }
            else
            {
                total += card.number;
            }
        }

        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        if (total > 21)
        {
            targetText.text = "バースト！ 合計：" + total;

            if (playerHand == hand)
                isBust = true;
            else if (playerHand == hand2)
                isBust2 = true;
            else if (playerHand == hand3)
                isBust3 = true;
        }
        else
        {
            targetText.text = "合計：" + total;
        }
    }

    private bool isStand = false;
    private bool isStand2 = false;
    private bool isStand3 = false;

    public void Stand()
    {
        isStand = true;
        Debug.Log("P1：STANDしました");
        CheckAllStand();
    }

    public void Stand2()
    {
        isStand2 = true;
        Debug.Log("P2：STANDしました");
        CheckAllStand();
    }

    public void Stand3()
    {
        isStand3 = true;
        Debug.Log("P3：STANDしました");
        CheckAllStand();
    }
    public void StartGame()
    {

<<<<<<< HEAD
=======
        // ベット
>>>>>>> ac21a4cf24cc0383cff0cb01b89315b7cab5aa71
        moneyManager.PlaceBet(1);
        moneyManager.PlaceBet(2);
        moneyManager.PlaceBet(3);

        // P1に2枚配る
        DrawCard();
        DrawCard();

        // P2に2枚配る
        DrawCard2();
        DrawCard2();

        // P3に2枚配る
        DrawCard3();
        DrawCard3();

        DrawDealerCard();
        DrawDealerCard();
    }

    void Start()
    {
        StartGame();
    }

    public void DrawDealerCard()
    {
        if (deck == null || cardPrefab == null || dealerHandArea == null)
        {
            Debug.LogError("ディーラーの設定を確認してね！");
            return;
        }

        Card card = deck.DrawCard();

        GameObject obj = Instantiate(cardPrefab, dealerHandArea);

        Image image = obj.GetComponent<Image>();

        if (image == null)
        {
            Destroy(obj);
            Debug.LogError("Card PrefabにImageがない！");
            return;
        }

        // 1枚目は裏向き、2枚目以降は表向き
        if (dealerHand.Count == 0)
        {
            image.sprite = deck.cardBackImage;
        }
        else
        {
            image.sprite = card.image;
        }

        image.enabled = true;
        dealerCardImages.Add(image);
        dealerHand.Add(card);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = cardSize;
        rect.anchoredPosition = new Vector2(
            (dealerHand.Count - 1) * 35,
            0
        );

        UpdateDealerPoint();
    }

    void UpdateDealerPoint()
    {
        CalculatePoint(dealerHand, dealerPointText);
    }

    void CheckAllStand()
    {
        if (isStand && isStand2 && isStand3)
        {
            DealerTurn();
        }
    }

    public void RevealDealerCards()
    {
        for (int i = 0; i < dealerHand.Count; i++)
        {
            dealerCardImages[i].sprite = dealerHand[i].image;
        }

        UpdateDealerPoint();
    }

    int GetDealerTotal()
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in dealerHand)
        {
            if (card.number == 1)
            {
                total += 11;
                aceCount++;
            }
            else if (card.number >= 11)
            {
                total += 10;
            }
            else
            {
                total += card.number;
            }
        }

        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }

    void DealerTurn()
    {
        // 17を超えるまでカードを引く
        while (GetDealerTotal() < 17)
        {
            DrawDealerCard();
        }

        // 引き終わったらカードを表にする
        RevealDealerCards();

        Debug.Log("ディーラーの最終点数：" + GetDealerTotal());

        CheckWinner();
    }

    void CheckWinner()
    {
        int dealerTotal = GetDealerTotal();

        CheckPlayerWinner(hand, resultText, "P1", dealerTotal, isBust);
        CheckPlayerWinner(hand2, resultText2, "P2", dealerTotal, isBust2);
        CheckPlayerWinner(hand3, resultText3, "P3", dealerTotal, isBust3);
    }

    void CheckPlayerWinner(
    List<Card> playerHand,
    Text targetText,
    string playerName,
    int dealerTotal,
    bool playerBust)
    {
        if (targetText == null) return;

        int playerTotal = CalculateHandTotal(playerHand);

        int playerNumber = 0;

        if (playerName == "P1")
            playerNumber = 1;
        else if (playerName == "P2")
            playerNumber = 2;
        else if (playerName == "P3")
            playerNumber = 3;


        if (playerBust)
        {
            targetText.text = "負け！";

            if (moneyManager != null)
                moneyManager.Lose(playerNumber);
        }
        else if (dealerTotal > 21)
        {
            targetText.text = "勝ち！";

            if (moneyManager != null)
                moneyManager.Win(playerNumber);
        }
        else if (playerTotal > dealerTotal)
        {
            targetText.text = "勝ち！";

            if (moneyManager != null)
                moneyManager.Win(playerNumber);
        }
        else if (playerTotal < dealerTotal)
        {
            targetText.text = "負け！";

            if (moneyManager != null)
                moneyManager.Lose(playerNumber);
        }
        else
        {
            targetText.text = "引き分け！";

            if (moneyManager != null)
                moneyManager.Draw(playerNumber);
        }
    }

    int CalculateHandTotal(List<Card> playerHand)
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in playerHand)
        {
            if (card.number == 1)
            {
                total += 11;
                aceCount++;
            }
            else if (card.number >= 11)
            {
                total += 10;
            }
            else
            {
                total += card.number;
            }
        }

        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }


        return total;
    }
}
//ここから下 変更用

