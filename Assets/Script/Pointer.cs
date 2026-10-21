
using UnityEngine;

public class Pointer : MonoBehaviour
{
    private GameObject currentTarget;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Card") ||
            other.CompareTag("GameButton"))
        {
            currentTarget = other.gameObject;
            Debug.Log("カーソルが触れた：" + other.gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (currentTarget == other.gameObject)
        {
            currentTarget = null;
            Debug.Log("カーソルが離れた");
        }
    }
}
