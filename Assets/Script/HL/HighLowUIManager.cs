
using UnityEngine;
using UnityEngine.UI;

public class HLUIManager : MonoBehaviour
{
    [Header("背景画像")]
    [SerializeField] private Image backgroundImage;

    [Header("背景スプライト")]
    [SerializeField] private Sprite normalBackground;
    [SerializeField] private Sprite revolutionBackground;

    // 革命状態に応じて背景を変更
    public void SetRevolutionBackground(bool isRevolution)
    {
        if (backgroundImage == null) return;

        backgroundImage.sprite = isRevolution
            ? revolutionBackground
            : normalBackground;
    }
}
