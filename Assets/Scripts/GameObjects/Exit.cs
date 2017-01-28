using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : Tile {

	public override void takeDamage(Tile tileDamagingUs, int amount, DamageType damageType) {
		// The exit doesn't take damage.
	}
}
