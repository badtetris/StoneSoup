using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// The Room class is the parent of your room generators.
//
public class Room : MonoBehaviour {

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


	// The generateRoom function is the one you'll need to override to generator your rooms.
	// It takes the LevelGenerator as a parameter so you have access to more global prefabs (like walls)
	// Additionaly, it takes an array of exits that need to exist (your room will have required exits if it's on the critical path).
	// For an exit to "exist", there can't be a wall in the center part of that edge of the room.

	// This implementation pulls a designed room out of a text file.
	public virtual void generateRoom(LevelGenerator ourGenerator, params Dir[] requiredExits) {

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
