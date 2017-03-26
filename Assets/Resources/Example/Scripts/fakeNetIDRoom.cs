using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Another example of room generation.
// This one generates walls in the center as well as around the border.
// Also spawns miscellaneous objects.
// This room is a good candidate for testing out Tiles as you make them.
public class fakeNetIDRoom : Room {

	public GameObject tilePrefab;

	public int minNumTiles = 1, maxNumTiles = 3;

	public float borderWallProbability = 0.9f;
	public float centerWallProbability = 0.9f;

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
				// Now check to see if we're in one of the center tiles
				if (x >= LevelGenerator.ROOM_WIDTH/2-1 
					&& x <= LevelGenerator.ROOM_WIDTH/2
					&& y >= LevelGenerator.ROOM_HEIGHT/2-1
					&& y <= LevelGenerator.ROOM_HEIGHT/2) {
					wallMap[x, y] = Random.value <= centerWallProbability;
					continue;
				}

				wallMap[x, y] = false;
			}
		}

		// Now actually spawn all the walls.
		bool[,] occupiedPositions = new bool[LevelGenerator.ROOM_WIDTH, LevelGenerator.ROOM_HEIGHT];
		for (int x = 0; x < LevelGenerator.ROOM_WIDTH; x++) {
			for (int y = 0; y < LevelGenerator.ROOM_HEIGHT; y++) {
				if (wallMap[x, y]) {
					Tile.spawnTile(ourGenerator.normalWallPrefab, transform, x, y);
					occupiedPositions[x, y] = true;
				}
				else {
					occupiedPositions[x, y] = false;
				}
			}
		}

		// Now spawn our miscallaneous tiles.
		List<Vector2> possibleSpawnPositions = new List<Vector2>(LevelGenerator.ROOM_WIDTH*LevelGenerator.ROOM_HEIGHT);
		int numTiles = Random.Range(minNumTiles, maxNumTiles+1);
		for (int i = 0; i < numTiles; i++) {
			possibleSpawnPositions.Clear();
			for (int x = 0; x < LevelGenerator.ROOM_WIDTH; x++) {
				for (int y = 0; y < LevelGenerator.ROOM_HEIGHT; y++) {
					if (occupiedPositions[x, y]) {
						continue;
					}
					possibleSpawnPositions.Add(new Vector2(x, y));
				}
			}
			if (possibleSpawnPositions.Count > 0) {
				Vector2 spawnPos = GlobalFuncs.randElem(possibleSpawnPositions);
				Tile.spawnTile(tilePrefab, transform, (int)spawnPos.x, (int)spawnPos.y);
				occupiedPositions[(int)spawnPos.x, (int)spawnPos.y] = true;
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
