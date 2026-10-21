using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Card> cards = new List<Card>();

    void Start()
    {
        CreateDeck();
        ShuffleDeck();
    }

    void CreateDeck()
    {
        cards.Clear();

        for (int suit = 0; suit < 4; suit++)
        {
            for (int number = 1; number <= 13; number++)
            {
                cards.Add(new Card((Suit)suit, number));
            }
        }
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int randomIndex = Random.Range(i, cards.Count);

            Card temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    public Card DrawCard()
    {
        if (cards.Count == 0)
        {
            CreateDeck();
            ShuffleDeck();
        }

        Card card = cards[0];
        cards.RemoveAt(0);

        return card;
    }
    
}