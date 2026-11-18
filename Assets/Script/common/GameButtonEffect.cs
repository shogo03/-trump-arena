
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameButtonEffect :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Settings")]
    [SerializeField] private Image buttonImage;

    [SerializeField]
    private Color normalColor = Color.white;

    [SerializeField]
    private Color hoverColor = new Color(1f, 0.82f, 0.42f);

    [SerializeField]
    private Color glowColor = new Color(1f, 0.72f, 0.2f);

    [SerializeField] private float hoverScale = 1.04f;

    private RectTransform rectTransform;
    private Outline outline;

    private bool isHovered;

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

        ResetEffect();
    }

    // オブジェクトが有効になったとき
    private void OnEnable()
    {
        ResetEffect();
    }

    // オブジェクトが無効になるとき
    private void OnDisable()
    {
        ResetEffect();
    }

    // カーソルがボタンに乗ったとき
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateEffect();
    }

    // カーソルがボタンから離れたとき
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateEffect();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    }

    // 初期状態に戻す
    private void ResetEffect()
    {
        isHovered = false;

        if (rectTransform != null)
            rectTransform.localScale = Vector3.one;

        if (buttonImage != null)
            buttonImage.color = normalColor;

        if (outline != null)
            outline.enabled = false;
    }

    // エフェクト更新
    private void UpdateEffect()
    {
        if (isHovered)
        {
            rectTransform.localScale =
                Vector3.one * hoverScale;

            if (buttonImage != null)
                buttonImage.color = hoverColor;

            outline.enabled = true;
        }
        else
        {
            ResetEffect();
        }
    }
}
