using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283AStarEnemy : apt283BFSEnemy {

	protected override void updatePathToTarget() {

		_currentPath.Clear();

		_agenda.Clear();
		_closedSet.Clear();

		SearchVertex startVertex = new SearchVertex();
		startVertex.parent = null;
		startVertex.numHops = 0;
		startVertex.gridPos = _targetGridPos;
		startVertex.costFromStart = 0;


		Vector2 targetPos = toGridCoord(_tileWereChasing.transform.position);

		startVertex.estimatedCostToTarget = estimatePathDistance(startVertex.gridPos, targetPos);

		_agenda.Add(startVertex);

		// We have a max hops so that we don't spend too long searching in vain. 
		int maxHops = maxSearchDepth;

		while (_agenda.Count > 0) {
			float minCost = _agenda[0].estimatedCostToTarget;
			SearchVertex currentVertex = _agenda[0];
			foreach (SearchVertex vertex in _agenda) {
				if (vertex.estimatedCostToTarget < minCost) {
					minCost = vertex.estimatedCostToTarget;
					currentVertex = vertex;
				}
			}
			_agenda.Remove(currentVertex);
			_closedSet.Add(currentVertex);

			if (currentVertex.gridPos == targetPos) {
				// If we made it to our target, reconstruct the path by going back up the parents.
				do {
					_currentPath.Add(currentVertex.gridPos);
					currentVertex = currentVertex.parent;
				} while (currentVertex != null);
				// Need to reverse the path before we return it. 
				_currentPath.Reverse();
				return;
			}

			// First, check to see if we've gone too far. 
			if (currentVertex.numHops >= maxHops) {
				continue; // Don't expand if we're already too far. 
			}
			for (int x = (int)currentVertex.gridPos.x-1; x <= (int)currentVertex.gridPos.x+1; x++) {
				for (int y = (int)currentVertex.gridPos.y-1; y <= (int)currentVertex.gridPos.y+1; y++) {
					if (x == (int)currentVertex.gridPos.x && y == (int)currentVertex.gridPos.y) {
						continue; // Ignore our own coordinate.
					}

					Vector2 neighborPos = new Vector2(x, y);
					// We ignore the neighbor if it's in the closed set 
					if (listContainsPosition(_closedSet, neighborPos)) {
						continue;
					}

					// Now check if we can even move onto that spot. 
					if (!canMoveBetweenPoints(currentVertex.gridPos, neighborPos, pathColliderRadius)) {
						continue;
					}

					// Now we have to see if the vertex is aleady in the agenda. 

					float tentativeCostFromStart = currentVertex.costFromStart + Vector2.Distance(currentVertex.gridPos, neighborPos);

					SearchVertex neighborVertex = null;
					foreach (SearchVertex vertex in _agenda) {
						if (vertex.gridPos == neighborPos) {
							neighborVertex = vertex;
							break;
						}
					}
					if (neighborVertex == null) {
						neighborVertex = new SearchVertex();
						neighborVertex.gridPos = neighborPos;
						neighborVertex.costFromStart = tentativeCostFromStart;
						neighborVertex.estimatedCostToTarget = tentativeCostFromStart + estimatePathDistance(neighborPos, targetPos);
						neighborVertex.parent = currentVertex;
						neighborVertex.numHops = currentVertex.numHops+1;
						_agenda.Add(neighborVertex);
					}
					else {
						if (tentativeCostFromStart < neighborVertex.costFromStart) {
							neighborVertex.costFromStart = tentativeCostFromStart;
							neighborVertex.estimatedCostToTarget = tentativeCostFromStart + estimatePathDistance(neighborPos, targetPos);
							neighborVertex.parent = currentVertex;
							neighborVertex.numHops = currentVertex.numHops+1;
						}
					}
				}
			}
		}

		// If we never reached the target, use the node from the closed set with the smallest estimated distance to target. 
		if (_closedSet.Count == 0) {
			return;
		}

		SearchVertex closestVertex = _closedSet[0];
		float closestVertexDistance = Vector2.Distance(closestVertex.gridPos, targetPos);
		foreach (SearchVertex vertex in _closedSet) {
			float vertexDistance = Vector2.Distance(vertex.gridPos, targetPos);
			if (vertexDistance < closestVertexDistance) {
				closestVertex = vertex;
				closestVertexDistance = vertexDistance;
			}
		}

		do {
			_currentPath.Add(closestVertex.gridPos);
			closestVertex = closestVertex.parent;
		} while (closestVertex != null);
		_currentPath.Reverse();
	}

	protected float estimatePathDistance(Vector2 gridPos1, Vector2 gridPos2) {
		return Vector2.Distance(gridPos1, gridPos2);
	}

}
