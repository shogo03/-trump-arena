using UnityEngine;

public class BJAbilityManager : MonoBehaviour
{
    // プレイヤーごとの能力強化段階
    public int[] winMoneyLevel = new int[3];
    public int[] betLimitLevel = new int[3];

    // バースト無効の所持数
    public int[] bustGuardCount = new int[3];

    // その他の能力
    public bool[] hasRedraw = new bool[3];
    public bool[] hasForceRedraw = new bool[3];
    public bool[] hasDealerInfo = new bool[3];

    public void SyncMoneyAbilities(MoneyManager money)
    {
        winMoneyLevel[0] = money.winStage1;
        winMoneyLevel[1] = money.winStage2;
        winMoneyLevel[2] = money.winStage3;

        betLimitLevel[0] = money.betUpStage1;
        betLimitLevel[1] = money.betUpStage2;
        betLimitLevel[2] = money.betUpStage3;
    }

    // バースト無効を購入
    public void AddBustGuard(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 3) return;

        bustGuardCount[playerIndex]++;

        Debug.Log("P" + (playerIndex + 1) +
            " バースト無効の残り回数：" + bustGuardCount[playerIndex]);
    }

    // バースト無効を使用
    public bool UseBustGuard(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 3) return false;

        if (bustGuardCount[playerIndex] <= 0)
            return false;

        bustGuardCount[playerIndex]--;

        Debug.Log("P" + (playerIndex + 1) + " バースト無効を使用！");
        return true;
    }

}