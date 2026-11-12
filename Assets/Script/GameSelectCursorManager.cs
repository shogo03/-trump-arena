
using UnityEngine;

public class GameSelectCursorManager : MonoBehaviour
{
    [Header("1PカーソルPrefab")]
    [SerializeField] private GameObject cursorPrefab;

    [Header("カーソルCanvas")]
    [SerializeField] private Canvas cursorCanvas;

    [Header("開始位置")]
    [SerializeField] private Vector2 startPosition;

    private GameObject cursor;

    private void Start()
    {
        ResetCursor();
    }

    // カーソルをリセットして生成
    private void ResetCursor()
    {
        // 既存カーソルを削除
        if (cursor != null)
        {
            Destroy(cursor);
            cursor = null;
        }

        if (cursorPrefab == null || cursorCanvas == null)
        {
            Debug.LogError("カーソルPrefabまたはCanvasが未設定です");
            return;
        }

        // 1Pカーソルを1個だけ生成
        cursor = Instantiate(
            cursorPrefab,
            cursorCanvas.transform
        );

        cursor.name = "GameSelectCursor1P";

        RectTransform rect = cursor.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchoredPosition = startPosition;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        // 1Pのゲームパッドを割り当て
        CursorController controller =
            cursor.GetComponent<CursorController>();

        if (controller != null)
        {
            controller.SetGamepadIndex(0);
        }

        Debug.Log("ゲーム選択画面：1Pカーソルを生成しました");
    }
}
