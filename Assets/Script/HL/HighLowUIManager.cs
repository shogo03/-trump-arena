
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighLowUIManager : MonoBehaviour
{
    [Header("背景")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite normalBackground;
    [SerializeField] private Sprite revolutionBackground;

    [Header("革命表示")]
    [SerializeField] private GameObject revolutionPanel;
    [SerializeField] private TextMeshProUGUI remainingTurnText;

    // 革命状態の変更
    public void SetRevolutionUI(bool active, int remainingTurns)
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = active
                ? revolutionBackground
                : normalBackground;
        }

        if (revolutionPanel != null)
            revolutionPanel.SetActive(active);

        if (active)
            UpdateRemainingTurns(remainingTurns);
    }

    // 残りターン更新
    public void UpdateRemainingTurns(int remainingTurns)
    {
        if (remainingTurnText != null)
        {
            remainingTurnText.text =
                "REMAINING : " + remainingTurns;
        }
    }
}
