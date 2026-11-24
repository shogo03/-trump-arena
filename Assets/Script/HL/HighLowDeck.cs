using System.Collections.Generic;
using UnityEngine;

public class HighLowDeck : MonoBehaviour
{
    [System.Serializable]
    public class CardData
    {
        public int number;
        public string suit;
        public Sprite image;

        public CardData(int number, string suit, Sprite image)
        {
            this.number = number;
            this.suit = suit;
            this.image = image;
        }
    }

    [Header("カード画像（52枚）")]
    [SerializeField] private Sprite[] cardImages = new Sprite[52];

    private List<CardData> deck = new List<CardData>();

    public void CreateDeck()
    {
        deck.Clear();

        string[] suits = { "Spade", "Heart", "Diamond", "Club" };

        for (int s = 0; s < suits.Length; s++)
        {
            for (int n = 1; n <= 13; n++)
            {
                int index = s * 13 + (n - 1);

                deck.Add(new CardData(
                    n,
                    suits[s],
                    cardImages[index]
                ));
            }
        }

        Shuffle();

        Debug.Log("52枚のデッキを作成しました");
    }

    public void Shuffle()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            CardData temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public CardData DrawCard()
    {
        if (deck.Count == 0)
        {
            Debug.Log("カードがありません");
            return null;
        }

        CardData card = deck[0];
        deck.RemoveAt(0);

        return card;
    }

    public int RemainingCards => deck.Count;
}