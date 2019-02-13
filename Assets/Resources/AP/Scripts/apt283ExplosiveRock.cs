using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A version of the rock that causes explosions when it hits things.
public class apt283ExplosiveRock : apt283Rock {

	public float explosionRadius = 2f;
	public float explosionForce = 2000;

	protected apt283PulseEffect _pulseEffect;
	public float normalPulsePeriod = 1f;
	public float heldPulsePeriod = 0.5f;

	public bool startsInAir = false;

	void Start() {
		_pulseEffect = GetComponentInChildren<apt283PulseEffect>();
		if (startsInAir) {
			_isInAir = true;
            _afterThrowCounter = afterThrowTime;
		}
	}

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

	protected override void Update() {
		base.Update();
		if (_pulseEffect != null) {
			if (_tileHoldingUs != null) {
				_pulseEffect.pulsePeriod = heldPulsePeriod;		
			}
			else {
				_pulseEffect.pulsePeriod = normalPulsePeriod;
			}
		}
	}

}
