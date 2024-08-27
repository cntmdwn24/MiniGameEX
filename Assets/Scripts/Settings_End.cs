using UnityEngine;

public class Settings_End : MonoBehaviour
{
    private Animator animator;
    public ButtonManager buttonManager;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void operateSettings_Down()
    {
        animator.SetTrigger("Settings_Close");
    }

    public void Settings_Down()
    {
        buttonManager.Settings_End();
    }
}