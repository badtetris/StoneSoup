using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour {

	public TextAsset designedLevelFile;

	public virtual void generateLevel() {
		string initialGridString = designedLevelFile.text.Trim();
		string[] rows = initialGridString.Trim().Split('\n');
		int width = rows[0].Split(',').Length;
		int height = rows.Length;

		int[,] indexGrid = new int[width, height];
		for (int r = 0; r < height; r++) {
			string row = rows[height-r-1].Trim();
			string[] cols = row.Split(',');
			for (int c = 0; c < width; c++) {
				indexGrid[c, r] = int.Parse(cols[c]);
			}
		}

		// Make a wall map to make sure we don't spawn irrelevant walls
		bool[,] wallMap = new bool[width, height];
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				wallMap[i, j] = indexGrid[i, j] == 1;
			}
		}
		bool[,] sparseWallMap = new bool[width, height];
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				sparseWallMap[i, j] = wallMap[i, j];
				if (!wallMap[i, j]) {
					continue;
				}
				if (i > 0 && !wallMap[i-1, j]) {
					continue;
				}
				if (i < width-1 && !wallMap[i+1, j]) {
					continue;
				}
				if (j > 0 && !wallMap[i, j-1]) {
					continue;
				}
				if (j < height-1 && !wallMap[i, j+1]) {
					continue;
				}
				if ((i > 0 && j > 0 && !wallMap[i-1, j-1])) {
					continue;
				}
				if ((i < width-1 && j > 0 && !wallMap[i+1, j-1])) {
					continue;
				}
				if ((i > 0 && j < height-1 && !wallMap[i-1, j+1])) {
					continue;
				}
				if ((i < width-1 && j < height-1 && !wallMap[i+1, j+1])) {
					continue;
				}

				sparseWallMap[i, j] = false;
			}
		}
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				if (sparseWallMap[i, j]) {
					Tile.spawnTile(GameManager.instance.generalTilePrefabs[0], GameManager.instance.transform, i, j);
				}
			}
		}
		// Now that we've dealt with sparse walls, let's deal with other spawning.
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				int tileIndex = indexGrid[i, j];
				if (tileIndex == 0 || tileIndex == 1) {
					continue;
				}
				Tile.spawnTile(GameManager.instance.generalTilePrefabs[tileIndex-1], GameManager.instance.transform, i, j);

			}
		}


		// Finally, set up the boundaries on the manager so the camera knows how to follow.
		GameManager.instance.currentRoomPosition.x = width*Tile.TILE_SIZE/2f;
		GameManager.instance.currentRoomPosition.y = height*Tile.TILE_SIZE/2f;
		GameManager.instance.currentRoomObj = GameManager.instance.gameObject;
		GameManager.instance.currentRoomAuthor = "AP Thomson";

	}
	
}
