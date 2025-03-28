using UnityEngine;

public class Block : MonoBehaviour
{
    [HideInInspector] public BlockSpawner blockSpawner;

    private void OnDestroy()
    {
        blockSpawner.RemoveBlock(gameObject);
    }
}
