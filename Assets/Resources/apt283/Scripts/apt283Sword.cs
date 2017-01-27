using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283Sword : Tile {

	protected bool _swinging = false;
	public float damageForce = 1000;

	public float swingSpeed = 720f;
	protected float _swingAngle;

	public Transform swingPivot;


	public override void useAsItem(Tile tileUsingUs) {
		if (_swinging || _tileHoldingUs != tileUsingUs) {
			return;
		}
		_swinging = true;
		swingPivot.transform.parent = tileUsingUs.transform;
		swingPivot.transform.localPosition = Vector3.zero;
		swingPivot.transform.localRotation = Quaternion.identity;
		transform.parent = swingPivot;
		transform.localPosition = new Vector3(0, -1f, 0);
		transform.localRotation = Quaternion.Euler(0, 0, 180);
		_swingAngle = 0;
	}

	public override void dropped(Tile tileDroppingUs) {
		if (_swinging) {
			return;
		}
		base.dropped(tileDroppingUs);
	}

	void Update() {
		if (_swinging) {
			_swingAngle += swingSpeed*Time.deltaTime;
			swingPivot.transform.localRotation = Quaternion.Euler(0, 0, _swingAngle);
			if (_swingAngle >= 360) {
				transform.parent = _tileHoldingUs.transform;
				transform.localPosition = new Vector3(0.2f, -0.1f, -0.1f);
				transform.localRotation = Quaternion.identity;
				swingPivot.transform.parent = transform;
				_swinging = false;
			}
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (_swinging && other.gameObject.tag == "Tile") {
			Tile otherTile = other.gameObject.GetComponent<Tile>();
			if (!otherTile.hasTag(TileTags.CanBeHeld)) {
				otherTile.takeDamage(1);
				otherTile.addForce((other.transform.position-_tileHoldingUs.transform.position).normalized*damageForce);
			}
		}
	}

}
