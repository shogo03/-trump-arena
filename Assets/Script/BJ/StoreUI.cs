using UnityEngine;

public class StoreUI : MonoBehaviour
{
    public GameObject storePanel;

    // ストアを開いている間に無効にするゲームUI
    public GameObject gameUI;

    public GameObject moneyText1;
    public GameObject moneyText2;
    public GameObject moneyText3;


    public void OpenStore()
    {
        // ストア表示
        storePanel.SetActive(true);

        // ゲーム側UIを非表示
        if (gameUI != null)
            gameUI.SetActive(false);

        // 所持金表示も非表示
        if (moneyText1 != null)
            moneyText1.SetActive(false);

        if (moneyText2 != null)
            moneyText2.SetActive(false);

        if (moneyText3 != null)
            moneyText3.SetActive(false);
    }


    public void CloseStore()
    {
        // ストア非表示
        storePanel.SetActive(false);

        // ゲーム側UIを再表示
        if (gameUI != null)
            gameUI.SetActive(true);

        // 所持金表示も再表示
        if (moneyText1 != null)
            moneyText1.SetActive(true);

        if (moneyText2 != null)
            moneyText2.SetActive(true);

        if (moneyText3 != null)
            moneyText3.SetActive(true);
    }
}