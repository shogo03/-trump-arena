
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MBCursorManager : MonoBehaviour
{
    [Header("プレイヤーごとのカーソルPrefab（最大4人）")]
    [SerializeField] private GameObject[] cursorPrefabs = new GameObject[4];

    [Header("カーソルの開始位置")]
    [SerializeField] private Vector2 startPosition = Vector2.zero;

    [Header("カーソル用Canvas")]
    [SerializeField] private Canvas cursorCanvas;

    private readonly List<GameObject> cursors = new();

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void Start()
    {
        UpdateCursors();
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
        if (cursorCanvas == null)
        {
            Debug.LogError("Cursor Canvasを設定してください。", this);
            return;
        }

        // 既存カーソルを削除
        foreach (GameObject cursor in cursors)
        {
            if (cursor != null)
                Destroy(cursor);
        }

        cursors.Clear();

        int gamepadIndex = 0;

        // 最大4人
        for (int i = 0; i < 4; i++)
        {
            // 配列の範囲を確認
            if (GameSettings.IsActive == null ||
                i >= GameSettings.IsActive.Length)
                continue;

            if (GameSettings.IsCPU == null ||
                i >= GameSettings.IsCPU.Length)
                continue;

            if (cursorPrefabs == null ||
                i >= cursorPrefabs.Length)
            {
                Debug.LogWarning($"P{i + 1}のカーソルPrefabがありません。");
                continue;
            }

            // 1Pは必ず表示
            if (i != 0 && !GameSettings.IsActive[i])
                continue;

            // CPUは表示しない
            if (GameSettings.IsCPU[i])
                continue;

            if (cursorPrefabs[i] == null)
                continue;

            // 1P以外はゲームパッドが必要
            if (i != 0 && gamepadIndex >= Gamepad.all.Count)
                break;

            GameObject cursor = Instantiate(
                cursorPrefabs[i],
                cursorCanvas.transform
            );

            cursor.name = $"Cursor{i + 1}P";

            RectTransform rect = cursor.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(
                    startPosition.x + gamepadIndex * 60f,
                    startPosition.y
                );

                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
            }

            foreach (SpriteRenderer sr in
                cursor.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sr.sortingOrder = 100;
            }

            CursorController controller =
                cursor.GetComponent<CursorController>();

            if (controller != null)
            {
                controller.SetGamepadIndex(
                    gamepadIndex < Gamepad.all.Count
                        ? gamepadIndex
                        : -1
                );
            }

            cursors.Add(cursor);

            if (gamepadIndex < Gamepad.all.Count)
                gamepadIndex++;
        }

        Debug.Log($"生成したカーソル数：{cursors.Count}");
    }
}
