
using UnityEngine;

public class MemoryBurstAbilityManager : MonoBehaviour
{
    private bool[] abilityUsed = new bool[4];

    private MemoryBurstCardManager cardManager;
    private MemoryBurstTurnManager turnManager;

    private bool gameOver;

    public void Initialize(
        MemoryBurstCardManager cards,
        MemoryBurstTurnManager turns)
    {
        cardManager = cards;
        turnManager = turns;

        abilityUsed = new bool[4];
        gameOver = false;
    }

    public bool CanUseAbility(int player)
    {
        if (gameOver || player < 1 || player > 4)
            return false;

        if (player != turnManager.CurrentPlayer)
            return false;

        if (abilityUsed[player - 1])
            return false;

        return true;
    }

    public bool Shuffle(int player)
    {
        if (!CanUseAbility(player))
            return false;

        if (!cardManager.ShuffleCards())
            return false;

        CompleteAbility(player);

        Debug.Log($"プレイヤー{player}がシャッフルしました！");
        return true;
    }

    public bool Skip(int player)
    {
        if (!CanUseAbility(player))
            return false;

        if (cardManager.IsChecking)
            return false;

        CompleteAbility(player);

        cardManager.EndCurrentTurn();
        turnManager.NextTurn();

        Debug.Log($"プレイヤー{player}がターンをスキップしました！");

        return true;
    }

    private void CompleteAbility(int player)
    {
        abilityUsed[player - 1] = true;
    }

    public void SetGameOver()
    {
        gameOver = true;
    }
}
