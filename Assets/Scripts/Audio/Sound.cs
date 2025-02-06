using System;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public class Sound : MonoBehaviour
{
    public String soundName; 
    public AudioClip soundFile; 

    [SerializeField]
    bool isRepeatable;

    public Sound (String name, AudioClip file, bool repeatable) {
        soundName = name; 
        soundFile = file;
        isRepeatable = repeatable;
    }
}
