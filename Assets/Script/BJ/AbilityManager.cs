using UnityEngine;

public class BJAbilityManager : MonoBehaviour
{
    // プレイヤーごとの能力強化段階

    public int[] winMoneyLevel = new int[3];
    public int[] betLimitLevel = new int[3];


    // バースト無効の所持数

    public int[] bustGuardCount = new int[3];


    // その他の能力

    // 引き直し
    public bool[] hasRedraw = new bool[3];

    // 自分以外に強制1枚
    public bool[] hasForceRedraw = new bool[3];

    // ディーラーが17以上か分かる
    public bool[] hasDealerInfo = new bool[3];

    private bool dealerInfoPurchasedThisRound = false;

    // MoneyManagerとの同期

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
        if (playerIndex < 0 || playerIndex >= 3)
            return;

        bustGuardCount[playerIndex]++;

        Debug.Log(
            "P" + (playerIndex + 1) +
            " バースト無効の残り回数：" +
            bustGuardCount[playerIndex]
        );
    }


    // バースト無効を使用

    public bool UseBustGuard(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 3)
            return false;

        if (bustGuardCount[playerIndex] <= 0)
            return false;

        bustGuardCount[playerIndex]--;

        Debug.Log(
            "P" + (playerIndex + 1) +
            " バースト無効を使用！"
        );

        return true;
    }


    // 引き直しを購入

    public void AddRedraw(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 3)
            return;

        hasRedraw[playerIndex] = true;

        Debug.Log(
            "P" + (playerIndex + 1) +
            " 引き直しを購入！"
        );
    }


    // 強制1枚追加を購入

    public void AddForceRedraw(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 3)
            return;

        hasForceRedraw[playerIndex] = true;

        Debug.Log(
            "P" + (playerIndex + 1) +
            " 強制1枚追加を購入！"
        );
    }


    // ディーラー情報を購入

    public void AddDealerInfo(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 3)
            return;

        hasDealerInfo[playerIndex] = true;

        Debug.Log(
            "P" + (playerIndex + 1) +
            " ディーラー17以上情報を購入！"
        );
    }


    // 引き直しを使用

    public bool UseRedraw(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 3)
            return false;

        if (!hasRedraw[playerIndex])
            return false;

        hasRedraw[playerIndex] = false;

        Debug.Log(
            "P" + (playerIndex + 1) +
            " 引き直しを使用！"
        );

        return true;
    }


    // 強制1枚追加を使用

    public bool UseForceRedraw(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 3)
            return false;

        if (!hasForceRedraw[playerIndex])
            return false;

        hasForceRedraw[playerIndex] = false;

        Debug.Log(
            "P" + (playerIndex + 1) +
            " 強制1枚追加を使用！"
        );

        return true;
    }


    // ディーラー情報を使用

    public bool UseDealerInfo(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 3)
            return false;

        if (!hasDealerInfo[playerIndex])
            return false;

        // 今回は使用しても消費しない
        Debug.Log(
            "P" + (playerIndex + 1) +
            " ディーラー情報を使用！"
        );

        return true;
    }
}