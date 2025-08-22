using System;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    GameObject catchedBlock;
    Vector3 prevPos;
    private Plane dragPlane;
    private float originalScale = 0.6f;
    private float increasedScale;
    public event Action onMouseReleased;
    bool isItemClicked = false;
    bool isBlockClicked = false;
    private HashSet<GameObject> lastPreviewedCubes = new HashSet<GameObject>();
    private void Start()
    {
        increasedScale = GameManager.Instance.increasedScale/1.75f;
    }
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
                catchedBlock.transform.localScale = new Vector3(increasedScale, increasedScale, increasedScale);
                catchedBlock.GetComponent<BlockMaterialControl>().isClicked = true;
            prevPos = catchedBlock.transform.position;
            StopBlinkingAll();
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

            if (isBlockClicked)
            {
                BlockMaterialControl blockMaterialControl = catchedBlock.GetComponent<BlockMaterialControl>();
                HashSet<GameObject> erasableCubes = new HashSet<GameObject>();
                if (blockMaterialControl.allHitCube)
                {
                    erasableCubes = GameManager.Instance.scoreManager.CheckBoardForPreview(blockMaterialControl.hitCubes);
                }

                // Stop blinking for cubes that are no longer in the preview
                HashSet<GameObject> cubesToStopBlinking = new HashSet<GameObject>(lastPreviewedCubes);
                cubesToStopBlinking.ExceptWith(erasableCubes);
                foreach (GameObject cube in cubesToStopBlinking)
                {
                    cube.GetComponent<Cube>().StopBlinking();
                }

                // Start blinking for new preview cubes
                foreach (GameObject cube in erasableCubes)
                {
                    cube.GetComponent<Cube>().StartBlinking();
                }

                lastPreviewedCubes = erasableCubes;
            }
        }
    }
    void ReleaseBlock()
    {
        if (catchedBlock == null) return;

        if (isBlockClicked)
        {
            HandleBlockRelease();
        }
        else if (isItemClicked)
        {
            HandleItemRelease();
        }

        // Reset common state
        catchedBlock = null;
        isBlockClicked = false;
        isItemClicked = false;
        StopBlinkingAll();
    }

    private void HandleBlockRelease()
    {
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
            blockMaterialControl.ChangeCubeMaterialBelow();
        }
        blockMaterialControl.isClicked = false;
    }

    private void HandleItemRelease()
    {
        ItemMaterialControl itemMaterialControl = catchedBlock.GetComponent<ItemMaterialControl>();
        if (itemMaterialControl.hitCubes != null)
        {
            int amount = 0;
            foreach (GameObject go in itemMaterialControl.hitCubes)
            {
                Cube cube = go.GetComponent<Cube>();
                if (cube.isFilled)
                {
                    cube.isFilled = false;
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
            itemMaterialControl.ChangeCubeMaterialBelow();
        }
        itemMaterialControl.isClicked = false;
    }

    private void StopBlinkingAll()
    {
        if (lastPreviewedCubes.Count > 0)
        {
            foreach (GameObject cube in lastPreviewedCubes)
            {
                if(cube != null)
                {
                    cube.GetComponent<Cube>().StopBlinking();
                }
            }
            lastPreviewedCubes.Clear();
        }
    }
}