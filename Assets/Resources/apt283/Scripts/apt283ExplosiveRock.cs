using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283ExplosiveRock : apt283Rock {

	public float explosionRadius = 1.125f;
	public float explosionForce = 2000;

	protected bool _alive = true;

	public override void takeDamage(Tile tileDamagingUs, int amount, DamageType damageType) {
		if (!_alive) {
			return;
		}
		base.die();
	}

	protected override void die() {
		_alive = false;
		// Create an explosion that covers a relatively large circle
		Collider2D[] maybeColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
		foreach (Collider2D maybeCollider in maybeColliders) {
			Tile tile = maybeCollider.GetComponent<Tile>();
			if (tile == this) {
				continue;
			}
			if (tile != null) {
				tile.takeDamage(this, 2, DamageType.Explosive);
				tile.addForce((tile.transform.position-transform.position)*explosionForce);
			}
		}
		base.die();
	}

	public override void OnCollisionEnter2D(Collision2D collision) {
		if (collision.relativeVelocity.magnitude > damageThreshold) {
			die();
		}
	}

}
