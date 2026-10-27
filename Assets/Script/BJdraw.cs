
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

    private List<Card> hand = new List<Card>();
    private List<Card> hand2 = new List<Card>();
    private List<Card> hand3 = new List<Card>();

    public void DrawCard()
    {
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
        rect.sizeDelta = new Vector2(45, 65);

        // カードを横に並べる
        rect.anchoredPosition = new Vector2(
            (hand.Count - 1) * 35,
            0
        );
        UpdatePoint();
    }

    public void DrawCard2()
    {
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
        rect.sizeDelta = new Vector2(45, 65);
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
        Debug.Log("STAND：カードを引くのをやめました");
    }
}
