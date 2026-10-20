
using UnityEngine;
using UnityEngine.InputSystem;

public class XboxControllerTest : MonoBehaviour
{
    [Header("プレイヤー番号（0から開始）")]
    public int playerIndex = 0;

    [Header("カーソル設定")]
    public float moveSpeed = 10f;
    public Vector2 minPosition = new Vector2(-8f, -4f);
    public Vector2 maxPosition = new Vector2(8f, 4f);

    private void Update()
    {
        // 接続されているコントローラーを取得
        if (Gamepad.all.Count <= playerIndex)
        {
            return;
        }

        Gamepad pad = Gamepad.all[playerIndex];

        // 左スティックでカーソルを動かす
        Vector2 stick = pad.leftStick.ReadValue();

        transform.position += new Vector3(
            stick.x,
            stick.y,
            0f
        ) * moveSpeed * Time.deltaTime;

        // 移動範囲を制限
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);
        transform.position = pos;

        // Aボタンで決定
        if (pad.buttonSouth.wasPressedThisFrame)
        {
            Debug.Log((playerIndex + 1) + "P：決定！");
        }
    }
}
