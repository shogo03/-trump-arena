
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;

    private int gamepadIndex;

    public int GamepadIndex => gamepadIndex;

    public void SetGamepadIndex(int index)
    {
        gamepadIndex = index;
    }

    private void Update()
    {
        if (gamepadIndex >= Gamepad.all.Count)
            return;

        Gamepad pad = Gamepad.all[gamepadIndex];

        Vector2 stick = pad.leftStick.ReadValue();

        transform.position += new Vector3(
            stick.x,
            stick.y,
            0f
        ) * moveSpeed * Time.deltaTime;
    }
}
