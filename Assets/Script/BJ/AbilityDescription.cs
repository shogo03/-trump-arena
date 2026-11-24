using UnityEngine;

public class AbilityDescription : MonoBehaviour
{
    public GameObject descriptionText;

    public void ShowDescription()
    {
        if (descriptionText != null)
        {
            descriptionText.SetActive(true);
        }
    }

    public void HideDescription()
    {
        if (descriptionText != null)
        {
            descriptionText.SetActive(false);
        }
    }
}