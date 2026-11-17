
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoryBurstAbilityManager : MonoBehaviour
{
    [Header("プレイヤーごとのUI")]
    [SerializeField] private GameObject player1UI;
    [SerializeField] private GameObject player2UI;
    [SerializeField] private GameObject player3UI;
    [SerializeField] private GameObject player4UI;

    [Header("プレイヤー1の能力")]
    [SerializeField] private Button shuffleButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button randomButton;

    [Header("プレイヤー2の能力")]
    [SerializeField] private Button shuffleButton2;
    [SerializeField] private Button skipButton2;
    [SerializeField] private Button randomButton2;

    [Header("プレイヤー3の能力")]
    [SerializeField] private Button shuffleButton3;
    [SerializeField] private Button skipButton3;
    [SerializeField] private Button randomButton3;

    [Header("プレイヤー4の能力")]
    [SerializeField] private Button shuffleButton4;
    [SerializeField] private Button skipButton4;
    [SerializeField] private Button randomButton4;

    [Header("プレイヤー1の回数表示")]
    [SerializeField] private TMP_Text shuffleCountText;
    [SerializeField] private TMP_Text skipCountText;
    [SerializeField] private TMP_Text randomCountText;

    [Header("プレイヤー2の回数表示")]
    [SerializeField] private TMP_Text shuffleCountText2;
    [SerializeField] private TMP_Text skipCountText2;
    [SerializeField] private TMP_Text randomCountText2;

    [Header("プレイヤー3の回数表示")]
    [SerializeField] private TMP_Text shuffleCountText3;
    [SerializeField] private TMP_Text skipCountText3;
    [SerializeField] private TMP_Text randomCountText3;

    [Header("プレイヤー4の回数表示")]
    [SerializeField] private TMP_Text shuffleCountText4;
    [SerializeField] private TMP_Text skipCountText4;
    [SerializeField] private TMP_Text randomCountText4;

    // 各能力の最大使用回数
    private const int MAX_SHUFFLE_USES = 1;
    private const int MAX_SKIP_USES = 1;

    [Header("ターン外の透明度")]
    [SerializeField, Range(0f, 1f)]
    private float inactiveAlpha = 0.35f;

    // 既存のPlayerUIに付いているCanvasGroup
    private CanvasGroup[] playerGroups = new CanvasGroup[4];

    // 能力の使用状況
    private bool[] shuffleUsed = new bool[4];
    private bool[] skipUsed = new bool[4];

    // ランダム公開は各プレイヤー2回まで
    private int[] randomUseCount = new int[4];
    private const int MAX_RANDOM_USES = 2;

    // そのターンに能力を使用したか
    private bool[] abilityUsedThisTurn = new bool[4];

    private MemoryBurstTurnManager turnManager;
    private MemoryBurstCardManager cardManager;

    // ランダム公開が予約されているプレイヤー
    private int pendingRandomPlayer = 0;


    private int activePlayerCount = 1;

    private bool isGameOver = false;

    // =================================
    // 初期化
    // =================================

    public void Initialize(
        int playerCount,
        MemoryBurstTurnManager turn,
        MemoryBurstCardManager card)
    {
        pendingRandomPlayer = 0;
        isGameOver = false;

        shuffleUsed = new bool[4];
        skipUsed = new bool[4];
        randomUseCount = new int[4];
        abilityUsedThisTurn = new bool[4];
        activePlayerCount = Mathf.Clamp(playerCount, 1, 4);

        turnManager = turn;
        cardManager = card;

        if (turnManager == null || cardManager == null)
        {
            Debug.LogError("AbilityManager：Managerが設定されていません！");
            return;
        }

        // 既存のPlayerUIからCanvasGroupを取得
        playerGroups[0] = GetPlayerCanvasGroup(player1UI);
        playerGroups[1] = GetPlayerCanvasGroup(player2UI);
        playerGroups[2] = GetPlayerCanvasGroup(player3UI);
        playerGroups[3] = GetPlayerCanvasGroup(player4UI);

        // ボタンイベント登録
        if (shuffleButton != null)
            shuffleButton.onClick.AddListener(ShufflePlayer1);

        if (shuffleButton2 != null)
            shuffleButton2.onClick.AddListener(ShufflePlayer2);

        if (shuffleButton3 != null)
            shuffleButton3.onClick.AddListener(ShufflePlayer3);

        if (shuffleButton4 != null)
            shuffleButton4.onClick.AddListener(ShufflePlayer4);

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipPlayer1);

        if (skipButton2 != null)
            skipButton2.onClick.AddListener(SkipPlayer2);

        if (skipButton3 != null)
            skipButton3.onClick.AddListener(SkipPlayer3);

        if (skipButton4 != null)
            skipButton4.onClick.AddListener(SkipPlayer4);

        if (randomButton != null)
            randomButton.onClick.AddListener(RandomPlayer1);

        if (randomButton2 != null)
            randomButton2.onClick.AddListener(RandomPlayer2);

        if (randomButton3 != null)
            randomButton3.onClick.AddListener(RandomPlayer3);

        if (randomButton4 != null)
            randomButton4.onClick.AddListener(RandomPlayer4);

        // ターン変更イベント登録
        turnManager.OnTurnChanged += OnTurnChanged;

        // プレイヤー人数に応じたUI表示
        UpdatePlayerUI(playerCount);

        // 能力使用回数の初期表示
        UpdateAbilityCountText();

        // 初期ターンのUI表示
        UpdatePlayerAlpha(turnManager.CurrentPlayer);

        UpdateButtons();

        Debug.Log("AbilityManager：初期化完了！");
    }

    // =================================
    // PlayerUIのCanvasGroup取得
    // =================================


    private void UpdateAbilityCountText()
    {
        UpdateCountText(
            shuffleCountText, shuffleUsed[0], MAX_SHUFFLE_USES);

        UpdateCountText(
            skipCountText, skipUsed[0], MAX_SKIP_USES);

        UpdateCountText(
            randomCountText, randomUseCount[0], MAX_RANDOM_USES);


        UpdateCountText(
            shuffleCountText2, shuffleUsed[1], MAX_SHUFFLE_USES);

        UpdateCountText(
            skipCountText2, skipUsed[1], MAX_SKIP_USES);

        UpdateCountText(
            randomCountText2, randomUseCount[1], MAX_RANDOM_USES);


        UpdateCountText(
            shuffleCountText3, shuffleUsed[2], MAX_SHUFFLE_USES);

        UpdateCountText(
            skipCountText3, skipUsed[2], MAX_SKIP_USES);

        UpdateCountText(
            randomCountText3, randomUseCount[2], MAX_RANDOM_USES);


        UpdateCountText(
            shuffleCountText4, shuffleUsed[3], MAX_SHUFFLE_USES);

        UpdateCountText(
            skipCountText4, skipUsed[3], MAX_SKIP_USES);

        UpdateCountText(
            randomCountText4, randomUseCount[3], MAX_RANDOM_USES);
    }
    // =================================
    // 回数表示の更新
    // =================================
    private void UpdateCountText(
        TMP_Text text,
        bool used,
        int maxUses)
    {
        if (text == null) return;

        int remaining = maxUses - (used ? 1 : 0);

        text.text = " ×" + remaining;
    }

    private void UpdateCountText(
        TMP_Text text,
        int usedCount,
        int maxUses)
    {
        if (text == null) return;

        int remaining = maxUses - usedCount;

        text.text = " ×" + remaining;
    }

    private CanvasGroup GetPlayerCanvasGroup(GameObject playerUI)
    {
        if (playerUI == null)
        {
            Debug.LogWarning("PlayerUIが設定されていません。");
            return null;
        }

        CanvasGroup group = playerUI.GetComponent<CanvasGroup>();

        if (group == null)
        {
            Debug.LogWarning(
                playerUI.name + " にCanvasGroupがありません。"
            );
        }

        return group;
    }

    // =================================
    // ターン変更
    // =================================

    private void OnTurnChanged(int player)
    {
        if (isGameOver)
            return;

        if (player < 1 || player > 4)
            return;

        // 新しいターンなので能力使用状態をリセット
        abilityUsedThisTurn[player - 1] = false;

        // ランダム公開の発動
        if (pendingRandomPlayer == player)
        {
            pendingRandomPlayer = 0;

            Debug.Log(
                $"プレイヤー{player}のターン開始！ランダム公開を発動！"
            );

            if (!cardManager.ForceRandomReveal())
            {
                Debug.LogWarning("ランダム公開できるカードがありませんでした。");
            }
        }

        UpdateButtons();
        UpdatePlayerAlpha(player);
    }

    // =================================
    // プレイヤーUIの透明度更新
    // =================================

    private void UpdatePlayerAlpha(int currentPlayer)
    {
        for (int i = 0; i < playerGroups.Length; i++)
        {
            if (playerGroups[i] == null)
                continue;

            playerGroups[i].alpha =
                (i == currentPlayer - 1) ? 1f : inactiveAlpha;
        }
    }

    // =================================
    // ボタン状態の更新
    // =================================

    public void UpdateButtons()
    {
        if (isGameOver)
            return;

        UpdatePlayerButtons(
            1, shuffleButton, skipButton, randomButton
        );

        UpdatePlayerButtons(
            2, shuffleButton2, skipButton2, randomButton2
        );

        UpdatePlayerButtons(
            3, shuffleButton3, skipButton3, randomButton3
        );

        UpdatePlayerButtons(
            4, shuffleButton4, skipButton4, randomButton4
        );
    }

    private void UpdatePlayerButtons(
        int player,
        Button shuffle,
        Button skip,
        Button random)
    {
        bool canUse = CanUseAbility(player);

        if (shuffle != null)
            shuffle.interactable =
                canUse && !shuffleUsed[player - 1];

        if (skip != null)
            skip.interactable =
                canUse && !skipUsed[player - 1];

        if (random != null)
        {
            random.interactable =
                canUse &&
                randomUseCount[player - 1] < MAX_RANDOM_USES &&
                pendingRandomPlayer == 0;
        }
    }

    // =================================
    // 能力使用可能判定
    // =================================

    private bool CanUseAbility(int player)
    {
        if (isGameOver)
            return false;

        if (turnManager == null || cardManager == null)
            return false;

        if (player < 1 || player > 4)
            return false;

        // 自分のターン以外は使用不可
        if (turnManager.CurrentPlayer != player)
            return false;

        // カード判定中・ゲーム終了時は使用不可
        if (cardManager.IsChecking || cardManager.AllCardsMatched)
            return false;

        // このターンに能力を使用済みなら不可
        if (abilityUsedThisTurn[player - 1])
            return false;

        return true;
    }

    // =================================
    // シャッフル能力
    // =================================

    private void ShufflePlayer1() => UseShuffle(1);
    private void ShufflePlayer2() => UseShuffle(2);
    private void ShufflePlayer3() => UseShuffle(3);
    private void ShufflePlayer4() => UseShuffle(4);

    private void UseShuffle(int player)
    {
        if (!CanUseAbility(player))
            return;

        if (shuffleUsed[player - 1])
            return;

        if (!cardManager.ShuffleUnmatchedCards())
            return;

      
        shuffleUsed[player - 1] = true;
        abilityUsedThisTurn[player - 1] = true;

        

        Debug.Log($"プレイヤー{player}がシャッフル能力を使用！");
       
        UpdateAbilityCountText();

        UpdateButtons();
    }

    // =================================
    // スキップ能力
    // =================================

    private void SkipPlayer1() => UseSkip(1);
    private void SkipPlayer2() => UseSkip(2);
    private void SkipPlayer3() => UseSkip(3);
    private void SkipPlayer4() => UseSkip(4);

    private void UseSkip(int player)
    {
        if (!CanUseAbility(player))
            return;

        if (skipUsed[player - 1])
            return;

        skipUsed[player - 1] = true;
        abilityUsedThisTurn[player - 1] = true;

        // 次のプレイヤーのターンをスキップ予約
        turnManager.RequestSkipNextTurn();

        Debug.Log($"プレイヤー{player}がスキップ能力を使用！");

        UpdateAbilityCountText();


        UpdateButtons();
    }

    // =================================
    // 強制ランダム公開能力
    // =================================

    private void RandomPlayer1() => UseRandom(1);
    private void RandomPlayer2() => UseRandom(2);
    private void RandomPlayer3() => UseRandom(3);
    private void RandomPlayer4() => UseRandom(4);

    private void UseRandom(int player)
    {
        if (!CanUseAbility(player))
            return;

        // すでに予約中なら使用できない
        if (pendingRandomPlayer != 0)
            return;

        // 使用回数の確認
        if (randomUseCount[player - 1] >= MAX_RANDOM_USES)
            return;

        // 相手のターンに発動するよう予約
        int opponent = player == 1 ? 2 : 1;
        pendingRandomPlayer = opponent;

        randomUseCount[player - 1]++;
        abilityUsedThisTurn[player - 1] = true;

        Debug.Log(
            $"プレイヤー{player}がランダム公開を予約！" +
            $"プレイヤー{opponent}のターン開始時に発動！" +
            $"残り回数：{MAX_RANDOM_USES - randomUseCount[player - 1]}回"
        );

        UpdateAbilityCountText();

        UpdateButtons();
    }

    // =================================
    // プレイヤー人数に応じたUI表示
    // =================================

    private void UpdatePlayerUI(int playerCount)
    {
        SetPlayerUI(player1UI, playerCount >= 1,
            shuffleButton, skipButton, randomButton,
            shuffleCountText, skipCountText, randomCountText);

        SetPlayerUI(player2UI, playerCount >= 2,
            shuffleButton2, skipButton2, randomButton2,
            shuffleCountText2, skipCountText2, randomCountText2);

        SetPlayerUI(player3UI, playerCount >= 3,
            shuffleButton3, skipButton3, randomButton3,
            shuffleCountText3, skipCountText3, randomCountText3);

        SetPlayerUI(player4UI, playerCount >= 4,
            shuffleButton4, skipButton4, randomButton4,
            shuffleCountText4, skipCountText4, randomCountText4);
    }

    private void SetPlayerUI(
     GameObject playerUI,
     bool visible,
     Button shuffle,
     Button skip,
     Button random,
     TMP_Text shuffleText,
     TMP_Text skipText,
     TMP_Text randomText)
    {
        if (playerUI != null)
            playerUI.SetActive(visible);

        if (shuffle != null)
            shuffle.gameObject.SetActive(visible);

        if (skip != null)
            skip.gameObject.SetActive(visible);

        if (random != null)
            random.gameObject.SetActive(visible);

        // 能力回数表示も人数に合わせて切り替え
        if (shuffleText != null)
            shuffleText.gameObject.SetActive(visible);

        if (skipText != null)
            skipText.gameObject.SetActive(visible);

        if (randomText != null)
            randomText.gameObject.SetActive(visible);
    }

    // =================================
    // ゲーム終了
    // =================================

    public void SetGameOver()
    {
        isGameOver = true;
        pendingRandomPlayer = 0;

        SetAllButtonsInteractable(false);

        Debug.Log("AbilityManager：ゲーム終了。能力を無効化しました。");
    }

    private void SetAllButtonsInteractable(bool value)
    {
        if (shuffleButton != null)
            shuffleButton.interactable = value;

        if (shuffleButton2 != null)
            shuffleButton2.interactable = value;

        if (shuffleButton3 != null)
            shuffleButton3.interactable = value;

        if (shuffleButton4 != null)
            shuffleButton4.interactable = value;

        if (skipButton != null)
            skipButton.interactable = value;

        if (skipButton2 != null)
            skipButton2.interactable = value;

        if (skipButton3 != null)
            skipButton3.interactable = value;

        if (skipButton4 != null)
            skipButton4.interactable = value;

        if (randomButton != null)
            randomButton.interactable = value;

        if (randomButton2 != null)
            randomButton2.interactable = value;

        if (randomButton3 != null)
            randomButton3.interactable = value;

        if (randomButton4 != null)
            randomButton4.interactable = value;
    }

    // =================================
    // イベント解除
    // =================================

    private void OnDestroy()
    {
        if (turnManager != null)
            turnManager.OnTurnChanged -= OnTurnChanged;

        if (shuffleButton != null)
            shuffleButton.onClick.RemoveListener(ShufflePlayer1);

        if (shuffleButton2 != null)
            shuffleButton2.onClick.RemoveListener(ShufflePlayer2);

        if (shuffleButton3 != null)
            shuffleButton3.onClick.RemoveListener(ShufflePlayer3);

        if (shuffleButton4 != null)
            shuffleButton4.onClick.RemoveListener(ShufflePlayer4);

        if (skipButton != null)
            skipButton.onClick.RemoveListener(SkipPlayer1);

        if (skipButton2 != null)
            skipButton2.onClick.RemoveListener(SkipPlayer2);

        if (skipButton3 != null)
            skipButton3.onClick.RemoveListener(SkipPlayer3);

        if (skipButton4 != null)
            skipButton4.onClick.RemoveListener(SkipPlayer4);

        if (randomButton != null)
            randomButton.onClick.RemoveListener(RandomPlayer1);

        if (randomButton2 != null)
            randomButton2.onClick.RemoveListener(RandomPlayer2);

        if (randomButton3 != null)
            randomButton3.onClick.RemoveListener(RandomPlayer3);

        if (randomButton4 != null)
            randomButton4.onClick.RemoveListener(RandomPlayer4);
    }
}
