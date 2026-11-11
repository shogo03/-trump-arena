using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    // 所持金

    public int money1 = 1000;
    public int money2 = 1000;
    public int money3 = 1000;

    // ベット額

    public int bet1 = 0;
    public int bet2 = 0;
    public int bet3 = 0;

    // 現在ショップを開いているプレイヤー
    public int currentShopPlayer = 1;

    // 表示用

    public TMP_Text moneyText1;
    public TMP_Text moneyText2;
    public TMP_Text moneyText3;

    // 勝ち金UPの段階
    public int winStage1 = 0;
    public int winStage2 = 0;
    public int winStage3 = 0;


    public int betUpStage1 = 0;
    public int betUpStage2 = 0;
    public int betUpStage3 = 0;

    void Start()
    {
        UpdateUI();
    }

    // ベットする


    public void PlaceBet(int player)
    {
        if (player == 1)
        {
            money1 -= bet1;
            Debug.Log("P1 ベット後の所持金：" + money1);
        }
        else if (player == 2)
        {
            money2 -= bet2;
            Debug.Log("P2 ベット後の所持金：" + money2);
        }
        else if (player == 3)
        {
            money3 -= bet3;
            Debug.Log("P3 ベット後の所持金：" + money3);
        }

        UpdateUI();
    }

    // ダブルダウンの追加BET
    public void DoubleDown(int player)
    {
        if (player == 1)
        {
            money1 -= bet1;
            bet1 *= 2;
        }
        else if (player == 2)
        {
            money2 -= bet2;
            bet2 *= 2;
        }
        else if (player == 3)
        {
            money3 -= bet3;
            bet3 *= 2;
        }

        Debug.Log("P" + player + " ダブルダウン！");
        UpdateUI();
    }

    // 勝ち

    public void Win(int player)
    {
        int bet = 0;
        int stage = 0;

        if (player == 1)
        {
            bet = bet1;
            stage = winStage1;
        }
        else if (player == 2)
        {
            bet = bet2;
            stage = winStage2;
        }
        else if (player == 3)
        {
            bet = bet3;
            stage = winStage3;
        }

        float[] multipliers = { 1.0f, 1.2f, 1.5f, 2.0f };

        int reward = bet + Mathf.RoundToInt(bet * multipliers[stage]);

        if (player == 1) money1 += reward;
        else if (player == 2) money2 += reward;
        else if (player == 3) money3 += reward;

        Debug.Log("P" + player + " 勝ち！報酬：" + reward);
        UpdateUI();
    }


    // 負け

    public void Lose(int player)
    {
        // BETした時点でお金を引いているので、
        // 負けた場合は何もしない

        Debug.Log("P" + player + " 負け！");

        UpdateUI();
    }

    // 引き分け

    public void Draw(int player)
    {
        if (player == 1)
        {
            money1 += bet1;
        }
        else if (player == 2)
        {
            money2 += bet2;
        }
        else if (player == 3)
        {
            money3 += bet3;
        }

        Debug.Log("P" + player + " 引き分け！");

        UpdateUI();
    }

    public int GetBetLimit(int player)
    {
        if (player == 1)
            return 500 + betUpStage1 * 50;

        else if (player == 2)
            return 500 + betUpStage2 * 50;

        else if (player == 3)
            return 500 + betUpStage3 * 50;

        return 500;
    }
    // P1 BET +10

    public void IncreaseBet1()
    {
        bet1 += 10;

        int limit = GetBetLimit(1);

        if (bet1 > limit)
            bet1 = limit;

        UpdateUI();
    }

    // P2 BET +10
    public void IncreaseBet2()
    {
        bet2 += 10;

        int limit = GetBetLimit(2);

        if (bet2 > limit)
            bet2 = limit;

        UpdateUI();
    }

    // P3 BET +10
    public void IncreaseBet3()
    {
        bet3 += 10;

        int limit = GetBetLimit(3);

        if (bet3 > limit)
            bet3 = limit;

        UpdateUI();
    }

    // P1 BET -10

    public void DecreaseBet1()
    {
        bet1 -= 10;

        if (bet1 < 10)
            bet1 = 10;

        UpdateUI();
    }

    // P2 BET -10
    public void DecreaseBet2()
    {
        bet2 -= 10;

        if (bet2 < 10)
            bet2 = 10;

        UpdateUI();
    }

    // P3 BET -10
    public void DecreaseBet3()
    {
        bet3 -= 10;

        if (bet3 < 10)
            bet3 = 10;

        UpdateUI();
    }
    // UI更新


    void UpdateUI()
    {
        if (moneyText1 != null)
            moneyText1.text = "Money: " + money1 + "\nBET: " + bet1;

        if (moneyText2 != null)
            moneyText2.text = "Money: " + money2 + "\nBET: " + bet2;

        if (moneyText3 != null)
            moneyText3.text = "Money: " + money3 + "\nBET: " + bet3;
    }

    public void BuyWinUp()
    {
        int price = 250;
        int player = currentShopPlayer;

        if (player == 1)
        {
            if (winStage1 >= 3 || money1 < price) return;

            money1 -= price;
            winStage1++;
        }
        else if (player == 2)
        {
            if (winStage2 >= 3 || money2 < price) return;

            money2 -= price;
            winStage2++;
        }
        else if (player == 3)
        {
            if (winStage3 >= 3 || money3 < price) return;

            money3 -= price;
            winStage3++;
        }

        Debug.Log("P" + player + " 勝ち金UP！現在の段階：" +
            (player == 1 ? winStage1 : player == 2 ? winStage2 : winStage3));

        UpdateUI();
    }

    public void SetShopPlayer(int player)
    {
        currentShopPlayer = player;
        Debug.Log("P" + player + " のショップを開きました");
    }

    public void BuyBetUp()
    {
        int price = 100;
        int player = currentShopPlayer;

        if (player == 1)
        {
            if (betUpStage1 >= 10 || money1 < price) return;

            money1 -= price;
            betUpStage1++;
        }
        else if (player == 2)
        {
            if (betUpStage2 >= 10 || money2 < price) return;

            money2 -= price;
            betUpStage2++;
        }
        else if (player == 3)
        {
            if (betUpStage3 >= 10 || money3 < price) return;

            money3 -= price;
            betUpStage3++;
        }

        Debug.Log("P" + player + " 掛け金UP！");

        UpdateUI();
    }

    public void ResetBet()
    {
        bet1 = 0;
        bet2 = 0;
        bet3 = 0;

        UpdateUI();

        Debug.Log("全プレイヤーのBETをリセットしました");
    }
}