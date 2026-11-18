
using UnityEngine;
using UnityEngine.UI;

public class TurnSkipDisplay : MonoBehaviour
{
    [Header("TURN SKIP UI")]
    [SerializeField] private GameObject turnSkipObject;
    [SerializeField] private Button turnSkipButton;

    [Header("各マネージャー")]
    [SerializeField] private MemoryBurstCardManager cardManager;
    [SerializeField] private MemoryBurstTurnManager turnManager;

    private void Start()
    {
        turnSkipObject.SetActive(false);

        turnSkipButton.onClick.AddListener(OnTurnSkipClicked);

        cardManager.OnPairFailed += ShowButton;
        turnManager.OnTurnChanged += OnTurnChanged;
    }

    private void ShowButton()
    {
        if (turnManager == null || turnManager.IsGameOver)
            return;

        turnSkipObject.SetActive(true);
    }

    private void OnTurnSkipClicked()
    {
        if (turnManager == null)
            return;

        // 残り時間に関係なくターンを終了
        turnManager.EndTurn();
    }

    private void OnTurnChanged(int player)
    {
        // 他のプレイヤーに移行したら非表示
        turnSkipObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (cardManager != null)
            cardManager.OnPairFailed -= ShowButton;

        if (turnManager != null)
            turnManager.OnTurnChanged -= OnTurnChanged;

        if (turnSkipButton != null)
            turnSkipButton.onClick.RemoveListener(OnTurnSkipClicked);
    }
}
