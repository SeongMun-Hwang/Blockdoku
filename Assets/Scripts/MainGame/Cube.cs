using System;
using UnityEngine;

public class Cube : MonoBehaviour
{
    [SerializeField] Material mat_Alpha;
    [SerializeField] Material mat_Fill;
    [SerializeField] GameObject itemMark;
    private bool _isFilled;
    private bool _isItemActive = false;
    private int turn = 0;
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
    private void Start()
    {
        GameManager.Instance.blockSpawner.OnBlocksSpawned += IncreaseItemTurn;
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
            SetItemMarkInactive();
        }
    }
    public void ChangeMaterialToAlpha()
    {
        GetComponent<Renderer>().material = mat_Alpha;
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