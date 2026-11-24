
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class MemoryBurstUIManager : MonoBehaviour
{
    [Header("プレイヤーUI")]
    [SerializeField] private GameObject[] playerUI = new GameObject[4];


    [Header("能力ボタン")]
    [SerializeField] private Button[] shuffleButtons = new Button[4];
    [SerializeField] private Button[] skipButtons = new Button[4];
    [SerializeField] private Button[] randomButtons = new Button[4];

    [Header("能力使用回数表示")]
    [SerializeField] private TMP_Text[] shuffleCountTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] skipCountTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] randomCountTexts = new TMP_Text[4];

    [Header("プレイヤーアイコン（人間）")]
    [SerializeField] private Sprite[] playerIcons = new Sprite[4];

    [Header("プレイヤーアイコン（CPU）")]
    [SerializeField] private Sprite[] cpuIcons = new Sprite[4];

    [Header("画面に表示するアイコン")]
    [SerializeField] private Image[] playerIconImages = new Image[4];

    [Header("UI透明度")]
    [SerializeField] private float inactiveAlpha = 0.35f;

    [Header("各プレイヤーのCanvasGroup")]
    [SerializeField] private CanvasGroup[] playerGroups = new CanvasGroup[4];

    // マネージャー
    private MemoryBurstAbilityManager abilityManager;
    private MemoryBurstTurnManager turnManager;

    // プレイヤー情報
    private int activePlayerCount = 1;
    private bool[] isCPUPlayers = new bool[4];

    private bool isGameOver;

    // ボタンイベントを保存して、自分が登録したイベントだけ解除する
    private UnityAction[] highActions = new UnityAction[4];
    private UnityAction[] lowActions = new UnityAction[4];
    private UnityAction[] shuffleActions = new UnityAction[4];
    private UnityAction[] skipActions = new UnityAction[4];
    private UnityAction[] randomActions = new UnityAction[4];

    // ==================================================
    // 初期化
    // ==================================================

    public void Initialize(
        int playerCount,
        MemoryBurstAbilityManager ability,
        MemoryBurstTurnManager turn,
        bool[] cpuPlayers = null)
    {
        isGameOver = false;

        activePlayerCount = Mathf.Clamp(playerCount, 1, 4);

        abilityManager = ability;
        turnManager = turn;

        // CPU情報を初期化
        isCPUPlayers = new bool[4];

        if (cpuPlayers != null)
        {
            for (int i = 0; i < Mathf.Min(cpuPlayers.Length, 4); i++)
            {
                isCPUPlayers[i] = cpuPlayers[i];
            }
        }

        if (abilityManager == null || turnManager == null)
        {
            Debug.LogError(
                "MBUIManager：AbilityManagerまたはTurnManagerが設定されていません。");
            return;
        }

        // Turnイベントの重複登録を防止
        turnManager.OnTurnChanged -= OnTurnChanged;
        turnManager.OnTurnChanged += OnTurnChanged;

        // スキップイベント登録
        turnManager.OnPlayerSkipped -= ShowSkippedPlayer;
        turnManager.OnPlayerSkipped += ShowSkippedPlayer;

        SetupPlayerUI();
        UpdatePlayerIcons();
        RegisterButtonEvents();

        UpdateAbilityCountText();
        UpdatePlayerAlpha(turnManager.CurrentPlayer);
        UpdateButtons();

        Debug.Log("MBUIManager：初期化完了");
    }

    // ==================================================
    // プレイヤーUIの表示設定
    // ==================================================

    private void SetupPlayerUI()
    {
        for (int i = 0; i < 4; i++)
        {
            bool active = i < activePlayerCount;

            // プレイヤーUI表示
            if (playerUI[i] != null)
            {
                playerUI[i].SetActive(active);
            }

            // CanvasGroup取得・設定
            if (playerGroups[i] == null && playerUI[i] != null)
            {
                playerGroups[i] =
                    playerUI[i].GetComponent<CanvasGroup>();

                if (playerGroups[i] == null)
                {
                    playerGroups[i] =
                        playerUI[i].AddComponent<CanvasGroup>();
                }
            }

          

            // 能力ボタン
            if (shuffleButtons[i] != null)
                shuffleButtons[i].gameObject.SetActive(active);

            if (skipButtons[i] != null)
                skipButtons[i].gameObject.SetActive(active);

            if (randomButtons[i] != null)
                randomButtons[i].gameObject.SetActive(active);

            // 使用回数表示
            if (shuffleCountTexts[i] != null)
                shuffleCountTexts[i].gameObject.SetActive(active);

            if (skipCountTexts[i] != null)
                skipCountTexts[i].gameObject.SetActive(active);

            if (randomCountTexts[i] != null)
                randomCountTexts[i].gameObject.SetActive(active);

            // アイコン
            if (playerIconImages[i] != null)
                playerIconImages[i].gameObject.SetActive(active);

            // スキップマーク
            if (skipMarkTexts[i] != null)
            {
                skipMarkTexts[i].gameObject.SetActive(false);
                skipMarkTexts[i].text = "×";
                skipMarkTexts[i].color = Color.red;
                skipMarkTexts[i].rectTransform.localScale =
                    Vector3.one;
            }
        }
    }

    // ==================================================
    // Player / CPU アイコン切り替え
    // ==================================================

    public void UpdatePlayerIcons()
    {
        for (int i = 0; i < activePlayerCount; i++)
        {
            if (playerIconImages[i] == null)
                continue;

            Sprite targetSprite = isCPUPlayers[i]
                ? cpuIcons[i]
                : playerIcons[i];

            if (targetSprite == null)
                continue;

            playerIconImages[i].sprite = targetSprite;
        }
    }

    // CPU設定を変更したいときに呼ぶ
    public void SetCPUPlayer(int playerNumber, bool isCPU)
    {
        int index = playerNumber - 1;

        if (index < 0 || index >= 4)
            return;

        isCPUPlayers[index] = isCPU;

        UpdatePlayerIcons();
        UpdateButtons();
    }

    // ==================================================
    // ボタン登録（HIGH / LOW / 能力）
    // ==================================================

    private void RegisterButtonEvents()
    {
        for (int i = 0; i < 4; i++)
        {
            int player = i + 1;

            // HIGH（次のカードを引いてHIGH判定）
            if (highButtons[i] != null)
            {
                if (highActions[i] != null)
                {
                    highButtons[i].onClick.RemoveListener(highActions[i]);
                }

                highActions[i] = () =>
                {
                    OnDrawNextCard(player, isHigh: true);
                };

                highButtons[i].onClick.AddListener(highActions[i]);
            }

            // LOW（次のカードを引いてLOW判定）
            if (lowButtons[i] != null)
            {
                if (lowActions[i] != null)
                {
                    lowButtons[i].onClick.RemoveListener(lowActions[i]);
                }

                lowActions[i] = () =>
                {
                    OnDrawNextCard(player, isHigh: false);
                };

                lowButtons[i].onClick.AddListener(lowActions[i]);
            }

            // Shuffle
            if (shuffleButtons[i] != null)
            {
                if (shuffleActions[i] != null)
                    shuffleButtons[i].onClick.RemoveListener(shuffleActions[i]);

                shuffleActions[i] = () =>
                {
                    if (abilityManager == null)
                        return;

                    if (!abilityManager.CanUseShuffle(player))
                        return;

                    abilityManager.UseShuffle(player);
                    RefreshUI();
                };

                shuffleButtons[i].onClick.AddListener(shuffleActions[i]);
            }

            // Skip
            if (skipButtons[i] != null)
            {
                if (skipActions[i] != null)
                    skipButtons[i].onClick.RemoveListener(skipActions[i]);

                skipActions[i] = () =>
                {
                    if (abilityManager == null)
                        return;

                    if (!abilityManager.CanUseSkip(player))
                        return;

                    abilityManager.UseSkip(player);
                    RefreshUI();
                };

                skipButtons[i].onClick.AddListener(skipActions[i]);
            }

            // Random
            if (randomButtons[i] != null)
            {
                if (randomActions[i] != null)
                    randomButtons[i].onClick.RemoveListener(randomActions[i]);

                randomActions[i] = () =>
                {
                    if (abilityManager == null)
                        return;

                    if (!abilityManager.CanUseRandom(player))
                        return;

                    abilityManager.UseRandom(player);
                    RefreshUI();
                };

                randomButtons[i].onClick.AddListener(randomActions[i]);
            }
        }
    }

    // 次のカードを引く処理
    private void OnDrawNextCard(int player, bool isHigh)
    {
        if (isGameOver || turnManager == null)
            return;

        // 現在の手番プレイヤー以外からの入力をガード
        if (turnManager.CurrentPlayer != player || isCPUPlayers[player - 1])
            return;

        // HIGH / LOW の既存処理
        Debug.Log(
            $"Player {player} が {(isHigh ? "HIGH" : "LOW")} を選択し、次のカードを引きます");

        RefreshUI();
    }

    // ==================================================
    // ターン変更時
    // ==================================================

    private void OnTurnChanged(int currentPlayer)
    {
        if (isGameOver)
            return;

        UpdatePlayerAlpha(currentPlayer);
        UpdateButtons();
        UpdateAbilityCountText();
    }

    // ==================================================
    // プレイヤーの透明度切り替え
    // ==================================================

    private void UpdatePlayerAlpha(int currentPlayer)
    {
        for (int i = 0; i < activePlayerCount; i++)
        {
            if (playerGroups[i] == null)
                continue;

            bool isActivePlayer = (i + 1) == currentPlayer;

            playerGroups[i].alpha = isActivePlayer
                ? 1f
                : inactiveAlpha;
        }
    }

    // ==================================================
    // 能力使用回数表示更新
    // ==================================================

    private void UpdateAbilityCountText()
    {
        if (abilityManager == null)
            return;

        for (int i = 0; i < activePlayerCount; i++)
        {
            int player = i + 1;

            if (shuffleCountTexts[i] != null)
            {
                shuffleCountTexts[i].text =
                    "×" + abilityManager.GetShuffleRemaining(player);
            }

            if (skipCountTexts[i] != null)
            {
                skipCountTexts[i].text =
                    "×" + abilityManager.GetSkipRemaining(player);
            }

            if (randomCountTexts[i] != null)
            {
                randomCountTexts[i].text =
                    "×" + abilityManager.GetRandomRemaining(player);
            }
        }
    }

    // ==================================================
    // ボタン使用可否更新
    // ==================================================

    private void UpdateButtons()
    {
        if (abilityManager == null)
            return;

        int currentPlayer =
            turnManager != null ? turnManager.CurrentPlayer : 1;

        for (int i = 0; i < 4; i++)
        {
            int player = i + 1;

            bool active = i < activePlayerCount && !isGameOver;

            bool isMyTurn =
                active &&
                player == currentPlayer &&
                !isCPUPlayers[i];

            // HIGH / LOW は自分のターン＆人間プレイヤーのみ押せるようにする
            if (highButtons[i] != null)
                highButtons[i].interactable = isMyTurn;

            if (lowButtons[i] != null)
                lowButtons[i].interactable = isMyTurn;

            if (shuffleButtons[i] != null)
            {
                shuffleButtons[i].interactable =
                    isMyTurn && abilityManager.CanUseShuffle(player);
            }

            if (skipButtons[i] != null)
            {
                skipButtons[i].interactable =
                    isMyTurn && abilityManager.CanUseSkip(player);
            }

            if (randomButtons[i] != null)
            {
                randomButtons[i].interactable =
                    isMyTurn && abilityManager.CanUseRandom(player);
            }
        }
    }

    // ==================================================
    // UI全体更新
    // ==================================================

    public void RefreshUI()
    {
        if (isGameOver)
            return;

        UpdateAbilityCountText();

        if (turnManager != null)
            UpdatePlayerAlpha(turnManager.CurrentPlayer);

        UpdateButtons();
    }

    // ==================================================
    // スキップされたプレイヤーの演出
    // ==================================================

    private void ShowSkippedPlayer(int playerNumber)
    {
        int index = playerNumber - 1;

        if (index < 0 ||
            index >= activePlayerCount ||
            skipMarkTexts[index] == null)
        {
            return;
        }

        // 同じプレイヤーの演出が再生中なら停止
        if (skipMarkCoroutines[index] != null)
        {
            StopCoroutine(skipMarkCoroutines[index]);
            skipMarkCoroutines[index] = null;
        }

        skipMarkCoroutines[index] =
            StartCoroutine(SkipMarkAnimation(index));
    }

    private IEnumerator SkipMarkAnimation(int index)
    {
        TMP_Text mark = skipMarkTexts[index];

        if (mark == null)
            yield break;

        mark.gameObject.SetActive(true);

        RectTransform rect = mark.rectTransform;

        Color originalColor = Color.red;
        mark.color = originalColor;
        mark.text = "×";

        // 最初は小さくする
        rect.localScale = Vector3.zero;

        // 拡大表示
        float showTime = 0.2f;
        float elapsed = 0f;

        while (elapsed < showTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / showTime);

            rect.localScale = Vector3.Lerp(
                Vector3.zero,
                Vector3.one * 1.3f,
                t);

            yield return null;
        }

        // 少し縮める
        float shrinkTime = 0.1f;
        elapsed = 0f;

        while (elapsed < shrinkTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / shrinkTime);

            rect.localScale = Vector3.Lerp(
                Vector3.one * 1.3f,
                Vector3.one,
                t);

            yield return null;
        }

        // 表示時間
        float fadeTime = Mathf.Max(0.1f, skipMarkDuration);
        elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;

            float alpha =
                1f - Mathf.Clamp01(elapsed / fadeTime);

            mark.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                alpha);

            yield return null;
        }

        // 非表示に戻す
        mark.gameObject.SetActive(false);

        rect.localScale = Vector3.one;
        mark.color = originalColor;

        skipMarkCoroutines[index] = null;
    }

    // ==================================================
    // ゲーム終了
    // ==================================================

    public void SetGameOver()
    {
        isGameOver = true;

        for (int i = 0; i < 4; i++)
        {
           

           if (shuffleButtons[i] != null)
                shuffleButtons[i].interactable = false;

            if (skipButtons[i] != null)
                skipButtons[i].interactable = false;

            if (randomButtons[i] != null)
                randomButtons[i].interactable = false;

            // 演出停止
            if (skipMarkCoroutines[i] != null)
            {
                StopCoroutine(skipMarkCoroutines[i]);
                skipMarkCoroutines[i] = null;
            }

            if (skipMarkTexts[i] != null)
            {
                skipMarkTexts[i].gameObject.SetActive(false);
            }
        }
    }

    // ==================================================
    // イベント解除
    // ==================================================

    private void OnDestroy()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnChanged -= OnTurnChanged;
            turnManager.OnPlayerSkipped -= ShowSkippedPlayer;
        }

        for (int i = 0; i < 4; i++)
        {
           

            if (shuffleButtons[i] != null && shuffleActions[i] != null)
                shuffleButtons[i].onClick.RemoveListener(shuffleActions[i]);

            if (skipButtons[i] != null && skipActions[i] != null)
                skipButtons[i].onClick.RemoveListener(skipActions[i]);

            if (randomButtons[i] != null && randomActions[i] != null)
                randomButtons[i].onClick.RemoveListener(randomActions[i]);
        }
    }
}
