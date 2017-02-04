using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A version of the rock that causes explosions when it hits things.
public class apt283ExplosiveRock : apt283Rock {

	public float explosionRadius = 2f;
	public float explosionForce = 2000;

	// When we die, we cause an explosion that does explosive damage to a surrounding radius.
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
