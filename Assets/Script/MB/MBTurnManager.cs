
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

    // タイマーをリセット
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

        // 次の参加プレイヤーを探す
        for (int i = 0; i < 4; i++)
        {
            currentPlayer++;

            if (currentPlayer > 4)
                currentPlayer = 1;

            int index = currentPlayer - 1;

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

            remainingTurnTime = turnTimeLimit;

            UpdateUI();
            OnTurnChanged?.Invoke(currentPlayer);

            Debug.Log($"次のプレイヤー：P{currentPlayer}");
            return;
        }

        // 全員がスキップ対象になった場合
        skipNextTurn = false;

        Debug.LogError("次の参加プレイヤーが見つかりません！");
    }

    // 次のプレイヤーのターンをスキップ予約
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
            turnText.gameObject.SetActive(true);
            turnText.text = $"P{currentPlayer}のターン";
            turnText.color = Color.white;
        }

        int index = currentPlayer - 1;
        if (index >= 0 && index < playerColors.Length)
        {
            Color color = playerColors[index];

            // 透明度を必ず100%にする
            color.a = 1f;

            turnText.color = color;
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text =
                $"残り時間：{Mathf.CeilToInt(remainingTurnTime)}秒";
        }

       
    }
}
