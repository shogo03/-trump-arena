
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    public TMP_Text p1Text;
    public TMP_Text p2Text;
    public TMP_Text p3Text;

    [Header("シーン名")]
    [SerializeField] private string gameSceneName = "blackjack";
    [SerializeField] private string titleSceneName = "title";

    void Start()
    {
        ShowResult();
    }

    void ShowResult()
    {
        MoneyManager money = FindAnyObjectByType<MoneyManager>();

        if (money == null)
        {
            Debug.LogError("MoneyManagerが見つかりません！");
            return;
        }

        List<PlayerData> players = new List<PlayerData>();

        players.Add(new PlayerData("P1", money.money1));

        if (GameSettings.IsActive[1])
            players.Add(new PlayerData("P2", money.money2));

        if (GameSettings.IsActive[2])
            players.Add(new PlayerData("P3", money.money3));

        // 所持金が多い順に並べる
        players = players
            .OrderByDescending(p => p.money)
            .ToList();

        // 順位表示
        p1Text.text = "";
        p2Text.text = "";
        p3Text.text = "";

        for (int i = 0; i < players.Count; i++)
        {
            string text = (i + 1) + "位  "
                + players[i].name + "   "
                + players[i].money + "チップ";

            if (i == 0)
                p1Text.text = text;
            else if (i == 1)
                p2Text.text = text;
            else if (i == 2)
                p3Text.text = text;
        }
    }

    // リトライ
    public void RetryGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // タイトルに戻る
    public void ReturnToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
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
