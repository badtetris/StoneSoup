using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Tiles can receive a number of tags that define how other tiles will react to them.
// For instance, the default enemies are programmed to pick up objects tagged "weapon"
// and to harm anything tagged "friendly" (not just the player).
[System.Flags]
public enum TileTags {
	Wall = 0x01,
	CanBeHeld = 0x02,
	Creature = 0x04,
	Player = 0x08,
	Enemy = 0x10,
	Friendly = 0x20,
	Weapon = 0x40,
	Exit = 0x80,
	Consumable = 0x100,
	Wearable = 0x200,
	Money = 0x400,
	Dateable = 0x800,
	Dirt = 0x1000,
	Water = 0x2000,
	Plant = 0x4000,
	Flammable = 0x8000,
	Merchant = 0x10000
}

// The two damage types available to us.
// Important because certain tiles (like walls) should normally not receive damage.
public enum DamageType {
	Normal,
	Explosive
}

///
// The Tile class should be the parent of all the objects you make.
// Tiles facilitate getting different objects to interact with each other in a consistent way. 
///
public class Tile : MonoBehaviour {

	// Max size of an individual tile (both width and height) in Unity Units.
	// You can easily make smaller tiles (such as the rocks)
	// Making larger tiles is not recommended because they'll extend beyond the boundaries of a single space
	// Even if you make sure your larger tiles never spawn overlapping other tiles
	// chaos mode will swap your tiles with another tile, so be careful.
	public const float TILE_SIZE = 2f;

	/////////////////////////////////
	// Our inspector modified properties
	// Most important is the tags property

	[SerializeField] 
	[EnumFlagsAttribute]
	public TileTags tags = 0;

	// Usually not used, but if the player can pick this tile up, this name will appear in the ui.
	public string tileName = "";

	// ALL tiles have health, when they run out, they die.
	public int health = 1;
	protected int _startHealth;

	// If you define a death effect for a tile, it'll be spawned when the tile dies.
	public GameObject deathEffect;
	// Similarly, if you define a death sound for a tile, it'll play when the tile dies.
	public AudioClip deathSFX;

	// When this tile get's picked up, it'll become a child of the tile that picked it up.
	// These properties control where the tile is positioned/oriented in relation to its parent when it's being held.
	public Vector2 heldOffset = new Vector2(0.2f, -0.1f);
	public float heldAngle = 0f;

	/////////////////////////////////


	/////////////////////////////////
	// These are properties that get modified by other tiles, but otherwise we want to keep them hidden from the inspector.
	// This could also be achieved by using protected variables and getters/setters.

	// If we're holding a tile, this value tells us which tile were holding. If we're not holding a tile this should be null
	[HideInInspector]
	public Tile tileWereHolding; 

	// Some tiles (like the rock) rely on an aim direction when they're used. 
	// 
	[HideInInspector]
	public Vector2 aimDirection;

	/////////////////////////////////


	// These are convenient functions for adding/removing/checking for tags
	// Since they involve bitwise operations, probably best to use these functions instead of altering the tags property directly from code
	public bool hasTag(TileTags tag) {
		return (tags & tag) != 0;	
	}

	public void removeTag(TileTags tagsToRemove) {
		tags = tags & ~(tagsToRemove);
	}

	public void addTag(TileTags tagsToAdd) {
		tags |= tagsToAdd;
	}



	/////////////////////////////////

	// Common Unity components that tiles might have.
	// If the tile has the component when init is called, these variables will fill with the appropriate values.
	protected Rigidbody2D _body;
	public virtual Rigidbody2D body {
		get { return _body; }
	}
	protected SpriteRenderer _sprite;
	public virtual SpriteRenderer sprite {
		get { return _sprite; }
	}
	protected Animator _anim;
	protected Collider2D _collider;
	public virtual Collider2D mainCollider {
		get { return _collider; }
	}

	/////////////////////////////////


	
	// Now for protected properties used by many tiles.

	// It's common for tiles to use the Rigidbody2D.Cast function to check for nearby tiles.
	// The specifics of that function require us to have a pre-made array to hold results of the call.
	// This property can be used for that purpose.
	// BE WARNED: Since it's static, it will likely be overwritten after you're done with it, so make sure you copy results you need for later.
	protected static RaycastHit2D[] _maybeRaycastResults = new RaycastHit2D[15];

	// Similarly, depending on the physics function we might need an array of a different type.
	protected static Collider2D[] _maybeColliderResults = new Collider2D[15];
	protected static ContactPoint2D[] _maybeContactResults = new ContactPoint2D[15];


