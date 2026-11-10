
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


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

        // P1に2枚
        DrawCard();
        DrawCard();

        // P2に2枚
        DrawCard2();
        DrawCard2();

        // P3に2枚
        DrawCard3();
        DrawCard3();

        // ディーラーに2枚
        bjDealer.DrawDealerCard();
        bjDealer.DrawDealerCard();

        isInitialDeal = false;

        Debug.Log("カードを配りました。BETを確定してください！");
    }

    void Start()
    {
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

        // ディーラーのターンを実行
        int dealerTotal = bjDealer.DealerTurn();

        // 3人それぞれの勝敗を判定
        bjResult.CheckWinner(hand, resultText, "P1",
            dealerTotal, isBust, moneyManager);

        bjResult.CheckWinner(hand2, resultText2, "P2",
            dealerTotal, isBust2, moneyManager);

        bjResult.CheckWinner(hand3, resultText3, "P3",
            dealerTotal, isBust3, moneyManager);

        StartCoroutine(WaitNextRound());
    }

    void NextTurn()
    {
        currentTurn++;

        // 終了しているプレイヤーは飛ばす
        while (currentTurn <= 3)
        {
            if (currentTurn == 1 && !isStand && !isBust)
            {
                Debug.Log("P1のターン！");
                return;
            }

            if (currentTurn == 2 && !isStand2 && !isBust2)
            {
                Debug.Log("P2のターン！");
                return;
            }

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
            ShowFinalResult();

            return;
        }

        currentRound++;
        StartNextRound();
    }

    void StartNextRound()
    {
        Debug.Log(currentRound + "ラウンド目開始！");

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

        string result = "最終結果\n";

        if (p1 == p2 && p2 == p3)
        {
            result += "全員同率1位！";
        }
        else
        {
            int maxMoney = Mathf.Max(p1, p2, p3);

            if (p1 == maxMoney)
                result += "P1 1位！\n";

            if (p2 == maxMoney)
                result += "P2 1位！\n";

            if (p3 == maxMoney)
                result += "P3 1位！\n";
        }

        result += "\nP1：" + p1 + " チップ";
        result += "\nP2：" + p2 + " チップ";
        result += "\nP3：" + p3 + " チップ";

        if (gameEndText != null)
            gameEndText.text = result;

        Debug.Log(result);
    }
}
//ここから下 変更用
//a
//i
//u
//e
//o