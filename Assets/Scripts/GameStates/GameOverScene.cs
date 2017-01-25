using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScene : MonoBehaviour {

	public void tryAgain() {
		SceneManager.LoadScene("PlayScene");
	}

	public void returnToMenu() {
		SceneManager.LoadScene("MainMenuScene");
	}
}
