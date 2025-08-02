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
    private bool _isItemActive = false;
    private int turn = 0;
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
    event Action<bool> onIsFilledChanged;
    
    private void Start()
    {
        onIsFilledChanged += ChangeMaterial;
        GameManager.Instance.blockSpawner.OnBlocksSpawned += IncreaseItemTurn;
        OnInit();
    }

    private void OnDisable()
    {
        onIsFilledChanged -= ChangeMaterial;
        GameManager.Instance.blockSpawner.OnBlocksSpawned -= IncreaseItemTurn;
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
                SetItemMarkInactive();
            }
        }
    }
    public void ChangeMaterialToAlpha()
    {
        GetComponent<Renderer>().material = mat_Alpha;
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
            StopCoroutine(blinkingCoroutine);
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
    public void SetItemMarkActive()
    {
        _isItemActive = true;
        itemMark.SetActive(true);
    }
    public void SetItemMarkInactive()
    {
        if (_isItemActive)
        {
            _isItemActive = false;
            itemMark.SetActive(false);

            GameManager.Instance.itemManager.GetItem();
        }
    }
    private void IncreaseItemTurn()
    {
        if (_isItemActive)
        {
            turn++;
            if (turn == 2)
            {
                itemMark.GetComponent<StarItem>().StartBlink();
            }
            else if (turn > 2)
            {
                _isItemActive = false;
                itemMark.SetActive(false);

            }
        }
    }
}