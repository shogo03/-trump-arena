using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BJAbilityUI : MonoBehaviour
{
    [Header("管理")]
    public BJAbilityManager abilityManager;
    public BJTurnManager turnManager;


    [Header("能力UI")]
    public GameObject[] abilityUI = new GameObject[6];

    [Header("所持数・レベル表示")]
    public TMP_Text[] countTexts = new TMP_Text[6];

    [Header("発動ボタン")]
    public Button redrawButton;
    public Button forceRedrawButton;


    void Start()
    {
        UpdateAbilityUI();
    }


    void Update()
    {
        // ターンが変わったら表示を更新
        UpdateAbilityUI();
    }


    public void UpdateAbilityUI()
    {
        if (abilityManager == null || turnManager == null)
            return;

        int player = turnManager.GetCurrentTurn();

        // 全部いったん非表示
        for (int i = 0; i < abilityUI.Length; i++)
        {
            if (abilityUI[i] != null)
                abilityUI[i].SetActive(false);
        }

        // ディーラーのターンなど
        if (player < 1 || player > 3)
            return;

        int index = player - 1;


        // abi1：かけ金UP

        if (abilityManager.betLimitLevel[index] > 0)
        {
            ShowAbility(
                0,
                "Lv." + abilityManager.betLimitLevel[index]
            );
        }


        // abi2：勝ち金UP

        if (abilityManager.winMoneyLevel[index] > 0)
        {
            ShowAbility(
                1,
                "Lv." + abilityManager.winMoneyLevel[index]
            );
        }


        // abi3：バースト無効

        if (abilityManager.bustGuardCount[index] > 0)
        {
            ShowAbility(
                2,
                "×" + abilityManager.bustGuardCount[index]
            );
        }


        // abi4：引き直し

        if (abilityManager.hasRedraw[index])
        {
            ShowAbility(3, "×1");

            if (redrawButton != null)
                redrawButton.gameObject.SetActive(true);
        }
        else
        {
            if (redrawButton != null)
                redrawButton.gameObject.SetActive(false);
        }


        // abi5：強制1枚

        if (abilityManager.hasForceRedraw[index])
        {
            ShowAbility(4, "×1");

            if (forceRedrawButton != null)
                forceRedrawButton.gameObject.SetActive(true);
        }
        else
        {
            if (forceRedrawButton != null)
                forceRedrawButton.gameObject.SetActive(false);
        }


        // abi6：17サーチ

        if (abilityManager.hasDealerInfo[index])
        {
            ShowAbility(5, "×1");
        }
    }


    void ShowAbility(int index, string count)
    {
        if (index < 0 || index >= abilityUI.Length)
            return;

        if (abilityUI[index] != null)
            abilityUI[index].SetActive(true);

        if (countTexts[index] != null)
            countTexts[index].text = count;
    }
}