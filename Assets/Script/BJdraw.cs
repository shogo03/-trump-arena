
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

    public BJDealer bjDealer;
    public BJResult bjResult;

   
    public Vector2 cardSize = new Vector2(60, 80);

    // 現在のターン（1=P1、2=P2、3=P3）
    private int currentTurn = 1;

    // 最初のカード配布中かどうか
    private bool isInitialDeal = true;

    private bool isBust = false;
    private bool isBust2 = false;
    private bool isBust3 = false;

    public void DrawCard()
    {
        if (!isInitialDeal && currentTurn != 1)
        {
            Debug.Log("現在はP1のターンではありません");
            return;
        }

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
        if (!isInitialDeal && currentTurn != 2)
        {
            Debug.Log("現在はP2のターンではありません");
            return;
        }

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
        if (!isInitialDeal && currentTurn != 3)
        {
            Debug.Log("現在はP3のターンではありません");
            return;
        }

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
            {
                isBust = true;
                isStand = true;
                Debug.Log("P1はバースト！強制STAND！");
            }
            else if (playerHand == hand2)
            {
                isBust2 = true;
                isStand2 = true;
                Debug.Log("P2はバースト！強制STAND！");
            }
            else if (playerHand == hand3)
            {
                isBust3 = true;
                isStand3 = true;
                Debug.Log("P3はバースト！強制STAND！");
            }

            if (!isInitialDeal)
            {
                NextTurn();
            }
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
        if (currentTurn != 1) return;
        if (isStand || isBust) return;

        isStand = true;
        Debug.Log("P1：STAND！");
        NextTurn();
    }

    public void Stand2()
    {
        if (currentTurn != 2) return;
        if (isStand2 || isBust2) return;

        isStand2 = true;
        Debug.Log("P2：STAND！");
        NextTurn();
    }

    public void Stand3()
    {
        if (currentTurn != 3) return;
        if (isStand3 || isBust3) return;

        isStand3 = true;
        Debug.Log("P3：STAND！");
        NextTurn();
    }
    public void StartGame()
    {
        isInitialDeal = true;

        // P1に2枚
        DrawCard();
        DrawCard();

        // P2に2枚
        DrawCard2();
        DrawCard2();

        // P3に2枚
        DrawCard3();
        DrawCard3();

        // ディーラーに2枚
        bjDealer.DrawDealerCard();
        bjDealer.DrawDealerCard();

        // 通常のターン制開始
        isInitialDeal = false;
        currentTurn = 1;

        Debug.Log("P1のターン！");
    }

    void Start()
    {
        StartGame();
    }


    void CheckAllStand()
    {
        if (isStand && isStand2 && isStand3)
        {
            DealerTurn();
        }
    }

    void DealerTurn()
    {
        if (bjDealer == null || bjResult == null)
        {
            Debug.LogError("BJDealerまたはBJResultが設定されていません！");
            return;
        }

        // ディーラーのターンを実行
        int dealerTotal = bjDealer.DealerTurn();

        // 3人それぞれの勝敗を判定
        bjResult.CheckWinner(hand, resultText, "P1",
            dealerTotal, isBust, moneyManager);

        bjResult.CheckWinner(hand2, resultText2, "P2",
            dealerTotal, isBust2, moneyManager);

        bjResult.CheckWinner(hand3, resultText3, "P3",
            dealerTotal, isBust3, moneyManager);
    }

    void NextTurn()
    {
        currentTurn++;

        // 終了しているプレイヤーは飛ばす
        while (currentTurn <= 3)
        {
            if (currentTurn == 1 && !isStand && !isBust)
            {
                Debug.Log("P1のターン！");
                return;
            }

            if (currentTurn == 2 && !isStand2 && !isBust2)
            {
                Debug.Log("P2のターン！");
                return;
            }

            if (currentTurn == 3 && !isStand3 && !isBust3)
            {
                Debug.Log("P3のターン！");
                return;
            }

            currentTurn++;
        }

        // 全員終了
        Debug.Log("全員終了！ディーラーのターン！");
        DealerTurn();
    }
}
//ここから下 変更用
//a
//i
//u