	// Variable that ensures we don't get multiple calls to die on the same frame (for instance if multiple damage sources hit us at the same time)
	protected bool _alive = true;

	// The complement to the tileWereHolding property from above. 
	// If a tile is holding us, this variable contains information about that tile
	// If no tile is holding us, this should be null.
	protected Tile _tileHoldingUs;

	public bool isBeingHeld {
		get { return _tileHoldingUs != null; }
	}

	/////////////////////////////////

	// Here are convenient getters/shortcuts that let us grab and modify individual coordinates without
	// having to go through transform.position.

	public float globalX {
		get { return transform.position.x; }
		set { transform.position = new Vector3(value, transform.position.y, transform.position.z); }
	}

	public float globalY {
		get { return transform.position.y; }
		set { transform.position = new Vector3(transform.position.x, value, transform.position.z); }
	}

	public float localX {
		get { return transform.localPosition.x; }
		set { transform.localPosition = new Vector3(value, transform.localPosition.y, transform.localPosition.z); }
	}

	public float localY {
		get { return transform.localPosition.y; }
		set { transform.localPosition = new Vector3(transform.localPosition.x, value, transform.localPosition.z); }
	}

	/////////////////////////////////

	// The two functions for moving tiles. You should ONLY attempt to move tiles via these functions to ensure that physics works out for everything.

	// This is a simple function usually caused by a tile moving itself (i.e. the player)
	// Thus it's protected so other tiles can't attempt to move a tile using this function.
	protected void moveViaVelocity(Vector2 direction, float speed, float acceleration) {
		if (_body != null) {
			Vector2 currentVelocity = _body.velocity;
			currentVelocity = Vector2.MoveTowards(currentVelocity, direction*speed, acceleration*Time.fixedDeltaTime);
			_body.velocity = currentVelocity;
		}
	}

	// The method used to move other tiles.
	public void addForce(Vector2 force) {
		if (body != null) {
			body.AddForce(force);
		}
	}


	/////////////////////////////////

	// Tile behavior functions defined here.

	// This function is explicitly called after a tile has been spawned and positioned.
	// We use this instead of the Unity-defined Start or Awake because we have extremely precise
	// control over when it's called (for instance, if we just instantiated a tile, Start won't be called until after our current code compeletes, etc.)
	public virtual void init() {
		_sprite = GetComponentInChildren<SpriteRenderer>();
		updateSpriteSorting();
		_anim = GetComponentInChildren<Animator>();
		if (hasTag(TileTags.Creature) && GetComponent<Rigidbody2D>() == null) {
			_body = gameObject.AddComponent<Rigidbody2D>();
		}
		else {
			_body = GetComponent<Rigidbody2D>();
		}
		_collider = GetComponent<Collider2D>();
		_startHealth = health;
	}

	protected virtual void updateSpriteSorting() {
		if (_sprite == null) {
			return;
		}
		if (_tileHoldingUs != null) {
			_sprite.sortingLayerID = _tileHoldingUs.sprite.sortingLayerID;
			_sprite.sortingOrder = _tileHoldingUs.sprite.sortingOrder+1;
			return;
		}
		else if (hasTag(TileTags.CanBeHeld)) {
			_sprite.sortingLayerID = SortingLayer.NameToID("Floor");
		}
		else {
			_sprite.sortingLayerID = SortingLayer.NameToID("Default");
		}
		_sprite.sortingOrder = -(int)globalY;
	}


	// Overloaded version of takeDamage that assumes most of the time we'll be dealing normal type damage.
	public void takeDamage(Tile tileDamagingUs, int damageAmount) {
		takeDamage(tileDamagingUs, damageAmount, DamageType.Normal);
	}

	// The actual take damage function that tiles can override for different effects.
	// For instance, indestructible walls ignore this function because they're indestructible.
	public virtual void takeDamage(Tile tileDamagingUs, int damageAmount, DamageType damageType) {
		// To ensure we don't die multiple times.
		if (!_alive) {
			return;
		}
		health-=damageAmount;
		if (health <= 0) {
			die();
		}
	}

	public virtual void restoreAllHealth() {
		health = _startHealth;
	}

