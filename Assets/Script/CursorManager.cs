
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    [Header("プレイヤーごとのカーソルPrefab")]
    [SerializeField] private GameObject[] cursorPrefabs = new GameObject[4];

    [Header("カーソルの開始位置")]
    [SerializeField] private Vector2 startPosition = Vector2.zero;

    [Header("カーソル用Canvas")]
    [SerializeField] private Canvas cursorCanvas;

    private readonly List<GameObject> cursors = new();

    private void Start()
    {
        UpdateCursors();
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(
        InputDevice device,
        InputDeviceChange change)
    {
        if (device is Gamepad &&
            (change == InputDeviceChange.Added ||
             change == InputDeviceChange.Removed ||
             change == InputDeviceChange.Disconnected ||
             change == InputDeviceChange.Reconnected))
        {
            UpdateCursors();
        }
    }

    private void UpdateCursors()
    {
        foreach (GameObject cursor in cursors)
        {
            if (cursor != null)
                Destroy(cursor);
        }

        cursors.Clear();

        int count = Mathf.Min(Gamepad.all.Count, cursorPrefabs.Length);

        for (int i = 0; i < count; i++)
        {
            if (cursorPrefabs[i] == null)
                continue;

            Vector3 position = new Vector3(
                startPosition.x + i * 1.5f,
                startPosition.y,
                0f
            );

            GameObject cursor = Instantiate(
    cursorPrefabs[i],
    cursorCanvas.transform
);

            cursor.name = "Cursor" + (i + 1) + "P";

            RectTransform rect = cursor.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.anchoredPosition = position;
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
            }

            cursor.name = "Cursor" + (i + 1) + "P";

            // UIより前面に表示するため、Sprite Rendererの描画順を設定
            SpriteRenderer[] renderers =
                cursor.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (SpriteRenderer sr in renderers)
            {
                sr.sortingOrder = 100;
            }

            CursorController controller =
                cursor.GetComponent<CursorController>();

            if (controller != null)
                controller.SetGamepadIndex(i);

            cursors.Add(cursor);
        }

        Debug.Log("接続コントローラー数：" + count);
    }
}
