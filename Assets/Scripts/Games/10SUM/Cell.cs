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
    public override void SetEmpty()
    {
        base.SetEmpty(); // IsEmpty = true

        Image img = GetComponent<Image>();
        if (img != null) img.enabled = false;

        TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) text.enabled = false;

        this.enabled = false;
    }

    public override void SetOccupied()
    {
        base.SetOccupied(); // IsEmpty = false

        Image img = GetComponent<Image>();
        if (img != null) img.enabled = true;

        TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) text.enabled = true;

        this.enabled = true;
    }
}
