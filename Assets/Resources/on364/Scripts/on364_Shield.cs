using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class on364_Shield : Tile {
	public Collider2D groundCollider, heldCollider;

	public Sprite heldSpr, onGroundSpr;

	public float cooldownTimer;

	public override Collider2D mainCollider {
		get { return groundCollider; }
	}

	//This shield has a bs level of HP, but takes damage from everything. Bashing also eats dur like nothing else.

	public override void pickUp(Tile master){
		base.pickUp (master);
		if (_tileHoldingUs == master) {
			transform.parent = null;
			_sprite.sprite = heldSpr;
			groundCollider.enabled = false;
			heldCollider.enabled = true;
			_body.bodyType = RigidbodyType2D.Dynamic;
			Joint2D thisJoint = GetComponent<Joint2D> ();
			thisJoint.connectedBody = _tileHoldingUs.body;
			thisJoint.enabled = true;
		}
	}

	public override void dropped(Tile dropLoc){
		base.dropped (dropLoc);
		if (_tileHoldingUs == null) {
			Joint2D thisJoint = GetComponent<Joint2D> ();
			thisJoint.enabled = false;
			thisJoint.connectedBody = null;
			_body.bodyType = RigidbodyType2D.Kinematic;
			_body.velocity = Vector2.zero;
			_body.angularVelocity = 0;

			heldCollider.enabled = false;
			groundCollider.enabled = true;
			_sprite.sprite = onGroundSpr;

			transform.parent = dropLoc.transform.parent;
			transform.position = dropLoc.transform.position;
		}
	}


    // Update is called once per frame
    void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate () {
		
		if (_tileHoldingUs != null) {
			float aimLoc = Mathf.Atan2 (_tileHoldingUs.aimDirection.y, _tileHoldingUs.aimDirection.x) * Mathf.Rad2Deg;
			_body.MoveRotation (aimLoc);
		}
	}

	public override void useAsItem(Tile tileUsingUs) {
		if (cooldownTimer > 0) {
			return;
		}
		_tileHoldingUs.addForce (tileUsingUs.aimDirection.normalized * 2500);
		cooldownTimer = 2.0f;
	}
}