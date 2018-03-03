using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDetector : MonoBehaviour {

	protected Tile _parentTile;

	public bool detectOnStay = true;

	[SerializeField] 
	[EnumFlagsAttribute]
	public TileTags tagsToDetect = 0;

	void Start() {
		_parentTile = GetComponentInParent<Tile>();
		gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (_parentTile == null) {
			return;
		}
		Tile otherTile = other.GetComponent<Tile>();
		if (otherTile != null && otherTile.hasTag(tagsToDetect)) {
			_parentTile.tileDetected(otherTile);
		}
	}

	void OnTriggerStay2D(Collider2D other) {
		if (_parentTile == null || !detectOnStay) {
			return;
		}
		Tile otherTile = other.GetComponent<Tile>();
		if (otherTile != null && otherTile.hasTag(tagsToDetect)) {
			_parentTile.tileDetected(otherTile);
		}
	}

}
