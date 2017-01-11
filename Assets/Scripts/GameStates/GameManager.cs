using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	/////////////////
	// TYPES AND CONSTANTS GO HERE

	// END TYPES
	/////////////////

	///////////////////
	// PREFABS GO HERE

	public GameObject[] generalTilePrefabs;


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



	// END GAME STATE PROPERTIES
	/////////////////////////////////

	////////////////////////
	// GAME LOGIC GOES HERE

	public void Awake () {
		_instance = this;
	}

	public void Start() {
		levelGenerator.generateLevel();
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
