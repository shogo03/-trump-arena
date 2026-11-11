using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class BJdraw : MonoBehaviour
{
    public Deck deck;
    public GameObject cardPrefab;
    public Transform handArea;
    public Transform handArea2;
    public Transform handArea3;
    public Text pointText;
    public Text pointText2;
    public Text pointText3;

    public Text resultText;
    public Text resultText2;
    public Text resultText3;

    public MoneyManager moneyManager;

    private List<Card> hand = new List<Card>();
    private List<Card> hand2 = new List<Card>();
    private List<Card> hand3 = new List<Card>();

    public BJDealer bjDealer;
    public BJResult bjResult;

   
    public Vector2 cardSize = new Vector2(60, 80);

    // 現在のターン（1=P1、2=P2、3=P3）
    private int currentTurn = 1;

    // ラウンド管理
    private int currentRound = 1;
    private int maxRound = 10;
    

    public Text roundText;
    public Text gameEndText;

    // 最初のカード配布中かどうか
    private bool isInitialDeal = true;

    private bool isBust = false;
    private bool isBust2 = false;
    private bool isBust3 = false;

    private bool betConfirmed1 = false;
    private bool betConfirmed2 = false;
    private bool betConfirmed3 = false;

    // ダブルダウンを使用したか
    private bool doubleDown1 = false;
    private bool doubleDown2 = false;
    private bool doubleDown3 = false;

    [Header("プレイヤーUI")]
    public GameObject player2UI;
    public GameObject player3UI;

    public void DrawCard()
    {
        if (!isInitialDeal && currentTurn != 1)
        {
            Debug.Log("現在はP1のターンではありません");
            return;
        }

        if (isBust)
        {
            Debug.Log("P1はバーストしているため引けません");
            return;
        }

        if (isStand)
        {
            Debug.Log("STAND中はカードを引けません");
            return;
        }

        if (!isInitialDeal && !betConfirmed1)
        {
            Debug.Log("先にBETを確定してください！");
            return;
        }

        

        if (deck == null)
        {
            Debug.LogError("Deckが未設定！");
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("Card Prefabが未設定！");
            return;
        }

        if (handArea == null)
        {
            Debug.LogError("Hand Areaが未設定！");
            return;
        }

        Card card = deck.DrawCard();

        GameObject obj = Instantiate(cardPrefab, handArea);

        Image image = obj.GetComponent<Image>();

        if (image == null)
        {
            Debug.LogError("Card PrefabにImageがない！");
            return;
        }

        // 初期配布中は裏向き、それ以外は表向き
        if (isInitialDeal)
        {
            image.sprite = deck.cardBackImage;
        }
        else
        {
            image.sprite = card.image;
        }
        image.enabled = true;

        hand.Add(card);

        RectTransform rect = obj.GetComponent<RectTransform>();

        // カードを小さくする
        rect.sizeDelta = cardSize;

        // カードを横に並べる
        rect.anchoredPosition = new Vector2(
            (hand.Count - 1) * 35,
            0
        );
        UpdatePoint();
    }

    public void DrawCard2()
    {
        if (!isInitialDeal && currentTurn != 2)
        {
            Debug.Log("現在はP2のターンではありません");
            return;
        }

        if (isBust2)
        {
            Debug.Log("P2はバーストしているため引けません");
            return;
        }

        if (isStand2)
        {
            Debug.Log("2人目はSTAND中です");
            return;
        }

        if (!isInitialDeal && !betConfirmed2)
        {
            Debug.Log("先にBETを確定してください！");
            return;
        }

        DrawForPlayer(hand2, handArea2);
        UpdatePoint2();
    }

    public void DrawCard3()
    {
        if (!isInitialDeal && currentTurn != 3)
        {
            Debug.Log("現在はP3のターンではありません");
            return;
        }

        if (isBust3)
        {
            Debug.Log("P3はバーストしているため引けません");
            return;
        }

        if (isStand3)
        {
            Debug.Log("3人目はSTAND中です");
            return;
        }

        if (!isInitialDeal && !betConfirmed3)
        {
            Debug.Log("先にBETを確定してください！");
            return;
        }

        DrawForPlayer(hand3, handArea3);
        UpdatePoint3();
    }

    void DrawForPlayer(List<Card> playerHand, Transform area)
    {
        if (deck == null || cardPrefab == null || area == null)
        {
            Debug.LogError("Deck、Card Prefab、Hand Areaの設定を確認してね！");
            return;
        }

        Card card = deck.DrawCard();

        GameObject obj = Instantiate(cardPrefab, area);

        Image image = obj.GetComponent<Image>();

        if (image == null)
        {
            Debug.LogError("Card PrefabにImageがない！");
            Destroy(obj);
            return;
        }

        // 初期配布中は裏向き、それ以外は表向き
        if (isInitialDeal)
        {
            image.sprite = deck.cardBackImage;
        }
        else
        {
            image.sprite = card.image;
        }

        image.enabled = true;
        image.enabled = true;

        playerHand.Add(card);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = cardSize;
        rect.anchoredPosition = new Vector2((playerHand.Count - 1) * 35, 0);
    }

    // プレイヤーの手札を表向きにする
    void RevealPlayerCards(List<Card> playerHand, Transform area)
    {
        for (int i = 0; i < playerHand.Count; i++)
        {
            Image image = area.GetChild(i).GetComponent<Image>();

            if (image != null)
            {
                image.sprite = playerHand[i].image;
            }
        }
    }

    public void ConfirmBet1()
    {
        if (currentTurn != 1 || betConfirmed1)
            return;

        moneyManager.PlaceBet(1);
        betConfirmed1 = true;

        RevealPlayerCards(hand, handArea);
        UpdatePoint();
        Debug.Log("プレイヤー1のBET確定！");
    }

    public void ConfirmBet2()
    {
        if (currentTurn != 2 || betConfirmed2)
            return;

        moneyManager.PlaceBet(2);
        betConfirmed2 = true;

        RevealPlayerCards(hand2, handArea2);
        UpdatePoint2();
        Debug.Log("プレイヤー2のBET確定！");
    }

    public void ConfirmBet3()
    {
        if (currentTurn != 3 || betConfirmed3)
            return;

        moneyManager.PlaceBet(3);
        betConfirmed3 = true;

        RevealPlayerCards(hand3, handArea3);
        UpdatePoint3();
        Debug.Log("プレイヤー3のBET確定！");
    }

    public void DoubleDown1()
    {
        if (currentTurn != 1 || !betConfirmed1 || doubleDown1)
            return;

        if (hand.Count != 2)
            return;

        moneyManager.DoubleDown(1);
        doubleDown1 = true;

        DrawCard();

        if (!isBust)
        {
            isStand = true;
            Debug.Log("P1：ダブルダウン！");
            NextTurn();
        }
    }

    public void DoubleDown2()
    {
        if (currentTurn != 2 || !betConfirmed2 || doubleDown2)
            return;

        // 最初の2枚だけで使用可能
        if (hand2.Count != 2)
            return;

        moneyManager.DoubleDown(2);

        doubleDown2 = true;

        // カードを1枚だけ引く
        DrawCard2();

        // バーストしていなければ強制STAND
        if (!isBust2)
        {
            isStand2 = true;
            Debug.Log("P2：ダブルダウン！");
            NextTurn();
        }
    }

    public void DoubleDown3()
    {
        if (currentTurn != 3 || !betConfirmed3 || doubleDown3)
            return;

        // 最初の2枚だけで使用可能
        if (hand3.Count != 2)
            return;

        moneyManager.DoubleDown(3);

        doubleDown3 = true;

        // カードを1枚だけ引く
        DrawCard3();

        // バーストしていなければ強制STAND
        if (!isBust3)
        {
            isStand3 = true;
            Debug.Log("P3：ダブルダウン！");
            NextTurn();
        }
    }

    void UpdatePoint()
    {
        CalculatePoint(hand, pointText);
    }

    void UpdatePoint2()
    {
        CalculatePoint(hand2, pointText2);
    }

    void UpdatePoint3()
    {
        CalculatePoint(hand3, pointText3);
    }

    void CalculatePoint(List<Card> playerHand, Text targetText)
    {
        if (targetText == null) return;

        int total = 0;
        int aceCount = 0;

        foreach (Card card in playerHand)
        {
            if (card.number == 1)
            {
                total += 11;
                aceCount++;
            }
            else if (card.number >= 11)
            {
                total += 10;
            }
            else
            {
                total += card.number;
            }
        }

        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        // 初期配布中は合計点数を隠す
        if (isInitialDeal)
        {
            targetText.text = "";
            return;
        }

        if (total > 21)
        {
            targetText.text = "バースト！ 合計：" + total;

            if (playerHand == hand)
            {
                isBust = true;
                isStand = true;
                Debug.Log("P1はバースト！強制STAND！");
            }
            else if (playerHand == hand2)
            {
                isBust2 = true;
                isStand2 = true;
                Debug.Log("P2はバースト！強制STAND！");
            }
            else if (playerHand == hand3)
            {
                isBust3 = true;
                isStand3 = true;
                Debug.Log("P3はバースト！強制STAND！");
            }

            if (!isInitialDeal)
            {
                NextTurn();
            }
        }
        else
        {
            targetText.text = "合計：" + total;
        }
    }

    private bool isStand = false;
    private bool isStand2 = false;
    private bool isStand3 = false;

    public void Stand()
    {
        if (!isInitialDeal && !betConfirmed1)
        {
            Debug.Log("先にBETを確定してください！");
            return;
        }

        if (currentTurn != 1) return;
        if (isStand || isBust) return;

        isStand = true;
        Debug.Log("P1：STAND！");
        NextTurn();
    }

    public void Stand2()
    {

        if (!isInitialDeal && !betConfirmed2)
        {
            Debug.Log("先にBETを確定してください！");
            return;
        }

        if (currentTurn != 2) return;
        if (isStand2 || isBust2) return;

        isStand2 = true;
        Debug.Log("P2：STAND！");
        NextTurn();
    }

    public void Stand3()
    {

        if (!isInitialDeal && !betConfirmed3)
        {
            Debug.Log("先にBETを確定してください！");
            return;
        }

        if (currentTurn != 3) return;
        if (isStand3 || isBust3) return;

        isStand3 = true;
        Debug.Log("P3：STAND！");
        NextTurn();
    }
    public void StartGame()
    {
        UpdateRoundText();

        isInitialDeal = true;

        // BET確定状態をリセット
        betConfirmed1 = false;
        betConfirmed2 = false;
        betConfirmed3 = false;



        // P1に2枚（常に参加）
        DrawCard();
        DrawCard();

        // P2が参加している場合
        if (GameSettings.IsActive[1])
        {
            DrawCard2();
            DrawCard2();
        }

        // P3が参加している場合
        if (GameSettings.IsActive[2])
        {
            DrawCard3();
            DrawCard3();
        }

        // ディーラーに2枚
        bjDealer.DrawDealerCard();
        bjDealer.DrawDealerCard();

        isInitialDeal = false;

        Debug.Log("カードを配りました。BETを確定してください！");

        // ダブルダウン使用状態をリセット
        doubleDown1 = false;
        doubleDown2 = false;
        doubleDown3 = false;
    }

    void Start()
    {
        UpdatePlayerUI();
        StartGame();
    }

    void CheckAllStand()
    {
        if (isStand && isStand2 && isStand3)
        {
            DealerTurn();
        }
    }


    void DealerTurn()
    {
        if (bjDealer == null || bjResult == null)
        {
            Debug.LogError("BJDealerまたはBJResultが設定されていません！");
            return;
        }

        // ディーラーのターン
        int dealerTotal = bjDealer.DealerTurn();

        // P1の勝敗判定（常に参加）
        bjResult.CheckWinner(
            hand, resultText, "P1",
            dealerTotal, isBust, moneyManager
        );

        // P2が参加している場合
        if (GameSettings.IsActive[1])
        {
            bjResult.CheckWinner(
                hand2, resultText2, "P2",
                dealerTotal, isBust2, moneyManager
            );
        }

        // P3が参加している場合
        if (GameSettings.IsActive[2])
        {
            bjResult.CheckWinner(
                hand3, resultText3, "P3",
                dealerTotal, isBust3, moneyManager
            );
        }

        StartCoroutine(WaitNextRound());
    }



    void NextTurn()
    {
        currentTurn++;

        // 終了しているプレイヤーや参加していないプレイヤーを飛ばす
        while (currentTurn <= 3)
        {
            // 参加していないプレイヤーは飛ばす
            if (!GameSettings.IsActive[currentTurn - 1])
            {
                currentTurn++;
                continue;
            }

            // P1
            if (currentTurn == 1 && !isStand && !isBust)
            {
                Debug.Log("P1のターン！");
                return;
            }

            // P2
            if (currentTurn == 2 && !isStand2 && !isBust2)
            {
                Debug.Log("P2のターン！");
                return;
            }

            // P3
            if (currentTurn == 3 && !isStand3 && !isBust3)
            {
                Debug.Log("P3のターン！");
                return;
            }

            currentTurn++;
        }

        // 全員終了
        Debug.Log("全員終了！ディーラーのターン！");
        DealerTurn();
    }


    void UpdateRoundText()
    {
        if (roundText != null)
        {
            roundText.text = "ROUND " + currentRound + " / " + maxRound;
        }
    }

    IEnumerator WaitNextRound()
    {
        // 結果を3秒間表示
        yield return new WaitForSeconds(3f);

        NextRound();
    }

    void NextRound()
    {
        if (currentRound >= maxRound)
        {
            Debug.Log("全10ラウンド終了！");

            // リザルトSceneへ移動
            SceneManager.LoadScene("BJResult");

            return;
        }

        currentRound++;
        StartNextRound();
    }

    void StartNextRound()
    {
        Debug.Log(currentRound + "ラウンド目開始！");

        // 全プレイヤーのBETを0にする
        moneyManager.ResetBet();

        // 手札をリセット
        hand.Clear();
        hand2.Clear();
        hand3.Clear();

        // 画面上のカードを削除
        ClearHandArea(handArea);
        ClearHandArea(handArea2);
        ClearHandArea(handArea3);

        // ディーラーの手札をリセット
        bjDealer.ResetDealer();

        // 点数と結果をリセット
        if (pointText != null) pointText.text = "合計：0";
        if (pointText2 != null) pointText2.text = "合計：0";
        if (pointText3 != null) pointText3.text = "合計：0";

        if (resultText != null) resultText.text = "";
        if (resultText2 != null) resultText2.text = "";
        if (resultText3 != null) resultText3.text = "";

        // プレイヤーの状態をリセット
        isBust = false;
        isBust2 = false;
        isBust3 = false;

        isStand = false;
        isStand2 = false;
        isStand3 = false;

        currentTurn = 1;

        UpdateRoundText();

        // 次のラウンド開始
        StartGame();
    }

    void ClearHandArea(Transform area)
    {
        if (area == null) return;

        foreach (Transform child in area)
        {
            Destroy(child.gameObject);
        }
    }


    void ShowFinalResult()
    {
        int p1 = moneyManager.money1;
        int p2 = moneyManager.money2;
        int p3 = moneyManager.money3;

        // 最高所持金を調べる
        int maxMoney = p1;

        if (GameSettings.IsActive[1] && p2 > maxMoney)
            maxMoney = p2;

        if (GameSettings.IsActive[2] && p3 > maxMoney)
            maxMoney = p3;

        string result = "最終結果\n";

        // 1位のプレイヤーを表示
        if (p1 == maxMoney)
            result += "P1 1位！\n";

        if (GameSettings.IsActive[1] && p2 == maxMoney)
            result += "P2 1位！\n";

        if (GameSettings.IsActive[2] && p3 == maxMoney)
            result += "P3 1位！\n";

        // 参加しているプレイヤーだけ所持金を表示
        result += "\nP1：" + p1 + " チップ";

        if (GameSettings.IsActive[1])
            result += "\nP2：" + p2 + " チップ";

        if (GameSettings.IsActive[2])
            result += "\nP3：" + p3 + " チップ";

        if (gameEndText != null)
            gameEndText.text = result;

        Debug.Log(result);
    }


    public int GetCurrentTurn()
    {
        return currentTurn;
    }

    void UpdatePlayerUI()
    {
        if (player2UI != null)
            player2UI.SetActive(GameSettings.IsActive[1]);

        if (player3UI != null)
            player3UI.SetActive(GameSettings.IsActive[2]);
    }
}
//ここから下 変更用
//a
//i
//u
//e
//o
//ka
//ki
//ku
//