using System;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    GameObject catchedBlock;
    private Plane dragPlane;
    public event Action onMouseReleased;
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
                catchedBlock = hit.collider.transform.parent.gameObject;
                catchedBlock.GetComponent<BlockMaterialControl>().isClicked = true;
            }
            dragPlane = new Plane(Vector3.up, hit.point);
        }
    }
    void MoveCatchedBlock()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPos = ray.GetPoint(distance);
            targetPos.y = catchedBlock.transform.position.y;
            catchedBlock.transform.position = targetPos;
        }
    }
    void ReleaseBlock()
    {
        if (catchedBlock != null)
        {
            BlockMaterialControl blockMaterialControl = catchedBlock.GetComponent<BlockMaterialControl>();
            if (blockMaterialControl.allHitCube)
            {
                foreach (GameObject go in blockMaterialControl.hitCubes)
                {
                    go.GetComponent<Cube>().isFilled = true;
                }
                Destroy(catchedBlock);
            }
            catchedBlock.GetComponent<BlockMaterialControl>().isClicked = false;
            catchedBlock = null;
        }
        onMouseReleased?.Invoke();
    }
}
