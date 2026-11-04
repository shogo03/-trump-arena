using UnityEngine;
using System;

public class MemoryBurstTurnManager : MonoBehaviour
{
    [SerializeField] private float turnTimeLimit = 15f;

    public int CurrentPlayer { get; private set; } = 1;
    public float RemainingTime { get; private set; }

    public bool IsTimeUp { get; private set; }

    public event Action<int> OnTurnChanged;
    public event Action<int> OnTimeUp;

    private int playerCount;
    private bool isPaused;

    public void Initialize(int count)
    {
        playerCount = Mathf.Clamp(count, 1, 4);
        CurrentPlayer = 1;
        RemainingTime = turnTimeLimit;
        IsTimeUp = false;
        isPaused = false;

        OnTurnChanged?.Invoke(CurrentPlayer);
    }

    private void Update()
    {
        if (isPaused || IsTimeUp || playerCount <= 0)
            return;

        RemainingTime -= Time.deltaTime;

        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            IsTimeUp = true;

            OnTimeUp?.Invoke(CurrentPlayer);
        }
    }

    public void NextTurn()
    {
        if (playerCount <= 0)
            return;

        CurrentPlayer = CurrentPlayer % playerCount + 1;

        RemainingTime = turnTimeLimit;
        IsTimeUp = false;

        OnTurnChanged?.Invoke(CurrentPlayer);
    }

    public void PauseTimer(bool pause)
    {
        isPaused = pause;
    }

    public void ResetTimer()
    {
        RemainingTime = turnTimeLimit;
        IsTimeUp = false;
    }
}