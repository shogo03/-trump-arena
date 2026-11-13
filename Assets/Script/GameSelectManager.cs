using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSelectManager : MonoBehaviour
{
    // メモリーバースト
    public void SelectMemoryBurst()
    {
        Debug.Log("MemoryBurstを選択しました");
        SceneManager.LoadScene("PlayerSelect");
    }

    // ブラックジャック
    public void SelectBlackjack()
    {
        Debug.Log("Blackjackを選択しました");
        SceneManager.LoadScene("BJPlayerSelect");
    }

    public void HighAndLow()
    {
        Debug.Log("HighAndLowを選択しました");
        SceneManager.LoadScene("BJPlayerSelect");
    }

    // 人数選択に戻る
    public void BackToPlayerSelect()
    {
        SceneManager.LoadScene("GameSelect");
    }
}