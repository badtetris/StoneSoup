using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is the class you should use to play audio to avoid playing too many sound effects at once.
// (and so you don't have to worry about making your own audio sources)
public class AudioManager {

	// The maximum number of SFX that can play at one time.
	public const int MAX_AUDIO_SOURCES = 5;


	protected static AudioManager _instance = null;
	public static AudioManager instance {
		get { return _instance; }
	}
	protected static void maybeInitInstance() {
		if (_instance == null) {
			_instance = new AudioManager();
		}
	}

	protected GameObject _audioObj;

	protected AudioSource[] _audioSources;

	public AudioManager() {
		_audioObj = new GameObject("_audio_manager");
		GameObject.DontDestroyOnLoad(_audioObj);
		_audioSources = new AudioSource[MAX_AUDIO_SOURCES];
		for (int i = 0; i < MAX_AUDIO_SOURCES; i++) {
			AudioSource newSource = _audioObj.AddComponent<AudioSource>();
			newSource.playOnAwake = false;
			_audioSources[i] = newSource;
		}
		_audioObj.AddComponent<AudioManagerUpdater>();
	}

	public void updateAudioLevels() {
		int numAudioPlaying = 0;
		foreach (AudioSource source in _audioSources) {
			if (source.isPlaying) {
				numAudioPlaying++;
			}
		}
		float volume = 1f / (float)numAudioPlaying;
		foreach (AudioSource source in _audioSources) {
			if (source.isPlaying) {
				source.volume = volume;
			}
		}

	}

	// The function you should use to play audio.
	// Returns true if it successfully found a free audio source to play the audio
	// false otherwise.
	public static bool playAudio(AudioClip audio) {
		maybeInitInstance();
		return _instance.instancePlayAudio(audio);
	}

	public bool instancePlayAudio(AudioClip audio) {
		foreach (AudioSource maybeSource in _audioSources) {
			if (!maybeSource.isPlaying) {
				maybeSource.clip = audio;
				maybeSource.Play();
				return true;
			}
		}
		return false;
	}

	public static bool stopAudio(AudioClip audio) {
		maybeInitInstance();
		return _instance.instanceStopAudio(audio);
	}

	public bool instanceStopAudio(AudioClip audio) {
		foreach (AudioSource maybeSource in _audioSources) {
			if (maybeSource.isPlaying && maybeSource.clip == audio) {
				maybeSource.Stop();
				return true;
			}
		}
		return false;
	}


}
