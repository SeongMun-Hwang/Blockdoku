using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public bool isFilled = false;
    public Image cellImage;

    private void Awake()
    {
        cellImage = GetComponent<Image>();
    }

    public void SetFilled(bool filled)
    {
        isFilled = filled;
        cellImage.color = filled ? Color.blue : Color.white; // Placeholder colors
    }
}
