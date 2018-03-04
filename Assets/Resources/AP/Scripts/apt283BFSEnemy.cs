using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283BFSEnemy : apt283FollowEnemy {
	
	// How far the object we're chasing has to be before we stop chasing it. 
	public int maxSearchDepth = 6;

	public float pathColliderRadius = 0.7f;

	protected override void takeStep() {


		_takingCorrectingStep = false;
		_timeSinceLastStep = 0f;


		if (_tileWereChasing == null) {
			_targetGridPos = toGridCoord(globalX, globalY);
			return;
		}

		// First, figure out if the target is too far away
		float distanceToTile = Vector2.Distance(transform.position, _tileWereChasing.transform.position);
		if (distanceToTile > maxDistanceToContinueChase) {
			_tileWereChasing = null;
			return;
		}

		// We do this to re-calculate exactly where we are right now. 
		_targetGridPos = Tile.toGridCoord(globalX, globalY);

		if (_tileWereChasing == null) {
			return;
		}

		updatePathToTarget();
		if (_currentPath.Count > 1) {
			_targetGridPos = _currentPath[1];
		}
		else {
			_tileWereChasing = null;
		}

	}

	// STUFF FOR PERFORMING OUR BFS.

	public class SearchVertex {
		public Vector2 gridPos;
		public SearchVertex parent;
		public int numHops = 0;
		public float costFromStart;
		public float estimatedCostToTarget;
	}

	protected bool listContainsPosition(List<SearchVertex> list, Vector2 pos) {
		foreach (SearchVertex vertex in list) {
			if (vertex.gridPos == pos){
				return true;
			}
		}
		return false;
	}


	protected static List<SearchVertex> _agenda = new List<SearchVertex>();
	protected static List<SearchVertex> _closedSet = new List<SearchVertex>();


	protected List<Vector2> _currentPath = new List<Vector2>(80);

	protected virtual void updatePathToTarget() {
		_currentPath.Clear();

		_agenda.Clear();
		_closedSet.Clear();

		SearchVertex startVertex = new SearchVertex();
		startVertex.parent = null;
		startVertex.numHops = 0;
		startVertex.gridPos = _targetGridPos;

		Vector2 targetPos = toGridCoord(_tileWereChasing.transform.position);


		_agenda.Add(startVertex);

		// We have a max hops so that we don't spend too long searching in vain. 
		int maxHops = maxSearchDepth;

		while (_agenda.Count > 0) {
			SearchVertex currentVertex = _agenda[0];
			_agenda.RemoveAt(0);
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

			// Otherwise, time to expand all the neighbors of the vertex. 


			// First, check to see if we've gone too far. 
			if (currentVertex.numHops >= maxHops) {
				continue; // Don't expand if we're already too far. 
			}

			// We can use a loop to iterate through the neighbor points. 
			for (int x = (int)currentVertex.gridPos.x-1; x <= (int)currentVertex.gridPos.x+1; x++) {
				for (int y = (int)currentVertex.gridPos.y-1; y <= (int)currentVertex.gridPos.y+1; y++) {
					if (x == (int)currentVertex.gridPos.x && y == (int)currentVertex.gridPos.y) {
						continue; // Ignore our own coordinate.
					}

					Vector2 neighborPos = new Vector2(x, y);
					// We ignore the neighbor if it's in the closed set or Already in the agenda. 
					if (listContainsPosition(_agenda, neighborPos)
						|| listContainsPosition(_closedSet, neighborPos)) {
						continue;
					}

					// Now check if we can even move onto that spot. 
					if (!canMoveBetweenPoints(currentVertex.gridPos, neighborPos, pathColliderRadius)) {
						continue;
					}

					// Otherwise, the neighbor can go in the agenda!
					SearchVertex neighborVertex = new SearchVertex();
					neighborVertex.gridPos = neighborPos;
					neighborVertex.parent = currentVertex;
					neighborVertex.numHops = currentVertex.numHops+1;
					_agenda.Add(neighborVertex);
				}
			}

		}

	}

	protected bool canMoveBetweenPoints(Vector2 startGridPos, Vector2 endGridPos, float radius) {
		Vector2 startWorldPos = toWorldCoord(startGridPos);
		Vector2 endWorldPos = toWorldCoord(endGridPos);
		Vector2 toEnd = endWorldPos-startWorldPos;
		float distance = toEnd.magnitude;
		toEnd.Normalize();
		int numResults = Physics2D.CircleCastNonAlloc(startWorldPos, radius, toEnd, _maybeRaycastResults, distance);
		for (int i = 0; i < numResults && i < _maybeRaycastResults.Length; i++) {
			RaycastHit2D result = _maybeRaycastResults[i];
			if (!CanOverlapIgnoreTargetTile(result)) {
				return false;
			}
		}
		return true;
	}


	
}
