using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283BFSEnemy : BasicAICreature {
	// How much force we inflict if something collides with us.
	public float damageForce = 1000;
	public int damageAmount = 1;

	// If we're chasing a friendly object, it'll be stored here.
	protected Tile _tileWereChasing = null;
	public Tile tileWereChasing {
		get { return _tileWereChasing; }
		set { _tileWereChasing = value; }
	}

	// How far the object we're chasing has to be before we stop chasing it. 
	public float maxDistanceToContinueChase = 12f;
	public int maxSearchDepth = 6;

	// When chasing we either
	// a. Choose our next target position once we reach our current one
	// or
	// b. Choose our next target position if we've gone too long without reaching our target position.
	// or
	// c. Recalculate our target position when we collide with something (not the target tile).
	protected float _timeSinceLastStep = 0f;

	public float pathColliderRadius = 0.7f;

	void Update() {
		if (_tileWereChasing != null) {
			_timeSinceLastStep += Time.deltaTime;
			Vector2 targetGlobalPos = Tile.toWorldCoord(_targetGridPos.x, _targetGridPos.y);
			float distanceToTarget = Vector2.Distance(transform.position, targetGlobalPos);
			if (distanceToTarget <= GRID_SNAP_THRESHOLD || _timeSinceLastStep >= 2f) {
				takeStep();
			}
		}
		else {
			// Always lock to the position we're in when we're not chasing anything. 
			_targetGridPos = toGridCoord(globalX, globalY);
		}
		updateSpriteSorting();
	}


	protected override void takeStep() {
		_timeSinceLastStep = 0f;

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

	protected void takeCorrectionStep() {
		// We do this when we need to correct where we think we are
		// i.e. if we and another creature think we're both on the same gridpos, one of us needs to switch to a neighboring gridPos.
		_timeSinceLastStep = 0f;

		Vector2 targetWorldPos = toWorldCoord(_targetGridPos);
		float xDistance = Mathf.Abs(targetWorldPos.x - transform.position.x);
		float yDistance = Mathf.Abs(targetWorldPos.y - transform.position.y);

		if (transform.position.y > targetWorldPos.y && yDistance > xDistance) {
			_targetGridPos += Vector2.up; // Correct upwards.
		}
		else if (transform.position.x > targetWorldPos.x && xDistance > yDistance) {
			_targetGridPos += Vector2.right; // Correct to the right.
		}
		else if (transform.position.y < targetWorldPos.y && yDistance > xDistance) {
			_targetGridPos -= Vector2.up; // Correct down.
		}
		else {
			_targetGridPos -= Vector2.right; // Correct left.
		}
	}



	// Colliding with a friendly should hurt it.
	void OnCollisionEnter2D(Collision2D collision) {
		Tile otherTile = collision.gameObject.GetComponent<Tile>();

		// If we're chasing something, then take a step probably
		if (otherTile != _tileWereChasing 
			&& otherTile != null 
			&& otherTile.hasTag(TileTags.Creature)) {

			BasicAICreature maybeOtherCreature = (otherTile as BasicAICreature);
			if (maybeOtherCreature != null
				&& maybeOtherCreature.targetGridPos == _targetGridPos
				&& transform.position.y < otherTile.transform.position.y) {
				// We now have to take a correction step.
				takeCorrectionStep();
			}

		}

		if (otherTile != null && otherTile.hasTag(tagsWeChase)) {
			otherTile.takeDamage(this, damageAmount);
			Vector2 toOtherTile = (Vector2)otherTile.transform.position - (Vector2)transform.position;
			toOtherTile.Normalize();
			otherTile.addForce(damageForce*toOtherTile);
		}

	}

	void OnCollisionStay2D(Collision2D collision) {
		OnCollisionEnter2D(collision);
	}

	public override void tileDetected(Tile otherTile) {
		if (_tileWereChasing == null && otherTile.hasTag(tagsWeChase)) {
			_tileWereChasing = otherTile;
			takeStep();
		}
	}

	// For the purposes of chasing an object, we make a special CanOverlapFunc that ignores the tile we're chasing
	protected bool CanOverlapIgnoreTargetTile(RaycastHit2D hitResult) {
		Tile maybeResultTile = hitResult.transform.GetComponent<Tile>();
		if (maybeResultTile == _tileWereChasing) {
			return true;
		}
		return DefaultCanOverlapFunc(hitResult);
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
