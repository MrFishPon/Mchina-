using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class SoundLoader : MonoBehaviour
{
    private List<float> soundsVolume = new List<float>();

    private List<float> musicVolume = new List<float>();

    [SerializeField] AudioSource[] sounds;

    [SerializeField] AudioSource[] musics; 
    
    void Update()
    {

        if (PlayerPrefs.HasKey("music"))
        {
            for (int i = 0; i < musics.Length; i++)
            {
                musics[i].volume = PlayerPrefs.GetFloat("music") * musicVolume[i];
            }
        }
        if (PlayerPrefs.HasKey("sounds"))
        {
            for(int i = 0; i < sounds.Length; i++ )
            {
                sounds[i].volume = PlayerPrefs.GetFloat("sounds") * soundsVolume[i];
            }
        }

    }
    private void Start()
    {
        foreach (AudioSource sound in sounds)
        {
            soundsVolume.Add(sound.volume);
        }
        foreach (AudioSource music in musics)
        {
            musicVolume.Add(music.volume);
        }
    }
}
