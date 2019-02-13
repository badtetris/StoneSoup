using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The level generator is where the magic happens. 
// Don't worry too much about this implementation.
// The one property you need to pay attention to is the netIDs list 
// as you'll need to put your netID in there to have your rooms be part of the generation.
public class LevelGenerator : MonoBehaviour {

	public const int ROOM_WIDTH = 10;
	public const int ROOM_HEIGHT = 8;

	public const int LOCAL_START_INDEX = 4;

	//public string[] netIDs;

	// For now, we use default rooms for the start and the exit.
	// This helps ensure only one room will generate a player (at the start)
	// and an exit (at the exit)
	public GameObject startRoomPrefab;
	public GameObject exitRoomPrefab;

	// Used by the default room generation implementation to map indices to tile prefabs.
	public GameObject[] globalTilePrefabs;

	// The normal wall tile.
	public GameObject normalWallPrefab;

	// Used to create the border objects (including the transition objects if we're generating in single room mode)
	public GameObject verticalTransitionPrefab;
	public GameObject verticalBorderWallPrefab;
	public GameObject horizontalTransitionPrefab;
	public GameObject horizontalBorderWallPrefab;
	public GameObject indestructibleWallPrefab;

	// How many rooms we have in each dimension.
	public int numXRooms = 4;
	public int numYRooms = 4;

	// We use a "bag" system similar to how tetris pieces are chosen to select which netID will 
	// generate the next room. 
	protected string[] _IDBag = null;
	protected int _currentBagIndex = 0;

	protected void createNewIDBag() {
		_IDBag = new string[ContributorList.instance.activeContributorIDs.Length*2];
		for (int i = 0; i < _IDBag.Length; i++) {
			_IDBag[i] = ContributorList.instance.activeContributorIDs[i % ContributorList.instance.activeContributorIDs.Length];
		}
		GlobalFuncs.shuffle(_IDBag);
		_currentBagIndex = 0;
	}

	protected GameObject nextRoomToSpawn() {
		if (_IDBag == null || _currentBagIndex >= _IDBag.Length) {
			createNewIDBag();
		}
		string netID = _IDBag[_currentBagIndex];
		_currentBagIndex++;

		string roomPath = string.Format("{0}/room", netID);
		return Resources.Load<GameObject>(roomPath); 
	}

	// The function called by the GameManager to actually generate the level.
	public virtual void generateLevel() {
		createNewIDBag();

		if (GameManager.gameMode == GameManager.GameMode.SingleRoom) {
			generateSingleRoomModeLevel();
		}
		else if (GameManager.gameMode == GameManager.GameMode.CombinedRooms
			|| GameManager.gameMode == GameManager.GameMode.Chaos) {
			generateCombinedRoomModeLevel();
		}
	}

	

