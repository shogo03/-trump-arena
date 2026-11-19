using UnityEngine;

public class HighLowCard : MonoBehaviour
{
    public int cardNumber;
    public string cardSuit;
    public Sprite cardImage;

    public void SetCard(int number, string suit, Sprite image)
    {
        cardNumber = number;
        cardSuit = suit;
        cardImage = image;
    }
}