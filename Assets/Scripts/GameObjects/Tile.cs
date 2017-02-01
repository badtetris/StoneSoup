using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum TileTags {
	Wall = 0x01,
	CanBeHeld = 0x02,
	Creature = 0x04,
	Player = 0x08,
	Enemy = 0x10,
	Friendly = 0x20,
	Weapon = 0x40,
	Exit = 0x80
}

public enum DamageType {
	Normal,
	Explosive
}

public class Tile : MonoBehaviour {

	public const float TILE_SIZE = 2f;

	public float globalX {
		get { return transform.position.x; }
		set { transform.position = new Vector3(value, transform.position.y, transform.position.z); }
	}

	public float globalY {
		get { return transform.position.y; }
		set { transform.position = new Vector3(transform.position.x, value, transform.position.z); }
	}


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

	public string tileName = "";

	public int maxHealth = 1;
	public int health = 1;

	public GameObject deathEffect;
	public AudioClip deathSFX;

	public Vector2 heldOffset = new Vector2(0.2f, -0.1f);
	public float heldAngle = 0f;


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

	protected RaycastHit2D[] _maybeRaycastResults = null;

	[HideInInspector]
	public Tile tileWereHolding;

	[HideInInspector]
	public Vector2 aimDirection;

	protected Tile _tileHoldingUs;

	protected void moveViaVelocity(Vector2 direction, float speed, float acceleration) {
		if (_body != null) {
			Vector2 currentVelocity = _body.velocity;
			currentVelocity = Vector2.MoveTowards(currentVelocity, direction*speed, acceleration*Time.fixedDeltaTime);
			_body.velocity = currentVelocity;
		}
	}

	public void addForce(Vector2 force) {
		if (_body != null) {
			_body.AddForce(force);
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

	public void takeDamage(Tile tileDamagingUs, int damageAmount) {
		takeDamage(tileDamagingUs, damageAmount, DamageType.Normal);
	}

	public virtual void takeDamage(Tile tileDamagingUs, int damageAmount, DamageType damageType) {
		health-=damageAmount;
		if (health <= 0) {
			die();
		}
	}

	protected virtual void die() {
		if (tileWereHolding != null) {
			tileWereHolding.dropped(this);
		}
		if (deathEffect != null) {
			Instantiate(deathEffect, transform.position, Quaternion.identity);
		}
		if (deathSFX != null) {
			AudioManager.playAudio(deathSFX);
		}


		Destroy(gameObject);
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
		transform.localPosition = new Vector3(heldOffset.x, heldOffset.y, -0.1f);
		transform.localRotation = Quaternion.Euler(0, 0, heldAngle);
		removeTag(TileTags.CanBeHeld);
		tilePickingUsUp.tileWereHolding = this;
		_tileHoldingUs = tilePickingUsUp;
		_sprite.sortingLayerID = SortingLayer.NameToID("Default");
	}

	public virtual void dropped(Tile tileDroppingUs) {
		if (_tileHoldingUs != tileDroppingUs) {
			return;
		}
		if (onTransitionArea()) {
			return; // Don't allow items to drop on the transition area.
		}

		if (_body != null) {
			_body.bodyType = RigidbodyType2D.Dynamic;
		}
		// We move ourselves to the current room when we're dropped
		transform.localPosition = new Vector3(0.2f, -0.4f, -0.1f);
		transform.localRotation = Quaternion.identity;
		transform.parent = GameManager.instance.currentRoom.transform;
		addTag(TileTags.CanBeHeld);
		_tileHoldingUs.tileWereHolding = null;
		_tileHoldingUs = null;
		_sprite.sortingLayerID = SortingLayer.NameToID("Floor");
	}


	public bool onTransitionArea() {
		if (_body != null) {
			if (_maybeRaycastResults == null) {
				_maybeRaycastResults = new RaycastHit2D[10];
			}
			int numObjectsFound = _body.Cast(Vector2.zero, _maybeRaycastResults);
			for (int i = 0; i < numObjectsFound && i < _maybeRaycastResults.Length; i++) {
				RaycastHit2D result = _maybeRaycastResults[i];
				if (result.transform.gameObject.layer == LayerMask.NameToLayer("TransitionSpace")) {
					return true;
				}
			}

		}
		return false;
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
		// Enforce constraints on where we spawn tiles.
		if (gridX < 0 || gridX >= LevelGenerator.ROOM_WIDTH || gridY < 0 || gridY >= LevelGenerator.ROOM_HEIGHT) {
			throw new UnityException(string.Format("Attempted to spawn tile outside room boundaries. Tile: {0}, Grid X: {1}, Grid Y: {1}", tilePrefab, gridX, gridY));
		}

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


