
using UnityEngine;
using UnityEngine.InputSystem;

public class HighLowCursorManager : MonoBehaviour
{
    [Header("プレイヤーごとのカーソルPrefab")]
    [SerializeField] private GameObject player1CursorPrefab;
    [SerializeField] private GameObject player2CursorPrefab;

    [Header("カーソルの開始位置")]
    [SerializeField] private Vector2 startPosition = Vector2.zero;

    [Header("カーソル用Canvas")]
    [SerializeField] private Canvas cursorCanvas;

    private GameObject[] playerCursors = new GameObject[2];

    private int currentTurn = 1;

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

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
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
        for (int i = 0; i < playerCursors.Length; i++)
        {
            if (playerCursors[i] != null)
            {
                Destroy(playerCursors[i]);
                playerCursors[i] = null;
            }
        }

        GameObject[] prefabs =
        {
            player1CursorPrefab,
            player2CursorPrefab
        };

        int gamepadIndex = 0;

        for (int i = 0; i < 2; i++)
        {
            if (prefabs[i] == null)
            {
                Debug.LogWarning($"P{i + 1}のカーソルPrefabが設定されていません。");
                continue;
            }

            // P2には2台目のゲームパッドが必要
            if (i == 1 && Gamepad.all.Count < 2)
            {
                Debug.LogWarning("P2用のゲームパッドが接続されていません。");
                continue;
            }

            GameObject cursor = Instantiate(
                prefabs[i],
                cursorCanvas.transform
            );

            cursor.name = $"Cursor{i + 1}P";

            RectTransform rect = cursor.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(
                    startPosition.x + i * 60f,
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
                controller.SetGamepadIndex(gamepadIndex);
            }

            playerCursors[i] = cursor;
            gamepadIndex++;
        }

        ApplyTurnVisibility();

        Debug.Log($"生成したカーソル数：{gamepadIndex}");
    }

    // ターン変更時に呼び出す
    public void SetCurrentTurn(int playerNumber)
    {
        if (playerNumber < 1 || playerNumber > 2)
            return;

        currentTurn = playerNumber;

        ApplyTurnVisibility();

        Debug.Log($"現在のターン：{currentTurn}P");
    }

    // 自分のターンのカーソルだけ表示
    private void ApplyTurnVisibility()
    {
        for (int i = 0; i < 2; i++)
        {
            if (playerCursors[i] != null)
            {
                playerCursors[i].SetActive(i + 1 == currentTurn);
            }
        }
    }
}
