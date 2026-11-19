
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    public Image cardImage;


    public void ShowCard(Card card)
    {
        if (card == null)
        {
            Debug.LogError("カードがありません");
            return;
        }

        if (cardImage == null)
        {
            Debug.LogError("Card Imageが設定されていません");
            return;
        }

        cardImage.sprite = card.image;
        cardImage.enabled = true;

        Debug.Log("表示するカード：" + card.suit + card.number);
    }

}
