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
    bool isBlockClicked = false;
    private HashSet<GameObject> lastPreviewedCubes = new HashSet<GameObject>();
    public float mouseSensivility = 1.5f;
    [SerializeField] private GameObject board;
    private void Start()
    {
        increasedScale = GameManager.Instance.increasedScale / 1.75f;
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

            if (board != null)
            {
                float boardY = board.transform.position.y;
                dragPlane = new Plane(Vector3.up, new Vector3(0, boardY, 0));
            }
            else
            {
                dragPlane = new Plane(Vector3.up, hit.point);
            }
        }
    }
    void MoveCatchedBlock()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            //catchedBlock.transform.localScale = new Vector3(increasedScale, increasedScale, increasedScale);
            Vector3 targetPos = ray.GetPoint(distance) + new Vector3(0, 0, 2f) * mouseSensivility;
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

        // Reset common state
        catchedBlock = null;
        isBlockClicked = false;
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
            //catchedBlock.transform.localScale = new Vector3(originalScale, originalScale, originalScale);
            blockMaterialControl.ChangeCubeMaterialBelow();
        }
        blockMaterialControl.isClicked = false;
    }
    private void StopBlinkingAll()
    {
        if (lastPreviewedCubes.Count > 0)
        {
            foreach (GameObject cube in lastPreviewedCubes)
            {
                if (cube != null)
                {
                    cube.GetComponent<Cube>().StopBlinking();
                }
            }
            lastPreviewedCubes.Clear();
        }
    }
}