using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum TileTags {
	Wall = 0x01,
	CanBeHeld = 0x02,
	Creature = 0x04,
	Player = 0x08,
	Enemy = 0x10
}

public class Tile : MonoBehaviour {

	public const float TILE_SIZE = 2f;

	public float x {
		get { return transform.localPosition.x; }
		set { transform.localPosition = new Vector3(value, transform.localPosition.y, transform.localPosition.z); }
	}

	public float y {
		get { return transform.localPosition.y; }
		set { transform.localPosition = new Vector3(transform.localPosition.x, value, transform.localPosition.z); }
	}

	[SerializeField] 
	[EnumFlagsAttribute]
	public TileTags tags = 0;

	public bool hasTag(TileTags tag) {
		return (tags & tag) != 0;	
	}

	protected void removeTag(TileTags tagsToRemove) {
		tags = tags & ~(tagsToRemove);
	}

	protected void addTag(TileTags tagsToAdd) {
		tags |= tagsToAdd;
	}



	protected Rigidbody2D _body;
	protected SpriteRenderer _sprite;
	public SpriteRenderer sprite {
		get { return _sprite; }
	}
	protected Animator _anim;
	protected Collider2D _collider;

	[HideInInspector]
	public Tile tileWereHolding;

	protected Tile _tileHoldingUs;

	protected void moveViaVelocity(Vector2 direction, float speed, float acceleration) {
		if (_body != null) {
			Vector2 currentVelocity = _body.velocity;
			currentVelocity = Vector2.MoveTowards(currentVelocity, direction*speed, acceleration*Time.fixedDeltaTime);
			_body.velocity = currentVelocity;
		}
	}

	

	public virtual void init() {
		_sprite = GetComponent<SpriteRenderer>();
		_anim = GetComponent<Animator>();
		if (hasTag(TileTags.Creature) && GetComponent<Rigidbody2D>() == null) {
			_body = gameObject.AddComponent<Rigidbody2D>();
		}
		else {
			_body = GetComponent<Rigidbody2D>();
		}
		_collider = GetComponent<Collider2D>();
	}

	public virtual void pickUp(Tile tilePickingUsUp) {
		if (!hasTag(TileTags.CanBeHeld)) {
			return;
		}
		if (_body != null) {
			_body.velocity = Vector2.zero;
			_body.bodyType = RigidbodyType2D.Kinematic;
		}
		transform.parent = tilePickingUsUp.transform;
		transform.localPosition = new Vector3(0.2f, -0.1f, -0.1f);
		removeTag(TileTags.CanBeHeld);
		tilePickingUsUp.tileWereHolding = this;
		_tileHoldingUs = tilePickingUsUp;
	}

	public virtual void dropped(Tile tileDroppingUs) {
		if (_tileHoldingUs != tileDroppingUs) {
			return;
		}
		if (_body != null) {
			_body.bodyType = RigidbodyType2D.Dynamic;
		}
		// We move ourselves to the current room when we're dropped
		transform.parent = GameManager.instance.currentRoom.transform;
		addTag(TileTags.CanBeHeld);
		_tileHoldingUs.tileWereHolding = null;
		_tileHoldingUs = null;
	}



	public virtual void useAsItem(Tile tileUsingUs) {

	}

	public static Vector2 toGridCoord(float x, float y) {
		return new Vector2(Mathf.Floor(x / TILE_SIZE), Mathf.Floor(y / TILE_SIZE));
	}

	public static Vector2 toLocalCoord(float x, float y) {
		return new Vector2(x*TILE_SIZE + TILE_SIZE/2, y*TILE_SIZE + TILE_SIZE/2);
	}

	public static Tile spawnTile(GameObject tilePrefab, Transform parentOfTile, int gridX, int gridY) {
		GameObject tileObj = Instantiate(tilePrefab) as GameObject;
		tileObj.transform.parent = parentOfTile;
		Tile tile = tileObj.GetComponent<Tile>();
		Vector2 tilePos = toLocalCoord(gridX, gridY);
		tile.x = tilePos.x;
		tile.y = tilePos.y;
		tile.init();
		return tile;
	}

}


