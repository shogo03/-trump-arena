
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BJdraw : MonoBehaviour
{
    public Deck deck;
    public GameObject cardPrefab;
    public Transform handArea;
    public Text pointText;

    private List<Card> hand = new List<Card>();

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

        // カードのサイズ
        rect.sizeDelta = new Vector2(60, 87);

        // カードを横に並べる
        rect.anchoredPosition = new Vector2(
            (hand.Count - 1) * 65,
            -100
        );
        UpdatePoint();
    }

    void UpdatePoint()
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in hand)
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
            pointText.text = "バースト！ 合計：" + total;
            isStand = true;
        }
        else
        {
            pointText.text = "合計：" + total;
        }




    }

    private bool isStand = false;

    public void Stand()
    {
        isStand = true;
        Debug.Log("STAND：カードを引くのをやめました");
    }
}
