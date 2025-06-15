using System;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    GameObject catchedBlock;
    Vector3 prevPos;
    private Plane dragPlane;
    private float originalScale = 0.6f;
    private float increasedScale = 0.6f;
    public event Action onMouseReleased;
    bool isItemClicked = false;
    bool isBlockClicked = false;
    private void Update()
    {
        HandleMouseInput();
    }
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryCatchBlock();
        }
        else if (Input.GetMouseButton(0) && catchedBlock != null)
        {
            MoveCatchedBlock();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ReleaseBlock();
        }
    }
    /*블록 잡기를 시도하는 함수*/
    void TryCatchBlock()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Block"))
            {
                isBlockClicked = true;
                catchedBlock = hit.collider.transform.parent.gameObject;
                catchedBlock.GetComponent<BlockMaterialControl>().isClicked = true;
                prevPos = catchedBlock.transform.position;
            }
            else if (hit.collider.CompareTag("Item"))
            {
                isItemClicked = true;
                catchedBlock = hit.collider.gameObject;
                catchedBlock.GetComponent<ItemMaterialControl>().isClicked = true;
                prevPos = catchedBlock.transform.position;
            }

            dragPlane = new Plane(Vector3.up, hit.point);
        }
    }
    void MoveCatchedBlock()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            //catchedBlock.transform.localScale = new Vector3(increasedScale, increasedScale, increasedScale);
            Vector3 targetPos = ray.GetPoint(distance) + new Vector3(0,0,3f);
            targetPos.y = catchedBlock.transform.position.y;
            catchedBlock.transform.position = targetPos;
        }
    }
    void ReleaseBlock()
    {
        if (catchedBlock != null)
        {
            if (isBlockClicked)
            {
                isBlockClicked = false;
                BlockMaterialControl blockMaterialControl = catchedBlock.GetComponent<BlockMaterialControl>();
                if (blockMaterialControl.allHitCube)
                {
                    GameManager.Instance.audioManager.PlayerBlockThudAudio();
                    foreach (GameObject go in blockMaterialControl.hitCubes)
                    {
                        go.GetComponent<Cube>().isFilled = true;
                    }
                    GameManager.Instance.blockSpawner.RemoveBlock(catchedBlock);
                    Destroy(catchedBlock);
                    onMouseReleased?.Invoke();
                }
                else
                {
                    if (blockMaterialControl.hitCubes.Count != 0)
                    {
                        GameManager.Instance.audioManager.PlayErrorAudio();
                    }
                    catchedBlock.transform.position = prevPos;
                    catchedBlock.transform.localScale = new Vector3(originalScale, originalScale, originalScale);
                    catchedBlock.GetComponent<BlockMaterialControl>().ChangeCubeMaterialBelow();
                }
                catchedBlock.GetComponent<BlockMaterialControl>().isClicked = false;
                catchedBlock = null;
            }
            else if (isItemClicked)
            {
                isItemClicked = false;
                ItemMaterialControl itemMaterialControl = catchedBlock.GetComponent<ItemMaterialControl>();
                if (itemMaterialControl.hitCubes != null)
                {
                    int amount = 0;
                    foreach (GameObject go in itemMaterialControl.hitCubes)
                    {
                        bool isFilled = go.GetComponent<Cube>().isFilled;
                        if (isFilled)
                        {
                            isFilled = false;
                            amount++;
                        }
                    }
                    GameManager.Instance.scoreManager.AddScore(amount);
                    GameManager.Instance.blockSpawner.RemoveBlock(catchedBlock);
                    Destroy(catchedBlock);
                    onMouseReleased?.Invoke();
                }
                else
                {
                    GameManager.Instance.audioManager.PlayErrorAudio();
                    catchedBlock.transform.position = prevPos;
                    catchedBlock.transform.localScale = new Vector3(originalScale, originalScale, originalScale);
                    catchedBlock.GetComponent<ItemMaterialControl>().ChangeCubeMaterialBelow();
                }
                catchedBlock.GetComponent<ItemMaterialControl>().isClicked = false;
                catchedBlock = null;
            }
        }
    }
}
