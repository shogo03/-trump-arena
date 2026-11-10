using UnityEngine;

public class StoreUI : MonoBehaviour
{
    public GameObject storePanel;
    public MoneyManager moneyManager;
    public BJdraw bjdraw;

    public void OpenStore()
    {
        int player = bjdraw.GetCurrentTurn();

        moneyManager.SetShopPlayer(player);
        storePanel.SetActive(true);
    }

    public void CloseStore()
    {
        storePanel.SetActive(false);
    }
}