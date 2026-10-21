
using UnityEngine;
using UnityEngine.UI;

public class CardBurstCardView : MonoBehaviour
{
    [Header("カード画像")]
    [SerializeField] private Image cardImage;

    [Header("カードの裏面画像")]
    [SerializeField] private Sprite backImage;

    private Card cardData;
    private bool isRevealed;

    // カードを設定して裏向きにする
    public void Setup(Card card)
    {
        cardData = card;
        isRevealed = false;

        if (cardImage == null)
        {
            Debug.LogError("Card Imageが設定されていません", this);
            return;
        }

        if (backImage == null)
        {
            Debug.LogError("カードの裏面画像が設定されていません", this);
            return;
        }

        cardImage.sprite = backImage;
        cardImage.enabled = true;
    }

    // カードを表向きにする
    public void Reveal()
    {
        if (isRevealed)
            return;

        if (cardData == null)
        {
            Debug.LogError("カードデータが設定されていません", this);
            return;
        }

        if (cardImage == null)
            return;

        cardImage.sprite = cardData.image;
        isRevealed = true;

        Debug.Log($"カードをめくった：{cardData.suit} {cardData.number}");
    }

    public bool IsRevealed()
    {
        return isRevealed;
    }
}