	public virtual void generateCombinedRoomModeLevel() {
		float totalRoomWidth = Tile.TILE_SIZE*ROOM_WIDTH;
		float totalRoomHeight = Tile.TILE_SIZE*ROOM_HEIGHT;

		Room[,] roomGrid = new Room[numXRooms, numYRooms];

		GameObject borderObjects = new GameObject("border_objects");
		borderObjects.transform.parent = GameManager.instance.transform;
		borderObjects.transform.localPosition = Vector3.zero;

		int currentRoomX = Random.Range(0, numXRooms);
		int currentRoomY = 0;
		List<Room> criticalPath = new List<Room>();


		Dir[] possibleDirsToPath = new Dir[] { Dir.Left, Dir.Left, Dir.Right, Dir.Right, Dir.Up };
		Dir currentDir = GlobalFuncs.randElem(possibleDirsToPath);
		Dir entranceDir = oppositeDir(currentDir);
		// Keep going in our current direction until we hit a will
		bool makingCriticalPath = true;
		GameObject roomToSpawn = startRoomPrefab;

		// This is based on Spelunky's method of building a critical path.
		// This code is kind of a mess and could likely be easily improved.=
		while (makingCriticalPath) {
			// Let's figure out what our required exits are going to be.
			Dir exitDir = Dir.Up;
			int nextRoomX = currentRoomX;
			int nextRoomY = currentRoomY;
			if (currentDir == Dir.Up) {
				if (currentRoomY >= numYRooms-1) {
					makingCriticalPath = false;
				}
				else {
					exitDir = Dir.Up;
					currentDir = GlobalFuncs.randElem(new Dir[] { Dir.Left, Dir.Right });
					nextRoomY++;
				}
			}
			else if (currentDir == Dir.Left) {
				if (currentRoomX <= 0) {
					if (currentRoomY >= numYRooms-1) {
						makingCriticalPath = false;
					}
					else {
						exitDir = Dir.Up;
						currentDir = Dir.Right;
						nextRoomY++;
					}
				}
				else {
					// Move on if we randomly choose to
					if (Random.Range(0, 5) == 0) {
						if (currentRoomY >= numYRooms-1) {
							makingCriticalPath = false;
						}
						else {
							exitDir = Dir.Up;
							nextRoomY++;
						}
					}
					else {
						exitDir = Dir.Left;
						nextRoomX--;
					}
				}
			}
			else if (currentDir == Dir.Right) {
				if (currentRoomX >= numXRooms-1) {
					if (currentRoomY >= numYRooms-1) {
						makingCriticalPath = false;
					}
					else {
						exitDir = Dir.Up;
						currentDir = Dir.Left;
						nextRoomY++;
					}
				}
				else {
					if (Random.Range(0, 5) == 0) {
						if (currentRoomY >= numYRooms-1) {
							makingCriticalPath = false;
						}
						else {
							exitDir = Dir.Up;
							nextRoomY++;
						}
					}
					else {
						exitDir = Dir.Right;
						nextRoomX++;
					}
				}
			}


			if (!makingCriticalPath) {
				roomToSpawn = exitRoomPrefab;
			}

			Room room = null;
            ExitConstraint requiredExits = new ExitConstraint();
			if (roomToSpawn == startRoomPrefab) {
                requiredExits.addDirConstraint(exitDir);
                room = Room.generateRoom(roomToSpawn, this, currentRoomX, currentRoomY, requiredExits);
				GameManager.instance.currentRoom = room;
			}
			else if (!makingCriticalPath) {
                requiredExits.addDirConstraint(entranceDir);
                room = Room.generateRoom(roomToSpawn, this, currentRoomX, currentRoomY, requiredExits);
			}
			else {
                requiredExits.addDirConstraint(entranceDir);
                requiredExits.addDirConstraint(exitDir);
                room = Room.generateRoom(roomToSpawn, this, currentRoomX, currentRoomY, requiredExits);
			}

			roomGrid[currentRoomX, currentRoomY] = room;
			criticalPath.Add(room);
			currentRoomX = nextRoomX;
			currentRoomY = nextRoomY;
			entranceDir = oppositeDir(exitDir);
			roomToSpawn = nextRoomToSpawn();
		}

		for (int x = 0; x < numXRooms; x++) {
			for (int y = 0; y < numYRooms; y++) {
				if (roomGrid[x, y] == null) {
                    roomGrid[x, y] = Room.generateRoom(nextRoomToSpawn(), this, x, y, ExitConstraint.None);
				}
				float roomLeftX = totalRoomWidth*x-Tile.TILE_SIZE/2;
				float roomRightX = totalRoomWidth*(x+1)+Tile.TILE_SIZE/2;
				float roomBottomY = totalRoomHeight*y-Tile.TILE_SIZE/2;
				float roomTopY = totalRoomHeight*(y+1)+Tile.TILE_SIZE/2;

				if (x == 0 && y == 0) {
					Vector2 bottomLeftWallGrid = Tile.toGridCoord(roomLeftX, roomBottomY);
					spawnTileOutsideRoom(indestructibleWallPrefab, borderObjects.transform, (int)bottomLeftWallGrid.x, (int)bottomLeftWallGrid.y);
				}
				if (x == 0 && y == numYRooms-1) {
					Vector2 topLeftWallGrid = Tile.toGridCoord(roomLeftX, roomTopY);
					spawnTileOutsideRoom(indestructibleWallPrefab, borderObjects.transform, (int)topLeftWallGrid.x, (int)topLeftWallGrid.y);
				}
				if (x == numXRooms-1 && y == numYRooms-1) {
					Vector2 topRightWallGrid = Tile.toGridCoord(roomRightX, roomTopY);
					spawnTileOutsideRoom(indestructibleWallPrefab, borderObjects.transform, (int)topRightWallGrid.x, (int)topRightWallGrid.y);
				}
				if (x == numXRooms-1 && y == 0) {
					Vector2 bottomRightWallGrid = Tile.toGridCoord(roomRightX, roomBottomY);
					spawnTileOutsideRoom(indestructibleWallPrefab, borderObjects.transform, (int)bottomRightWallGrid.x, (int)bottomRightWallGrid.y);
				}

				if (x == 0) {
					GameObject wallObj = Instantiate(verticalBorderWallPrefab) as GameObject;
					wallObj.transform.parent = borderObjects.transform;
					wallObj.transform.position = new Vector3(roomLeftX, (roomTopY+roomBottomY)/2f, 0);
				}
				if (x == numXRooms-1) {
					GameObject wallObj = Instantiate(verticalBorderWallPrefab) as GameObject;
					wallObj.transform.parent = borderObjects.transform;
					wallObj.transform.position = new Vector3(roomRightX, (roomTopY+roomBottomY)/2f, 0);
				}
				if (y == 0) {
					GameObject wallObj = Instantiate(horizontalBorderWallPrefab) as GameObject;
					wallObj.transform.parent = borderObjects.transform;
					wallObj.transform.position = new Vector3((roomLeftX+roomRightX)/2f, roomBottomY, 0);
				}
				if (y == numYRooms-1) {
					GameObject wallObj = Instantiate(horizontalBorderWallPrefab) as GameObject;
					wallObj.transform.parent = borderObjects.transform;
					wallObj.transform.position = new Vector3((roomLeftX+roomRightX)/2f, roomTopY, 0);
				}
			}
		}


		GameManager.instance.roomGrid = roomGrid;
		GameManager.instance.borderObjects = borderObjects;

		// Now as a final step, if we're doing chaos mode, we need to randomly rearrange all spawned tiles (that aren't walls, players, or exits)
		if (GameManager.gameMode == GameManager.GameMode.Chaos) {
			List<Tile> tilesToRearrange = new List<Tile>();
			// Go through each room looking for tiles.
			for (int x = 0; x < numXRooms; x++) {
				for (int y = 0; y < numYRooms; y++) {
					Room room = roomGrid[x, y];
					foreach (Tile tile in room.GetComponentsInChildren<Tile>(true)) {
						if (tile.hasTag(TileTags.Player | TileTags.Wall | TileTags.Exit)) {
							continue;
						}
						tilesToRearrange.Add(tile);
					}
				}
			}

			// Now we have a list of tiles, let's randomly shuffle their locations.
			for (int i = 0; i < tilesToRearrange.Count*4; i++) {
				Tile tile1 = GlobalFuncs.randElem(tilesToRearrange);
				Tile tile2 = GlobalFuncs.randElem(tilesToRearrange);

				Transform tile1OldParent = tile1.transform.parent;
				Vector2 tile1OldPosition = tile1.transform.localPosition;

				tile1.transform.parent = tile2.transform.parent;
				tile1.transform.localPosition = new Vector3(tile2.transform.localPosition.x, tile2.transform.localPosition.y, tile1.transform.localPosition.z);

				tile2.transform.parent = tile1OldParent;
				tile2.transform.localPosition = new Vector3(tile1OldPosition.x, tile1OldPosition.y, tile2.transform.localPosition.z);

			}

		}




	}

