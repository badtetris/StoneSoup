using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283Room : Room {


	public float borderWallProbability = 0.9f;

	public override void generateRoom(LevelGenerator ourGenerator, params Dir[] requiredExits) {
		// Basically we go over the border and determining where to spawn walls.
		bool[,] wallMap = new bool[LevelGenerator.ROOM_WIDTH, LevelGenerator.ROOM_HEIGHT];
		for (int x = 0; x < LevelGenerator.ROOM_WIDTH; x++) {
			for (int y = 0; y < LevelGenerator.ROOM_HEIGHT; y++) {
				if (x == 0 || x == LevelGenerator.ROOM_WIDTH-1
					|| y == 0 || y == LevelGenerator.ROOM_HEIGHT-1) {
					
					if (x == LevelGenerator.ROOM_WIDTH/2 
						&& y == LevelGenerator.ROOM_HEIGHT-1
						&& containsDir(requiredExits, Dir.Up)) {
						wallMap[x, y] = false;
					}
					else if (x == LevelGenerator.ROOM_WIDTH-1
						&& y == LevelGenerator.ROOM_HEIGHT/2
						&& containsDir(requiredExits, Dir.Right)) {
						wallMap[x, y] = false;
					}
					else if (x == LevelGenerator.ROOM_WIDTH/2
						&& y == 0
						&& containsDir(requiredExits, Dir.Down)) {
						wallMap[x, y] = false;
					}
					else if (x == 0 
						&& y == LevelGenerator.ROOM_HEIGHT/2 
						&& containsDir(requiredExits, Dir.Left)) {
						wallMap[x, y] = false;
					}
					else {
						wallMap[x, y] = Random.value <= borderWallProbability;
					}
					continue;
				}
				wallMap[x, y] = false;
			}
		}

		// Now actually spawn all the walls.
		for (int x = 0; x < LevelGenerator.ROOM_WIDTH; x++) {
			for (int y = 0; y < LevelGenerator.ROOM_HEIGHT; y++) {
				if (wallMap[x, y]) {
					Tile.spawnTile(ourGenerator.normalWallPrefab, transform, x, y);
				}
			}
		}


	}


	protected bool containsDir (Dir[] dirArray, Dir dirToCheck) {
		foreach (Dir dir in dirArray) {
			if (dirToCheck == dir) {
				return true;
			}
		}
		return false;
	}

}
