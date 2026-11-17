
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BJCardManager : MonoBehaviour
{
    public List<Card> hand = new List<Card>();
    public List<Card> hand2 = new List<Card>();
    public List<Card> hand3 = new List<Card>();




    // カードの生成と表示を担当する
    public void CreateCard(Card card, Transform handArea, GameObject cardPrefab, Vector2 cardSize, int cardCount, Sprite displayImage)
    {
        GameObject obj = Instantiate(cardPrefab, handArea);

        Image image = obj.GetComponent<Image>();

        if (image == null)
        {
            Debug.LogError("Card PrefabにImageがない！");
            return;
        }

        image.sprite = displayImage;
        image.enabled = true;

        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.sizeDelta = cardSize;

        rect.anchoredPosition = new Vector2(
            (cardCount - 1) * 35,
            0
        );
    }

    // 手札を表向きにする
    public void RevealCards(List<Card> playerHand, Transform area)
    {
        if (playerHand == null || area == null)
            return;

        for (int i = 0; i < playerHand.Count; i++)
        {
            Image image = area.GetChild(i).GetComponent<Image>();

            if (image != null)
            {
                image.sprite = playerHand[i].image;
            }
        }
    }

    // 手札の合計点数を計算する
    public int CalculatePoint(List<Card> playerHand)
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

        // 21を超えたらAを11から1に変更
        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }

    public void ClearCards(Transform area)
    {
        if (area == null) return;

        foreach (Transform child in area)
        {
            Destroy(child.gameObject);
        }
    }

    public void DrawAndDisplayCard(Deck deck, List<Card> playerHand, Transform area, GameObject cardPrefab, Vector2 cardSize, bool isInitialDeal)
    {
        if (deck == null || playerHand == null ||
            area == null || cardPrefab == null)
        {
            Debug.LogError("カード配布に必要な設定が不足しています！");
            return;
        }

        Card card = deck.DrawCard();

        playerHand.Add(card);

        Sprite displayImage;

        if (isInitialDeal)
        {
            displayImage = deck.cardBackImage;
        }
        else
        {
            displayImage = card.image;
        }

        CreateCard(
            card,
            area,
            cardPrefab,
            cardSize,
            playerHand.Count,
            displayImage
        );
    }
}
