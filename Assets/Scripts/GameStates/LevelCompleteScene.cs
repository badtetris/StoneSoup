using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteScene : MonoBehaviour {

	public void playAnotherLevel() {
		SceneManager.LoadScene("PlayScene");
	}

	public void returnToMenu() {
		SceneManager.LoadScene("MainMenuScene");
	}
}
