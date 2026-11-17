
using System;
using TMPro;
using UnityEngine;

public class MemoryBurstTurnManager : MonoBehaviour
{
    [Header("ターン表示")]
    [SerializeField] private TextMeshProUGUI turnText;

    [Header("タイマー表示")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("制限時間")]
    [SerializeField] private float turnTimeLimit = 15f;

    [Header("プレイヤーごとの文字色")]
    [SerializeField] private Color[] playerColors = new Color[4];

    [Header("カーソル管理")]
    [SerializeField] private MBCursorManager cursorManager;

    private int currentPlayer = 1;
    private float remainingTurnTime;
    private bool isGameOver;
    private bool skipNextTurn;

    public int CurrentPlayer => currentPlayer;
    public float RemainingTurnTime => remainingTurnTime;

    // 現在のプレイヤーがCPUかどうか
    public bool IsCurrentPlayerCPU
    {
        get
        {
            int index = currentPlayer - 1;

            return index >= 0 &&
                   index < 4 &&
                   GameSettings.IsCPU != null &&
                   index < GameSettings.IsCPU.Length &&
                   GameSettings.IsCPU[index];
        }
    }

    public event Action<int> OnTurnChanged;
    public event Action OnTurnTimeout;

    // =================================
    // 初期化
    // =================================

    public void Initialize(int count)
    {
        currentPlayer = 1;
        remainingTurnTime = turnTimeLimit;
        isGameOver = false;
        skipNextTurn = false;

        UpdateUI();

        // 最初のターンのカーソルを表示
        if (cursorManager != null)
        {
            cursorManager.SetCurrentTurn(currentPlayer);
        }

        Debug.Log($"参加人数：{count}");
        Debug.Log($"最初のターン：プレイヤー{currentPlayer}");
    }

    // =================================
    // タイマー更新
    // =================================

    public void Tick(bool paused)
    {
        if (isGameOver || paused)
            return;

        remainingTurnTime -= Time.deltaTime;

        if (remainingTurnTime <= 0f)
        {
            remainingTurnTime = 0f;
            UpdateUI();

            OnTurnTimeout?.Invoke();
            return;
        }

        UpdateUI();
    }

    // =================================
    // タイマーリセット
    // =================================

    public void ResetTimer()
    {
        if (isGameOver)
            return;

        remainingTurnTime = turnTimeLimit;
        UpdateUI();

        Debug.Log($"プレイヤー{currentPlayer}のタイマーをリセット！");
    }

    // =================================
    // 次のターンへ
    // =================================

    public void NextTurn()
    {
        if (isGameOver)
            return;

        // 次の参加プレイヤーを探す
        for (int i = 0; i < 4; i++)
        {
            currentPlayer++;

            if (currentPlayer > 4)
                currentPlayer = 1;

            int index = currentPlayer - 1;

            // 配列の範囲確認
            if (GameSettings.IsActive == null ||
                index >= GameSettings.IsActive.Length)
                continue;

            // 参加していないプレイヤーは飛ばす
            if (!GameSettings.IsActive[index])
                continue;

            // スキップ予約があれば、このプレイヤーを飛ばす
            if (skipNextTurn)
            {
                Debug.Log($"プレイヤー{currentPlayer}のターンをスキップ！");
                skipNextTurn = false;
                continue;
            }

            // タイマーリセット
            remainingTurnTime = turnTimeLimit;

            UpdateUI();

            // カーソルを現在のプレイヤーに切り替える
            if (cursorManager != null)
            {
                cursorManager.SetCurrentTurn(currentPlayer);
            }

            // ターン変更イベント
            OnTurnChanged?.Invoke(currentPlayer);

            Debug.Log($"次のプレイヤー：P{currentPlayer}");

            return;
        }

        // 全員がスキップ対象になった場合
        skipNextTurn = false;

        Debug.LogError("次の参加プレイヤーが見つかりません！");
    }

    // =================================
    // スキップ予約
    // =================================

    public void RequestSkipNextTurn()
    {
        if (isGameOver)
            return;

        skipNextTurn = true;

        Debug.Log("次のプレイヤーのスキップを予約しました");
    }

    // =================================
    // ゲーム終了
    // =================================

    public void SetGameOver()
    {
        isGameOver = true;
        UpdateUI();
    }

    // =================================
    // UI更新
    // =================================

    private void UpdateUI()
    {
        if (turnText != null)
        {
            turnText.gameObject.SetActive(true);
            turnText.text = $"P{currentPlayer}のターン";
            turnText.color = Color.white;

            int index = currentPlayer - 1;

            if (index >= 0 && index < playerColors.Length)
            {
                Color color = playerColors[index];

                // 透明度を必ず100%にする
                color.a = 1f;

                turnText.color = color;
            }
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);

            timerText.text =
                $"残り時間：{Mathf.CeilToInt(remainingTurnTime)}秒";
        }
    }
}
