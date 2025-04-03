using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] public int width;
    [SerializeField] public int height;

    public void ReverseSize()
    {
        int temp = width;
        width = height;
        height = temp;
    }
}
