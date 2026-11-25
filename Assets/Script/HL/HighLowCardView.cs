
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HighLowCardView : MonoBehaviour
{
    [Header("カード画像")]
    [SerializeField] private Image cardImage;

    [Header("アニメーション速度")]
    [SerializeField] private float flipDuration = 0.8f;

    private Sprite frontImage;
    private Sprite backImage;

    private bool isFlipping = false;

    public bool IsFlipping => isFlipping;

    public void Setup(Sprite front, Sprite back)
    {
        frontImage = front;
        backImage = back;

        cardImage.sprite = backImage;
        transform.localRotation = Quaternion.identity;
    }

    public void Flip()
    {
        if (isFlipping) return;

        StartCoroutine(FlipAnimation());
    }


    private IEnumerator FlipAnimation()
    {
        isFlipping = true;

        float halfDuration = flipDuration / 2f;
        float elapsed = 0f;

        // 前半：0度から90度まで回転
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;

            float angle = Mathf.Lerp(
                0f, 90f, elapsed / halfDuration
            );

            transform.localRotation =
                Quaternion.Euler(0f, angle, 0f);

            yield return null;
        }

        // カードが横向きになった瞬間に画像変更
        cardImage.sprite = frontImage;

        elapsed = 0f;

        // 後半：90度から0度まで戻す
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;

            float angle = Mathf.Lerp(
                90f, 0f, elapsed / halfDuration
            );

            transform.localRotation =
                Quaternion.Euler(0f, angle, 0f);

            yield return null;
        }

        // 回転をリセット
        transform.localRotation = Quaternion.identity;

        isFlipping = false;
    }

}
