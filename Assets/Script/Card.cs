
public enum Suit
{
    Spade,   // スペード
    Heart,   // ハート
    Diamond, // ダイヤ
    Club     // クラブ
}

public class Card
{
    public Suit suit;
    public int number;

    public Card(Suit suit, int number)
    {
        this.suit = suit;
        this.number = number;
    }

    public int GetPoint()
    {
        if (number == 1)
        {
            return 11;
        }

        if (number >= 11)
        {
            return 10;
        }

        return number;
    }
}
