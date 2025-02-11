using System.Collections.Generic;
using UnityEngine;

public class BlockMaterialControl : MonoBehaviour
{
    List<GameObject> cubes = new List<GameObject>(); // 모든 Cube 저장 리스트
    [SerializeField] Material mat_Red;
    [SerializeField] Material mat_Green;
    [SerializeField] Material mat_Alpha;
    HashSet<GameObject> previouslyHitCubes = new HashSet<GameObject>();
    private void Start()
    {
        foreach (Transform child in transform)
        {
            cubes.Add(child.gameObject); // 초기 모든 Cube 저장
        }
    }
    private void Update()
    {
        ChangeCubeMaterialBelow();
    }
    void ChangeCubeMaterialBelow()
    {
        bool allHitCube = true;
        HashSet<GameObject> hitCubes = new HashSet<GameObject>(); // 레이를 맞은 Cube 저장 (중복 제거)

        // 모든 큐브에 대해 레이 쏘기
        foreach (GameObject cube in cubes)
        {
            Ray ray = new Ray(cube.transform.position, Vector3.down); // 큐브에서 아래로 레이 쏘기
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Cube"))
                {
                    Debug.Log("Hit Cube: " + hit.collider.name);
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
        previouslyHitCubes.UnionWith(hitCubes);

        // 모든 Ray가 Cube를 맞췄다면 Green, 하나라도 실패하면 Red 적용
        Material newMaterial = allHitCube ? mat_Green : mat_Red;

        // Ray에 맞은 Cube에는 Green 또는 Red 매터리얼 적용
        foreach (GameObject cube in hitCubes)
        {
            ApplyMaterial(cube, newMaterial);
        }
        foreach (GameObject cube in previouslyHitCubes)
        {
            if (!hitCubes.Contains(cube))
            {
                ApplyMaterial(cube, mat_Alpha);
                previouslyHitCubes.Remove(cube);
            }
        }
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