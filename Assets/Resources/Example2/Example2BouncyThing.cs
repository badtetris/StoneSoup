using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example2BouncyThing : Tile {

	void OnCollisionEnter2D(Collision2D collisionInfo) {
		Tile otherTile = collisionInfo.gameObject.GetComponent<Tile>();
		if (otherTile != null) {
			otherTile.takeDamage(this, 1, DamageType.Explosive);
		}
	}

}
