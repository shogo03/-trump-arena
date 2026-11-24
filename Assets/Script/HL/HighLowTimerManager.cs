
using System;
using TMPro;
using UnityEngine;

public class HighLowTimerManager : MonoBehaviour
{
    [Header("タイマー表示")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("試合時間（秒）")]
    [SerializeField] private float gameTimeLimit = 60f;

    private float remainingTime;
    private bool isRunning;

    // 試合終了を通知するイベント
    public event Action OnTimeUp;

    public float RemainingTime => remainingTime;
    public bool IsRunning => isRunning;

    // タイマー初期化
    public void Initialize()
    {
        remainingTime = gameTimeLimit;
        isRunning = false;

        UpdateTimerUI();
    }

    // タイマー開始
    public void StartTimer()
    {
        remainingTime = gameTimeLimit;
        isRunning = true;

        UpdateTimerUI();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;

            UpdateTimerUI();

            Debug.Log("制限時間終了！");
            OnTimeUp?.Invoke();

            return;
        }

        UpdateTimerUI();
    }

    // タイマー停止
    public void StopTimer()
    {
        isRunning = false;
    }

    // 時間表示
    private void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        int seconds = Mathf.CeilToInt(remainingTime);
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        timerText.text = $"{minutes:00}:{remainingSeconds:00}";
    }
}
