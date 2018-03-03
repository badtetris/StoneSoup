using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A Room that can guarantee certain paths being open or not.
public class apt283SearchRoom : Room {

	protected static List<Vector2> _agenda = new List<Vector2>(80);
	protected static List<Vector2> _alreadyExpanded = new List<Vector2>(80);

	public virtual bool doesEntranceExist(Dir entrance) {
		// First thing we do is construct a grid out of our file. 
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

		Vector2 startPoint = Vector2.zero;
		if (entrance == Dir.Up) {
			startPoint = new Vector2(LevelGenerator.ROOM_WIDTH/2, LevelGenerator.ROOM_HEIGHT-1);
		}
		else if (entrance == Dir.Right) {
			startPoint = new Vector2(LevelGenerator.ROOM_WIDTH-1, LevelGenerator.ROOM_HEIGHT/2);
		}
		else if (entrance == Dir.Down) {
			startPoint = new Vector2(LevelGenerator.ROOM_WIDTH/2, 0);
		}
		else if (entrance == Dir.Left) {
			startPoint = new Vector2(0, LevelGenerator.ROOM_HEIGHT/2);
		}

		return isOpenSpot(indexGrid[(int)startPoint.x, (int)startPoint.y]);
	}

	public virtual bool doesPathExist(Dir entrance, Dir exit) {

		// First thing we do is construct a grid out of our file. 
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


		// Now let's start at the entrance and ensure there's a path from entrance to exit. 
		// We're going to need to do a BFS to find a path. 

		// First find our start point. 
		Vector2 startPoint = Vector2.zero;
		if (entrance == Dir.Up) {
			startPoint = new Vector2(LevelGenerator.ROOM_WIDTH/2, LevelGenerator.ROOM_HEIGHT-1);
		}
		else if (entrance == Dir.Right) {
			startPoint = new Vector2(LevelGenerator.ROOM_WIDTH-1, LevelGenerator.ROOM_HEIGHT/2);
		}
		else if (entrance == Dir.Down) {
			startPoint = new Vector2(LevelGenerator.ROOM_WIDTH/2, 0);
		}
		else if (entrance == Dir.Left) {
			startPoint = new Vector2(0, LevelGenerator.ROOM_HEIGHT/2);
		}

		Vector2 targetPoint = Vector2.zero;
		if (exit == Dir.Up) {
			targetPoint = new Vector2(LevelGenerator.ROOM_WIDTH/2, LevelGenerator.ROOM_HEIGHT-1);
		}
		else if (exit == Dir.Right) {
			targetPoint = new Vector2(LevelGenerator.ROOM_WIDTH-1, LevelGenerator.ROOM_HEIGHT/2);
		}
		else if (exit == Dir.Down) {
			targetPoint = new Vector2(LevelGenerator.ROOM_WIDTH/2, 0);
		}
		else if (exit == Dir.Left) {
			targetPoint = new Vector2(0, LevelGenerator.ROOM_HEIGHT/2);
		}

		// If either the start or the target isn't empty, we're done. 
		if (!isOpenSpot(indexGrid[(int)startPoint.x, (int)startPoint.y]) 
			|| !isOpenSpot(indexGrid[(int)targetPoint.x, (int)targetPoint.y])) {
			return false;
		}

		_agenda.Clear();
		_alreadyExpanded.Clear();
		_agenda.Add(startPoint);

		while (_agenda.Count > 0) {
			Vector2 currentVertex = _agenda[0];
			_agenda.RemoveAt(0);
			// Mark the current vertex as expanded. (we could do this afterwards, but it's fine to do it here)
			_alreadyExpanded.Add(currentVertex);

			if (currentVertex == targetPoint) {
				// We're done! We found a path!
				return true;
			}

			// Now let's look at all the neighbors. 
			Vector2 upPoint = currentVertex+Vector2.up;
			if (inGrid(upPoint) 
				&& !_alreadyExpanded.Contains(upPoint)
				&& isOpenSpot(indexGrid[(int)upPoint.x, (int)upPoint.y])) {
				_agenda.Add(upPoint);
			}
			Vector2 rightPoint = currentVertex+Vector2.right;
			if (inGrid(rightPoint)
				&& !_alreadyExpanded.Contains(rightPoint)
				&& isOpenSpot(indexGrid[(int)rightPoint.x, (int)rightPoint.y])) {
				_agenda.Add(rightPoint);
			}
			Vector2 downPoint = currentVertex-Vector2.up;
			if (inGrid(downPoint)
				&& !_alreadyExpanded.Contains(downPoint)
				&& isOpenSpot(indexGrid[(int)downPoint.x, (int)downPoint.y])) {
				_agenda.Add(downPoint);
			}
			Vector2 leftPoint = currentVertex-Vector2.right;
			if (inGrid(leftPoint)
				&& !_alreadyExpanded.Contains(leftPoint)
				&& isOpenSpot(indexGrid[(int)leftPoint.x, (int)leftPoint.y])) {
				_agenda.Add(leftPoint);
			}
		}
		return false;
	}

	protected virtual bool inGrid(Vector2 point) {
		return (int)point.x >= 0 && (int)point.x < LevelGenerator.ROOM_WIDTH && (int)point.y >= 0 && (int) point.y < LevelGenerator.ROOM_HEIGHT;
	}

	protected virtual bool isOpenSpot(int index) {
		return index == 0;
	}

}
