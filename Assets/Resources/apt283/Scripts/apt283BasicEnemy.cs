using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Example of a basic enemy tile.
// Enemies have the following behavior:
// Every once in a while, they'll try to move to a neighboring empty spot.
// If they are empty handed and find a weapon, they pick up the weapon.
// Every once in a while, they scan for friendly tiles.
// If they find a friendly tile and they're holding a weapon, they aim the weapon at the friendly tile
// and try to use it.
public class apt283BasicEnemy : Tile {

	// How much force we inflict if something collides with us.
	public float damageForce = 1000;

	// When we move, we try to move to grid snapped locations, so our current target
	// is stored in grid coordinates.
	protected Vector2 _targetGridPos;

	// We move similar to how the player moves, so we keep similar tunable values.
	public float moveSpeed = 5;
	public float moveAcceleration = 100;

	// We use counters to determine when to next try to move.
	protected float _nextMoveCounter;
	public float timeBetweenMovesMin = 1.5f;
	public float timeBetweenMovesMax = 3f;

	// Similarly, we use counters to determine when to scan for friendlies
	// (we don't scan for friendlies every frame because that would potentially cause performance issues)
	protected float _checkForPlayerCounter;
	public float checkForPlayerTime = 0.5f;
	public float playerAwarenessRadius = 12f;

	// To avoid constantly making a new List data structure, we keep the same list for checking our neighbor positions
	protected List<Vector2> _neighborPositions;

	void Start() {
		_targetGridPos = Tile.toGridCoord(globalX, globalY);
		_nextMoveCounter = Random.Range(timeBetweenMovesMin, timeBetweenMovesMax);
		_checkForPlayerCounter = Random.Range(0, checkForPlayerTime);

		_neighborPositions = new List<Vector2>(4);
		if (_maybeRaycastResults == null) {
			_maybeRaycastResults = new RaycastHit2D[10];
		}
	}

	void Update() {
		// Update our counters.
		if (_nextMoveCounter > 0) {
			_nextMoveCounter -= Time.deltaTime;
		}
		if (_checkForPlayerCounter > 0) {
			_checkForPlayerCounter -= Time.deltaTime;
		}

		// When it's time to try a new move.
		if (_nextMoveCounter <= 0) {
			// Try to move to one of our neighboring positions if it is empty.
			_neighborPositions.Clear();

			// We test neighbor locations by casting in specific directions. 

			Vector2 upGridNeighbor = new Vector2(_targetGridPos.x, _targetGridPos.y+1);
			if (isValidMoveDir(Vector2.up)) {
				_neighborPositions.Add(upGridNeighbor);
			}
			Vector2 rightGridNeighbor = new Vector2(_targetGridPos.x+1, _targetGridPos.y);
			if (isValidMoveDir(Vector2.right)) {
				_neighborPositions.Add(rightGridNeighbor);
			}
			Vector2 downGridNeighbor = new Vector2(_targetGridPos.x, _targetGridPos.y-1);
			if (isValidMoveDir(-Vector2.up)) {
				_neighborPositions.Add(downGridNeighbor);
			}
			Vector2 leftGridNeighbor = new Vector2(_targetGridPos.x-1, _targetGridPos.y);
			if (isValidMoveDir(-Vector2.right)) {
				_neighborPositions.Add(leftGridNeighbor);
			}

			// If there's an empty neighbor, choose one randomly.
			if (_neighborPositions.Count > 0) {
				_targetGridPos = GlobalFuncs.randElem(_neighborPositions);
				_nextMoveCounter = Random.Range(timeBetweenMovesMin, timeBetweenMovesMax);
			}

			// If we're not carrying something, but we're on top of a weapon that can be picked up, pick it up!
			if (tileWereHolding == null) {
				int numObjectsFound = _body.Cast(Vector2.zero, _maybeRaycastResults);
				for (int i = 0; i < numObjectsFound && i < _maybeRaycastResults.Length; i++) {
					RaycastHit2D result = _maybeRaycastResults[i];
					Tile tileHit = result.transform.GetComponent<Tile>();
					if (tileHit != null && tileHit.hasTag(TileTags.Weapon) && tileHit.hasTag(TileTags.CanBeHeld)) {
						tileHit.pickUp(this);
					}
				}
			}

		}

		// We only scan for friendlies when we're holding a weapon.
		if (tileWereHolding != null) {
			if (_checkForPlayerCounter <= 0) {
				// Send out a big circle to look for friendlies.
				Collider2D[] maybeColliders = Physics2D.OverlapCircleAll(transform.position, playerAwarenessRadius);
				foreach (Collider2D maybeCollider in maybeColliders) {
					Tile tile = maybeCollider.GetComponent<Tile>();
					if (tile != null && tile.hasTag(TileTags.Friendly)) {
						// We've found something to use our weapon on
						aimDirection = ((Vector2)tile.transform.position-(Vector2)transform.position).normalized;
						tileWereHolding.useAsItem(this);
						break;
					}
				}
				_checkForPlayerCounter = checkForPlayerTime;
			}
		}
	}	

	// Utility function that casts in a direction to see if it's empty (and therefore we can move onto it).
	protected bool isValidMoveDir(Vector2 direction) {
		int numCollisions = _body.Cast(direction, _maybeRaycastResults, Tile.TILE_SIZE);
		for (int i = 0; i < numCollisions && i < _maybeRaycastResults.Length; i++) {
			RaycastHit2D result = _maybeRaycastResults[i];
			Tile resultTile = result.transform.GetComponent<Tile>();
			if (resultTile != null) {
				if (resultTile.hasTag(TileTags.Wall | TileTags.Creature | TileTags.Exit)) {
					return false;
				}
			}
			else {
				return false;
			}
		}
		return true;
	}

	// We simply try to move towards our target location unless we're too close to it.
	// Again, since we're moving in a continuous way, have to do movement on fixedUpdate.
	void FixedUpdate() {
		Vector2 targetGlobalPos = Tile.toWorldCoord(_targetGridPos.x, _targetGridPos.y);
		if (Vector2.Distance(transform.position, targetGlobalPos) >= 0.1f) {
			// If we're away from our target position, move towards it.
			Vector2 toTargetPos = (targetGlobalPos - (Vector2)transform.position).normalized;
			moveViaVelocity(toTargetPos, moveSpeed, moveAcceleration);
		}
		else {
			moveViaVelocity(Vector2.zero, 0, moveAcceleration);
		}
	}

	// Colliding with a friendly should hurt it.
	void OnCollisionEnter2D(Collision2D collision) {
		Tile otherTile = collision.gameObject.GetComponent<Tile>();
		if (otherTile != null && otherTile.hasTag(TileTags.Friendly)) {
			otherTile.takeDamage(this, 1);
			Vector2 toOtherTile = (Vector2)otherTile.transform.position - (Vector2)transform.position;
			toOtherTile.Normalize();
			otherTile.addForce(damageForce*toOtherTile);
		}
	}

	// Check for potential weapons the moment we overlap them. 
	void OnTriggerEnter2D(Collider2D other) {
		Tile otherTile = other.GetComponent<Tile>();
		if (otherTile != null && tileWereHolding == null && otherTile.hasTag(TileTags.CanBeHeld) && otherTile.hasTag(TileTags.Weapon)) {
			otherTile.pickUp(this);
		}
	}

}
