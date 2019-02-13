using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// The Room class is the parent of your room generators.
//
public class Room : MonoBehaviour {


	// This is the function used by the level generator to generate the whole room. 
	// It makes use of the "createRoom" and "fillRoom" functions to decide what sort of room to create and also how to fill it
	public static Room generateRoom(GameObject roomPrefab, LevelGenerator ourGenerator, int roomX, int roomY, ExitConstraint requiredExits) {
		Room newRoom = roomPrefab.GetComponent<Room>().createRoom(requiredExits);

		float totalRoomWidth = Tile.TILE_SIZE*LevelGenerator.ROOM_WIDTH;
		float totalRoomHeight = Tile.TILE_SIZE*LevelGenerator.ROOM_HEIGHT;
		if (GameManager.gameMode == GameManager.GameMode.SingleRoom) {
			totalRoomWidth = Tile.TILE_SIZE*(LevelGenerator.ROOM_WIDTH+1);
			totalRoomHeight = Tile.TILE_SIZE*(LevelGenerator.ROOM_HEIGHT+1);
		}

		

		newRoom.transform.parent = GameManager.instance.transform;
		newRoom.transform.localPosition = new Vector3(totalRoomWidth*roomX, totalRoomHeight*roomY, 0);
		
		newRoom.roomGridX = roomX;
		newRoom.roomGridY = roomY;
		
		newRoom.fillRoom(ourGenerator, requiredExits);

		return newRoom;	
	}


	// Set this value to let everyone know who made the current room. 
	public string roomAuthor = "";

	// The default room implementation uses a simple text file 
	// To spawn a set of designed prefabs. 
	public TextAsset designedRoomFile;
	public GameObject[] localTilePrefabs;

	// Used by single room mode for centering the camera properly
	public Vector2 roomCenter {
		get { 
			return new Vector2(transform.localPosition.x+LevelGenerator.ROOM_WIDTH*Tile.TILE_SIZE/2,
				transform.localPosition.y+LevelGenerator.ROOM_HEIGHT*Tile.TILE_SIZE/2);
		}
	}

	// This will be set by the level generator. Don't touch it yourself.
	[HideInInspector]
	public int roomGridX, roomGridY;


	// The create room function actually instantiates a relevant prefab for a room. 
	// A good question you might have is why there's a separate function for creating rooms and why we don't 
	// just instantiate the provided room prefab. 
	// The reason is we sometimes want to make a room that instantiates OTHER room prefabs 
	// For example, a room might want to randomly select from other rooms when generating.
	public virtual Room createRoom(ExitConstraint requiredExits) {
		GameObject roomObj = Instantiate(gameObject);
		return roomObj.GetComponent<Room>();
	}

	// The fillRoom function is the one you'll need to override to fill your rooms.
	// It takes the LevelGenerator as a parameter so you have access to more global prefabs (like walls)
	// Additionaly, it takes an array of exits that need to exist (your room will have required exits if it's on the critical path).
	// For an exit to "exist", there can't be a wall in the center part of that edge of the room.

	// This implementation pulls a designed room out of a text file.
    public virtual void fillRoom(LevelGenerator ourGenerator, ExitConstraint requiredExits) {

		string initialGridString = designedRoomFile.text;
		string[] rows = initialGridString.Trim().Split('\n');
		int width = rows[0].Trim().Split(',').Length;
		int height = rows.Length;
		if (height != LevelGenerator.ROOM_HEIGHT) {
			throw new UnityException(string.Format("Error in room by {0}. Wrong height, Expected: {1}, Got: {2}", roomAuthor, LevelGenerator.ROOM_HEIGHT, height));
		}
		if (width != LevelGenerator.ROOM_WIDTH) {
			throw new UnityException(string.Format("Error in room by {0}. Wrong width, Expected: {1}, Got: {2}", roomAuthor, LevelGenerator.ROOM_WIDTH, width));
		}
		int[,] indexGrid = new int[width, height];
		for (int r = 0; r < height; r++) {
			string row = rows[height-r-1];
			string[] cols = row.Trim().Split(',');
			for (int c = 0; c < width; c++) {
				indexGrid[c, r] = int.Parse(cols[c]);
			}
		}
		
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				int tileIndex = indexGrid[i, j];
				if (tileIndex == 0) {
					continue; // 0 is nothing.
				}
				GameObject tileToSpawn;
				if (tileIndex < LevelGenerator.LOCAL_START_INDEX) {
					tileToSpawn = ourGenerator.globalTilePrefabs[tileIndex-1];
				}
				else {
					tileToSpawn = localTilePrefabs[tileIndex-LevelGenerator.LOCAL_START_INDEX];
				}
				Tile.spawnTile(tileToSpawn, transform, i, j);
			}
		}


	}

}
