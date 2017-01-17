using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour {

	public const int ROOM_WIDTH = 10;
	public const int ROOM_HEIGHT = 8;

	public const int LOCAL_START_INDEX = 3;

	public GameObject startRoomPrefab;
	public GameObject[] otherRoomPrefabs;


	public GameObject[] globalTilePrefabs;

	public GameObject verticalTransitionPrefab;
	public GameObject verticalBorderWallPrefab;
	public GameObject horizontalTransitionPrefab;
	public GameObject horizontalBorderWallPrefab;

	public int numXRooms = 4;
	public int numYRooms = 4;


	public virtual void generateLevel() {
		if (GameManager.gameMode == GameManager.GameMode.SingleRoom) {
			generateSingleRoomModeLevel();
		}
	}

	public virtual void generateSingleRoomModeLevel() {
		// We work by spawning rooms, positioning them, and having the rooms generate their items.

		// TODO: build a critical path here and make sure that's included in the calculation.
		// TODO: Create an invisible boundary that prevents the player from moving off screen.

		// We add 2 to the tile width/height here to make room for the padding areas.
		float totalRoomWidth = (Tile.TILE_SIZE)*(ROOM_WIDTH+1);
		float totalRoomHeight = (Tile.TILE_SIZE)*(ROOM_HEIGHT+1);

		Room[,] roomGrid = new Room[numXRooms, numYRooms];


		for (int x = 0; x < numXRooms; x++) {
			for (int y = 0; y < numYRooms; y++) {
				GameObject roomToSpawn = GlobalFuncs.getRandom(otherRoomPrefabs);

				bool isStartRoom = x == numXRooms/2 && y == 0;
				if (isStartRoom) {
					roomToSpawn = startRoomPrefab;
				}

				GameObject roomObj = Instantiate(roomToSpawn) as GameObject;
				roomObj.transform.parent = GameManager.instance.transform;
				// Position our new room first.
				roomObj.transform.localPosition = new Vector3(totalRoomWidth*x, totalRoomHeight*y, 0);

				Room room = roomObj.GetComponent<Room>();
				room.generateRoom(this);
				roomGrid[x, y] = room;
				room.roomGridX = x;
				room.roomGridY = y;

				if (isStartRoom) {
					GameManager.instance.currentRoom = room;
				}
				else {
					roomObj.SetActive(false);
				}

				// Spawn the objects to sit in the padding between rooms in single room mode.

				float roomLeftX = totalRoomWidth*x-Tile.TILE_SIZE/2;
				float roomRightX = totalRoomWidth*(x+1)-Tile.TILE_SIZE/2;
				float roomBottomY = totalRoomHeight*y-Tile.TILE_SIZE/2;
				float roomTopY = totalRoomHeight*(y+1)-Tile.TILE_SIZE/2;

				// Always spawn a lower left wall.
				Vector2 bottomLeftWallGrid = Tile.toGridCoord(roomLeftX, roomBottomY);
				Tile.spawnTile(globalTilePrefabs[0], GameManager.instance.transform, (int)bottomLeftWallGrid.x, (int)bottomLeftWallGrid.y);
				if (y == numYRooms-1) {
					// Spawn an upper left wall if we're at the top
					Vector2 topLeftWallGrid = Tile.toGridCoord(roomLeftX, roomTopY);
					Tile.spawnTile(globalTilePrefabs[0], GameManager.instance.transform, (int)topLeftWallGrid.x, (int)topLeftWallGrid.y);
				}
				if (x == numXRooms-1) {
					// Spawn a lower right wall if we're at the right
					Vector2 bottomRightWallGrid = Tile.toGridCoord(roomRightX, roomBottomY);
					Tile.spawnTile(globalTilePrefabs[0], GameManager.instance.transform, (int)bottomRightWallGrid.x, (int)bottomRightWallGrid.y);
				}
				if (x == numXRooms-1 && y == numYRooms-1) {
					// Spawn an upper right wall only if we're the upper right corner
					Vector2 topRightWallGrid = Tile.toGridCoord(roomRightX, roomTopY);
					Tile.spawnTile(globalTilePrefabs[0], GameManager.instance.transform, (int)topRightWallGrid.x, (int)topRightWallGrid.y);
				}

				// Now spawn the walls and the transitions objects.
				if (x > 0) {
					GameObject transitionObj = Instantiate(verticalTransitionPrefab) as GameObject;
					transitionObj.transform.parent = GameManager.instance.transform;
					transitionObj.transform.position = new Vector3(roomLeftX, (roomBottomY+roomTopY)/2f, 0);
				}
				else {
					GameObject wallObj = Instantiate(verticalBorderWallPrefab) as GameObject;
					wallObj.transform.parent = GameManager.instance.transform;
					wallObj.transform.position = new Vector3(roomLeftX, (roomTopY+roomBottomY)/2f, 0);
				}
				if (x == numXRooms-1) {
					GameObject wallObj = Instantiate(verticalBorderWallPrefab) as GameObject;
					wallObj.transform.parent = GameManager.instance.transform;
					wallObj.transform.position = new Vector3(roomRightX, (roomTopY+roomBottomY)/2f, 0);
				}

				if (y > 0) {
					GameObject transitionObj = Instantiate(horizontalTransitionPrefab) as GameObject;
					transitionObj.transform.parent = GameManager.instance.transform;
					transitionObj.transform.position = new Vector3((roomLeftX+roomRightX)/2f, roomBottomY, 0);
				}
				else {
					GameObject wallObj = Instantiate(horizontalBorderWallPrefab) as GameObject;
					wallObj.transform.parent = GameManager.instance.transform;
					wallObj.transform.position = new Vector3((roomLeftX+roomRightX)/2f, roomBottomY, 0);
				}
				if (y == numYRooms-1) {
					GameObject wallObj = Instantiate(horizontalBorderWallPrefab) as GameObject;
					wallObj.transform.parent = GameManager.instance.transform;
					wallObj.transform.position = new Vector3((roomLeftX+roomRightX)/2f, roomTopY, 0);
				}




			}
		}

		GameManager.instance.roomGrid = roomGrid;


	}




	
}
