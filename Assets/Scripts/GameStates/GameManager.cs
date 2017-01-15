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
	[HideInInspector]
	public Vector2 currentRoomPosition; // The center position of the current room.

	[HideInInspector]
	public GameObject currentRoomObj; // Used primarily by single room mode to ensure there's only one room on at a time.

	[HideInInspector]
	public string currentRoomAuthor = "Student A";

	// The min/max of the entire world (for the purposes of the camera follow)
	// Set up as (minX, minY, maxX, maxY)
	[HideInInspector]
	public Vector4 worldBoundaries; 

	[HideInInspector]
	public RoomGenerator[,] rooms;


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
		
	}

	// END GAME LOGIC
	////////////////////////

	////////////////////////////
	// BUTTON FUNCTIONS GO HERE

	// END BUTTON FUNCTIONS
	////////////////////////////

	////////////////////////////
	// UTILITY FUNCTIONS GO HERE

	// END UTILITY FUNCTIONS
	////////////////////////////
}
