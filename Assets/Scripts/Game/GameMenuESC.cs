using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameMenuESC : MonoBehaviour
{
    [SerializeField] GameObject escv1;

    [SerializeField] GameObject settings;

    [SerializeField] GameObject esc;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            escv1.SetActive(!escv1.activeInHierarchy);


    }

    public void Countinue()
    {
        escv1.SetActive(!escv1.activeInHierarchy);
    }

    public void Leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            SceneManager.LoadScene(0);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();

            SceneManager.LoadScene(0);
        }


    }

    public void Switch()
    {
        settings.SetActive(!settings.activeInHierarchy);

        esc.SetActive(!esc.activeInHierarchy);  
    }

}
