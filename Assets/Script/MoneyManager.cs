using UnityEngine;
using TMPro;
public class MoneyManager : MonoBehaviour
{
    // 所持金

    public int money1 = 1000;
    public int money2 = 1000;
    public int money3 = 1000;


    // ベット額

    public int bet1 = 100;
    public int bet2 = 100;
    public int bet3 = 100;


    // UI

    public TMP_Text moneyText1;
    public TMP_Text moneyText2;
    public TMP_Text moneyText3;


    void Start()
    {
        UpdateUI();
    }


    // ベットする

    public void PlaceBet(int player)
    {
        if (player == 1)
        {
            if (money1 >= bet1)
            {
                money1 -= bet1;
                Debug.Log("P1 ベット：" + bet1);
            }
            else
            {
                Debug.Log("P1は所持金不足！");
                return;
            }
        }
        else if (player == 2)
        {
            if (money2 >= bet2)
            {
                money2 -= bet2;
                Debug.Log("P2 ベット：" + bet2);
            }
            else
            {
                Debug.Log("P2は所持金不足！");
                return;
            }
        }
        else if (player == 3)
        {
            if (money3 >= bet3)
            {
                money3 -= bet3;
                Debug.Log("P3 ベット：" + bet3);
            }
            else
            {
                Debug.Log("P3は所持金不足！");
                return;
            }
        }

        UpdateUI();
    }


    // 勝ち

    public void Win(int player)
    {
        if (player == 1)
        {
            money1 += bet1 * 2;
        }
        else if (player == 2)
        {
            money2 += bet2 * 2;
        }
        else if (player == 3)
        {
            money3 += bet3 * 2;
        }

        Debug.Log("P" + player + " 勝ち！");

        UpdateUI();
    }


    // 負け

    public void Lose(int player)
    {
        // ベット時にすでに所持金から引いているので
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
}
