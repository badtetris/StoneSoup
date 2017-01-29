using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteScene : MonoBehaviour {

	public void playAnotherLevel() {
		GameManager.levelNumber++;
		SceneManager.LoadScene("PlayScene");
	}

	public void returnToMenu() {
		GameManager.levelNumber = 0;
		SceneManager.LoadScene("MainMenuScene");
	}
}