	// The function that's called when a tile dies.
	// Protected because tiles shouldn't be able to force death on each other (should only be able to deal damage).
	// You might wish to override it to give a special behavior for your tile's death (for instance, the explosive rocks
	// cause a big explosion when they die).
	protected virtual void die() {
		_alive = false;

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


	// The function called when a tile attempts to pick up this tile. 
	// Note that the tile being picked up DECIDES WHETHER IT WAS ACTUALLY PICKED UP
	// The tile picking it up will need to check whether tileWereHolding was actually set
	// to see if the pickup was successful.
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
		updateSpriteSorting();
	}

	// Similarly, when a tile is dropped, it DECIDES WHETHER IT WAS ACTUALLY DROPPED
	// The tile attempting to drop it should check tileWereHolding to see if the drop was successful.
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
		transform.parent = tileDroppingUs.transform.parent;
		addTag(TileTags.CanBeHeld);
		_tileHoldingUs.tileWereHolding = null;
		_tileHoldingUs = null;
		updateSpriteSorting();
	}

	// When a tile is being held, it can be used as an item.
	// Override this function to decide what the tile does when it's used.
	public virtual void useAsItem(Tile tileUsingUs) {

	}

	// This is a function you can override to respond to messages from a
	// DetectTile object
	// Example: An enemy has a child object that's listening for approaching friendly objects.
	// When a friendly object entres the trigger, it informs it's parent tile that an object was detected by using
	// this function.
	public virtual void tileDetected(Tile detectedTile) {

	}

	// Similarly, when we can no longer detect a tile. 
	public virtual void tileNoLongerDetected(Tile detectedTile) {
		
	}


	/////////////////////////////////

	// Utility functions for tiles.

	// In single-room mode, the transition areas are designed to make it possible to properly link rooms together
	// BUT you shouldn't be able to drop or use items there, so this function checks if this tile is in a transition area.
	public bool onTransitionArea() {
		if (_body != null) {
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

	// Much of the level and room generation is done using grid coordinates.
	// To actually position the tiles, we need to be able to convert grid coordinates to world coordinates.
	// This function does that.
	public static Vector2 toWorldCoord(float x, float y) {
		return new Vector2(x*TILE_SIZE + TILE_SIZE/2, y*TILE_SIZE + TILE_SIZE/2);
	}

	// Vector version
	public static Vector2 toWorldCoord(Vector2 gridCoord) {
		return new Vector2(gridCoord.x*TILE_SIZE + TILE_SIZE/2, gridCoord.y*TILE_SIZE + TILE_SIZE/2);
	}


	// Sometimes your tile will want to be able to snap to specific grid locations (such as the basic enemies).
	// This tile will convert world coordinates to grid coordinates.
	public static Vector2 toGridCoord(float x, float y) {
		return new Vector2(Mathf.Floor(x / TILE_SIZE), Mathf.Floor(y / TILE_SIZE));
	}

	// Vector2 version
	public static Vector2 toGridCoord(Vector2 worldCoord) {
		return new Vector2(Mathf.Floor(worldCoord.x / TILE_SIZE), Mathf.Floor(worldCoord.y / TILE_SIZE));
	}


	// This function provides a rough estimate of how much impact was involved in a collision.
	// Can be used to figure out if something hit something else hard enough to deal damage etc. 
	// WARNING: This is a little rough and has some known issues (i.e. GetContacts appears to be a bit buggy on the Unity side)
	protected float collisionImpactLevel(Collision2D collision) {
		int numContacts = collision.GetContacts(_maybeContactResults);
		float maxContactImpact = 0;
		// GetContacts probably SHOULDN'T ever return 0, but sometimes it does (a bug maybe?)
		// In that rare case, it's okay to allocate the contact array.
		if (numContacts == 0) {
			foreach (ContactPoint2D contact in collision.contacts) {
				float contactImpact = Vector2.Dot(contact.relativeVelocity, contact.normal);
				if (contactImpact > maxContactImpact) {
					maxContactImpact = contactImpact;
				}
			}
			return maxContactImpact;
		}
		for (int i = 0; i < numContacts && i < _maybeContactResults.Length; i++) {
			float contactImpact = Vector2.Dot(_maybeContactResults[i].relativeVelocity, _maybeContactResults[i].normal);
			if (contactImpact > maxContactImpact) {
				maxContactImpact = contactImpact;
			}
		}
		return maxContactImpact;
	}


	// This function will look for a tile type at a specific point and return the first version it finds
	// Returns null if it can't find one
	public static Tile tileAtPoint(Vector2 point, TileTags testTags) {
		int numObjects = Physics2D.OverlapPointNonAlloc(point, _maybeColliderResults);
		for (int i = 0; i < numObjects && i < _maybeColliderResults.Length; i++) {
			Tile maybeTile = _maybeColliderResults[i].GetComponent<Tile>();
			if (maybeTile != null && maybeTile.hasTag(testTags)) {
				return maybeTile;
			}
		}
		return null;
	}


	// Delegate types used for pathfinding (basic or otherwise)
	public delegate bool CanOverlapFunc(RaycastHit2D hitResult);

	protected bool DefaultCanOverlapFunc(RaycastHit2D hitResult) {
		if (hitResult.collider.isTrigger) {
			return true;
		}
		Tile maybeResultTile = hitResult.transform.GetComponent<Tile>();
		if (maybeResultTile != null) {
			if (maybeResultTile == this) {
				return true;
			}
			if (maybeResultTile.hasTag(TileTags.Wall | TileTags.Creature | TileTags.Exit)) {
				return false;
			}
		}
		else {
			return false; // By default any solid collider that isn't a tile is something we can't overlap with.
		}
		return true;
	}

	// This function tells you if a path between you and a target point is clear of obstacles. 
	// Super useful for coding up some basic AI for tiles. 
	// What counts as an "Obstacle?" That's where the optional "CanOverlapFunc" comes in. 
	protected bool pathIsClear(Vector2 target, CanOverlapFunc canOverlapFunc=null) {
		if (canOverlapFunc == null) {
			canOverlapFunc = DefaultCanOverlapFunc; // Default function for checking overlaps. 
		}
		Vector2 toTarget = target-(Vector2)transform.position;
		float distanceToTarget = toTarget.magnitude;
		toTarget.Normalize();

		// Now we perform a collider "Cast" i.e. we simulate moving our collider forward along the path from us to our target. 
		// Any objects that the physics engine detects as collider with this simulated collider will be stored in _maybeRaycastResults

		// This is a kinda weird way of handling this. The Cast function returns an int instead of just returning the array of results (i.e. the thing we actually care about).
		// Why is it like this? Short answer is for performance. 
		int numCollisions = _collider.Cast(toTarget, _maybeRaycastResults, distanceToTarget);
		for (int i = 0; i < numCollisions && i < _maybeRaycastResults.Length; i++) {
			if (!canOverlapFunc(_maybeRaycastResults[i])) {
				return false;
			}
		}	
		// If we can overlap with all of the possible barriers, then the path is clear!
		return true;
	}

	// This function tells you if a target tile is "visible"
	// i.e. it casts a ray from your position to the target.
	// If nothing interrupts the ray, it decides you can "see" the target
	// Not perfect for all situations, but it should be useful most of the time.
	protected bool canSeeTile(Tile target) {
		Vector2 toTarget = target.transform.position - transform.position;
		float distancetoTarget = toTarget.magnitude;
		toTarget.Normalize();

		int numCollisions = Physics2D.RaycastNonAlloc(transform.position, toTarget, _maybeRaycastResults, distancetoTarget);
		for (int i = 0; i < numCollisions && i < _maybeRaycastResults.Length; i++) {
			RaycastHit2D result = _maybeRaycastResults[i];
			if (result.collider.isTrigger
				|| result.transform == transform
				|| result.transform == target.transform) {
				continue;
			}
			return false;
		}
		return true;
	}


	// This is the function used to spawn tiles. 
	// YOU SHOULDN'T BE SPAWNING TILES WITH ANY OTHER FUNCTION.
	// This function ensures that tiles are properly contained (and parented) to a room
	public static Tile spawnTile(GameObject tilePrefab, Transform parentOfTile, int gridX, int gridY) {
		// Enforce constraints on where we spawn tiles.
		if (gridX < 0 || gridX >= LevelGenerator.ROOM_WIDTH || gridY < 0 || gridY >= LevelGenerator.ROOM_HEIGHT) {
			throw new UnityException(string.Format("Attempted to spawn tile outside room boundaries. Tile: {0}, Grid X: {1}, Grid Y: {1}", tilePrefab, gridX, gridY));
		}

		GameObject tileObj = Instantiate(tilePrefab) as GameObject;
		tileObj.transform.parent = parentOfTile;
		Tile tile = tileObj.GetComponent<Tile>();
		Vector2 tilePos = toWorldCoord(gridX, gridY);
		tile.localX = tilePos.x;
		tile.localY = tilePos.y;
		tile.init();
		return tile;
	}

	

}


