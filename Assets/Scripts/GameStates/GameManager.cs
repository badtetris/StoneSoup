using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	/////////////////
	// TYPES AND CONSTANTS GO HERE
	public enum GameMode {
		SingleRoom,
		CombinedRooms,
		Chaos
	}
	public static GameMode gameMode = GameMode.SingleRoom;


	// END TYPES
	/////////////////

	///////////////////
	// PREFABS GO HERE


	// END PREFABS
	///////////////////

	///////////////////////////////////
	// PUBLIC REFERENCE OBJECTS GO HERE
	public LevelGenerator levelGenerator;

	// END PUBLIC REFERENCE OBJECTS
	///////////////////////////////////

	/////////////////////////////////
	// GAME STATE PROPERTIES GO HERE
	protected static GameManager _instance = null;
	public static GameManager instance {
		get { return _instance; }
	}

	// These are protected properties that we only want to access via script (not via inspector)

	// The min/max of the entire world (for the purposes of the camera follow)
	// Set up as (minX, minY, maxX, maxY)
	[HideInInspector]
	public Vector4 worldBoundaries; 

	[HideInInspector]
	public Room[,] roomGrid;

	[HideInInspector]
	public Room currentRoom;
	protected Room _previousRoom; // So we can turn off the previous room in singleroom mode.


	// END GAME STATE PROPERTIES
	/////////////////////////////////

	////////////////////////
	// GAME LOGIC GOES HERE

	public void Awake () {
		_instance = this;
		levelGenerator.generateLevel();
	}

	public void Start() {
	}
	
	public void Update () {
		// In Single Room Mode, we handle room transitions!
		if (gameMode == GameMode.SingleRoom) {
			Player player = Player.instance;
			Room maybeRoomToMoveTo = null;
			int roomGridWidth = roomGrid.GetLength(0);
			int roomGridHeight = roomGrid.GetLength(1);

			// Check if the player's gone up a room
			if (player.y > LevelGenerator.ROOM_HEIGHT*Tile.TILE_SIZE + Tile.TILE_SIZE/2f
				&& currentRoom.roomGridY < roomGridHeight-1) {
				maybeRoomToMoveTo = roomGrid[currentRoom.roomGridX, currentRoom.roomGridY+1];
			}
			else if (player.x > LevelGenerator.ROOM_WIDTH*Tile.TILE_SIZE + Tile.TILE_SIZE/2f
				&& currentRoom.roomGridX < roomGridWidth-1) {
				maybeRoomToMoveTo = roomGrid[currentRoom.roomGridX+1, currentRoom.roomGridY];
			}
			else if (player.y < -Tile.TILE_SIZE/2f
				&& currentRoom.roomGridY > 0) {
				maybeRoomToMoveTo = roomGrid[currentRoom.roomGridX, currentRoom.roomGridY-1];
			}
			else if (player.x < -Tile.TILE_SIZE/2f
				&& currentRoom.roomGridX > 0) {
				maybeRoomToMoveTo = roomGrid[currentRoom.roomGridX-1, currentRoom.roomGridY];
			}

			if (maybeRoomToMoveTo != null) {
				maybeRoomToMoveTo.gameObject.SetActive(true);
				Time.timeScale = 0;
				player.transform.parent = maybeRoomToMoveTo.transform;
				_previousRoom = currentRoom;
				currentRoom = maybeRoomToMoveTo;
				unscaledInvoke("finishRoomTransition", 0.3f);
			}



		}
	}

	public void finishRoomTransition() {
		_previousRoom.gameObject.SetActive(false);
		Time.timeScale = 1;
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
