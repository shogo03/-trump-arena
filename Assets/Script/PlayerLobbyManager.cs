using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLobbyManager : MonoBehaviour
{
    [Header("CPU追加ボタン（2～4枠目）")]
    [SerializeField] private Button[] cpuAddButtons;

    [Header("CPU削除ボタン（2～4枠目）")]
    [SerializeField] private Button[] cpuRemoveButtons;

    [Header("空き枠の表示")]
    [SerializeField] private GameObject[] emptyObjects;

    [Header("CPU表示")]
    [SerializeField] private GameObject[] cpuObjects;

    [Header("CPUレベル")]
    [SerializeField] private TMP_Dropdown cpuLevelDropdown;

    [Header("人数表示")]
    [SerializeField] private TextMeshProUGUI playerCountText;

    private bool[] cpuSlots = new bool[4];

    private void Start()
    {
        // 初期状態は自分だけ
        for (int i = 0; i < 4; i++)
        {
            cpuSlots[i] = false;
        }

        // ボタン登録
        for (int i = 0; i < 3; i++)
        {
            int slot = i + 1;

            if (cpuAddButtons[i] != null)
            {
                cpuAddButtons[i].onClick.AddListener(
                    () => AddCPU(slot)
                );
            }

            if (cpuRemoveButtons[i] != null)
            {
                cpuRemoveButtons[i].onClick.AddListener(
                    () => RemoveCPU(slot)
                );
            }
        }

        UpdateUI();
    }

    // CPU追加
    public void AddCPU(int slot)
    {
        if (slot < 1 || slot > 3)
            return;

        if (cpuSlots[slot])
            return;

        cpuSlots[slot] = true;

        UpdateUI();
    }

    // CPU削除
    public void RemoveCPU(int slot)
    {
        if (slot < 1 || slot > 3)
            return;

        cpuSlots[slot] = false;

        UpdateUI();
    }

    // 画面更新
    private void UpdateUI()
    {
        int cpuCount = 0;

        for (int i = 1; i < 4; i++)
        {
            if (cpuSlots[i])
                cpuCount++;

            if (emptyObjects[i - 1] != null)
                emptyObjects[i - 1].SetActive(!cpuSlots[i]);

            if (cpuObjects[i - 1] != null)
                cpuObjects[i - 1].SetActive(cpuSlots[i]);

            if (cpuAddButtons[i - 1] != null)
                cpuAddButtons[i - 1].gameObject.SetActive(!cpuSlots[i]);

            if (cpuRemoveButtons[i - 1] != null)
                cpuRemoveButtons[i - 1].gameObject.SetActive(cpuSlots[i]);
        }

        GameSettings.HumanPlayerCount = 1;
        GameSettings.CPUCount = cpuCount;

        if (playerCountText != null)
        {
            playerCountText.text =
                $"参加人数：{1 + cpuCount} / 4";
        }
    }

    // ゲーム開始
    public void StartGame()
    {
        if (GameSettings.TotalPlayerCount < 1)
            return;

        GameSettings.IsCPU = (bool[])cpuSlots.Clone();

        if (cpuLevelDropdown != null)
            GameSettings.CPULevel = cpuLevelDropdown.value + 1;

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            "MemoryBurst"
        );
    }
}