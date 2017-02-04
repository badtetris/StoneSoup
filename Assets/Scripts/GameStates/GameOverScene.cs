using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScene : MonoBehaviour {

	public AudioClip clickSound;

	public Text numLevelsCompletedText;

	void Start() {
		numLevelsCompletedText.text = string.Format("Levels Completed: {0}", GameManager.levelNumber);
	}

	public void tryAgain() {
		AudioManager.playAudio(clickSound);
		GameManager.levelNumber = 0;
		SceneManager.LoadScene("PlayScene");
	}

	public void returnToMenu() {
		AudioManager.playAudio(clickSound);
		GameManager.levelNumber = 0;
		SceneManager.LoadScene("MainMenuScene");
	}
}
