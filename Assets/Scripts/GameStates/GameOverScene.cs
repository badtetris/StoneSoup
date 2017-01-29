using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScene : MonoBehaviour {

	public void tryAgain() {
		GameManager.levelNumber = 0;
		SceneManager.LoadScene("PlayScene");
	}

	public void returnToMenu() {
		GameManager.levelNumber = 0;
		SceneManager.LoadScene("MainMenuScene");
	}
}
