using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283SwordSwing : MonoBehaviour {

	public apt283BetterSword parentWeapon;
	public Tile parentSwinger;

	public float timeTillDeath = 0.5f;

	public float moveForwardSpeed = 0.2f;

	protected List<Tile> _buffer;
	protected int maxBufferSize = 10;


	void Start() {
		_buffer = new List<Tile>(maxBufferSize);
		Invoke("destroySelf", timeTillDeath);
	}

	void Update() {
		transform.localPosition += transform.right*moveForwardSpeed*Time.deltaTime;
		_buffer.RemoveAll(item => item == null);
		foreach (Tile tile in _buffer) {
			reApplyForce(tile);
		}
	}

	public void destroySelf() {
		Destroy(gameObject);
	}

	void OnTriggerEnter2D(Collider2D collider) {
		// Check to see if it has a body. If so, hit it! If not, end the swing!
		Tile otherTile = collider.GetComponent<Tile>();
		if (otherTile == parentWeapon || otherTile == parentSwinger) {
			return;
		}
		
		Rigidbody2D otherBody = collider.GetComponent<Rigidbody2D>();
		if (!collider.isTrigger && otherBody != null) {
			otherBody.AddForce((otherTile.transform.position-transform.position).normalized*1000f*otherBody.mass);
			if (otherTile != null && !otherTile.isBeingHeld && parentWeapon != null && !parentWeapon.attackedDuringSwing.Contains(otherTile)) {
				otherTile.takeDamage(parentWeapon, 1);
				parentWeapon.attackedDuringSwing.Add(otherTile);
				parentWeapon.takeSwingDamage();
				if (_buffer.Count < maxBufferSize) {
					_buffer.Add(otherTile);
				}
			}
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		Tile otherTile = other.GetComponent<Tile>();
		_buffer.Remove(otherTile);
	}



	protected void reApplyForce(Tile otherTile) {
		if (otherTile == parentWeapon || otherTile == parentSwinger) {
			return;
		}
		Rigidbody2D otherBody = otherTile.GetComponent<Rigidbody2D>();
		if (otherBody != null) {
			otherBody.AddForce((otherTile.transform.position-transform.position).normalized*500f*otherBody.mass);
		}
	}
}
