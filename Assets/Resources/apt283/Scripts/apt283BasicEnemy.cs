using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283BasicEnemy : Tile {

	public GameObject zapPrefab;


	public float damageForce = 2000;

	protected Vector2 _targetGridPos;

	public float moveSpeed = 5;
	public float moveAcceleration = 100;

	protected float _nextMoveCounter;
	public float timeBetweenMovesMin = 1.5f;
	public float timeBetweenMovesMax = 3f;

	protected float _checkForPlayerCounter;
	public float checkForPlayerTime = 1f;
	public float playerAwarenessRadius = 4f;


	protected List<Vector2> _neighborPositions;

	protected override void die() {
		base.die();
		Instantiate(zapPrefab, transform.position, Quaternion.identity);
	}


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
		if (_nextMoveCounter > 0) {
			_nextMoveCounter -= Time.deltaTime;
		}
		if (_checkForPlayerCounter > 0) {
			_checkForPlayerCounter -= Time.deltaTime;
		}

		if (_nextMoveCounter <= 0) {
			// Try to move to one of our neighboring positions if it is empty.
			_neighborPositions.Clear();

			float oldX = x;
			float oldY = y;

			// Try up.
			Vector2 upGridNeighbor = new Vector2(_targetGridPos.x, _targetGridPos.y+1);
			// Use a collider cast to figure out if anything's blocking us up there.
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

			if (_neighborPositions.Count > 0) {
				_targetGridPos = GlobalFuncs.getRandom(_neighborPositions);
				_nextMoveCounter = Random.Range(timeBetweenMovesMin, timeBetweenMovesMax);
			}

			// If we're not carrying something, but we're on top of a weapon that can be picked up, pick it up!
			if (tileWereHolding == null) {

				int numObjectsFound = _body.Cast(Vector2.zero, _maybeRaycastResults);
				for (int i = 0; i < numObjectsFound && i < _maybeRaycastResults.Length; i++) {
					RaycastHit2D result = _maybeRaycastResults[i];
					if (result.transform.gameObject.tag != "Tile") {
						continue;
					}
					Tile tileHit = result.transform.GetComponent<Tile>();
					if (tileHit.hasTag(TileTags.Weapon) && tileHit.hasTag(TileTags.CanBeHeld)) {
						tileHit.pickUp(this);
					}
				}
			}

		}

		if (tileWereHolding != null) {
			if (_checkForPlayerCounter <= 0) {
			// Send our a big circle to look for the player.
				Collider2D[] maybeColliders = Physics2D.OverlapCircleAll(transform.position, playerAwarenessRadius);
				foreach (Collider2D maybeCollider in maybeColliders) {
					if (maybeCollider.tag == "Tile") {
						Tile tile = maybeCollider.GetComponent<Tile>();
						if (tile.hasTag(TileTags.Friendly)) {
							// We've found something to throw a rock at
							aimDirection = ((Vector2)tile.transform.position-(Vector2)transform.position).normalized;
							tileWereHolding.useAsItem(this);
							break;
						}
					}
				}

				_checkForPlayerCounter = checkForPlayerTime;
			}
		}



	}	

	protected bool isValidMoveDir(Vector2 direction) {
		int numCollisions = _body.Cast(direction, _maybeRaycastResults, Tile.TILE_SIZE);
		for (int i = 0; i < numCollisions && i < _maybeRaycastResults.Length; i++) {
			RaycastHit2D result = _maybeRaycastResults[i];
			if (result.transform.gameObject.tag == "Tile") {
				Tile resultTile = result.transform.GetComponent<Tile>();
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

	void FixedUpdate() {
		Vector2 targetGlobalPos = Tile.toLocalCoord(_targetGridPos.x, _targetGridPos.y);
		if (Vector2.Distance(transform.position, targetGlobalPos) >= 0.1f) {
			// If we're away from our target position, move towards it.
			Vector2 toTargetPos = (targetGlobalPos - (Vector2)transform.position).normalized;
			moveViaVelocity(toTargetPos, moveSpeed, moveAcceleration);
		}
		else {
			moveViaVelocity(Vector2.zero, 0, moveAcceleration);
		}
	}

	void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.tag == "Tile") {
			Tile otherTile = collision.gameObject.GetComponent<Tile>();
			if (otherTile.hasTag(TileTags.Friendly)) {
				otherTile.takeDamage(this, 1);
				Vector2 toOtherTile = (Vector2)otherTile.transform.position - (Vector2)transform.position;
				toOtherTile.Normalize();
				otherTile.addForce(damageForce*toOtherTile);
			}
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		Tile otherTile = other.GetComponent<Tile>();
		if (tileWereHolding == null && otherTile != null && otherTile.hasTag(TileTags.CanBeHeld) && otherTile.hasTag(TileTags.Weapon)) {
			otherTile.pickUp(this);
		}
	}

}
