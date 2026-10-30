
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BJDealer : MonoBehaviour
{
    public Deck deck;
    public GameObject cardPrefab;
    public Transform dealerHandArea;
    public Text dealerPointText;

    public Vector2 cardSize = new Vector2(60, 80);

    private List<Card> dealerHand = new List<Card>();
    private List<Image> dealerCardImages = new List<Image>();

    // ディーラーのカードを1枚引く
    public void DrawDealerCard()
    {
        if (deck == null || cardPrefab == null || dealerHandArea == null)
        {
            Debug.LogError("ディーラーの設定を確認してね！");
            return;
        }

        Card card = deck.DrawCard();

        GameObject obj = Instantiate(cardPrefab, dealerHandArea);
        Image image = obj.GetComponent<Image>();

        if (image == null)
        {
            Destroy(obj);
            Debug.LogError("Card PrefabにImageがない！");
            return;
        }

        // 最初のカードは裏向き
        if (dealerHand.Count == 0)
        {
            image.sprite = deck.cardBackImage;
        }
        else
        {
            image.sprite = card.image;
        }

        image.enabled = true;

        dealerCardImages.Add(image);
        dealerHand.Add(card);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = cardSize;
        rect.anchoredPosition = new Vector2(
            (dealerHand.Count - 1) * 35,
            0
        );

        UpdateDealerPoint();
    }

    // ディーラーの点数を計算する
    public int GetDealerTotal()
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in dealerHand)
        {
            if (card.number == 1)
            {
                total += 11;
                aceCount++;
            }
            else if (card.number >= 11)
            {
                total += 10;
            }
            else
            {
                total += card.number;
            }
        }

        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }

    // 点数表示を更新する
    void UpdateDealerPoint()
    {
        if (dealerPointText != null)
        {
            dealerPointText.text = "合計：" + GetDealerTotal();
        }
    }

    // 裏向きのカードを表にする
    public void RevealDealerCards()
    {
        for (int i = 0; i < dealerHand.Count; i++)
        {
            dealerCardImages[i].sprite = dealerHand[i].image;
        }

        UpdateDealerPoint();
    }

    // ディーラーのターン
    public int DealerTurn()
    {
        // 17未満ならカードを引く
        while (GetDealerTotal() < 17)
        {
            DrawDealerCard();
        }

        RevealDealerCards();

        Debug.Log("ディーラーの最終点数：" + GetDealerTotal());

        return GetDealerTotal();
    }
}
