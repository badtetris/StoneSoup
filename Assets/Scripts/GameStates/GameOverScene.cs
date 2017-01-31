using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScene : MonoBehaviour {

	public Text numLevelsCompletedText;

	void Start() {
		numLevelsCompletedText.text = string.Format("Levels Completed: {0}", GameManager.levelNumber);
	}

	public void tryAgain() {
		GameManager.levelNumber = 0;
		SceneManager.LoadScene("PlayScene");
	}

	public void returnToMenu() {
		GameManager.levelNumber = 0;
		SceneManager.LoadScene("MainMenuScene");
	}
}
