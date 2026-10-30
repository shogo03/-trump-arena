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

    [Header("CPU難易度")]
    [SerializeField] private TMP_Dropdown levelDropdown;

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

        // CPU難易度
        levelDropdown.ClearOptions();
        levelDropdown.AddOptions(
            new List<string>
            {
                "かんたん",
                "ふつう",
                "むずかしい"
            }
        );

        humanDropdown.value = 0;
        cpuDropdown.value = 0;
        levelDropdown.value = 0;

        // 選択変更時の処理
        humanDropdown.onValueChanged.AddListener(OnSettingChanged);
        cpuDropdown.onValueChanged.AddListener(OnSettingChanged);
        levelDropdown.onValueChanged.AddListener(OnSettingChanged);

        UpdateSettings();
    }

    private void OnSettingChanged(int value)
    {
        UpdateSettings();
    }

    private void UpdateSettings()
    {
        int humanCount = humanDropdown.value + 1;

        // 最大4人になるようCPU人数を制限
        int maxCPU = 4 - humanCount;

        cpuDropdown.ClearOptions();

        List<string> cpuOptions = new List<string>();

        for (int i = 0; i <= maxCPU; i++)
        {
            cpuOptions.Add(i + "人");
        }

        cpuDropdown.AddOptions(cpuOptions);

        // CPU人数が上限を超えないようにする
        cpuDropdown.value = Mathf.Clamp(
            cpuDropdown.value,
            0,
            maxCPU
        );

        int cpuCount = cpuDropdown.value;

        // 設定を保存
        GameSettings.HumanPlayerCount = humanCount;
        GameSettings.CPUCount = cpuCount;
        GameSettings.CPULevel = levelDropdown.value + 1;

        int total = humanCount + cpuCount;

        if (totalText != null)
        {
            totalText.text =
                $"合計人数：{total} / 4人";
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
        humanDropdown.onValueChanged.RemoveListener(OnSettingChanged);
        cpuDropdown.onValueChanged.RemoveListener(OnSettingChanged);
        levelDropdown.onValueChanged.RemoveListener(OnSettingChanged);
    }
}