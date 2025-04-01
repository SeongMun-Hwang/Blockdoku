using UnityEngine;

public class Block : MonoBehaviour
{
    [HideInInspector] public BlockSpawner blockSpawner;
    [SerializeField] public int width;
    [SerializeField] public int height;
}
