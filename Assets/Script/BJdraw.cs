
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

        Card card = deck.DrawCard();

        GameObject obj = Instantiate(cardPrefab, handArea);

        Image image = obj.GetComponent<Image>();
        image.sprite = card.image;
        image.enabled = true;

        hand.Add(card);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2((hand.Count - 1) * 100, 0);
        rect.sizeDelta = new Vector2(100, 145);

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
