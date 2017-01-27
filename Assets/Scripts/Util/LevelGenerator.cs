using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour {

	public const int ROOM_WIDTH = 10;
	public const int ROOM_HEIGHT = 8;

	public const int LOCAL_START_INDEX = 4;

	public GameObject startRoomPrefab;
	public GameObject exitRoomPrefab;

	public string[] netIDs;


	public GameObject[] globalTilePrefabs;

	public GameObject normalWallPrefab;

	public GameObject verticalTransitionPrefab;
	public GameObject verticalBorderWallPrefab;
	public GameObject horizontalTransitionPrefab;
	public GameObject horizontalBorderWallPrefab;
	public GameObject indestructibleWallPrefab;

	public int numXRooms = 4;
	public int numYRooms = 4;


	protected GameObject nextRoomToSpawn() {
		string netID = GlobalFuncs.getRandom(netIDs);
		string roomPath = string.Format("{0}/room", netID);
		return Resources.Load<GameObject>(roomPath); 
	}

	public virtual void generateLevel() {
		if (GameManager.gameMode == GameManager.GameMode.SingleRoom) {
			generateSingleRoomModeLevel();
		}
		else if (GameManager.gameMode == GameManager.GameMode.CombinedRooms) {
			generateCombinedRoomModeLevel();
		}
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
		Dir currentDir = GlobalFuncs.getRandom(possibleDirsToPath);
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
					currentDir = GlobalFuncs.getRandom(new Dir[] { Dir.Left, Dir.Right });
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

			GameObject roomObj = Instantiate(roomToSpawn) as GameObject;
			roomObj.transform.parent = GameManager.instance.transform;
			roomObj.transform.localPosition = new Vector3(totalRoomWidth*currentRoomX, totalRoomHeight*currentRoomY, 0);
			Room room = roomObj.GetComponent<Room>();

			if (roomToSpawn == startRoomPrefab) {
				room.generateRoom(this, exitDir);
				GameManager.instance.currentRoom = room;
			}
			else if (!makingCriticalPath) {
				room.generateRoom(this, entranceDir);
			}
			else {
				room.generateRoom(this, entranceDir, exitDir);
			}

			room.roomGridX = currentRoomX;
			room.roomGridY = currentRoomY;

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
					GameObject roomObj = Instantiate(nextRoomToSpawn()) as GameObject;
					roomObj.transform.parent = GameManager.instance.transform;
					roomObj.transform.localPosition = new Vector3(totalRoomWidth*x, totalRoomHeight*y, 0);
					Room room = roomObj.GetComponent<Room>();
					room.generateRoom(this);
					room.roomGridX = x;
					room.roomGridY = y;
					roomGrid[x, y] = room;
				}
				float roomLeftX = totalRoomWidth*x-Tile.TILE_SIZE/2;
				float roomRightX = totalRoomWidth*(x+1)+Tile.TILE_SIZE/2;
				float roomBottomY = totalRoomHeight*y-Tile.TILE_SIZE/2;
				float roomTopY = totalRoomHeight*(y+1)+Tile.TILE_SIZE/2;

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


	}

	public virtual void generateSingleRoomModeLevel() {
		// We work by spawning rooms, positioning them, and having the rooms generate their items.


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
		Dir currentDir = GlobalFuncs.getRandom(possibleDirsToPath);
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
					currentDir = GlobalFuncs.getRandom(new Dir[] { Dir.Left, Dir.Right });
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

			GameObject roomObj = Instantiate(roomToSpawn) as GameObject;
			roomObj.transform.parent = GameManager.instance.transform;
			roomObj.transform.localPosition = new Vector3(totalRoomWidth*currentRoomX, totalRoomHeight*currentRoomY, 0);
			Room room = roomObj.GetComponent<Room>();

			if (roomToSpawn == startRoomPrefab) {
				room.generateRoom(this, exitDir);
				GameManager.instance.currentRoom = room;
			}
			else if (!makingCriticalPath) {
				room.generateRoom(this, entranceDir);
			}
			else {
				room.generateRoom(this, entranceDir, exitDir);
			}

			room.roomGridX = currentRoomX;
			room.roomGridY = currentRoomY;

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
					GameObject roomObj = Instantiate(nextRoomToSpawn()) as GameObject;
					roomObj.transform.parent = GameManager.instance.transform;
					roomObj.transform.localPosition = new Vector3(totalRoomWidth*x, totalRoomHeight*y, 0);
					Room room = roomObj.GetComponent<Room>();
					room.generateRoom(this);
					room.roomGridX = x;
					room.roomGridY = y;
					roomGrid[x, y] = room;
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
				Tile.spawnTile(indestructibleWallPrefab, borderObjects.transform, (int)bottomLeftWallGrid.x, (int)bottomLeftWallGrid.y);
				if (y == numYRooms-1) {
					// Spawn an upper left wall if we're at the top
					Vector2 topLeftWallGrid = Tile.toGridCoord(roomLeftX, roomTopY);
					Tile.spawnTile(indestructibleWallPrefab, borderObjects.transform, (int)topLeftWallGrid.x, (int)topLeftWallGrid.y);
				}
				if (x == numXRooms-1) {
					// Spawn a lower right wall if we're at the right
					Vector2 bottomRightWallGrid = Tile.toGridCoord(roomRightX, roomBottomY);
					Tile.spawnTile(indestructibleWallPrefab, borderObjects.transform, (int)bottomRightWallGrid.x, (int)bottomRightWallGrid.y);
				}
				if (x == numXRooms-1 && y == numYRooms-1) {
					// Spawn an upper right wall only if we're the upper right corner
					Vector2 topRightWallGrid = Tile.toGridCoord(roomRightX, roomTopY);
					Tile.spawnTile(indestructibleWallPrefab, borderObjects.transform, (int)topRightWallGrid.x, (int)topRightWallGrid.y);
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


		// Finally, activate the letterbox
		float letterBoxTop = ((ROOM_HEIGHT+2)*Tile.TILE_SIZE)/2+Tile.TILE_SIZE/2f;
		float letterBoxBottom = -((ROOM_HEIGHT+2)*Tile.TILE_SIZE)/2+Tile.TILE_SIZE/2f;
		float letterBoxRight = ((ROOM_WIDTH+2)*Tile.TILE_SIZE)/2;
		float letterBoxLeft = -letterBoxRight;
		GameManager.instance.letterBox.activateLetterBox(letterBoxTop, letterBoxRight, letterBoxBottom, letterBoxLeft);


	}




	
}
