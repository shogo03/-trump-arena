using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    public int gamepadIndex;
    public float moveSpeed = 800f;

    private RectTransform rectTransform;
    private RectTransform canvasRect;

    public int GamepadIndex => gamepadIndex;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
            canvasRect = canvas.transform as RectTransform;
    }

    public void SetGamepadIndex(int index)
    {
        gamepadIndex = index;
    }

    private void Update()
    {
        if (gamepadIndex < 0 || gamepadIndex >= Gamepad.all.Count)
            return;

        if (rectTransform == null || canvasRect == null)
            return;

        Vector2 stick =
            Gamepad.all[gamepadIndex].leftStick.ReadValue();

        rectTransform.anchoredPosition +=
            stick * moveSpeed * Time.unscaledDeltaTime;

        // Canvas の範囲内に制限
        Vector2 pos = rectTransform.anchoredPosition;
        Rect rect = canvasRect.rect;

        pos.x = Mathf.Clamp(
            pos.x,
            rect.xMin,
            rect.xMax
        );

        pos.y = Mathf.Clamp(
            pos.y,
            rect.yMin,
            rect.yMax
        );

        rectTransform.anchoredPosition = pos;
    }
}