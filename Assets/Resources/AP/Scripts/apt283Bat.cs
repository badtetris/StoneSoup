using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283Bat : Tile {

	protected bool _swinging = false;

	public float swingForce = -10000f;

	public float pConstant = 100f;
	public float dConstant = 10f;

	public Sprite bigBatSprite;
	public Sprite smallBatSprite;

	// We use a pivot object to swing around whatever's holding us
	// (since we can't rotate whatever's holding us).
	// When we're not swinging, the pivot hangs around as our child.
	// When we're swinging, we swap places with the pivot so it becomes our parent.
	public Transform swingPivot;

	// Walls only take explosive damage.
	public override void takeDamage(Tile tileDamagingUs, int amount, DamageType damageType) {
		if (damageType == DamageType.Explosive) {
			base.takeDamage(tileDamagingUs, amount, damageType);
		}
	}

	void FixedUpdate() {
		if (_tileHoldingUs != null && !_swinging) {
			// We try to aim 135 degrees to the left of the aim direction. 
			float aimAngle = Mathf.Atan2(_tileHoldingUs.aimDirection.y, _tileHoldingUs.aimDirection.x)*Mathf.Rad2Deg;
			swingPivot.transform.localRotation = Quaternion.Euler(0, 0, aimAngle+135);
		}
		updateSpriteSorting();
	}

	public override void pickUp(Tile tilePickingUsUp) {
		base.pickUp(tilePickingUsUp);
		if (_tileHoldingUs == tilePickingUsUp) {
			swingPivot.transform.parent = _tileHoldingUs.transform;
			swingPivot.transform.localPosition = Vector3.zero;
			transform.parent = swingPivot;
			transform.localPosition = new Vector3(1, 0, 0);
			transform.localRotation = Quaternion.identity;
		}
	}

	
	public override void useAsItem(Tile tileUsingUs) {
		if (_swinging) {
			return;
		}


		_swinging = true;
		StartCoroutine(swingProcess());
	}

	public override void dropped(Tile tileDroppingUs) {
		if (_swinging) {
			return;
		}
		base.dropped(tileDroppingUs);
	}

	protected Vector2 _swingDir;
	protected Coroutine _swingRoutine;
	protected float _startSwingAngle;

	protected IEnumerator swingProcess() {
		_swingDir = _tileHoldingUs.aimDirection.normalized;
		float aimAngle = Mathf.Atan2(_tileHoldingUs.aimDirection.y, _tileHoldingUs.aimDirection.x)*Mathf.Rad2Deg;
		_startSwingAngle = aimAngle+135f;
		// Start with a windup. 
		_sprite.sprite = bigBatSprite;

		float windupAmount = 0f;
		while (windupAmount < 1f) {
			_tileHoldingUs.addForce(-_swingDir*300f);
			windupAmount += Time.fixedDeltaTime*16f;
			swingPivot.localPosition = -_swingDir*0.5f*windupAmount;
			yield return new WaitForFixedUpdate();
		}

		// then the Swing. 

		GetComponent<TrailRenderer>().enabled = true;
		GetComponent<TrailRenderer>().Clear();

		swingPivot.transform.localPosition = Vector3.zero;
		float swingAmount = 0f;
		float swingSpeed = 1440f;
		while (swingAmount < 200) {
			//Debug.Log(swingAmount);
			Debug.Log(_tileHoldingUs.GetComponent<Rigidbody2D>().velocity);
			//float angle = aimAngle+135f-swingAmount;
			//Vector2 forceDir = new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad), Mathf.Sin(angle*Mathf.Deg2Rad));
			_tileHoldingUs.addForce(_swingDir*300f);
			swingAmount += Time.fixedDeltaTime*swingSpeed;
			swingPivot.transform.localRotation = Quaternion.Euler(0, 0, aimAngle+135f-swingAmount);
			yield return new WaitForFixedUpdate();
		}

		yield return new WaitForSeconds(0.1f);

		_swinging = false;
		GetComponent<TrailRenderer>().enabled = false;
		_sprite.sprite = smallBatSprite;

	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (_swinging) {
			// Check to see if it has a body. If so, hit it! If not, end the swing!
			Tile otherTile = collider.GetComponent<Tile>();
			if (otherTile != null && otherTile != _tileHoldingUs) {
				otherTile.takeDamage(this, 1);
			}
			Rigidbody2D otherBody = collider.GetComponent<Rigidbody2D>();
			if (otherBody != null && otherTile != _tileHoldingUs) {
				otherBody.AddForce(_swingDir*1000f);
			}

		}
	}

}
