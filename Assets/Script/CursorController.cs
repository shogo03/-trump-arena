
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    [Header("カーソル設定")]
    [SerializeField] private float moveSpeed = 800f;

    private int gamepadIndex;

    public int GamepadIndex => gamepadIndex;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetGamepadIndex(int index)
    {
        gamepadIndex = index;
    }

    private void Update()
    {
        if (gamepadIndex < 0 || gamepadIndex >= Gamepad.all.Count)
            return;

        Gamepad pad = Gamepad.all[gamepadIndex];

        Vector2 stick = pad.leftStick.ReadValue();

        Vector3 movement = new Vector3(
            stick.x,
            stick.y,
            0f
        ) * moveSpeed * Time.unscaledDeltaTime;

        transform.position += movement;

        // カーソルが画面外に出ないように制限
        Vector3 position = transform.position;

        float halfWidth = 0f;
        float halfHeight = 0f;

        if (rectTransform != null)
        {
            halfWidth = rectTransform.rect.width
                * rectTransform.lossyScale.x / 2f;

            halfHeight = rectTransform.rect.height
                * rectTransform.lossyScale.y / 2f;
        }

        position.x = Mathf.Clamp(
            position.x,
            halfWidth,
            Screen.width - halfWidth
        );

        position.y = Mathf.Clamp(
            position.y,
            halfHeight,
            Screen.height - halfHeight
        );

        position.z = transform.position.z;
        transform.position = position;
    }
}
