using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283SimpleBullet : Tile {

	// Use this for initialization
	void Start () {
		if (GetComponent<TrailRenderer>() != null) {
			GetComponent<TrailRenderer>().Clear();
		}
	}
	
	protected override void updateSpriteSorting() {
		base.updateSpriteSorting();
		_sprite.sortingOrder++;
		_sprite.sortingLayerID = SortingLayer.NameToID("Air");
	}

	void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.GetComponent<Tile>() != null) {
			Tile otherTile = collision.gameObject.GetComponent<Tile>();
			otherTile.takeDamage(this, 1);
		}
		die();
	}
}
