using System;
using UnityEngine;

public class Cube : MonoBehaviour
{
    [SerializeField] Material mat_Alpha;
    [SerializeField] Material mat_Fill;

    private bool _isFilled;
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

    private void OnEnable()
    {
        onIsFilledChanged += ChangeMaterial;
    }
    private void OnDisable()
    {
        onIsFilledChanged -= ChangeMaterial;
    }

    void ChangeMaterial(bool isFilled)
    {
        if (isFilled)
        {
            GetComponent<Renderer>().material = mat_Fill;
        }
        else
        {
            GameManager.Instance.audioManager.PlayerBlockDestoryAudio();
            GetComponent<Animator>().SetTrigger("ChangeMaterial");
        }
    }
    public void ChangeMaterialToAlpha()
    {
        GetComponent<Renderer>().material = mat_Alpha;
    }
}
