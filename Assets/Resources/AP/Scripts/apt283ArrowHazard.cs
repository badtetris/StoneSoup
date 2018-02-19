using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283ArrowHazard : BasicAICreature {

	public GameObject arrowPrefab;
	public Vector2 arrowSpawnOffset;
	public Vector2 fireDirection = new Vector2(1, 0);

	public Sprite unloadedSprite;

	public GameObject tileDetectorObj;

	protected bool _loaded = true;

	public float shootForce = 2000f;

	protected float _timeUntilCanFire = 0.1f; // This just ensures that we don't fire at anything that's in view when we first spawn.

	// acts like a will and only takes explosive damage.
	public override void takeDamage(Tile tileDamagingUs, int amount, DamageType damageType) {
		if (damageType == DamageType.Explosive) {
			base.takeDamage(tileDamagingUs, amount, damageType);
		}
	}

	void Update() {
		takeStep();
		if (_timeUntilCanFire > 0) {
			_timeUntilCanFire -= Time.deltaTime;
		}
	}

	public override void tileDetected(Tile otherTile) {
		if (_timeUntilCanFire > 0 || !_loaded) {
			return;
		}
		// Now, see if we can SEE the other tile. 
		if (!canSeeTile(otherTile)) {
			return;
		}

		// If we can, then FIRE
		GameObject arrowObj = Instantiate(arrowPrefab);
		arrowObj.transform.parent = transform.parent;
		Physics2D.IgnoreCollision(_collider, arrowObj.GetComponent<Collider2D>());
		arrowObj.transform.position = transform.position + (Vector3)arrowSpawnOffset;

		float aimAngle = Mathf.Atan2(fireDirection.y, fireDirection.x)*Mathf.Rad2Deg;
		arrowObj.transform.rotation = Quaternion.Euler(0, 0, aimAngle);

		arrowObj.GetComponent<Tile>().init();
		arrowObj.GetComponent<Tile>().addForce(fireDirection.normalized*shootForce);

		_loaded = false;
		_sprite.sprite = unloadedSprite;
		Destroy(tileDetectorObj);
	}

	void OnEnable() {
		_timeUntilCanFire = 0.1f;
	}

	
}
