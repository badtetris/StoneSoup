using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Expose the swing parameters as public variables.
public class apt283BetterSword : Tile {

	protected bool _swinging = false;

	protected bool _readyForSecondSwing = false;
	protected bool _doSecondSwing = false;
	
	public Transform swingPivot;

	protected Vector2 _swingDir;

	public GameObject swingPrefab;
	public AudioClip swingSound;

	// To ensure we don't double attack. 
	protected List<Tile> _attackedDuringSwing = new List<Tile>(10);
	public List<Tile> attackedDuringSwing {
		get { return _attackedDuringSwing; }
	}

	/*public override string inventoryDescription { 
		get { return health.ToString(); }
	}*/

	public override void takeDamage(Tile tileDamagingUs, int damageAmount, DamageType damageType) {
		if (!_alive) {
			return;
		}
		health-=damageAmount;
		if (health <= 0) {
			die();
		}
	}

	protected bool _takenSwingDamage = false;
	public void takeSwingDamage() {
		if (!_takenSwingDamage) {
			takeDamage(this, 1);
			_takenSwingDamage = true;
		}
	}

	protected override void die() {
		if (swingPivot != null) {
			Destroy(swingPivot.gameObject);
		}
		base.die();
	}

	protected virtual void aim() {
		if (_swinging) {
			return;
		}
		float aimAngle = Mathf.Atan2(_tileHoldingUs.aimDirection.y, _tileHoldingUs.aimDirection.x)*Mathf.Rad2Deg;
		swingPivot.transform.localRotation = Quaternion.Euler(0, 0, aimAngle+135);
	}

	/*
    public override bool canAutoPickUp(Tile tilePickingUsUp) {
		if (_tileHoldingUs != null || !_alive) {
			return false;
		}
		FarmSword maybeSword = null;
		foreach (Transform child in tilePickingUsUp.transform) {
			maybeSword = child.GetComponentInChildren<FarmSword>(true);
			if (maybeSword != null &&  maybeSword.GetType() == GetType() && maybeSword.swingPrefab == swingPrefab) {
				return true;
			}
		}
		return false;
	}
	*/

	public override void pickUp(Tile tilePickingUsUp) {
		if (!_alive) {
			return;
		}
		base.pickUp(tilePickingUsUp);
		mainCollider.enabled = false;
		if (_tileHoldingUs == tilePickingUsUp) {
			finishPickUp(tilePickingUsUp);
		}
	}

	public virtual void finishPickUp(Tile tilePickingUsUp) {
		swingPivot.transform.parent = _tileHoldingUs.transform;
		swingPivot.transform.localPosition = Vector3.zero;
		transform.parent = swingPivot;
		transform.localPosition = new Vector3(1f, 0, 0);
		transform.localRotation = Quaternion.identity;
	}

	void Update() {
		if (_tileHoldingUs != null) {
			aim();
		}
		updateSpriteSorting();
	}

	public override void dropped(Tile tileDroppingUs) {
		if (_swinging) {
			return;
		}
		base.dropped(tileDroppingUs);
		mainCollider.enabled = true;
		if (swingPivot != null) {
			swingPivot.transform.parent = transform;
		}
	}

	public override void useAsItem(Tile tileUsingUs) {
		if (_swinging) {
			if (_readyForSecondSwing) {
				_doSecondSwing = true;
			}
			return;
		}
		StartCoroutine(swingProcess());
	}

	void OnDisable() {
		_swinging = false;
		StopAllCoroutines();
	}

	void OnEnable() {
		if (_tileHoldingUs != null && swingPivot != null) {
			transform.parent = swingPivot.transform;
		}
	}


