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


    public BJDealer bjDealer;
    public BJResult bjResult;

   
    public Vector2 cardSize = new Vector2(60, 80);

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

    private bool isStand = false;
    private bool isStand2 = false;
    private bool isStand3 = false;

    // P1の引き直し管理
    private bool hasHit1 = false;
    private bool hasRedrawn1 = false;

    // P2の引き直し管理
    private bool hasHit2 = false;
    private bool hasRedrawn2 = false;

    // バースト無効で返金されたか
    private bool bustGuardUsed1 = false;
    private bool bustGuardUsed2 = false;
    private bool bustGuardUsed3 = false;

    private bool forceCardHidden2 = false;
    private bool forceCardHidden3 = false;

    public BJCardManager cardManager;
    public BJTurnManager turnManager;
    public BJRoundManager roundManager;
    public BJAbilityManager abilityManager;
    public BJCursorManager cursorManager;

    [Header("プレイヤーUI")]
    public GameObject player2UI;
    public GameObject player3UI;

    


    public void DrawCard()
    {
        if (!isInitialDeal && turnManager.GetCurrentTurn() != 1)
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

        if (cardManager == null)
        {
            Debug.LogError("BJCardManagerが設定されていません！");
            return;
        }

        if (!isInitialDeal)
        {
            hasHit1 = true;
        }

        cardManager.DrawAndDisplayCard(deck, cardManager.hand, handArea, cardPrefab, cardSize, isInitialDeal);

        UpdatePoint();
    }

    public void DrawCard2()
    {
        // 強制追加されたカードを表向きにする
        if (!isInitialDeal && forceCardHidden2)
        {
            cardManager.RevealCards(cardManager.hand2, handArea2);
            forceCardHidden2 = false;
        }

        if (!isInitialDeal && turnManager.GetCurrentTurn() != 2)
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

        if (cardManager == null)
        {
            Debug.LogError("BJCardManagerが設定されていません！");
            return;
        }

        if (!isInitialDeal)
        {
            hasHit2 = true;
        }

        cardManager.DrawAndDisplayCard(deck, cardManager.hand2, handArea2, cardPrefab, cardSize, isInitialDeal);

        if (cardManager.hand2.Count > handArea2.childCount)
        {
            cardManager.RevealCards(cardManager.hand2, handArea2);
        }

        if (!forceCardHidden2)
        {
            UpdatePoint2();
        }
    }

    public void DrawCard3()
    {
        // 強制追加されたカードを表向きにする
        if (!isInitialDeal && forceCardHidden3)
        {
            cardManager.RevealCards(cardManager.hand3, handArea3);
            forceCardHidden3 = false;
        }

        if (!isInitialDeal && turnManager.GetCurrentTurn() != 3)
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

        if (cardManager == null)
        {
            Debug.LogError("BJCardManagerが設定されていません！");
            return;
        }

        cardManager.DrawAndDisplayCard(deck, cardManager.hand3, handArea3, cardPrefab, cardSize, isInitialDeal);
        if (!forceCardHidden3)
        {
            UpdatePoint3();
        }
    }

    public void ForceRedraw1()
    {
        // P1のターンでなければ使用できない
        if (turnManager.GetCurrentTurn() != 1)
        {
            Debug.Log("P1のターンではありません！");
            return;
        }

        // P1がBET確定していなければ使用できない
        if (!betConfirmed1)
        {
            Debug.Log("先にBETを確定してください！");
            return;
        }

        // 能力を持っているか確認して消費
        if (abilityManager == null || !abilityManager.UseForceRedraw(0))
        {
            Debug.Log("強制引き直し能力を持っていません！");
            return;
        }

        // P2が参加中で、バースト・STANDしていなければ1枚引く
        if (GameSettings.IsActive[1] && !isBust2 && !isStand2)
        {
            cardManager.DrawAndDisplayCard(
                deck, cardManager.hand2, handArea2,
                cardPrefab, cardSize, true
            );

            forceCardHidden2 = true;
        }

        // P3が参加中で、バースト・STANDしていなければ1枚引く
        if (GameSettings.IsActive[2] && !isBust3 && !isStand3)
        {
            cardManager.DrawAndDisplayCard(
                deck, cardManager.hand3, handArea3,
                cardPrefab, cardSize, true
            );

            forceCardHidden3 = true;
        }

        Debug.Log("強制引き直しを発動！P2・P3にカードを配りました！");
    }

    public void ConfirmBet1()
    {
        if (turnManager.GetCurrentTurn() != 1 || betConfirmed1)
            return;

        moneyManager.PlaceBet(1);
        betConfirmed1 = true;

        cardManager.RevealCards(cardManager.hand, handArea);
        UpdatePoint();
        Debug.Log("プレイヤー1のBET確定！");
    }

    public void ConfirmBet2()
    {
        if (turnManager.GetCurrentTurn() != 2 || betConfirmed2)
            return;

        moneyManager.PlaceBet(2);
        betConfirmed2 = true;

        cardManager.RevealCards(cardManager.hand2, handArea2);
        UpdatePoint2();
        Debug.Log("プレイヤー2のBET確定！");
    }

    public void ConfirmBet3()
    {
        if (turnManager.GetCurrentTurn() != 3 || betConfirmed3)
            return;

        moneyManager.PlaceBet(3);
        betConfirmed3 = true;

        cardManager.RevealCards(cardManager.hand3, handArea3);
        UpdatePoint3();
        Debug.Log("プレイヤー3のBET確定！");
    }

    public void DoubleDown1()
    {
        if (turnManager.GetCurrentTurn() != 1 || !betConfirmed1 || doubleDown1)
            return;

        if (cardManager.hand.Count != 2)
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
        if (turnManager.GetCurrentTurn() != 2 || !betConfirmed2 || doubleDown2)
            return;

        // 最初の2枚だけで使用可能
        if (cardManager.hand2.Count != 2)
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
        if (turnManager.GetCurrentTurn() != 3 || !betConfirmed3 || doubleDown3)
            return;

        // 最初の2枚だけで使用可能
        if (cardManager.hand3.Count != 2)
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
        if (isInitialDeal) return;

        cardManager.UpdatePointText(cardManager.hand, pointText);
        CheckBust(cardManager.hand, pointText);
    }

    void UpdatePoint2()
    {
        if (isInitialDeal) return;

        cardManager.UpdatePointText(cardManager.hand2, pointText2);
        CheckBust(cardManager.hand2, pointText2);
    }

    void UpdatePoint3()
    {
        if (isInitialDeal) return;

        cardManager.UpdatePointText(cardManager.hand3, pointText3);
        CheckBust(cardManager.hand3, pointText3);
    }

    void CheckBust(List<Card> playerHand, Text targetText)
    {

        Debug.Log("CheckBustが呼ばれた！");
        int total = cardManager.CalculatePoint(playerHand);

        if (isInitialDeal || total <= 21)
            return;

        targetText.text = "バースト！ 合計：" + total;

        // P1
        if (playerHand == cardManager.hand)
        {
            if (isBust) return;

            Debug.Log("P1のバースト無効所持数：" + abilityManager.bustGuardCount[0]);

            // バースト無効を持っている場合
            if (abilityManager != null && abilityManager.UseBustGuard(0))
            {
                bustGuardUsed1 = true;
                moneyManager.RefundBet(1);
                isStand = true;

                targetText.text = "バースト無効！掛け金返却！";
                Debug.Log("P1返金処理を実行！ BET額：" + moneyManager.bet1);
                NextTurn();
                return;
            }

            isBust = true;
            isStand = true;
        }
        // P2
        else if (playerHand == cardManager.hand2)
        {
            if (isBust2) return;

            if (abilityManager != null && abilityManager.UseBustGuard(1))
            {
                bustGuardUsed2 = true;
                moneyManager.RefundBet(2);
                isStand2 = true;

                targetText.text = "バースト無効！掛け金返却！";
                Debug.Log("P2返金処理を実行！ BET額：" + moneyManager.bet2);
                NextTurn();
                return;
            }

            isBust2 = true;
            isStand2 = true;
        }
        // P3
        else if (playerHand == cardManager.hand3)
        {
            if (isBust3) return;

            if (abilityManager != null && abilityManager.UseBustGuard(2))
            {
                bustGuardUsed3 = true;
                moneyManager.RefundBet(3);
                isStand3 = true;

                targetText.text = "バースト無効！掛け金返却！";
                Debug.Log("P3返金処理を実行！ BET額：" + moneyManager.bet3);
                NextTurn();
                return;
            }

            isBust3 = true;
            isStand3 = true;
        }

        NextTurn();
    }

    public void Stand()
    {
        if (!isInitialDeal && !betConfirmed1)
        {
            Debug.Log("先にBETを確定してください！");
            return;
        }

        if (turnManager.GetCurrentTurn() != 1) return;
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

        if (turnManager.GetCurrentTurn() != 2) return;
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

        if (turnManager.GetCurrentTurn() != 3) return;
        if (isStand3 || isBust3) return;

        isStand3 = true;
        Debug.Log("P3：STAND！");
        NextTurn();
    }
    public void StartGame()
    {

        UpdateRoundText();

        if (cursorManager != null)
        {
            cursorManager.SetTurnCursor(1);
        }

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
        if (bustGuardUsed1)
        {
            resultText.text = "バースト無効！掛け金返却！";
        }
        else
        {
            bjResult.CheckWinner(
                cardManager.hand, resultText, "P1",
                dealerTotal, isBust, moneyManager
            );
        }

        // P2が参加している場合
        if (GameSettings.IsActive[1])
        {
            if (GameSettings.IsActive[1])
            {
                if (bustGuardUsed2)
                {
                    resultText2.text = "バースト無効！掛け金返却！";
                }
                else
                {
                    if (GameSettings.IsActive[2])
                    {
                        if (bustGuardUsed3)
                        {
                            resultText3.text = "バースト無効！掛け金返却！";
                        }
                        else
                        {
                            bjResult.CheckWinner(
                                cardManager.hand3, resultText3, "P3",
                                dealerTotal, isBust3, moneyManager
                            );
                        }
                    }
                }
            }
        }

        // P3が参加している場合
        if (GameSettings.IsActive[2])
        {
            bjResult.CheckWinner(
                cardManager.hand3, resultText3, "P3",
                dealerTotal, isBust3, moneyManager
            );
        }

        StartCoroutine(WaitNextRound());
    }

    void NextTurn()
    {
        bool[] activePlayers =
        {
        GameSettings.IsActive[0],
        GameSettings.IsActive[1],
        GameSettings.IsActive[2]
    };

        bool[] finishedPlayers =
        {
        isStand || isBust,
        isStand2 || isBust2,
        isStand3 || isBust3
    };

        // 次のプレイヤーを取得
        int nextTurn = turnManager.NextTurn(
            activePlayers,
            finishedPlayers
        );

        // 全員終了
        if (nextTurn == 4)
        {
            // 全員のカーソルを非表示
            if (cursorManager != null)
            {
                cursorManager.SetTurnCursor(0);
            }

            Debug.Log("全員終了！ディーラーのターン！");
            DealerTurn();
            return;
        }

        // 次のプレイヤーのカーソルだけ表示
        if (cursorManager != null)
        {
            cursorManager.SetTurnCursor(nextTurn);
        }

        Debug.Log("プレイヤー" + nextTurn + "のターン！");
    }


    void UpdateRoundText()
    {
        if (roundText != null)
        {
            roundText.text = roundManager.GetRoundText();
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
        if (roundManager.IsLastRound())
        {
            Debug.Log("全10ラウンド終了！");

            // リザルトSceneへ移動
            SceneManager.LoadScene("BJResult");

            return;
        }

        // 次のラウンドへ
        roundManager.NextRound();

        StartNextRound();
    }

    void StartNextRound()
    {
        Debug.Log(roundManager.currentRound + "ラウンド目開始！");

        // 全プレイヤーのBETを0にする
        moneyManager.ResetBet();

        // 手札をリセット
        cardManager.hand.Clear();
        cardManager.hand2.Clear();
        cardManager.hand3.Clear();

        // 画面上のカードを削除
        cardManager.ClearCards(handArea);
        cardManager.ClearCards(handArea2);
        cardManager.ClearCards(handArea3);

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

        bustGuardUsed1 = false;
        bustGuardUsed2 = false;
        bustGuardUsed3 = false;

        // P1のディーラー情報能力をリセット
        abilityManager.hasDealerInfo[0] = false;

        turnManager.ResetTurn();

        UpdateRoundText();

        // カーソルを再表示
        if (cursorManager != null)
        {
            //cursorManager.UpdateCursors();
            cursorManager.SetTurnCursor(1);
        }

        // 次のラウンド開始
        StartGame();
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
        return turnManager.GetCurrentTurn();
    }

    void UpdatePlayerUI()
    {
        if (player2UI != null)
            player2UI.SetActive(GameSettings.IsActive[1]);

        if (player3UI != null)
            player3UI.SetActive(GameSettings.IsActive[2]);
    }

 
 // BET変更

    public void IncreaseBet1()
    {
        if (turnManager.GetCurrentTurn() != 1)
            return;

        moneyManager.IncreaseBet1();
    }

    public void DecreaseBet1()
    {
        if (turnManager.GetCurrentTurn() != 1)
            return;

        moneyManager.DecreaseBet1();
    }


    public void IncreaseBet2()
    {
        if (turnManager.GetCurrentTurn() != 2)
            return;

        moneyManager.IncreaseBet2();
    }

    public void DecreaseBet2()
    {
        if (turnManager.GetCurrentTurn() != 2)
            return;

        moneyManager.DecreaseBet2();
    }

    public void IncreaseBet3()
    {
        if (turnManager.GetCurrentTurn() != 3)
            return;

        moneyManager.IncreaseBet3();
    }

    public void DecreaseBet3()
    {
        if (turnManager.GetCurrentTurn() != 3)
            return;

        moneyManager.DecreaseBet3();
    }

    public void Redraw1()
    {
        // P1のターンか確認
        if (turnManager.GetCurrentTurn() != 1)
            return;

        // BET確定前なら使えない
        if (!betConfirmed1)
            return;

        // HIT後・使用済みなら使えない
        if (hasHit1 || hasRedrawn1)
            return;

        // 引き直し能力を持っているか確認
        if (abilityManager == null ||
            !abilityManager.UseRedraw(0))
        {
            Debug.Log("引き直し能力を持っていません！");
            return;
        }

        // 古い手札を消す
        cardManager.hand.Clear();
        cardManager.ClearCards(handArea);

        // 状態をリセット
        isBust = false;
        isStand = false;
        hasRedrawn1 = true;

        // 新しいカードを2枚配る
        cardManager.DrawAndDisplayCard(
            deck, cardManager.hand, handArea,
            cardPrefab, cardSize, false
        );

        cardManager.DrawAndDisplayCard(
            deck, cardManager.hand, handArea,
            cardPrefab, cardSize, false
        );

        // 点数更新
        UpdatePoint();

        Debug.Log("P1 引き直し完了！");
    }

    public void Redraw2()
    {
        // P2のターンか確認
        if (turnManager.GetCurrentTurn() != 2)
            return;

        // BET確定前なら使えない
        if (!betConfirmed2)
            return;

        // HIT後・使用済みなら使えない
        if (hasHit2 || hasRedrawn2)
            return;

        // 引き直し能力を持っているか確認
        if (abilityManager == null ||
            !abilityManager.UseRedraw(1))
        {
            Debug.Log("P2は引き直し能力を持っていません！");
            return;
        }

        // 古い手札を消す
        cardManager.hand2.Clear();
        cardManager.ClearCards(handArea2);

        // 状態をリセット
        isBust2 = false;
        isStand2 = false;
        hasRedrawn2 = true;

        // 新しいカードを2枚配る
        cardManager.DrawAndDisplayCard(
            deck, cardManager.hand2, handArea2,
            cardPrefab, cardSize, false
        );

        cardManager.DrawAndDisplayCard(
            deck, cardManager.hand2, handArea2,
            cardPrefab, cardSize, false
        );

        // 点数更新
        UpdatePoint2();

        Debug.Log("P2 引き直し完了！");
    }

    public void CheckDealerInfo1()
    {
        if (abilityManager == null)
        {
            Debug.LogError("BJAbilityManagerが設定されていません！");
            return;
        }

        // 能力を所持しているか確認
        if (!abilityManager.UseDealerInfo(0))
        {
            Debug.Log("ディーラー情報能力を持っていません！");
            return;
        }

        // ディーラーの合計点を取得
        int dealerPoint = bjDealer.GetDealerTotal();

        // 17以上か未満か表示
        if (dealerPoint >= 17)
        {
            Debug.Log("ディーラーの手札は17以上！");
        }
        else
        {
            Debug.Log("ディーラーの手札は17未満！");
        }
    }
}
//ここから下 変更用

