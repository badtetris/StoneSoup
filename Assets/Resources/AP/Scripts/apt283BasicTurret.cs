using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283BasicTurret : BasicAICreature {

	public GameObject bulletPrefab;

	protected Tile _lastTileWeFiredAt; 

	protected float _timeSinceLastFire = 0f;
	public float timeSinceLastFire {
		get { return _timeSinceLastFire; }
	}

	public float minTimeBetweenFires = 0.5f;

	public float shootForce = 500;
	
	// Update is called once per frame
	void Update () {
		_timeSinceLastFire += Time.deltaTime;
		if (_lastTileWeFiredAt != null) {
			if (!canSeeTile(_lastTileWeFiredAt)) {
				_lastTileWeFiredAt = null;
			}
			else {
				aimDirection = (_lastTileWeFiredAt.transform.position-transform.position).normalized;
			}
		}
	}

	public override void tileDetected(Tile otherTile) {
		if (otherTile == this) {
			return;
		}

		if (!canSeeTile(otherTile)) {
			return;
		}

		if (otherTile.hasTag(tagsWeChase) && _timeSinceLastFire >= minTimeBetweenFires) {
			// Time to fire at the tile. 
			_lastTileWeFiredAt = otherTile;
			_timeSinceLastFire = 0f;
			fire();			
		}
	}

	protected virtual void fire() {
		aimDirection = (_lastTileWeFiredAt.transform.position-transform.position).normalized;
		float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x)*Mathf.Rad2Deg;
		GameObject newBullet = Instantiate(bulletPrefab);
		newBullet.transform.parent = transform.parent;
		newBullet.transform.position = transform.position;
		newBullet.transform.rotation = Quaternion.Euler(0, 0, aimAngle);

		Tile newBulletTile = newBullet.GetComponent<Tile>();


		newBulletTile.init();
		newBulletTile.sprite.sortingOrder = sprite.sortingOrder+1;
		newBulletTile.sprite.sortingLayerID = SortingLayer.NameToID("Air");
		newBulletTile.addForce(aimDirection*shootForce);


		Physics2D.IgnoreCollision(mainCollider, newBullet.GetComponent<Tile>().mainCollider);
	}

}
