using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283Bullet : Tile {

	public float damageThreshold = 14;

	public float onGroundThreshold = 1f;

	protected float _destroyTimer = 0.5f;

	protected ContactPoint2D[] _contacts = null;

	void Start() {
		_contacts = new ContactPoint2D[10];
		GetComponent<TrailRenderer>().Clear();
	}

	void Update() {
		// If we're moving kinda slow now we can just delete ourselves.
		if (_body.velocity.magnitude <= onGroundThreshold) {
			_destroyTimer -= Time.deltaTime;
			if (_destroyTimer <= 0) {
				Destroy(gameObject);
			}
		}
	}

	public virtual void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.GetComponent<Tile>() != null) {
			bool collisionPowerfulEnoughToHurt = false;
			int numContacts = collision.GetContacts(_contacts);
			Debug.Log("START BULLET COLLISION WITH: " + collision.gameObject);
			Debug.Log("Number of contacts: " + numContacts);
			if (numContacts == 0) {
				foreach (ContactPoint2D contact in collision.contacts) {
					Debug.Log("(EXTRA) Normal: " + contact.normalImpulse + " Tangent: " + contact.tangentImpulse + "CALCULATED: " + Vector2.Dot(contact.relativeVelocity, contact.normal));				
					//if (Vector2.Dot(contact.relativeVelocity, contact.normal) >= damageThreshold) {
					//	collisionPowerfulEnoughToHurt = true;
					//}
				}
			}
			else {
				for (int i = 0; i < numContacts && i < _contacts.Length; i++) {
					Debug.Log("Normal: " + _contacts[i].normalImpulse + " Tangent: " + _contacts[i].tangentImpulse + "CALCULATED: " + Vector2.Dot(_contacts[i].relativeVelocity, _contacts[i].normal));				
					if (Vector2.Dot(_contacts[i].relativeVelocity, _contacts[i].normal) >= damageThreshold) {
						collisionPowerfulEnoughToHurt = true;
					}
				}
			}
			if (!collisionPowerfulEnoughToHurt) {
				Debug.Log("UNHURT!");
				return;
			}
			Debug.Log("HURT!");
			Tile otherTile = collision.gameObject.GetComponent<Tile>();
			otherTile.takeDamage(this, 1);
		}
	}
}
