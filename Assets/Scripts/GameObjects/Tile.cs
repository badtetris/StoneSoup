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

	protected Rigidbody2D _body;
	protected SpriteRenderer _sprite;
	protected Animator _anim;

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


