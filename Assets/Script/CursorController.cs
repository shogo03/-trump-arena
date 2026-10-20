
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private int gamepadIndex;

    public void SetGamepadIndex(int index)
    {
        gamepadIndex = index;
    }

    private void Update()
    {
        if (gamepadIndex >= Gamepad.all.Count)
            return;

        Gamepad pad = Gamepad.all[gamepadIndex];

        // 左スティックでカーソルを移動
        Vector2 stick = pad.leftStick.ReadValue();

        transform.position += new Vector3(
            stick.x,
            stick.y,
            0f
        ) * moveSpeed * Time.deltaTime;

        // Aボタンで決定
        if (pad.buttonSouth.wasPressedThisFrame)
        {
            Debug.Log((gamepadIndex + 1) + "P：決定！");
        }
    }
}
