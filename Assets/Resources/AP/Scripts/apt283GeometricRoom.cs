using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class apt283GeometricRoom : Room {

	public int minWalls = 40;
	public int maxWalls = 60;

	protected bool[,] _wallMap;
	
	public static int fileIndex = 0;

	protected string saveFilePrefix = "/Resources/Class6/Rooms/generated_room_1_";

	public override void fillRoom(LevelGenerator ourGenerator, ExitConstraint requiredExits) {

		_wallMap = new bool[LevelGenerator.ROOM_WIDTH, LevelGenerator.ROOM_HEIGHT];
		for (int x = 0; x < LevelGenerator.ROOM_WIDTH; x++) {
			for (int y = 0; y < LevelGenerator.ROOM_HEIGHT; y++) {
				_wallMap[x, y] = false;
			}
		}

		List<Vector2> maybeWallPoints = new List<Vector2>(LevelGenerator.ROOM_WIDTH*LevelGenerator.ROOM_HEIGHT);

		int numWalls = Random.Range(minWalls, maxWalls+1);

		for (int i = 0; i < numWalls; i++) {
			maybeWallPoints.Clear();

			for (int x = 0; x < LevelGenerator.ROOM_WIDTH; x++) {
				for (int y = 0; y < LevelGenerator.ROOM_HEIGHT; y++) {
					if (!_wallMap[x, y]) {
						maybeWallPoints.Add(new Vector2(x, y));
					}
				}
			}

			GlobalFuncs.shuffle(maybeWallPoints);
			maybeWallPoints.Sort(compareWallPoints);

			Vector2 wallPoint = geometricPick(maybeWallPoints);

			Tile.spawnTile(ourGenerator.normalWallPrefab, transform, (int)wallPoint.x, (int)wallPoint.y);
			_wallMap[(int)wallPoint.x, (int)wallPoint.y] = true;

		}

		//saveRoomToFile();
	}

	protected Vector2 geometricPick(List<Vector2> points, float geometricWeight=0.3f) {
		int currentIndex = 0;
		while(currentIndex < points.Count) {
			if (Random.value < geometricWeight) {
				return points[currentIndex];
			}
			currentIndex++;
		}
		return points[points.Count-1];
	}

	protected bool inGrid(int x, int y) {
		return x >= 0 && x < LevelGenerator.ROOM_WIDTH && y >= 0 && y < LevelGenerator.ROOM_HEIGHT;
	}

	protected int compareWallPoints(Vector2 point1, Vector2 point2) {
		
		int cardinalNeighbors1 = 0;
		int diagonalNeighbors1 = 0;

		int cardinalNeighbors2 = 0;
		int diagonalNeighbors2 = 0;

		for (int x = (int)point1.x-1; x <= (int)point1.x+1; x++) {
			for (int y = (int)point1.y-1; y <= (int)point1.y+1; y++) {
				if (x == (int)point1.x && y == (int)point1.y) {
					continue;
				}

				if (x == (int)point1.x || y == (int)point1.y) {
					if (inGrid(x, y) && _wallMap[x, y]) {
						cardinalNeighbors1++;
					}
				}
				else {
					if (inGrid(x, y) && _wallMap[x, y]) {
						diagonalNeighbors1++;
					}
				}

			}
		}

		for (int x = (int)point2.x-1; x <= (int)point2.x+1; x++) {
			for (int y = (int)point2.y-1; y <= (int)point2.y+1; y++) {
				if (x == (int)point2.x && y == (int)point2.y) {
					continue;
				}

				if (x == (int)point2.x || y == (int)point2.y) {
					if (inGrid(x, y) && _wallMap[x, y]) {
						cardinalNeighbors2++;
					}
				}
				else {
					if (inGrid(x, y) && _wallMap[x, y]) {
						diagonalNeighbors2++;
					}
				}

			}
		}

		if (cardinalNeighbors1 == 3 && cardinalNeighbors2 != 3) {
			return -1;
		}
		else if (cardinalNeighbors2 == 3 && cardinalNeighbors1 != 3) {
			return 1;
		}
		else if (cardinalNeighbors1 > cardinalNeighbors2) {
			return -1;
		}
		else if (cardinalNeighbors2 > cardinalNeighbors1) {
			return 1;
		}
		if (diagonalNeighbors1 > diagonalNeighbors2) {
			return 1;
		}
		else if (diagonalNeighbors2 > diagonalNeighbors1) {
			return -1;
		}
		else {
			return 0;
		}
	}

	protected void saveRoomToFile() {

		string roomString = "";
		for (int y = LevelGenerator.ROOM_HEIGHT-1; y >= 0; y--) {
			for (int x = 0; x < LevelGenerator.ROOM_WIDTH; x++) {
				int index = _wallMap[x, y] ? 1 : 0;
				roomString += index.ToString();
				if (x < LevelGenerator.ROOM_WIDTH-1) {
					roomString += ",";
				}
			}
			roomString += "\n";
		}
		string filePath = Application.dataPath + saveFilePrefix + fileIndex + ".csv";
		Debug.Log(filePath);
		File.WriteAllText(filePath, roomString);
		fileIndex++;
	}


}
