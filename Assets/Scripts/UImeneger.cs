
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButtonExample : MonoBehaviour
{
    Animator animator;

    public string Panel;

    public void VoidScene()
    {

        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void PlayAnimation()
    {
        animator.Play("Loading");
    }

    
}
    