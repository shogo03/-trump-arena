
using System.Collections.Generic;
using UnityEngine;

public class MemoryBurstDeck : MonoBehaviour
{
    [Header("メモリーバースト用カード画像（24枚）")]
    [SerializeField] private Sprite[] cardImages = new Sprite[24];

    private readonly List<Card> cards = new List<Card>();

    // 山札を初期化する
    public void Initialize()
    {
        cards.Clear();

        if (cardImages == null || cardImages.Length != 24)
        {
            Debug.LogError("カード画像を24枚設定してください。", this);
            return;
        }

        for (int i = 0; i < cardImages.Length; i++)
        {
            if (cardImages[i] == null)
            {
                Debug.LogError($"カード画像 Element {i} が未設定です。", this);
                return;
            }

            // 同じ画像がすでに登録されていないか確認
            for (int j = 0; j < i; j++)
            {
                if (cardImages[i] == cardImages[j])
                {
                    Debug.LogError(
                        $"同じカード画像が重複しています：Element {i} と Element {j}",
                        this
                    );
                    return;
                }
            }

            // 最初の12枚はハート、後半の12枚はスペード
            Suit suit = (Suit)(i / 12);
            int number = (i % 12) + 1;

            cards.Add(new Card(suit, number, cardImages[i]));
        }

        Shuffle();

        Debug.Log($"メモリーバースト用デッキを作成：{cards.Count}枚", this);
    }

    // シャッフル
    private void Shuffle()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int randomIndex = Random.Range(i, cards.Count);
            (cards[i], cards[randomIndex]) = (cards[randomIndex], cards[i]);
        }
    }

    // 山札から1枚引く
    public Card DrawCard()
    {
        if (cards.Count == 0)
        {
            Debug.LogError("山札が空です。Initialize() を確認してください。", this);
            return null;
        }

        Card card = cards[0];
        cards.RemoveAt(0);
        return card;
    }


}
