using UnityEngine;

public class Menu : MonoBehaviour
{

    [SerializeField] GameObject panel;
    [SerializeField] GameObject lobby;
    [SerializeField] GameObject menu;
   
    public void SwitchLobby()
    {

        lobby.SetActive(!lobby.activeInHierarchy);

        menu.SetActive(!menu.activeInHierarchy);

    }

    
    public void Quit()
    {
        Application.Quit();
    }
}
