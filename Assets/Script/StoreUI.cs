using UnityEngine;

public class StoreUI : MonoBehaviour
{
    public GameObject storePanel;

    public void OpenStore()
    {
        storePanel.SetActive(true);
    }

    public void CloseStore()
    {
        storePanel.SetActive(false);
    }
}