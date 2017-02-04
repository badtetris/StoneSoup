using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	public AudioClip clickSound;

	public void startGameSingleRoom() {
		AudioManager.playAudio(clickSound);
		GameManager.gameMode = GameManager.GameMode.SingleRoom;
		SceneManager.LoadScene("PlayScene");
	}

	public void startGameCombinedRoom() {
		AudioManager.playAudio(clickSound);
		GameManager.gameMode = GameManager.GameMode.CombinedRooms;
		SceneManager.LoadScene("PlayScene");
	}

	public void startGameChoas() {
		AudioManager.playAudio(clickSound);
		GameManager.gameMode = GameManager.GameMode.Chaos;
		SceneManager.LoadScene("PlayScene");
	}

	public void quit() {
		Application.Quit();
	}
}
