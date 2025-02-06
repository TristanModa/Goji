using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    static AudioSource audioPlayer; 

    [SerializeField]
    List<String> soundNames = new List<String>();

    [SerializeField]
    List<AudioClip> soundClips = new List<AudioClip>();

    static Dictionary<String, AudioClip> soundLibrary = new Dictionary<string, AudioClip>();

    static bool audioExists = false; 

    void Awake() {
        if (AudioManager.audioExists == false) {
            AudioManager.audioExists = true; 
            Setup();
        }
        else {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Setup() {
        audioPlayer = gameObject.GetComponent<AudioSource>();
        
        for (int i = 0; i < soundClips.Count; i++) {
            soundLibrary.Add(soundNames[i], soundClips[i]);
        }
    }

    public static void PlaySFX(String whichSound) {
        if (soundLibrary.TryGetValue(whichSound, out AudioClip clip)) {
            audioPlayer.PlayOneShot(clip);
        }
    }

    void PlayBGM() {
        //TODO
    }
}
