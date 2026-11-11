
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BJPlayerLobbyManager : MonoBehaviour
{
    [Header("2P～3Pの選択ボタン")]
    [SerializeField] private Button[] playerButtons;
    [SerializeField] private Button[] cpuButtons;

    [Header("2P～3Pの表示テキスト")]
    [SerializeField] private TextMeshProUGUI[] slotTexts;

    [Header("CPUごとの難易度Dropdown")]
    [SerializeField] private TMP_Dropdown[] cpuLevelDropdowns;

    [Header("人数表示")]
    [SerializeField] private TextMeshProUGUI playerCountText;

    [Header("2P～3Pの解除ボタン")]
    [SerializeField] private Button[] deselectButtons;

    [Header("開始ボタン")]
    [SerializeField] private Button startButton;

    private bool[] isSelected = new bool[3];
    private bool[] isCPU = new bool[3];

    private void Start()
    {
        // 1Pは最初から人間プレイヤー
        isSelected[0] = true;
        isCPU[0] = false;

        // 2P～4Pのボタン登録
        for (int i = 0; i < 2; i++)
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

        for (int i = 0; i < 3; i++)
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
                slotTexts[i - 1].text =
                    !isSelected[i]
                    ? $"{i + 1}P：未選択"
                    : isCPU[i]
                        ? $"{i + 1}P：CPU"
                        : $"{i + 1}P：プレイヤー";

                // CPUを選択した枠だけ難易度を表示
                if (cpuLevelDropdowns[i - 1] != null)
                {
                    cpuLevelDropdowns[i - 1]
                        .gameObject.SetActive(
                            isSelected[i] && isCPU[i]
                        );
                }
            }
        }

        GameSettings.HumanPlayerCount = humanCount;
        GameSettings.CPUCount = cpuCount;

        GameSettings.IsCPU = (bool[])isCPU.Clone();
        GameSettings.IsActive = (bool[])isSelected.Clone();

        if (playerCountText != null)
        {
            playerCountText.text =
                $"参加人数：{humanCount + cpuCount} / 3";
        }

        if (startButton != null)
            startButton.interactable = true;
    }

    // プレイヤーの選択を解除
    public void DeselectPlayer(int slot)
    {
        if (slot < 1 || slot > 2)
            return;

        // 指定した枠と、それより後ろの枠を解除
        for (int i = slot; i < 3; i++)
        {
            isSelected[i] = false;
            isCPU[i] = false;
        }

        UpdateUI();
    }

    // ゲーム開始
    public void StartGame()
    {
        for (int i = 1; i < 3; i++)
        {
            if (isCPU[i] && cpuLevelDropdowns[i - 1] != null)
            {
                GameSettings.CPULevels[i] =
                    cpuLevelDropdowns[i - 1].value + 1;
            }
        }

        SceneManager.LoadScene("blackjack");
    }
}
