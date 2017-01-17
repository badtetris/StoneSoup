using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : Tile {

	public float throwForce = 100f;

	public float onGroundThreshold = 0.1f;

	protected Tile _tileThatThrewUs = null;
	protected bool _isInAir = false;
	protected float _afterThrowCounter;
	public float afterThrowTime = 0.2f;

	// Idea is that we get thrown when we're used
	public override void useAsItem(Tile tileUsingUs) {
		if (_tileHoldingUs != tileUsingUs) {
			return;
		}
		if (onTransitionArea()) {
			return; // Don't allow us to be thrown while we're on a transition area.
		}
		_tileThatThrewUs = tileUsingUs;
		_isInAir = true;
		if (_tileThatThrewUs.GetComponent<Collider2D>() != null) {
			Physics2D.IgnoreCollision(_tileThatThrewUs.GetComponent<Collider2D>(), _collider, true);
		}
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 throwDir = (mousePos - (Vector2)transform.position).normalized;
		_body.bodyType = RigidbodyType2D.Dynamic;
		transform.parent = GameManager.instance.currentRoom.transform;
		_tileHoldingUs.tileWereHolding = null;
		_tileHoldingUs = null;

		_collider.isTrigger = false;
		_body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		_body.AddForce(throwDir*throwForce);

		_afterThrowCounter = afterThrowTime;
	}

	void Update() {
		if (_isInAir) {
			if (_afterThrowCounter > 0) {
				_afterThrowCounter -= Time.deltaTime;
			}
			else if (_body.velocity.magnitude <= onGroundThreshold) {
				_body.velocity = Vector2.zero;
				_body.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

				_collider.isTrigger = true;
				if (_tileThatThrewUs.GetComponent<Collider2D>() != null) {
					Physics2D.IgnoreCollision(_tileThatThrewUs.GetComponent<Collider2D>(), _collider, false);
				}
				addTag(TileTags.CanBeHeld);
				_isInAir = false;
			}
		}
	}


}
