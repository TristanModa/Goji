using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.Rendering.DebugUI;

public class AudioManager : MonoBehaviour
{
	#region Properties
	public static AudioManager Instance
	{
		get => _instance;
		private set
		{
			// Avoid setting the singleton if it is not null
			if (_instance != null)
			{
				Destroy(value);
				return;
			}

			_instance = value;
		}
	}

	private AudioSource MusicSource { get; set; }

	private Dictionary<string, AudioClip> SFXDictionary { get; set; }
	private List<AudioSource> ActiveSources { get; set; }
	#endregion

	#region Settings
	[SerializeField]
	AudioClip musicClip;

	[SerializeField]
	List<AudioClip> sfxClips = new List<AudioClip>();
	#endregion

	#region Fields
	private static AudioManager _instance;
	#endregion

	#region Methods
	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		// Create the SFX Dictionary and add audio clips to it
		SFXDictionary = new Dictionary<string, AudioClip>();
		foreach (AudioClip clip in sfxClips)
		{
			string name = clip.name.Replace("SFX", "");
			SFXDictionary.Add(name, clip);
		}

		// Create the active audio sources list
		ActiveSources = new List<AudioSource>();

		// Create the music source
		GameObject musicSourceObject = new GameObject();
		musicSourceObject.name = "Music Source";
		musicSourceObject.transform.parent = transform;
		MusicSource = musicSourceObject.AddComponent<AudioSource>();
		MusicSource.loop = true;
		MusicSource.clip = musicClip;
		MusicSource.Play();
	}

	private void Update()
	{
		foreach (AudioSource source in ActiveSources.FindAll(x => !x.isPlaying))
		{
			ActiveSources.Remove(source);
			Destroy(source.gameObject);
		}
	}

	public static void PlaySFX(string name)
	{
		if (!Instance.SFXDictionary.TryGetValue(name, out AudioClip clip))
		{
			Debug.Log($"Attempted to play SFX \"{name}\" but that SFX does not exist.");
			return;
		}

		GameObject sourceObject = new GameObject();
		sourceObject.name = $"{name} SFX Instance";
		sourceObject.transform.parent = Instance.transform;
		
		AudioSource source = sourceObject.AddComponent<AudioSource>();
		source.clip = clip;
		source.Play();

		Instance.ActiveSources.Add(source);
	}
	#endregion
}
