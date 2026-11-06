
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
    private bool skipNextTurn = false;

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
        skipNextTurn = false;

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

        // 通常は次のプレイヤーへ
        currentPlayer = currentPlayer % playerCount + 1;

        // スキップ予約があれば、そのプレイヤーを飛ばす
        if (skipNextTurn)
        {
            Debug.Log($"プレイヤー{currentPlayer}のターンをスキップ！");

            currentPlayer = currentPlayer % playerCount + 1;

            skipNextTurn = false;
        }

        remainingTurnTime = turnTimeLimit;

                UpdateUI();
                OnTurnChanged?.Invoke(currentPlayer);

                Debug.Log($"次のプレイヤー：{currentPlayer}");

                return;
            }
        }

        Debug.LogError("参加プレイヤーが見つかりません！");
    }

    //次のプレイヤーのターンをスキップ予約
    public void RequestSkipNextTurn()
    {
        if (isGameOver)
            return;
        skipNextTurn = true;

        Debug.Log("次のプレイヤーのスキップを予約しました");
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
                : $"P{currentPlayer}のターン";
        }

        if (timerText != null)
        {
            timerText.text =
                $"残り時間：{Mathf.CeilToInt(remainingTurnTime)}秒";
        }
    }
}
