using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283Rock : Tile {

	public float throwForce = 100f;

	public float onGroundThreshold = 0.8f;

	public float damageThreshold = 7;
	public float damageForce = 1000;

	protected Tile _tileThatThrewUs = null;
	protected bool _isInAir = false;
	protected float _afterThrowCounter;
	public float afterThrowTime = 0.2f;

	public override void takeDamage(Tile tileDamagingUs, int amount, DamageType damageType) {
		if (damageType == DamageType.Explosive) {
			base.takeDamage(tileDamagingUs, amount, damageType);
		}
	}


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
		Vector2 throwDir = _tileThatThrewUs.aimDirection.normalized;
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
				if (_afterThrowCounter <= 0 && _tileThatThrewUs != null && _tileThatThrewUs.GetComponent<Collider2D>() != null) {
					Physics2D.IgnoreCollision(_tileThatThrewUs.GetComponent<Collider2D>(), _collider, false);
				}
				_body.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
				_collider.isTrigger = true;
				addTag(TileTags.CanBeHeld);
				_isInAir = false;
				_sprite.sortingLayerID = SortingLayer.NameToID("Floor");

			}
		}
	}

	public virtual void OnCollisionEnter2D(Collision2D collision) {
		if (_isInAir && collision.gameObject.GetComponent<Tile>() != null) {
			// First, make sure we're going fast enough to do damage
			if (collision.relativeVelocity.magnitude <= damageThreshold) {
				return;
			}
			Tile otherTile = collision.gameObject.GetComponent<Tile>();
			otherTile.takeDamage(this, 1);
			otherTile.addForce(_body.velocity.normalized*damageForce);
		}
	}



}
