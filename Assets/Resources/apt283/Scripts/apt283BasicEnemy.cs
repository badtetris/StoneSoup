using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283BasicEnemy : Tile {

	public float damageForce = 2000;

	protected Vector2 _targetGridPos;

	public float moveSpeed = 5;
	public float moveAcceleration = 100;


	void Start() {
		_targetGridPos = Tile.toGridCoord(globalX, globalY);
	}

	void Update() {
	}

	void FixedUpdate() {
		Vector2 targetGlobalPos = Tile.toLocalCoord(_targetGridPos.x, _targetGridPos.y);
		if (Vector2.Distance(transform.position, targetGlobalPos) >= 0.5f) {
			// If we're away from our target position, move towards it.
			Vector2 toTargetPos = (targetGlobalPos - (Vector2)transform.position).normalized;
			moveViaVelocity(toTargetPos, moveSpeed, moveAcceleration);
		}
	}

	void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.tag == "Tile") {
			Tile otherTile = collision.gameObject.GetComponent<Tile>();
			if (otherTile.hasTag(TileTags.Friendly)) {
				otherTile.takeDamage(1);
				Vector2 toOtherTile = (Vector2)otherTile.transform.position - (Vector2)transform.position;
				toOtherTile.Normalize();
				otherTile.addForce(damageForce*toOtherTile);
			}
		}
	}
}
