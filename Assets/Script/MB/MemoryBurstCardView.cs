
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MemoryBurstCardView : MonoBehaviour, IPointerClickHandler
{
    [Header("カード画像")]
    [SerializeField] private Image cardImage;

    private Card cardData;
    private Sprite backImage;
    private Action<MemoryBurstCardView> onClicked;

    private bool isRevealed;
    private bool isMatched;

    public int Number => cardData != null ? cardData.number : -1;
    public bool IsRevealed => isRevealed;
    public bool IsMatched => isMatched;

    public void Setup(
        Card card,
        Sprite back,
        Action<MemoryBurstCardView> clickCallback)
    {
        cardData = card;
        backImage = back;
        onClicked = clickCallback;

        isRevealed = false;
        isMatched = false;

        if (cardImage == null || cardData == null || backImage == null)
        {
            Debug.LogError("カードの設定を確認してください。", this);
            return;
        }

        Color color = cardImage.color;
        color.a = 1f;
        cardImage.color = color;

        cardImage.sprite = backImage;
        cardImage.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isRevealed || isMatched)
            return;

        onClicked?.Invoke(this);
    }

    public void Reveal()
    {
        if (isRevealed || isMatched || cardData == null)
            return;

        cardImage.sprite = cardData.image;
        isRevealed = true;
    }

    public void FlipBack()
    {
        if (isMatched)
            return;

        cardImage.sprite = backImage;
        isRevealed = false;
    }

    public void SetMatched()
    {
        isMatched = true;
        isRevealed = true;

        Color color = cardImage.color;
        color.a = 0f;
        cardImage.color = color;

        cardImage.raycastTarget = false;
    }
}
