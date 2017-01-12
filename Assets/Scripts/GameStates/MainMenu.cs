using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	public void startGameSingleRoom() {
		GameManager.gameMode = GameManager.GameMode.SingleRoom;
		SceneManager.LoadScene("PlayScene");
	}

	public void startGameCombinedRoom() {
		GameManager.gameMode = GameManager.GameMode.CombinedRooms;
		SceneManager.LoadScene("PlayScene");
	}

	public void startGameChoas() {
		GameManager.gameMode = GameManager.GameMode.Chaos;
		SceneManager.LoadScene("PlayScene");
	}
}
