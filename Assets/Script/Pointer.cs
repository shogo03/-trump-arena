
using UnityEngine;

public class CursorTip : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Card"))
        {
            Debug.Log("カードに指先が触れた！");
        }
    }
}
