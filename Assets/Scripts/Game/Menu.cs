using UnityEngine;

public class Menu : MonoBehaviour
{

    [SerializeField] GameObject settings;
    [SerializeField] GameObject lobby;
    [SerializeField] GameObject menu;
   
    public void SwitchLobby()
    {

        lobby.SetActive(!lobby.activeInHierarchy);

        menu.SetActive(!menu.activeInHierarchy);

    }
    public void SwitchSettings()
    {

        settings.SetActive(!settings.activeInHierarchy);

        menu.SetActive(!menu.activeInHierarchy);

    }


    public void Quit()
    {
        Application.Quit();
    }
}
