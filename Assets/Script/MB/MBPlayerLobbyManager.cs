
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MBPlayerLobbyManager : MonoBehaviour
{
    [Header("2P～4Pの選択ボタン")]
    [SerializeField] private Button[] playerButtons;
    [SerializeField] private Button[] cpuButtons;

    [Header("2P～4Pの表示テキスト")]
    [SerializeField] private TextMeshProUGUI[] slotTexts;

    [Header("CPUごとの難易度Dropdown")]
    [SerializeField] private TMP_Dropdown[] cpuLevelDropdowns;

    [Header("人数表示")]
    [SerializeField] private TextMeshProUGUI playerCountText;

    [Header("2P～4Pの解除ボタン")]
    [SerializeField] private Button[] deselectButtons;

    [Header("プレイヤーキャラクター画像")]
    [SerializeField] private Image[] characterImages;

    // 人間プレイヤー用
    [SerializeField] private Sprite[] characterSprites;

    // CPU専用画像
    [SerializeField] private Sprite[] cpuSprites;

    [Header("開始ボタン")]
    [SerializeField] private Button startButton;

    [Header("戻るボタン")]
    [SerializeField] private Button backButton;

    private bool[] isSelected = new bool[4];
    private bool[] isCPU = new bool[4];

    private void Start()
    {
        // 1Pは最初から人間プレイヤー
        isSelected[0] = true;
        isCPU[0] = false;

        // 2P～4Pのボタン登録
        for (int i = 0; i < 3; i++)
        {
            int slot = i + 1;

            playerButtons[i].onClick.AddListener(
                () => SelectPlayer(slot)
            );

            cpuButtons[i].onClick.AddListener(
                () => SelectCPU(slot)
            );

            deselectButtons[i].onClick.AddListener(
                () => DeselectPlayer(slot)
            );

            // 難易度Dropdownの初期設定
            if (cpuLevelDropdowns[i] != null)
            {
                cpuLevelDropdowns[i].ClearOptions();
                cpuLevelDropdowns[i].AddOptions(
                    new System.Collections.Generic.List<string>
                    {
                        "Easy",
                        "Normal",
                        "Hard"
                    }
                );

                cpuLevelDropdowns[i].value = 1;
                cpuLevelDropdowns[i].gameObject.SetActive(false);
            }
        }

        // 開始ボタンの登録
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }

        // 戻るボタンの登録
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToTitle);
        }

        // キャラクター画像の初期設定
        for (int i = 0; i < 3; i++)
        {
            if (characterImages[i] != null)
            {
                characterImages[i].gameObject.SetActive(false);
            }
        }

        UpdateUI();
    }

    // 人間プレイヤーを選択
    public void SelectPlayer(int slot)
    {
        if (!CanSelect(slot))
            return;

        isSelected[slot] = true;
        isCPU[slot] = false;

        UpdateUI();
    }

    // CPUを選択
    public void SelectCPU(int slot)
    {
        if (!CanSelect(slot))
            return;

        isSelected[slot] = true;
        isCPU[slot] = true;

        UpdateUI();
    }

    // 選択できるか確認
    private bool CanSelect(int slot)
    {
        if (slot < 1 || slot > 3)
            return false;

        if (slot >= 2 && !isSelected[slot - 1])
            return false;

        return true;
    }

    // 表示更新

    private void UpdateUI()
    {
        int humanCount = 0;
        int cpuCount = 0;

        for (int i = 0; i < 4; i++)
        {
            if (isSelected[i])
            {
                if (isCPU[i])
                    cpuCount++;
                else
                    humanCount++;
            }

            if (i > 0)
            {
                // プレイヤー状態の表示
                // 未選択のときだけSlot Textを表示
                if (slotTexts[i - 1] != null)
                {
                    slotTexts[i - 1].gameObject.SetActive(!isSelected[i]);

                    slotTexts[i - 1].text = $"{i + 1}P：未選択";
                }

                // CPUを選択した枠だけ難易度を表示
                if (cpuLevelDropdowns[i - 1] != null)
                {
                    cpuLevelDropdowns[i - 1]
                        .gameObject.SetActive(
                            isSelected[i] && isCPU[i]
                        );
                }

                // キャラクター画像の表示
                if (characterImages[i - 1] != null)
                {
                    Image characterImage = characterImages[i - 1];

                    // 選択されている場合だけ画像を表示
                    characterImage.gameObject.SetActive(
                        isSelected[i]
                    );

                    if (isSelected[i])
                    {
                        // CPUの場合
                        if (isCPU[i])
                        {
                            characterImage.sprite = cpuSprites[i];
                        }
                        // 人間プレイヤーの場合
                        else
                        {
                            characterImage.sprite =
                                characterSprites[i];
                        }
                    }
                }

                // 追加ボタンの表示切り替え
                playerButtons[i - 1].gameObject.SetActive(
                    !isSelected[i]
                );

                cpuButtons[i - 1].gameObject.SetActive(
                    !isSelected[i]
                );

                // 解除ボタンの表示切り替え
                deselectButtons[i - 1].gameObject.SetActive(
                    isSelected[i]
                );
            }
        }

        // GameSettingsに保存
        GameSettings.HumanPlayerCount = humanCount;
        GameSettings.CPUCount = cpuCount;

        GameSettings.IsCPU = (bool[])isCPU.Clone();
        GameSettings.IsActive = (bool[])isSelected.Clone();

        // 参加人数の表示
        if (playerCountText != null)
        {
            playerCountText.text =
                $"参加人数：{humanCount + cpuCount} / 4";
        }

        // 開始ボタン
        if (startButton != null)
            startButton.interactable = true;
    }


    // プレイヤーの選択を解除
    public void DeselectPlayer(int slot)
    {
        if (slot < 1 || slot > 3)
            return;

        // 指定した枠と、それより後ろの枠を解除
        for (int i = slot; i < 4; i++)
        {
            isSelected[i] = false;
            isCPU[i] = false;
        }

        UpdateUI();
    }

    // ゲーム開始
    public void StartGame()
    {
        for (int i = 1; i < 4; i++)
        {
            if (isCPU[i] && cpuLevelDropdowns[i - 1] != null)
            {
                GameSettings.CPULevels[i] =
                    cpuLevelDropdowns[i - 1].value + 1;
            }
        }

        SceneManager.LoadScene("MemoryBurst");
    }

    // タイトル画面に戻る
    public void BackToTitle()
    {
        SceneManager.LoadScene("GameSelect");
    }
}
