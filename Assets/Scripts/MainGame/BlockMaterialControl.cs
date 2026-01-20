using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockMaterialControl : MonoBehaviour
{
    List<GameObject> cubes = new List<GameObject>(); // 모든 Cube 저장 리스트
    HashSet<GameObject> previouslyHitCubes = new HashSet<GameObject>();

    [SerializeField] Material mat_Red;
    [SerializeField] Material mat_Green;
    [SerializeField] Material mat_Alpha;

    public HashSet<GameObject> hitCubes = new HashSet<GameObject>();
    public bool isClicked = false;
    public bool allHitCube;
    private void Start()
    {
        foreach (Transform child in transform)
        {
            cubes.Add(child.gameObject); // 초기 모든 Cube 저장
        }
    }
    private void Update()
    {
        if (isClicked)
        {
            ChangeCubeMaterialBelow();
        }
    }
    public void ChangeCubeMaterialBelow()
    {
        allHitCube = true;
        hitCubes = new HashSet<GameObject>(); // 레이를 맞은 Cube 저장

        // 모든 큐브에 대해 레이 쏘기
        foreach (GameObject cube in cubes)
        {
            Ray ray = new Ray(cube.transform.position, Vector3.down); // 큐브에서 아래로 레이 쏘기
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Cube") && !hit.collider.GetComponent<Cube>().isFilled)
                {
                    hitCubes.Add(hit.collider.gameObject); // 레이를 맞은 큐브 추가
                }
                else
                {
                    allHitCube = false; // Cube가 아닌 오브젝트를 맞으면 실패
                }
            }
            else
            {
                allHitCube = false; // 레이가 아무것도 맞추지 못하면 실패
            }
        }

        // 블록 모양이 일치하는지 확인
        if (allHitCube)
        {
            allHitCube = CheckShape(cubes, hitCubes);
        }

        previouslyHitCubes.UnionWith(hitCubes);

        // 모든 Ray가 Cube를 맞췄다면 Green, 하나라도 실패하면 Red 적용
        Material newMaterial = allHitCube ? mat_Green : mat_Red;

        // Ray에 맞은 Cube에는 Green 또는 Red 매터리얼 적용
        foreach (GameObject cube in hitCubes)
        {
            ApplyMaterial(cube, newMaterial);
        }
        foreach (GameObject cube in previouslyHitCubes.ToList())
        {
            if (!hitCubes.Contains(cube))
            {
                ApplyMaterial(cube, mat_Alpha);
                previouslyHitCubes.Remove(cube);
            }
        }
    }

    private bool CheckShape(List<GameObject> blockCubes, HashSet<GameObject> hitBoardCubes)
    {
        if (blockCubes.Count != hitBoardCubes.Count)
        {
            return false;
        }

        if (blockCubes.Count <= 1)
        {
            return true;
        }

        // Sort both lists by world position to establish a common reference frame.
        var blockCubesSorted = blockCubes.OrderBy(c => c.transform.position.x).ThenBy(c => c.transform.position.z).ToList();
        var hitCubesSorted = hitBoardCubes.OrderBy(c => c.transform.position.x).ThenBy(c => c.transform.position.z).ToList();

        // Get the world position of the anchor cube for the held block.
        Vector3 blockAnchorPos = blockCubesSorted[0].transform.position;
        // Get the world position of the anchor cube for the corresponding board cells.
        Vector3 hitAnchorPos = hitCubesSorted[0].transform.position;

        // Check if the relative positions of all other cubes match.
        for (int i = 1; i < blockCubesSorted.Count; i++)
        {
            // Calculate the relative position of a cube in the held block.
            Vector3 blockRelativePos = blockCubesSorted[i].transform.position - blockAnchorPos;
            // Calculate the relative position of the corresponding cube on the board.
            Vector3 hitRelativePos = hitCubesSorted[i].transform.position - hitAnchorPos;

            // If the relative positions don't match (within a tolerance), the shape is wrong.
            if (Vector3.Distance(blockRelativePos, hitRelativePos) > 0.1f)
            {
                return false;
            }
        }

        return true;
    }

    void ApplyMaterial(GameObject obj, Material mat)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = mat;
        }
    }
}
