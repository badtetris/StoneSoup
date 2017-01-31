using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompleteScene : MonoBehaviour {

	public Text numLevelsCompletedText;

	void Start() {
		numLevelsCompletedText.text = string.Format("Levels Completed: {0}", GameManager.levelNumber);
	}

	public void playAnotherLevel() {
		GameManager.levelNumber++;
		SceneManager.LoadScene("PlayScene");
	}

	public void returnToMenu() {
		GameManager.levelNumber = 1;
		SceneManager.LoadScene("MainMenuScene");
	}
}
