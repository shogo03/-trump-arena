
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameButtonEffect :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    private static GameButtonEffect selectedButton;

    [Header("Settings")]
    [SerializeField] private Image buttonImage;
    [SerializeField]
    private Color normalColor =
        Color.white;

    [SerializeField]
    private Color selectedColor =
        new Color(1f, 0.82f, 0.42f);

    [SerializeField]
    private Color glowColor =
        new Color(1f, 0.72f, 0.2f);

    [SerializeField] private float hoverScale = 1.04f;
    [SerializeField] private float selectedScale = 1.05f;

    private RectTransform rectTransform;
    private Outline outline;

    private bool isHovered;
    private bool isSelected;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        outline = GetComponent<Outline>();

        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.effectColor = glowColor;
        outline.effectDistance = new Vector2(3, 3);
        outline.enabled = false;

        if (buttonImage != null)
            buttonImage.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateEffect();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateEffect();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 前に選択されていたボタンを解除
        if (selectedButton != null)
        {
            selectedButton.isSelected = false;
            selectedButton.UpdateEffect();
        }

        // 今のボタンを選択
        selectedButton = this;
        isSelected = true;

        UpdateEffect();
    }

    private void UpdateEffect()
    {
        if (isSelected)
        {
            rectTransform.localScale =
                Vector3.one * selectedScale;

            if (buttonImage != null)
                buttonImage.color = selectedColor;

            outline.enabled = true;
        }
        else if (isHovered)
        {
            rectTransform.localScale =
                Vector3.one * hoverScale;

            if (buttonImage != null)
                buttonImage.color = normalColor;

            outline.enabled = true;
        }
        else
        {
            rectTransform.localScale = Vector3.one;

            if (buttonImage != null)
                buttonImage.color = normalColor;

            outline.enabled = false;
        }
    }
}
