
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Card> cards = new List<Card>();

    [Header("カード画像（♠A、♠2、...、♣Kの順）")]
    public Sprite[] cardImages = new Sprite[52];
    public Sprite cardBackImage;

    void Start()
    {
        CreateDeck();
        ShuffleDeck();

        Debug.Log("山札の枚数：" + cards.Count);
    }

    // 52枚のカードを作成する
    void CreateDeck()
    {
        cards.Clear();

        Suit[] suits =
        {
            Suit.Spade,
            Suit.Heart,
            Suit.Diamond,
            Suit.Club
        };

        for (int s = 0; s < suits.Length; s++)
        {
            for (int number = 1; number <= 13; number++)
            {
                int imageIndex = s * 13 + number - 1;

                cards.Add(
                    new Card(suits[s], number, cardImages[imageIndex])
                );
            }
        }
    }

    // 山札をシャッフルする
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

    // 山札から1枚引く
    public Card DrawCard()
    {
        if (cards.Count == 0)
        {
            Debug.Log("新しい山札を作ります");

            CreateDeck();
            ShuffleDeck();
        }

        Card card = cards[0];
        cards.RemoveAt(0);

        return card;
    }
}
