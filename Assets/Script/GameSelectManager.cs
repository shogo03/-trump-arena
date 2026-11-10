using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSelectManager : MonoBehaviour
{
    // メモリーバースト
    public void SelectMemoryBurst()
    {
        Debug.Log("MemoryBurstを選択しました");
        SceneManager.LoadScene("MemoryBurst");
    }

    // ブラックジャック
    public void SelectBlackjack()
    {
        Debug.Log("Blackjackを選択しました");
        SceneManager.LoadScene("blackjack");
    }

    // 人数選択に戻る
    public void BackToPlayerSelect()
    {
        SceneManager.LoadScene("PlayerSelect");
    }
}