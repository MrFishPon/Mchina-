using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{

    [SerializeField] Slider sound;

    [SerializeField] Slider music;


    public void SetSound() 
    { 
        if (sound != null)
        {
            PlayerPrefs.SetFloat("sound", sound.value);

            PlayerPrefs.Save();
        }
        

    }
    public void SetMusic()
    {
        if (music != null)
        {
            PlayerPrefs.SetFloat("music", music.value);

            PlayerPrefs.Save();
        }


    }
    private void Start()
    {
        if (PlayerPrefs.HasKey("sound"))
            sound.value = PlayerPrefs.GetFloat("sound");

        if (PlayerPrefs.HasKey("music"))
            music.value = PlayerPrefs.GetFloat("music");        
    }
}
