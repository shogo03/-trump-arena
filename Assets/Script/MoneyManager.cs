using UnityEngine;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour
{
    // 初期所持金
    public int money1 = 1000;
    public int money2 = 1000;
    public int money3 = 1000;

    // 画面に表示するText
    public Text moneyText1;
    public Text moneyText2;
    public Text moneyText3;

    void Start()
    {
        UpdateMoneyUI();
    }

    public void Win(int player)
    {
        if (player == 1)
        {
            money1 += 100;
        }
        else if (player == 2)
        {
            money2 += 100;
        }
        else if (player == 3)
        {
            money3 += 100;
        }

        UpdateMoneyUI();
    }

    public void Lose(int player)
    {
        if (player == 1)
        {
            money1 -= 100;
        }
        else if (player == 2)
        {
            money2 -= 100;
        }
        else if (player == 3)
        {
            money3 -= 100;
        }

        UpdateMoneyUI();
    }

    public void Draw(int player)
    {
        // 引き分けなら変化なし
        Debug.Log("P" + player + " 引き分け");
    }

    void UpdateMoneyUI()
    {
        if (moneyText1 != null)
            moneyText1.text = "所持金：" + money1;

        if (moneyText2 != null)
            moneyText2.text = "所持金：" + money2;

        if (moneyText3 != null)
            moneyText3.text = "所持金：" + money3;
    }
}