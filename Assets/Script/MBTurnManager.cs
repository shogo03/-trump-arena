
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
    private int playerCount = 1;
    private float remainingTurnTime;
    private bool isGameOver;

    public int CurrentPlayer => currentPlayer;
    public float RemainingTurnTime => remainingTurnTime;

    public event Action<int> OnTurnChanged;
    public event Action OnTurnTimeout;

    public void Initialize(int count)
    {
        playerCount = count;
        currentPlayer = 1;
        remainingTurnTime = turnTimeLimit;
        isGameOver = false;

        UpdateUI();
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

    // タイマーだけリセット（プレイヤーは交代しない）
    public void ResetTimer()
    {
        if (isGameOver)
            return;

        remainingTurnTime = turnTimeLimit;
        UpdateUI();

        Debug.Log($"プレイヤー{currentPlayer}のタイマーをリセット！");
    }

    // 次のプレイヤーへ交代
    public void NextTurn()
    {
        if (isGameOver)
            return;

        currentPlayer = currentPlayer % playerCount + 1;
        remainingTurnTime = turnTimeLimit;

        UpdateUI();
        OnTurnChanged?.Invoke(currentPlayer);

        Debug.Log($"次のプレイヤー：{currentPlayer}");
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
