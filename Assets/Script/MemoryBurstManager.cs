
using UnityEngine;
using UnityEngine.UI;

public class MemoryBurstManager : MonoBehaviour
{
    [Header("山札")]
    [SerializeField] private MemoryBurstDeck deck;

    [Header("カード")]
    [SerializeField] private MemoryBurstCardView cardPrefab;

    [Header("カードを並べる場所")]
    [SerializeField] private Transform cardArea;

    [Header("設定")]
    [SerializeField] private int cardCount = 24;
    [SerializeField] private int columns = 6;

    private void Start()
    {
        CreateCards();
    }

    private void CreateCards()
    {
        deck.Initialize();
        if (deck == null || cardPrefab == null || cardArea == null)
        {
            Debug.LogError(
                "Deck、Card Prefab、Card AreaをInspectorで設定してください。",
                this
            );
            return;
        }

        // 既存のカードを削除
        foreach (Transform child in cardArea)
        {
            Destroy(child.gameObject);
        }

        // 山札からカードを取得して配置
        for (int i = 0; i < cardCount; i++)
        {
            Card card = deck.DrawCard();

            MemoryBurstCardView view =
                Instantiate(cardPrefab, cardArea);

            view.Setup(card);
        }

        Debug.Log(cardCount + "枚のカードを配置しました。");
    }
}