	protected virtual IEnumerator swingProcess() {
		_swinging = true;
		_readyForSecondSwing = false;
		_doSecondSwing = false;
		_takenSwingDamage = false;

		_attackedDuringSwing.Clear();

		mainCollider.enabled = true;

		AudioManager.playAudio(swingSound);

		float aimAngle = Mathf.Atan2(_tileHoldingUs.aimDirection.y, _tileHoldingUs.aimDirection.x)*Mathf.Rad2Deg;
		GameObject swingObj = Instantiate(swingPrefab);

		swingObj.transform.rotation = Quaternion.Euler(0, 0, aimAngle);
		//swingObj.transform.position = transform.position + swingObj.transform.right*1f;
		swingObj.transform.position = _tileHoldingUs.transform.position + swingObj.transform.right*1f;
		swingObj.transform.parent = _tileHoldingUs.transform;
		swingObj.GetComponent<apt283SwordSwing>().parentWeapon = this;
		swingObj.GetComponent<apt283SwordSwing>().parentSwinger = _tileHoldingUs;


		float swingAmount = 0f;
		float swingSpeed = 720f;
		float swingAcceleration = 8000f;
		_swingDir = _tileHoldingUs.aimDirection.normalized;

		transform.localPosition = new Vector3(_sprite.sprite.bounds.size.x/2f+0.5f, 0, 0);
		if (_tileHoldingUs.body.velocity.magnitude < 8f) {
			_tileHoldingUs.addForce(_tileHoldingUs.aimDirection.normalized*500f);
		}

		while (swingAmount < 210f) {
			swingAmount += Time.fixedDeltaTime*swingSpeed;
			swingSpeed += swingAcceleration*Time.fixedDeltaTime;
			swingPivot.transform.localRotation = Quaternion.Euler(0, 0, aimAngle+135f-swingAmount);

			if (swingAmount >= 100f) {
				_readyForSecondSwing = true;
			}

			yield return new WaitForFixedUpdate();
		}

		yield return new WaitForSeconds(0.05f);

		transform.localPosition = new Vector3(1f, 0, 0);
		if (_doSecondSwing) {
			yield return StartCoroutine(secondSwingProcess(aimAngle));
		}
		_swinging = false;
		mainCollider.enabled = false;
	}

	protected virtual IEnumerator secondSwingProcess(float aimAngle) {

		GameObject swingObj = Instantiate(swingPrefab);

		AudioManager.playAudio(swingSound);
		_takenSwingDamage = false;
		_attackedDuringSwing.Clear();

		swingObj.transform.rotation = Quaternion.Euler(0, 0, aimAngle);
		swingObj.transform.position = transform.position + swingObj.transform.right*1f;
		//swingObj.transform.position = _tileHoldingUs.transform.position + swingObj.transform.right*1f;

		swingObj.transform.parent = _tileHoldingUs.transform;
		swingObj.GetComponent<apt283SwordSwing>().parentWeapon = this;
		swingObj.GetComponent<apt283SwordSwing>().parentSwinger = _tileHoldingUs;


		float swingAmount = 0f;
		float swingSpeed = 720f;
		float swingAcceleration = 10000f;
		transform.localPosition = new Vector3(_sprite.sprite.bounds.size.x/2f+0.9f, 0, 0);
		if (_tileHoldingUs.body.velocity.magnitude < 8f) {
			_tileHoldingUs.addForce(_tileHoldingUs.aimDirection.normalized*700f);
		}
		while (swingAmount < 210f) {
			swingAmount += Time.fixedDeltaTime*swingSpeed;
			swingSpeed += swingAcceleration*Time.fixedDeltaTime;
			swingPivot.transform.localRotation = Quaternion.Euler(0, 0, aimAngle-135f+swingAmount);

			yield return new WaitForFixedUpdate();
		}
		transform.localPosition = new Vector3(1f, 0, 0);

	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (_swinging) {
			// Check to see if it has a body. If so, hit it! If not, end the swing!
			Tile otherTile = collider.GetComponent<Tile>();
			Rigidbody2D otherBody = collider.GetComponent<Rigidbody2D>();
			if (!collider.isTrigger && otherBody != null && otherTile != _tileHoldingUs) {
				otherBody.AddForce((otherTile.transform.position-_tileHoldingUs.transform.position).normalized*1000f*otherBody.mass);
				if (otherTile != null && !otherTile.isBeingHeld && !_attackedDuringSwing.Contains(otherTile)) {
					otherTile.takeDamage(this, 1);
					_attackedDuringSwing.Add(otherTile);
					takeSwingDamage();
				}
			}
		}
	}

	void OnTriggerStay2D(Collider2D collider) {
		if (_swinging) {
			Tile otherTile = collider.GetComponent<Tile>();
			Rigidbody2D otherBody = collider.GetComponent<Rigidbody2D>();
			if (!collider.isTrigger && otherBody != null && otherTile != _tileHoldingUs) {
				otherBody.AddForce((otherTile.transform.position-_tileHoldingUs.transform.position).normalized*500f*otherBody.mass);
			}
		}
	}




}
