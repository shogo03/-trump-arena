
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MemoryBurstCardView : MonoBehaviour, IPointerClickHandler
{
    [Header("カード画像")]
    [SerializeField] private Image cardImage;

    [Header("公開時の発光")]
    [SerializeField] private Color glowColor = new Color(1f, 0.8f, 0.15f, 1f);
    [SerializeField] private float glowDuration = 2f;
    [SerializeField] private float glowSpeed = 5f;

    private Card cardData;
    private Sprite backImage;
    private Action<MemoryBurstCardView> onClicked;

    private bool isRevealed;
    private bool isMatched;

    private Outline glowOutline;
    private Coroutine glowCoroutine;

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

        SetupGlow();
    }

    // =================================
    // 発光設定
    // =================================

    private void SetupGlow()
    {
        if (cardImage == null)
            return;

        glowOutline = cardImage.GetComponent<Outline>();

        if (glowOutline == null)
        {
            glowOutline = cardImage.gameObject.AddComponent<Outline>();
        }

        glowOutline.effectColor = glowColor;
        glowOutline.effectDistance = new Vector2(5f, 5f);
        glowOutline.useGraphicAlpha = true;
        glowOutline.enabled = false;
    }

    // =================================
    // 発光演出
    // =================================

    public void PlayGlow()
    {
        if (glowOutline == null)
            SetupGlow();

        if (glowOutline == null)
            return;

        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
        }

        glowCoroutine = StartCoroutine(GlowRoutine());
    }

    private IEnumerator GlowRoutine()
    {
        glowOutline.enabled = true;

        float elapsed = 0f;

        while (elapsed < glowDuration)
        {
            elapsed += Time.deltaTime;

            float alpha =
                0.3f + Mathf.PingPong(elapsed * glowSpeed, 0.7f);

            Color color = glowColor;
            color.a = alpha;

            glowOutline.effectColor = color;

            yield return null;
        }

        glowOutline.enabled = false;
        glowCoroutine = null;
    }

    // =================================
    // クリック
    // =================================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isRevealed || isMatched)
            return;

        onClicked?.Invoke(this);
    }

    // =================================
    // 表向き
    // =================================

    public void Reveal()
    {
        if (isRevealed || isMatched || cardData == null)
            return;

        cardImage.sprite = cardData.image;
        isRevealed = true;
    }

    // =================================
    // 裏向き
    // =================================

    public void FlipBack()
    {
        if (isMatched)
            return;

        StopGlow();

        cardImage.sprite = backImage;
        isRevealed = false;
    }

    // =================================
    // ペア成立
    // =================================

    public void SetMatched()
    {
        isMatched = true;
        isRevealed = true;

        StopGlow();

        Color color = cardImage.color;
        color.a = 0f;
        cardImage.color = color;

        cardImage.raycastTarget = false;
    }

    // =================================
    // 発光停止
    // =================================

    public void StopGlow()
    {
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = null;
        }

        if (glowOutline != null)
        {
            glowOutline.enabled = false;
        }
    }

    private void OnDestroy()
    {
        StopGlow();
    }
}
