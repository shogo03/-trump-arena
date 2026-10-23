
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MemoryBurstCardView : MonoBehaviour, IPointerClickHandler
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
        cardImage.raycastTarget = true;
    }

    // マウスでクリックされたとき
    public void OnPointerClick(PointerEventData eventData)
    {
        Reveal();
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
        {
            Debug.LogError("Card Imageが設定されていません", this);
            return;
        }

        Sprite sprite = cardData.image;

        if (sprite == null)
        {
            Debug.LogError(
                $"カード画像が設定されていません：{cardData.suit} {cardData.number}",
                this
            );
            return;
        }

        cardImage.sprite = sprite;
        cardImage.enabled = true;
        isRevealed = true;

        Debug.Log($"画像を設定しました：{sprite.name}", this);
    }


    public bool IsRevealed()
    {
        return isRevealed;
    }
}
