using UnityEngine;

public enum Suit
{
    Spade,
    Heart,
    Diamond,
    Club
}

public class Card
{
    public Suit suit;
    public int number;
    public Sprite image;

    public Card(Suit suit, int number, Sprite image)
    {
        this.suit = suit;
        this.number = number;
        this.image = image;
    }

    public int GetPoint()
    {
        if (number == 1)
            return 11;

        if (number >= 11)
            return 10;

        return number;
    }
}