using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemMaterialControl : MonoBehaviour
{
    [SerializeField] List<GameObject> cubes = new List<GameObject>();

    public HashSet<GameObject> hitCubes = new HashSet<GameObject>();
    HashSet<GameObject> previouslyHitCubes = new HashSet<GameObject>();

    public bool isClicked = false;
    [SerializeField] Material mat_Alpha;
    Dictionary<GameObject, Coroutine> activeCoroutines = new Dictionary<GameObject, Coroutine>();
    Coroutine blinkCoroutine;
    List<GameObject> blinkingCubes = new List<GameObject>();
    private void Update()
    {
        if (isClicked)
        {
            ChangeCubeMaterialBelow();
        }
    }
    public void ChangeCubeMaterialBelow()
    {
        hitCubes.Clear();

        foreach (GameObject cube in cubes)
        {
            Ray ray = new Ray(cube.transform.position, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Cube"))
                {
                    hitCubes.Add(hit.collider.gameObject);
                }
            }
        }

        // 중단 대상 찾기
        var cubesToStop = blinkingCubes.Except(hitCubes).ToList();
        foreach (var cube in cubesToStop)
        {
            // 1. 머티리얼 적용
            ApplyMaterial(cube, mat_Alpha);

            // 2. PropertyBlock 클리어
            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.SetPropertyBlock(null); // 또는 new MaterialPropertyBlock()도 가능
            }
        }

        blinkingCubes = hitCubes.ToList();

        if (blinkCoroutine == null)
        {
            blinkCoroutine = StartCoroutine(BlinkAllCubes());
        }
    }


    private IEnumerator BlinkAllCubes()
    {
        Color redColor = Color.red;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        while (true)
        {
            for (float t = 0; t < 1f; t += Time.deltaTime)
            {
                float alpha = Mathf.Lerp(1f, 0f, t);
                redColor.a = alpha;
                foreach (var cube in blinkingCubes)
                {
                    var renderer = cube.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.GetPropertyBlock(mpb);
                        mpb.SetColor("_BaseColor", redColor);
                        renderer.SetPropertyBlock(mpb);
                    }
                }
                yield return null;
            }
            for (float t = 0; t < 1f; t += Time.deltaTime)
            {
                float alpha = Mathf.Lerp(0f, 1f, t);
                redColor.a = alpha;
                foreach (var cube in blinkingCubes)
                {
                    var renderer = cube.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.GetPropertyBlock(mpb);
                        mpb.SetColor("_BaseColor", redColor);
                        renderer.SetPropertyBlock(mpb);
                    }
                }
                yield return null;
            }
        }
    }


    public void ApplyAlphaMaterial()
    {
        foreach (GameObject cube in previouslyHitCubes.ToList())
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