using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour {

	public string roomAuthor = "";

	public TextAsset designedRoomFile;

	public GameObject[] localTilePrefabs;

	public virtual void generateRoom(LevelGenerator ourGenerator) {

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
