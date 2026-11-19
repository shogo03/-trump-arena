using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BJCursorManager : MonoBehaviour
{
    [Header("プレイヤーごとのカーソルPrefab（最大3人）")]
    [SerializeField] private GameObject[] cursorPrefabs = new GameObject[3];

    [Header("カーソルの開始位置")]
    [SerializeField] private Vector2 startPosition = Vector2.zero;

    [Header("カーソル用Canvas")]
    [SerializeField] private Canvas cursorCanvas;

    // 実際に生成したカーソル一覧
    private readonly List<GameObject> cursors = new();

    // P1～P3それぞれのカーソル
    private GameObject[] playerCursors = new GameObject[3];


    // 有効になったとき

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }


    // 無効になったとき

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }


    // ゲーム開始

    private void Start()
    {
        UpdateCursors();
    }


    // ゲームパッド接続・切断を検知

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


    // カーソルを生成・更新

    public void UpdateCursors()
    {
        if (cursorCanvas == null)
        {
            Debug.LogError("Cursor Canvasを設定してください。", this);
            return;
        }


        // 以前のカーソルを削除

        foreach (GameObject cursor in cursors)
        {
            if (cursor != null)
                Destroy(cursor);
        }

        cursors.Clear();


        // P1～P3のカーソル情報をリセット
        for (int i = 0; i < playerCursors.Length; i++)
        {
            playerCursors[i] = null;
        }


        int gamepadIndex = 0;


        // 最大3人分のカーソルを生成

        for (int i = 0; i < 3; i++)
        {
            // P1以外で参加していないプレイヤーはスキップ
            if (i != 0 && !GameSettings.IsActive[i])
                continue;


            // CPUはカーソルを表示しない
            if (GameSettings.IsCPU[i])
                continue;


            // カーソルPrefabが設定されていない
            if (cursorPrefabs[i] == null)
                continue;


            // P1以外はゲームパッドが必要
            if (i != 0 && gamepadIndex >= Gamepad.all.Count)
                break;

            // カーソル生成

            GameObject cursor = Instantiate(
                cursorPrefabs[i],
                cursorCanvas.transform
            );


            cursor.name = $"Cursor{i + 1}P";


            // カーソル位置設定

            RectTransform rect =
                cursor.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(
                    startPosition.x + gamepadIndex * 60f,
                    startPosition.y
                );

                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
            }


            // カーソルを最前面に表示

            foreach (SpriteRenderer sr in
                cursor.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sr.sortingOrder = 100;
            }


            // CursorControllerにゲームパッド番号を設定

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


            // P1/P2/P3のカーソルとして保存

            playerCursors[i] = cursor;

            cursors.Add(cursor);


            // ゲームパッド番号を進める
            if (gamepadIndex < Gamepad.all.Count)
                gamepadIndex++;
        }


        Debug.Log($"生成したカーソル数：{cursors.Count}");


        // 最初はP1のカーソルだけ表示

        SetTurnCursor(1);
    }


    // 現在のターンのカーソルだけ表示

    public void SetTurnCursor(int turn)
    {
        for (int i = 0; i < 3; i++)
        {
            // カーソルが存在しない場合
            if (playerCursors[i] == null)
                continue;


            // turnと一致したプレイヤーだけ表示
            bool show = (i + 1 == turn);


            playerCursors[i].SetActive(show);
        }


        if (turn == 0)
        {
            Debug.Log("全プレイヤーのカーソルを非表示");
        }
        else
        {
            Debug.Log("表示カーソル：P" + turn);
        }
    }
}