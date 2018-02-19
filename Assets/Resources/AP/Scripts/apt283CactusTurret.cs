using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283CactusTurret : apt283BasicTurret {

	public int numBullets = 8;

	public float bulletSpawnOffset = 0.5f;

	protected bool _firing = false;

	public Sprite blinkSprite, normalSprite;

	protected override void fire() {
		if (_firing) {
			return;
		}
		_firing = true;
		_sprite.GetComponent<ObjShake>().shake();
		_sprite.sprite = blinkSprite;
		float shakeTime = _sprite.GetComponent<ObjShake>().shakeTime;
		Invoke("finishFire", shakeTime);
	}

	protected void finishFire() {
		_firing = false;

		_sprite.sprite = normalSprite;

		// Now we spawn all of our bullets in a circle.
		float anglePerBullet = 360f / (float)numBullets;

		for (int i = 0; i < numBullets; i++) {
			float angle = anglePerBullet*i;

			Vector2 bulletAim = new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad), Mathf.Sin(angle*Mathf.Deg2Rad));

			GameObject newBullet = Instantiate(bulletPrefab);
			newBullet.transform.parent = transform.parent;
			newBullet.transform.position = transform.position + (Vector3)(bulletSpawnOffset*bulletAim);
			newBullet.transform.rotation = Quaternion.Euler(0, 0, angle);
			Tile newBulletTile = newBullet.GetComponent<Tile>();
			newBulletTile.init();
			newBulletTile.addForce(bulletAim*shootForce);

			Physics2D.IgnoreCollision(mainCollider, newBulletTile.mainCollider);
		}

	}

}
