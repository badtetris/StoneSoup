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
public class apt283BasicEnemy : BasicAICreature{

	// How much force we inflict if something collides with us.
	public float damageForce = 1000;
	public int damageAmount = 1;

	// We use counters to determine when to next try to move.
	protected float _nextMoveCounter;
	public float timeBetweenMovesMin = 1.5f;
	public float timeBetweenMovesMax = 3f;

	// Occasionally we'll start with a weapon pre-spawned on top of us. 
	public GameObject[] maybeWeaponsToStartWith;

	public override void init() {
		base.init();

		// Here's where we spawn a random weapon for us. 
		if (maybeWeaponsToStartWith != null && maybeWeaponsToStartWith.Length > 0) {
			GameObject maybeWeaponPrefab = GlobalFuncs.randElem(maybeWeaponsToStartWith);
			if (maybeWeaponPrefab != null) {
				Vector2 ourGridPos = toGridCoord(localX, localY);
				Tile.spawnTile(maybeWeaponPrefab, transform.parent, (int)ourGridPos.x, (int)ourGridPos.y);
			}
		}
	}



	public override void Start() {
		_targetGridPos = Tile.toGridCoord(globalX, globalY);
		_nextMoveCounter = Random.Range(timeBetweenMovesMin, timeBetweenMovesMax);
	}

	void Update() {
		// Update our counters.
		if (_nextMoveCounter > 0) {
			_nextMoveCounter -= Time.deltaTime;
		}

		// When it's time to try a new move.
		if (_nextMoveCounter <= 0) {
			takeStep();
		}

		updateSpriteSorting();
	}	

	protected override void takeStep() {
		// Try to move to one of our neighboring positions if it is empty.
		_neighborPositions.Clear();

		// We test neighbor locations by casting in specific directions. 

		Vector2 upGridNeighbor = new Vector2(_targetGridPos.x, _targetGridPos.y+1);
		if (pathIsClear(toWorldCoord(upGridNeighbor))) {
			_neighborPositions.Add(upGridNeighbor);
		}
		Vector2 rightGridNeighbor = new Vector2(_targetGridPos.x+1, _targetGridPos.y);
		if (pathIsClear(toWorldCoord(rightGridNeighbor))) {
			_neighborPositions.Add(rightGridNeighbor);
		}
		Vector2 downGridNeighbor = new Vector2(_targetGridPos.x, _targetGridPos.y-1);
		if (pathIsClear(toWorldCoord(downGridNeighbor))) {
			_neighborPositions.Add(downGridNeighbor);
		}
		Vector2 leftGridNeighbor = new Vector2(_targetGridPos.x-1, _targetGridPos.y);
		if (pathIsClear(toWorldCoord(leftGridNeighbor))) {
			_neighborPositions.Add(leftGridNeighbor);
		}

		// If there's an empty neighbor, choose one randomly.
		if (_neighborPositions.Count > 0) {
			_targetGridPos = GlobalFuncs.randElem(_neighborPositions);
			_nextMoveCounter = Random.Range(timeBetweenMovesMin, timeBetweenMovesMax);
		}
	}


	public override void tileDetected(Tile otherTile) {
		if (otherTile == this) {
			return;
		}
		// If we're holding a weapon and we detect something we'd like to attack, FIRE!
		if (tileWereHolding != null && otherTile.hasTag(tagsWeChase)) {
			aimDirection = ((Vector2)otherTile.transform.position-(Vector2)transform.position).normalized;
			tileWereHolding.useAsItem(this);
		}
		// If we're not holding a weapon and we detect a nearby weapon, PICK IT UP!
		if (tileWereHolding == null && otherTile.hasTag(TileTags.Weapon) && otherTile.hasTag(TileTags.CanBeHeld)) {
			otherTile.pickUp(this);
		}
	}


	// Colliding with somethign we want to attack should hurt it.
	void OnCollisionEnter2D(Collision2D collision) {
		Tile otherTile = collision.gameObject.GetComponent<Tile>();
		if (otherTile != null && otherTile.hasTag(tagsWeChase)) {
			otherTile.takeDamage(this, damageAmount);
			Vector2 toOtherTile = (Vector2)otherTile.transform.position - (Vector2)transform.position;
			toOtherTile.Normalize();
			otherTile.addForce(damageForce*toOtherTile);
		}
	}

	// Check for potential weapons the moment we overlap them (we also poll for them). 
	void OnTriggerEnter2D(Collider2D other) {
		Tile otherTile = other.GetComponent<Tile>();
		if (otherTile != null && tileWereHolding == null && otherTile.hasTag(TileTags.CanBeHeld) && otherTile.hasTag(TileTags.Weapon)) {
			otherTile.pickUp(this);
		}
	}

}
