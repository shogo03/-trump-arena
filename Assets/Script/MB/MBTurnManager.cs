
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

    public bool IsGameOver => isGameOver;

    public int CurrentPlayer => currentPlayer;
    public float RemainingTurnTime => remainingTurnTime;

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

    // スキップされたプレイヤーを通知
    public event Action<int> OnPlayerSkipped;

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

        if (cursorManager != null)
        {
            cursorManager.SetCurrentTurn(currentPlayer);
        }

        Debug.Log($"参加人数：{count}");
        Debug.Log($"最初のターン：プレイヤー{currentPlayer}");
    }

    // =================================
    // タイマー
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

    public void ResetTimer()
    {
        if (isGameOver)
            return;

        remainingTurnTime = turnTimeLimit;
        UpdateUI();

        Debug.Log($"プレイヤー{currentPlayer}のタイマーをリセット！");
    }

    // =================================
    // 次のターン
    // =================================

    public void NextTurn()
    {
        if (isGameOver)
            return;

        // 現在のプレイヤーを含めず、次の参加者を探す
        for (int i = 0; i < 4; i++)
        {
            currentPlayer++;

            if (currentPlayer > 4)
                currentPlayer = 1;

            int index = currentPlayer - 1;

            if (GameSettings.IsActive == null ||
                index >= GameSettings.IsActive.Length ||
                !GameSettings.IsActive[index])
            {
                continue;
            }

            // スキップ予約があれば、このプレイヤーを飛ばす
            if (skipNextTurn)
            {
                Debug.Log($"プレイヤー{currentPlayer}のターンをスキップ！");

                skipNextTurn = false;

                // UIManagerに対象プレイヤーを通知
                OnPlayerSkipped?.Invoke(currentPlayer);

                continue;
            }

            // 通常のターン開始
            StartPlayerTurn();

            return;
        }

        skipNextTurn = false;

        Debug.LogError("次の参加プレイヤーが見つかりません！");
    }

    // =================================
    // ターン開始
    // =================================

    private void StartPlayerTurn()
    {
        remainingTurnTime = turnTimeLimit;

        UpdateUI();

        if (cursorManager != null)
        {
            cursorManager.SetCurrentTurn(currentPlayer);
        }

        OnTurnChanged?.Invoke(currentPlayer);

        Debug.Log($"次のプレイヤー：P{currentPlayer}");
    }

    // =================================
    // スキップ予約
    // =================================

    public void RequestSkipNextTurn()
    {
        if (isGameOver)
            return;

        // 参加者が1人しかいない場合はスキップしない
        if (GetActivePlayerCount() <= 1)
        {
            Debug.Log("参加プレイヤーが1人のため、スキップできません。");
            return;
        }

        skipNextTurn = true;

        Debug.Log("次のプレイヤーのスキップを予約しました");
    }

    // =================================
    // 参加人数取得
    // =================================

    private int GetActivePlayerCount()
    {
        if (GameSettings.IsActive == null)
            return 0;

        int count = 0;

        for (int i = 0; i < GameSettings.IsActive.Length && i < 4; i++)
        {
            if (GameSettings.IsActive[i])
                count++;
        }

        return count;
    }

    // =================================
    // ゲーム終了
    // =================================

    public void SetGameOver()
    {
        isGameOver = true;
        skipNextTurn = false;

        UpdateUI();
    }

    public void EndTurn()
    {
        if (isGameOver)
            return;

        NextTurn();
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
                color.a = 1f;

                turnText.color = color;
            }
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);

            timerText.text =
                $"残り：{Mathf.CeilToInt(remainingTurnTime)}秒";
        }
    }
}