	public virtual void generateSingleRoomModeLevel() {
		// We add 2 to the tile width/height here to make room for the padding areas.
		float totalRoomWidth = (Tile.TILE_SIZE)*(ROOM_WIDTH+1);
		float totalRoomHeight = (Tile.TILE_SIZE)*(ROOM_HEIGHT+1);

		Room[,] roomGrid = new Room[numXRooms, numYRooms];

		GameObject borderObjects = new GameObject("border_objects");
		borderObjects.transform.parent = GameManager.instance.transform;
		borderObjects.transform.localPosition = Vector3.zero;

		int currentRoomX = Random.Range(0, numXRooms);
		int currentRoomY = 0;
		List<Room> criticalPath = new List<Room>();


		Dir[] possibleDirsToPath = new Dir[] { Dir.Left, Dir.Left, Dir.Right, Dir.Right, Dir.Up };
		Dir currentDir = GlobalFuncs.randElem(possibleDirsToPath);
		Dir entranceDir = oppositeDir(currentDir);
		// Keep going in our current direction until we hit a will
		bool makingCriticalPath = true;
		GameObject roomToSpawn = startRoomPrefab;

		while (makingCriticalPath) {
			// Let's figure out what our required exits are going to be.
			Dir exitDir = Dir.Up;
			int nextRoomX = currentRoomX;
			int nextRoomY = currentRoomY;
			if (currentDir == Dir.Up) {
				if (currentRoomY >= numYRooms-1) {
					makingCriticalPath = false;
				}
				else {
					exitDir = Dir.Up;
					currentDir = GlobalFuncs.randElem(new Dir[] { Dir.Left, Dir.Right });
					nextRoomY++;
				}
			}
			else if (currentDir == Dir.Left) {
				if (currentRoomX <= 0) {
					if (currentRoomY >= numYRooms-1) {
						makingCriticalPath = false;
					}
					else {
						exitDir = Dir.Up;
						currentDir = Dir.Right;
						nextRoomY++;
					}
				}
				else {
					// Move on if we randomly choose to
					if (Random.Range(0, 4) == 0) {
						if (currentRoomY >= numYRooms-1) {
							makingCriticalPath = false;
						}
						else {
							exitDir = Dir.Up;
							nextRoomY++;
						}
					}
					else {
						exitDir = Dir.Left;
						nextRoomX--;
					}
				}
			}
			else if (currentDir == Dir.Right) {
				if (currentRoomX >= numXRooms-1) {
					if (currentRoomY >= numYRooms-1) {
						makingCriticalPath = false;
					}
					else {
						exitDir = Dir.Up;
						currentDir = Dir.Left;
						nextRoomY++;
					}
				}
				else {
					if (Random.Range(0, 4) == 0) {
						if (currentRoomY >= numYRooms-1) {
							makingCriticalPath = false;
						}
						else {
							exitDir = Dir.Up;
							nextRoomY++;
						}
					}
					else {
						exitDir = Dir.Right;
						nextRoomX++;
					}
				}
			}


			if (!makingCriticalPath) {
				roomToSpawn = exitRoomPrefab;
			}

			Room room = null;
            ExitConstraint requiredExits = new ExitConstraint();
			if (roomToSpawn == startRoomPrefab) {
                requiredExits.addDirConstraint(exitDir);
                room = Room.generateRoom(roomToSpawn, this, currentRoomX, currentRoomY, requiredExits);
				GameManager.instance.currentRoom = room;
			}
			else if (!makingCriticalPath) {
                requiredExits.addDirConstraint(entranceDir);
                room = Room.generateRoom(roomToSpawn, this, currentRoomX, currentRoomY, requiredExits);
			}
			else {
                requiredExits.addDirConstraint(entranceDir);
                requiredExits.addDirConstraint(exitDir);
                room = Room.generateRoom(roomToSpawn, this, currentRoomX, currentRoomY, requiredExits);
			}


			roomGrid[currentRoomX, currentRoomY] = room;
			criticalPath.Add(room);
			currentRoomX = nextRoomX;
			currentRoomY = nextRoomY;
			entranceDir = oppositeDir(exitDir);
			roomToSpawn = nextRoomToSpawn();
		}

		for (int x = 0; x < numXRooms; x++) {
			for (int y = 0; y < numYRooms; y++) {
				if (roomGrid[x, y] == null) {
                    roomGrid[x, y] = Room.generateRoom(nextRoomToSpawn(), this, x, y, ExitConstraint.None);
				}
				if (roomGrid[x, y] != GameManager.instance.currentRoom) {
					roomGrid[x, y].gameObject.SetActive(false);
				}
				float roomLeftX = totalRoomWidth*x-Tile.TILE_SIZE/2;
				float roomRightX = totalRoomWidth*(x+1)-Tile.TILE_SIZE/2;
				float roomBottomY = totalRoomHeight*y-Tile.TILE_SIZE/2;
				float roomTopY = totalRoomHeight*(y+1)-Tile.TILE_SIZE/2;

				// Always spawn a lower left wall.
				Vector2 bottomLeftWallGrid = Tile.toGridCoord(roomLeftX, roomBottomY);
				spawnTileOutsideRoom(indestructibleWallPrefab, borderObjects.transform, (int)bottomLeftWallGrid.x, (int)bottomLeftWallGrid.y);
				if (y == numYRooms-1) {
					// Spawn an upper left wall if we're at the top
					Vector2 topLeftWallGrid = Tile.toGridCoord(roomLeftX, roomTopY);
					spawnTileOutsideRoom(indestructibleWallPrefab, borderObjects.transform, (int)topLeftWallGrid.x, (int)topLeftWallGrid.y);
				}
				if (x == numXRooms-1) {
					// Spawn a lower right wall if we're at the right
					Vector2 bottomRightWallGrid = Tile.toGridCoord(roomRightX, roomBottomY);
					spawnTileOutsideRoom(indestructibleWallPrefab, borderObjects.transform, (int)bottomRightWallGrid.x, (int)bottomRightWallGrid.y);
				}
				if (x == numXRooms-1 && y == numYRooms-1) {
					// Spawn an upper right wall only if we're the upper right corner
					Vector2 topRightWallGrid = Tile.toGridCoord(roomRightX, roomTopY);
					spawnTileOutsideRoom(indestructibleWallPrefab, borderObjects.transform, (int)topRightWallGrid.x, (int)topRightWallGrid.y);
				}

				// Now spawn the walls and the transitions objects.
				if (x > 0) {
					GameObject transitionObj = Instantiate(verticalTransitionPrefab) as GameObject;
					transitionObj.transform.parent = borderObjects.transform;
					transitionObj.transform.position = new Vector3(roomLeftX, (roomBottomY+roomTopY)/2f, 0);
				}
				else {
					GameObject wallObj = Instantiate(verticalBorderWallPrefab) as GameObject;
					wallObj.transform.parent = borderObjects.transform;
					wallObj.transform.position = new Vector3(roomLeftX, (roomTopY+roomBottomY)/2f, 0);
				}
				if (x == numXRooms-1) {
					GameObject wallObj = Instantiate(verticalBorderWallPrefab) as GameObject;
					wallObj.transform.parent = borderObjects.transform;
					wallObj.transform.position = new Vector3(roomRightX, (roomTopY+roomBottomY)/2f, 0);
				}

				if (y > 0) {
					GameObject transitionObj = Instantiate(horizontalTransitionPrefab) as GameObject;
					transitionObj.transform.parent = borderObjects.transform;
					transitionObj.transform.position = new Vector3((roomLeftX+roomRightX)/2f, roomBottomY, 0);
				}
				else {
					GameObject wallObj = Instantiate(horizontalBorderWallPrefab) as GameObject;
					wallObj.transform.parent = borderObjects.transform;
					wallObj.transform.position = new Vector3((roomLeftX+roomRightX)/2f, roomBottomY, 0);
				}
				if (y == numYRooms-1) {
					GameObject wallObj = Instantiate(horizontalBorderWallPrefab) as GameObject;
					wallObj.transform.parent = borderObjects.transform;
					wallObj.transform.position = new Vector3((roomLeftX+roomRightX)/2f, roomTopY, 0);
				}
			}
		}

		GameManager.instance.roomGrid = roomGrid;
		GameManager.instance.borderObjects = borderObjects;


		// Finally, activate the letterbox
		float letterBoxTop = ((ROOM_HEIGHT+2)*Tile.TILE_SIZE)/2+Tile.TILE_SIZE/2f;
		float letterBoxBottom = -((ROOM_HEIGHT+2)*Tile.TILE_SIZE)/2+Tile.TILE_SIZE/2f;
		float letterBoxRight = ((ROOM_WIDTH+2)*Tile.TILE_SIZE)/2;
		float letterBoxLeft = -letterBoxRight;
		GameManager.instance.letterBox.activateLetterBox(letterBoxTop, letterBoxRight, letterBoxBottom, letterBoxLeft);


	}


	// Utility functions used by the generator.

	protected Tile spawnTileOutsideRoom(GameObject tilePrefab, Transform parentOfTile, int gridX, int gridY) {
		GameObject tileObj = Instantiate(tilePrefab) as GameObject;
		tileObj.transform.parent = parentOfTile;
		Tile tile = tileObj.GetComponent<Tile>();
		Vector2 tilePos = Tile.toWorldCoord(gridX, gridY);
		tile.localX = tilePos.x;
		tile.localY = tilePos.y;
		tile.init();
		return tile;
	}


	protected Dir oppositeDir(Dir dir) {
		if (dir == Dir.Up) {
			return Dir.Down;
		}
		else if (dir == Dir.Right) {
			return Dir.Left;
		}
		else if (dir == Dir.Down) {
			return Dir.Up;
		}
		else {
			return Dir.Right;
		}
	}

	
}
