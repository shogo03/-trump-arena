
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerSelectManager : MonoBehaviour
{
    [Header("人数設定")]
    [SerializeField] private TMP_Dropdown humanDropdown;
    [SerializeField] private TMP_Dropdown cpuDropdown;

    [Header("CPUごとの難易度")]
    [SerializeField] private TMP_Dropdown[] levelDropdowns;

    [Header("表示")]
    [SerializeField] private TextMeshProUGUI totalText;

    [Header("ボタン")]
    [SerializeField] private Button startButton;

    [Header("シーン")]
    [SerializeField] private string gameSceneName = "MemoryBurst";

    private void Start()
    {
        // 人間人数：1～4人
        humanDropdown.ClearOptions();
        humanDropdown.AddOptions(
            new List<string> { "1人", "2人", "3人", "4人" }
        );

        // CPU人数：0～3人
        cpuDropdown.ClearOptions();
        cpuDropdown.AddOptions(
            new List<string> { "0人", "1人", "2人", "3人" }
        );

        // CPUごとの難易度
        foreach (TMP_Dropdown dropdown in levelDropdowns)
        {
            if (dropdown == null) continue;

            dropdown.ClearOptions();
            dropdown.AddOptions(
                new List<string>
                {
                    "かんたん",
                    "ふつう",
                    "むずかしい"
                }
            );

            dropdown.value = 1;
        }

        humanDropdown.value = 0;
        cpuDropdown.value = 0;

        humanDropdown.onValueChanged.AddListener(OnSettingChanged);
        cpuDropdown.onValueChanged.AddListener(OnSettingChanged);

        foreach (TMP_Dropdown dropdown in levelDropdowns)
        {
            if (dropdown != null)
                dropdown.onValueChanged.AddListener(OnSettingChanged);
        }

        UpdateSettings();
    }

    private void OnSettingChanged(int value)
    {
        UpdateSettings();
    }

    private void UpdateSettings()
    {
        int humanCount = humanDropdown.value + 1;

        int maxCPU = 4 - humanCount;

        int previousCPU = cpuDropdown.value;

        cpuDropdown.ClearOptions();

        List<string> cpuOptions = new List<string>();

        for (int i = 0; i <= maxCPU; i++)
        {
            cpuOptions.Add(i + "人");
        }

        cpuDropdown.AddOptions(cpuOptions);

        cpuDropdown.value = Mathf.Clamp(
            previousCPU,
            0,
            maxCPU
        );

        int cpuCount = cpuDropdown.value;

        GameSettings.HumanPlayerCount = humanCount;
        GameSettings.CPUCount = cpuCount;

        // プレイヤー構成を初期化
        for (int i = 0; i < 4; i++)
        {
            GameSettings.IsActive[i] = false;
            GameSettings.IsCPU[i] = false;
        }

        // 人間プレイヤーを先に配置
        for (int i = 0; i < humanCount; i++)
        {
            GameSettings.IsActive[i] = true;
        }

        // CPUを人間の後ろに配置
        for (int i = 0; i < cpuCount; i++)
        {
            int index = humanCount + i;

            GameSettings.IsActive[index] = true;
            GameSettings.IsCPU[index] = true;

            if (i < levelDropdowns.Length &&
                levelDropdowns[i] != null)
            {
                GameSettings.CPULevels[index] =
                    levelDropdowns[i].value + 1;
            }
        }

        // CPU難易度Dropdownの表示切り替え
        for (int i = 0; i < levelDropdowns.Length; i++)
        {
            if (levelDropdowns[i] != null)
            {
                levelDropdowns[i].gameObject.SetActive(i < cpuCount);
            }
        }

        int total = humanCount + cpuCount;

        if (totalText != null)
        {
            totalText.text = $"合計人数：{total} / 4人";
        }

        if (startButton != null)
        {
            startButton.interactable = total <= 4;
        }
    }

    public void StartGame()
    {
        UpdateSettings();
        SceneManager.LoadScene(gameSceneName);
    }

    public void BackToGameSelect()
    {
        SceneManager.LoadScene("GameSelect");
    }

    private void OnDestroy()
    {
        if (humanDropdown != null)
            humanDropdown.onValueChanged.RemoveListener(OnSettingChanged);

        if (cpuDropdown != null)
            cpuDropdown.onValueChanged.RemoveListener(OnSettingChanged);

        foreach (TMP_Dropdown dropdown in levelDropdowns)
        {
            if (dropdown != null)
                dropdown.onValueChanged.RemoveListener(OnSettingChanged);
        }
    }
}
