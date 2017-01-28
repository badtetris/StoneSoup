﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : Tile {

	public override void takeDamage(Tile tileDamagingUs, int amount, DamageType damageType) {
		if (damageType == DamageType.Explosive) {
			base.takeDamage(tileDamagingUs, amount, damageType);
		}
	}
}
