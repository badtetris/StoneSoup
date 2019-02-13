using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283ArrowTrapDFSRoom : apt283RandomDFSRoom {

	public GameObject faceLeftArrowTrapPrefab;
	public GameObject faceRightArrowTrapPrefab;


    public override void fillRoom(LevelGenerator ourGenerator, ExitConstraint requiredExits) {
		base.fillRoom(ourGenerator, requiredExits);
		foreach (SearchVertex vertex in _closed) {
			// Only look at vertices that were dead ends and weren't neighboring the exits.
			if (!vertex.isDeadEnd) {
				continue;
			}

			bool closeToExit = false;
			foreach (Vector2Int exitPoint in requiredExits.requiredExitLocations()) {
				int manDistanceToExit = (int)Mathf.Abs(exitPoint.x-vertex.gridPos.x)+(int)Mathf.Abs(exitPoint.y-vertex.gridPos.y);
				if (manDistanceToExit <= 1) {
					closeToExit = true;
					break;
				}
			}
			if (closeToExit) {
				continue;
			}

			// Spawn the arrow traps depending on if we're open to the left or the right.
			if (vertex.parent.gridPos.x < vertex.gridPos.x) {
				Tile.spawnTile(faceLeftArrowTrapPrefab, transform, (int)vertex.gridPos.x, (int)vertex.gridPos.y);
			}
			else if (vertex.parent.gridPos.x > vertex.gridPos.x) {
				Tile.spawnTile(faceRightArrowTrapPrefab, transform, (int)vertex.gridPos.x, (int)vertex.gridPos.y);
			}

		}
	}

}
