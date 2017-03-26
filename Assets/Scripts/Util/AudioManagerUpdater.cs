using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerUpdater : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		AudioManager.instance.updateAudioLevels();
	}
}
