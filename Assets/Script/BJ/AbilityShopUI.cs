using UnityEngine;
using UnityEngine.UI;

public class AbilityShopUI : MonoBehaviour
{
    [Header("管理")]
    public MoneyManager moneyManager;

    [Header("かけ金UP")]
    public Image betUpImage;
    public Sprite betUpNormalSprite;
    public Sprite betUpGraySprite;

    [Header("勝ち金UP")]
    public Image winUpImage;
    public Sprite winUpNormalSprite;
    public Sprite winUpGraySprite;


    void Start()
    {
        UpdateShopImages();
    }


    public void UpdateShopImages()
    {
        if (moneyManager == null)
            return;


        // かけ金UP
        // 最大Lv.10

        if (betUpImage != null)
        {
            if (GetCurrentBetLevel() >= 10)
            {
                betUpImage.sprite = betUpGraySprite;
            }
            else
            {
                betUpImage.sprite = betUpNormalSprite;
            }
        }


        // 勝ち金UP
        // 最大Lv.3

        if (winUpImage != null)
        {
            if (GetCurrentWinLevel() >= 3)
            {
                winUpImage.sprite = winUpGraySprite;
            }
            else
            {
                winUpImage.sprite = winUpNormalSprite;
            }
        }
    }


    int GetCurrentBetLevel()
    {
        int player = moneyManager.currentShopPlayer;

        if (player == 1)
            return moneyManager.betUpStage1;

        if (player == 2)
            return moneyManager.betUpStage2;

        if (player == 3)
            return moneyManager.betUpStage3;

        return 0;
    }


    int GetCurrentWinLevel()
    {
        int player = moneyManager.currentShopPlayer;

        if (player == 1)
            return moneyManager.winStage1;

        if (player == 2)
            return moneyManager.winStage2;

        if (player == 3)
            return moneyManager.winStage3;

        return 0;
    }
}