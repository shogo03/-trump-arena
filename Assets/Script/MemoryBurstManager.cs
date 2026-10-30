using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MemoryBurstManager : MonoBehaviour
{
    [Header("山札")]
    [SerializeField] private MemoryBurstDeck deck;

    [Header("カード")]
    [SerializeField] private MemoryBurstCardView cardPrefab;
    [SerializeField] private Sprite backImage;

    [Header("カードを並べる場所")]
    [SerializeField] private Transform cardArea;

    [Header("設定")]
    [SerializeField] private int cardCount = 24;
    [SerializeField] private int columns = 6;
    [SerializeField] private float flipBackDelay = 1f;
    [SerializeField] private float matchedDisplayTime = 1f;

    [Header("得点表示（プレイヤー1～4）")]
    [SerializeField]
    private TextMeshProUGUI[] playerScoreTexts =
        new TextMeshProUGUI[4];

    [Header("ターン表示")]
    [SerializeField] private TextMeshProUGUI turnText;

    [Header("リザルト")]
    [SerializeField] private string resultSceneName = "MemoryBurstResult";

    [Header("プレイヤー１の妨害能力")]
    [SerializeField] private Button shuffleButton;
    

    [Header("プレイヤー2の妨害能力")]
    [SerializeField] private Button shuffleButton2;


    private readonly List<MemoryBurstCardView> cards = new();

    private MemoryBurstCardView firstCard;
    private MemoryBurstCardView secondCard;

    private int currentPlayer = 1;
    private int playerCount = 1;
    private int[] scores = new int[4];

    private bool isChecking;
    private bool isGameOver;
    private int remainingCards;
   
    private bool[] abilityUsed = new bool[4];

    public int CurrentPlayer => currentPlayer;

    // 現在のプレイヤーに対応するコントローラーだけ操作可能
    // 現在のプレイヤーをコントローラーで操作できるか
    public bool CanControl(int gamepadIndex)
    {
        if (isChecking || isGameOver)
            return false;

        // 人間プレイヤーの人数を超えていたらCPUのターン
        if (currentPlayer > GameSettings.HumanPlayerCount)
            return false;

        // コントローラーの接続確認
        if (gamepadIndex < 0 ||
            gamepadIndex >= Gamepad.all.Count)
            return false;

        // プレイヤー番号とコントローラー番号を対応させる
        return gamepadIndex == currentPlayer - 1;
    }

    private void Start()
    {
        playerCount = GameSettings.TotalPlayerCount;

        Debug.Log($"プレイヤー人数：{playerCount}");

        // シャッフルボタンのイベント登録
        if (shuffleButton != null)
        {
            shuffleButton.onClick.AddListener(ShuffleUnmatchedCardsPlayer1);
        }

        if (shuffleButton2 != null)
        {
            shuffleButton2.onClick.AddListener(ShuffleUnmatchedCardsPlayer2);
        }

        CreateCards();
        UpdateUI();
    }

    private void CreateCards()
    {
        if (deck == null || cardPrefab == null ||
            cardArea == null || backImage == null)
        {
            Debug.LogError(
                "Deck、Card Prefab、Back Image、Card Areaを設定してください。",
                this
            );
            return;
        }

        if (cardCount < 2 || cardCount % 2 != 0)
        {
            Debug.LogError("カード枚数は2以上の偶数にしてください。", this);
            return;
        }

        deck.Initialize();

        foreach (Transform child in cardArea)
            Destroy(child.gameObject);

        cards.Clear();
        firstCard = null;
        secondCard = null;
        currentPlayer = 1;
        scores = new int[4];
        isChecking = false;
        isGameOver = false;
        remainingCards = 0;
        abilityUsed = new bool[4];

        for (int i = 0; i < cardCount; i++)
        {
            Card card = deck.DrawCard();

            if (card == null)
            {
                Debug.LogError($"カードを取得できませんでした：{i}", this);
                break;
            }

            MemoryBurstCardView view =
                Instantiate(cardPrefab, cardArea);

            view.Setup(card, backImage, OnCardClicked);

            cards.Add(view);
            remainingCards++;
        }

        UpdateUI();
    }

    // 妨害能力を使用できるかの判定
    private bool CanUseAbility(int player)
    {
        if (isGameOver || isChecking)
            return false;

        if (firstCard != null || secondCard != null)
            return false;

        if (currentPlayer != player)
            return false;

        if (player < 1 || player > playerCount)
            return false;

        if (abilityUsed[player - 1])
            return false;

        return true;
    }

    private void OnCardClicked(MemoryBurstCardView card)
    {
        if (isChecking || isGameOver || card == null)
            return;

        if (firstCard == card || secondCard == card)
            return;

        if (card.IsRevealed || card.IsMatched)
            return;

        card.Reveal();

        if (firstCard == null)
        {
            firstCard = card;
            return;
        }

        secondCard = card;
        StartCoroutine(CheckPair());
    }

    private IEnumerator CheckPair()
    {
        isChecking = true;
        UpdateUI();

        if (firstCard.Number == secondCard.Number)
        {
            yield return new WaitForSeconds(matchedDisplayTime);

            firstCard.SetMatched();
            secondCard.SetMatched();

            scores[currentPlayer - 1]++;
            remainingCards -= 2;

            Debug.Log($"プレイヤー{currentPlayer}がペア成立！");

            firstCard = null;
            secondCard = null;

            if (remainingCards <= 0)
            {
                isGameOver = true;
                isChecking = false;
                UpdateUI();
                ShowResult();
                yield break;
            }

            // ペア成立時は同じプレイヤーのターンを継続
        }
        else
        {
            yield return new WaitForSeconds(flipBackDelay);

            if (firstCard != null)
                firstCard.FlipBack();

            if (secondCard != null)
                secondCard.FlipBack();

            firstCard = null;
            secondCard = null;

            // ペア不成立なら次のプレイヤーへ
            currentPlayer = currentPlayer % playerCount + 1;
        }

        isChecking = false;
        UpdateUI();

      
    }

    private void UpdateUI()
    {
        for (int i = 0; i < 4; i++)
        {
            if (playerScoreTexts == null ||
                i >= playerScoreTexts.Length ||
                playerScoreTexts[i] == null)
                continue;

            bool active = i < playerCount;
            playerScoreTexts[i].gameObject.SetActive(active);

            if (active)
                playerScoreTexts[i].text =
                    $"プレイヤー{i + 1}：{scores[i]}点";
        }

        if (turnText != null && !isGameOver)
        {
            turnText.text = isChecking
                ? "カード判定中..."
                : $"プレイヤー{currentPlayer}のターン";
        }

        // プレイヤー1の妨害ボタン
        bool player1CanUse = CanUseAbility(1);

        if (shuffleButton != null)
            shuffleButton.interactable = player1CanUse;

      


        // プレイヤー2の妨害ボタン
        bool player2CanUse = CanUseAbility(2);

        if (shuffleButton2 != null)
            shuffleButton2.interactable = player2CanUse;

        
    }

    private void ShowResult()
    {
        int maxScore = 0;

        for (int i = 0; i < playerCount; i++)
        {
            maxScore = Mathf.Max(maxScore, scores[i]);
        }

        List<int> winners = new();

        for (int i = 0; i < playerCount; i++)
        {
            if (scores[i] == maxScore)
            {
                winners.Add(i + 1);
            }
        }

        // リザルトシーンへ得点と勝者を引き継ぐ
        GameResultData.PlayerCount = playerCount;
        GameResultData.Scores = (int[])scores.Clone();
        GameResultData.Winners = winners.ToArray();
        GameResultData.IsSinglePlayer = playerCount == 1;

        Debug.Log("ゲーム終了！リザルトシーンへ移動します。");

        SceneManager.LoadScene(resultSceneName);
    }


    
    // シャッフル妨害能力（共通処理）
    private bool ShuffleCards()
    {
       
        // ゲーム終了中・ペア判定中・1枚目を選択中は使えない
        if (isGameOver || isChecking ||
            firstCard != null || secondCard != null)
        {
            return false;
        }

        // 裏向きで、まだ獲得されていないカードを集める
        List<MemoryBurstCardView> targets = cards.FindAll(
            card => card != null &&
                    !card.IsMatched &&
                    !card.IsRevealed
        );

        if (targets.Count < 2)
        {
            Debug.Log("シャッフルできるカードが2枚未満です。", this);
            return false;
        }

        // 現在の並び順を保存
        List<Transform> order = new List<Transform>();

        for (int i = 0; i < cardArea.childCount; i++)
        {
            order.Add(cardArea.GetChild(i));
        }

        // シャッフル対象の位置を記録
        List<int> slots = new List<int>();

        foreach (MemoryBurstCardView card in targets)
        {
            slots.Add(card.transform.GetSiblingIndex());
        }

        // Fisher-Yates法
        for (int i = targets.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            (targets[i], targets[j]) = (targets[j], targets[i]);
        }

        // 対象カードの位置を入れ替える
        for (int i = 0; i < slots.Count; i++)
        {
            order[slots[i]] = targets[i].transform;
        }

        // 最終的な並び順を適用
        for (int i = 0; i < order.Count; i++)
        {
            order[i].SetSiblingIndex(i);
        }

        Debug.Log("シャッフル成功！", this);

        return true;
    }



    // プレイヤー1の妨害能力使用
    public void ShuffleUnmatchedCardsPlayer1()
    {
        if (!TryUseAbility(1))
            return;

        if (!ShuffleCards())
            return;

        CompleteAbility(1);
    }

    // プレイヤー2の妨害能力使用
    public void ShuffleUnmatchedCardsPlayer2()
    {
        if (!TryUseAbility(2))
            return;

        if (!ShuffleCards())
            return;

        CompleteAbility(2);
    }




    //妨害能力共通処理
    private bool TryUseAbility(int player)
    {
        if (!CanUseAbility(player))
            return false;

        abilityUsed[player - 1] = true;

        UpdateUI();

        Debug.Log($"プレイヤー{player}が妨害能力を使用しました！");

        return true;
    }

    // 妨害能力の使用完了処理
    private void CompleteAbility(int player)
    {
        // 使用済みにする
        abilityUsed[player - 1] = true;

        // ボタンの状態を更新
        UpdateUI();

        Debug.Log($"プレイヤー{player}は妨害能力を使用済みにしました！");
    }

    private void OnDestroy()
    {
        if (shuffleButton != null)
        {
            shuffleButton.onClick.RemoveListener(
                ShuffleUnmatchedCardsPlayer1
            );
        }
        if(shuffleButton2 != null)
        {
            shuffleButton2.onClick.RemoveListener(
                ShuffleUnmatchedCardsPlayer2);
        }
    }


}
