using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	/////////////////
	// TYPES AND CONSTANTS GO HERE
	public enum GameMode {
		SingleRoom,
		CombinedRooms,
		Chaos
	}
	public static GameMode gameMode = GameMode.CombinedRooms;


	// END TYPES
	/////////////////

	///////////////////
	// PREFABS GO HERE


	// END PREFABS
	///////////////////

	///////////////////////////////////
	// PUBLIC REFERENCE OBJECTS GO HERE
	public LevelGenerator levelGenerator;

	public LetterBox letterBox; // Used by single room mode to make sure we only see one room at a time.


	// END PUBLIC REFERENCE OBJECTS
	///////////////////////////////////

	/////////////////////////////////
	// GAME STATE PROPERTIES GO HERE
	protected static GameManager _instance = null;
	public static GameManager instance {
		get { return _instance; }
	}


	// A counter for how many levels we've played so far. 
	// Perhaps you could use this in your generation to make your rooms steadily increase in difficulty.
	public static int levelNumber = 0;

	// These are protected properties that we only want to access via script (not via inspector)

	[HideInInspector]
	public Room[,] roomGrid;

	[HideInInspector]
	public Room currentRoom;
	protected Room _previousRoom; // So we can turn off the previous room in singleroom mode.

	[HideInInspector]
	public GameObject borderObjects;

	protected bool _gameIsOver = false;
	public bool gameIsOver {
		get { return _gameIsOver; }
	}

	// END GAME STATE PROPERTIES
	/////////////////////////////////

	////////////////////////
	// GAME LOGIC GOES HERE

	public void Awake () {
		Application.targetFrameRate = 60;
		_instance = this;
		levelGenerator.generateLevel();
	}

	public void Start() {
	}
	
	public void Update () {
		if (_gameIsOver) {
			return;
		}

		// See if we should return to the main menu screen?
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SceneManager.LoadScene("MainMenuScene");
		}


		// In Single Room Mode, we handle room transitions!
		if (gameMode == GameMode.SingleRoom) {
			Player player = Player.instance;
			Room maybeRoomToMoveTo = null;
			int roomGridWidth = roomGrid.GetLength(0);
			int roomGridHeight = roomGrid.GetLength(1);

			// Check if the player's gone up a room
			if (player.localY > LevelGenerator.ROOM_HEIGHT*Tile.TILE_SIZE + 3*Tile.TILE_SIZE/4f
				&& currentRoom.roomGridY < roomGridHeight-1) {
				maybeRoomToMoveTo = roomGrid[currentRoom.roomGridX, currentRoom.roomGridY+1];
			}
			else if (player.localX > LevelGenerator.ROOM_WIDTH*Tile.TILE_SIZE + 3*Tile.TILE_SIZE/4f
				&& currentRoom.roomGridX < roomGridWidth-1) {
				maybeRoomToMoveTo = roomGrid[currentRoom.roomGridX+1, currentRoom.roomGridY];
			}
			else if (player.localY < -3*Tile.TILE_SIZE/4f
				&& currentRoom.roomGridY > 0) {
				maybeRoomToMoveTo = roomGrid[currentRoom.roomGridX, currentRoom.roomGridY-1];
			}
			else if (player.localX < -3*Tile.TILE_SIZE/4f
				&& currentRoom.roomGridX > 0) {
				maybeRoomToMoveTo = roomGrid[currentRoom.roomGridX-1, currentRoom.roomGridY];
			}

			if (maybeRoomToMoveTo != null) {
				maybeRoomToMoveTo.gameObject.SetActive(true);
				Time.timeScale = 0;
				player.transform.parent = maybeRoomToMoveTo.transform;
				_previousRoom = currentRoom;
				currentRoom = maybeRoomToMoveTo;
				unscaledInvoke("finishRoomTransition", 0.5f);
			}
		}
		// Otherwise, just detect our current room based on where the player is
		else {
			Player player = Player.instance;

			int playerRoomX = Mathf.FloorToInt(player.transform.position.x / (LevelGenerator.ROOM_WIDTH*Tile.TILE_SIZE));
			int playerRoomY = Mathf.FloorToInt(player.transform.position.y / (LevelGenerator.ROOM_HEIGHT*Tile.TILE_SIZE));
			int numXRooms = roomGrid.GetLength(0);
			int numYRooms = roomGrid.GetLength(1);
			if (playerRoomX >= 0 && playerRoomX < numXRooms && playerRoomY >= 0 && playerRoomY < numYRooms) {
				currentRoom = roomGrid[playerRoomX, playerRoomY];
				Player.instance.transform.parent = currentRoom.transform;
			}


		}

	}

	public void finishRoomTransition() {
		_previousRoom.gameObject.SetActive(false);
		Time.timeScale = 1;
	}

	public void playerJustDefeated(Tile tileThatKilledPlayer) {
		if (_gameIsOver) {
			return;
		}
		Time.timeScale = 0;
		_gameIsOver = true;
		// Start by making the player and tile that killed the player not children of the rooms
		Player.instance.transform.parent = transform;
		tileThatKilledPlayer.transform.parent = transform;
		tileThatKilledPlayer.transform.localPosition = new Vector3(tileThatKilledPlayer.transform.localPosition.x, tileThatKilledPlayer.transform.localPosition.y, Player.instance.transform.localPosition.z-0.1f);
		borderObjects.SetActive(false);
		StartCoroutine(playerDefeatedSequence());
	}

	protected IEnumerator playerDefeatedSequence() {
		if (gameMode == GameMode.SingleRoom) {
			currentRoom.gameObject.SetActive(false);
		}
		else {
			int numXRooms = roomGrid.GetLength(0);
			int numYRooms = roomGrid.GetLength(1);
			for (int x = 0; x < numXRooms; x++) {
				for (int y = 0; y < numYRooms; y++) {
					roomGrid[x, y].gameObject.SetActive(false);
					float t = 0.05f;
					while (t > 0) {
						yield return 0;
						t -= Time.unscaledDeltaTime;
					}
				}
			}
		}
		float finalT = 0.5f;
		while (finalT > 0) {
			yield return 0;
			finalT -= Time.unscaledDeltaTime;
		}
		Time.timeScale = 1;
		SceneManager.LoadScene("GameOverScene");

	}

	public void playerJustWon() {
		if (_gameIsOver) {
			return;
		}
		levelNumber++;
		Time.timeScale = 0;
		_gameIsOver = true;
		borderObjects.SetActive(false);
		int numXRooms = roomGrid.GetLength(0);
		int numYRooms = roomGrid.GetLength(1);
		for (int x = 0; x < numXRooms; x++) {
			for (int y = 0; y < numYRooms; y++) {
				if (roomGrid[x, y] != currentRoom) {
					roomGrid[x, y].gameObject.SetActive(false);
				}
			}
		}
		StartCoroutine(wonSequence());
	}

	protected IEnumerator wonSequence() {
		foreach (Transform child in currentRoom.transform) {
			Tile tile = child.GetComponent<Tile>();
			if (tile != null && !tile.hasTag(TileTags.Player | TileTags.Exit)) {
				tile.gameObject.SetActive(false);
				float t = 0.05f;
				while (t > 0) {
					yield return 0;
					t -= Time.unscaledDeltaTime;
				}
			}
		}
		Time.timeScale = 1;
		SceneManager.LoadScene("LevelCompleteScene");
	}

	// END GAME LOGIC
	////////////////////////

	////////////////////////////
	// BUTTON FUNCTIONS GO HERE

	// END BUTTON FUNCTIONS
	////////////////////////////

	////////////////////////////
	// UTILITY FUNCTIONS GO HERE

	protected void unscaledInvoke(string messageName, float time) {
		StartCoroutine(doUnscaledInvoke(messageName, time));
	}
	protected IEnumerator doUnscaledInvoke(string messageName, float time) {
		float t = time;
		while (t > 0) {
			yield return 0;
			t -= Time.unscaledDeltaTime;
		}
		SendMessage(messageName);
	}



	// END UTILITY FUNCTIONS
	////////////////////////////
}
