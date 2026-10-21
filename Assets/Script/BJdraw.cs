
using UnityEngine;

public class BJdraw : MonoBehaviour
{
    public Deck deck;
    public CardView cardView;


    public void DrawCard()
    {
        Debug.Log("ƒ{ƒ^ƒ“‚ª‰Ÿ‚³‚ê‚Ü‚µ‚½I");

        if (deck == null)
        {
            Debug.LogError("Deck‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
            return;
        }

        if (cardView == null)
        {
            Debug.LogError("Card View‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
            return;
        }

        Card card = deck.DrawCard();

        cardView.ShowCard(card);
    }

}
