using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Cell : BaseCell
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayClearAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Clear");
        }
        else
        {
            SetEmpty();
        }
    }

    // Animation Event
    public void SetEmpty()
    {
        Image img = GetComponent<Image>();
        if (img != null) img.enabled = false;

        TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) text.enabled = false;

        this.enabled = false;
    }
}
