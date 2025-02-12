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
            if(_isFilled != value)
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
        GetComponent<Renderer>().material = isFilled?mat_Fill:mat_Alpha;
    }
}
