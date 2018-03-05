 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283Shield : Tile {

	public Collider2D onGroundCollider;
	public Collider2D heldCollider;

	public Sprite heldSprite;
	public Sprite onGroundSprite;

	public override Collider2D mainCollider {
		get { return onGroundCollider; }
	}

	// shields only take explosive damage.
	//public override void takeDamage(Tile tileDamagingUs, int amount, DamageType damageType) {
		//if (damageType == DamageType.Explosive) {
		//	base.takeDamage(tileDamagingUs, amount, damageType);
		//}
	//}

	public override void pickUp(Tile tilePickingUsUp) {
		base.pickUp(tilePickingUsUp);
		if (_tileHoldingUs == tilePickingUsUp) {
			transform.parent = null; // To make joints work, we have to do this. 
			_sprite.sprite = heldSprite;
			onGroundCollider.enabled = false;
			heldCollider.enabled = true;
			_body.bodyType = RigidbodyType2D.Dynamic;
			Joint2D ourJoint = GetComponent<Joint2D>();
			ourJoint.connectedBody = _tileHoldingUs.body;
			ourJoint.enabled = true;
		}
	}

	public override void dropped(Tile tileDroppingUs) {
		base.dropped(tileDroppingUs);

		if (_tileHoldingUs == null) {
			Joint2D ourJoint = GetComponent<Joint2D>();
			ourJoint.enabled = false;
			ourJoint.connectedBody = null;

			_body.bodyType = RigidbodyType2D.Kinematic;
			_body.velocity = Vector2.zero;
			_body.angularVelocity = 0;

			heldCollider.enabled = false;
			onGroundCollider.enabled = true;
			_sprite.sprite = onGroundSprite;

			transform.parent = tileDroppingUs.transform.parent;
			transform.position = tileDroppingUs.transform.position;
		}
	}

	void Update() {
		if (_tileHoldingUs != null) {
			tileName = string.Format("Shield (HP: {0})", health);
		}
	}

	void FixedUpdate() {
		if (_tileHoldingUs != null) {
			// Let's try to rotate towards the aim direction. 
			float aimAngle = Mathf.Atan2(_tileHoldingUs.aimDirection.y, _tileHoldingUs.aimDirection.x)*Mathf.Rad2Deg;
 			_body.MoveRotation(aimAngle);
		}
	}




}
