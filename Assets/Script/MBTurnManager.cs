
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

    private int currentPlayer = 1;
    private float remainingTurnTime;
    private bool isGameOver;

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
                   GameSettings.IsCPU[index];
        }
    }

    public event Action<int> OnTurnChanged;
    public event Action OnTurnTimeout;

    public void Initialize(int count)
    {
        currentPlayer = 1;
        remainingTurnTime = turnTimeLimit;
        isGameOver = false;

        UpdateUI();

        Debug.Log($"参加人数：{count}");
        Debug.Log($"最初のターン：プレイヤー{currentPlayer}");
    }

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

    // タイマーだけリセット
    public void ResetTimer()
    {
        if (isGameOver)
            return;

        remainingTurnTime = turnTimeLimit;
        UpdateUI();

        Debug.Log($"プレイヤー{currentPlayer}のタイマーをリセット！");
    }

    // 次の参加プレイヤーへ交代
    public void NextTurn()
    {
        if (isGameOver)
            return;

        // 最大4人分を確認
        for (int i = 0; i < 4; i++)
        {
            currentPlayer++;

            // 4Pの次は1P
            if (currentPlayer > 4)
                currentPlayer = 1;

            // 参加しているプレイヤーならターンを開始
            if (GameSettings.IsActive[currentPlayer - 1])
            {
                remainingTurnTime = turnTimeLimit;

                UpdateUI();
                OnTurnChanged?.Invoke(currentPlayer);

                Debug.Log($"次のプレイヤー：{currentPlayer}");

                return;
            }
        }

        Debug.LogError("参加プレイヤーが見つかりません！");
    }

    public void SetGameOver()
    {
        isGameOver = true;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (turnText != null)
        {
            turnText.text = isGameOver
                ? "GAME SET"
                : $"PLAYER {currentPlayer} TURN";
        }

        if (timerText != null)
        {
            timerText.text =
                $"残り時間：{Mathf.CeilToInt(remainingTurnTime)}秒";
        }
    }
}
