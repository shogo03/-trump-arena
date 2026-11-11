using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ResultManager : MonoBehaviour
{
    public TMP_Text p1Text;
    public TMP_Text p2Text;
    public TMP_Text p3Text;
    public TMP_Text winnerText;

    void Start()
    {
        ShowResult();
    }

    void ShowResult()
    {
        MoneyManager money = FindFirstObjectByType<MoneyManager>();

        if (money == null)
        {
            Debug.LogError("MoneyManagerが見つかりません！");
            return;
        }

        // プレイヤーの名前と所持金をまとめる
        List<PlayerData> players = new List<PlayerData>
        {
            new PlayerData("P1", money.money1),
            new PlayerData("P2", money.money2),
            new PlayerData("P3", money.money3)
        };

        // 所持金が多い順に並べる
        players = players
            .OrderByDescending(p => p.money)
            .ToList();

        // 順位を表示
        p1Text.text = "1位  " + players[0].name + "   " + players[0].money + "チップ";
        p2Text.text = "2位  " + players[1].name + "   " + players[1].money + "チップ";
        p3Text.text = "3位  " + players[2].name + "   " + players[2].money + "チップ";

        // 優勝者
        winnerText.text = "優勝：" + players[0].name;
    }


    // プレイヤーデータ
    class PlayerData
    {
        public string name;
        public int money;

        public PlayerData(string name, int money)
        {
            this.name = name;
            this.money = money;
        }
    }
}