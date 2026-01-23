using System;
using System.Collections;
using UnityEngine;

public class Cube : MonoBehaviour
{
    [SerializeField] Material mat_Alpha;
    [SerializeField] Material mat_Fill;
    [SerializeField] Material mat_Item;
    [SerializeField] GameObject itemMark;
    public bool _isFilled;
    private bool isBlinking = false;
    private Coroutine blinkingCoroutine;
    public bool isFilled
    {
        get { return _isFilled; }
        set
        {
            if (_isFilled != value)
            {
                _isFilled = value;
                onIsFilledChanged?.Invoke(_isFilled);
            }
        }
    }
    public event Action<bool> onIsFilledChanged;
    
    private void Start()
    {
        onIsFilledChanged += ChangeMaterial;
        OnInit();
    }

    private void OnDisable()
    {
        onIsFilledChanged -= ChangeMaterial;
    }
    private void OnInit()
    {
        if (isFilled)
        {
            GetComponent<Renderer>().material = mat_Fill;
        }
    }
    void ChangeMaterial(bool isFilled)
    {
        if (isFilled)
        {
            GetComponent<Renderer>().material = mat_Fill;
        }
        else
        {
            if (!isBlinking)
            {
                GameManager.Instance.audioManager.PlayerBlockDestoryAudio();
                GetComponent<Animator>().SetTrigger("ChangeMaterial");
            }
        }
    }
    public void ChangeMaterialToAlpha()
    {
        GetComponent<Renderer>().material = mat_Alpha;
    }

    public void SetToFillMaterial()
    {
        GetComponent<Renderer>().material = mat_Fill;
    }

    public void StartBlinking()
    {
        if (!isBlinking && blinkingCoroutine==null)
        {
            isBlinking = true;
            blinkingCoroutine = StartCoroutine(Blink());
        }
    }

    public void StopBlinking()
    {
        if (isBlinking)
        {
            isBlinking = false;
            if(blinkingCoroutine != null) StopCoroutine(blinkingCoroutine);
            blinkingCoroutine=null;
            Renderer renderer = GetComponent<Renderer>();
            Color originalColor = renderer.material.color;
            renderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f); // 알파 값을 1로 복원
            if (isFilled)
            {
                GetComponent<Renderer>().material = mat_Fill;
            }
            else
            {
                GetComponent<Renderer>().material = mat_Alpha;
            }
        }
    }

    private IEnumerator Blink()
    {
        Renderer renderer = GetComponent<Renderer>();
        Color originalColor = renderer.material.color;
        float fadeDuration = 0.75f;

        while (true)
        {
            // Fade out
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0.2f, elapsedTime / fadeDuration);
                renderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }

            // Fade in
            elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0.2f, 1f, elapsedTime / fadeDuration);
                renderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }
    }
}