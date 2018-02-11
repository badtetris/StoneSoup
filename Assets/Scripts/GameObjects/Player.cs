using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 
// The actual avatar you control
// Spawned by the start room. 
//
public class Player : Tile {

	// SFX we play
	public AudioClip pickupDropSound, hurtSound, exitSound;

	// A reference to the object that displays when we can pickup an item.
	public GameObject handSymbol;

	// Tunable values for how we move.
	public float moveSpeed = 10f;
	public float moveAcceleration = 100f;


	protected int _walkDirection = 2;

	// Used to ensure we don't drop an item and then pick up the exact same item.
	protected Tile _lastTileWeHeld = null;

	// Used to make us invincible for a short period of time after taking damage.
	protected float _iFrameTimer = 0;
	public float totalIFrameTime = 0.5f;

	// Like the GameManager, there should always only be one player, globally accessible
	protected static Player _instance = null;
	public static Player instance {
		get { return _instance; }
	}
	void Awake() {
		_instance = this;
	}
	void OnDestroy() {
		_instance = null;
	}

	// The player has a special function for takeDamage because if the player dies, 
	// instead of calling die, the player tells the GameManager that the game is over.
	public override void takeDamage(Tile tileDamagingUs, int amount, DamageType damageType) {
		if (_iFrameTimer <= 0 && !GameManager.instance.gameIsOver) {
			AudioManager.playAudio(hurtSound);

			// If this is enough damage to kill us, we start the death sequence.
			if (amount >= health) {
				if (health > 0) {
					GameManager.instance.playerJustDefeated(tileDamagingUs);
				}
				health = 0;
			}
			else {
				base.takeDamage(tileDamagingUs, amount, damageType);
				_iFrameTimer = totalIFrameTime;
			}
			
		}
	}

	// The player does movement inside FixedUpdate because the player's movement is continuous 
	// and thus should happen on the physics updates.
	void FixedUpdate() {
		// Let's move via the keyboard controls

		bool tryToMoveUp = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
		bool tryToMoveRight = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
		bool tryToMoveDown = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
		bool tryToMoveLeft = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);

		Vector2 attemptToMoveDir = Vector2.zero;

		if (tryToMoveUp) {
			attemptToMoveDir += Vector2.up;
		}
		else if (tryToMoveDown) {
			attemptToMoveDir -= Vector2.up;			
		}
		if (tryToMoveRight) {
			attemptToMoveDir += Vector2.right;
		}
		else if (tryToMoveLeft) {
			attemptToMoveDir -= Vector2.right;
		}
		attemptToMoveDir.Normalize();

		// We flip our sprite based on whether we're facing right or not.
		if (attemptToMoveDir.x > 0) {
			_sprite.flipX = false;
		}
		else if (attemptToMoveDir.x < 0) {
			_sprite.flipX = true;
		}

		// We use the walk direction variable to tell our animator what animation to play.
		if (attemptToMoveDir.y > 0 && attemptToMoveDir.x == 0) {
			_walkDirection = 0;
		}
		else if (attemptToMoveDir.y < 0 && attemptToMoveDir.x == 0) {
			_walkDirection = 2;
		}
		else if (attemptToMoveDir.x != 0) {
			_walkDirection = 1;
		}
		_anim.SetBool("Walking", attemptToMoveDir.x != 0 || attemptToMoveDir.y != 0);
		_anim.SetInteger("Direction", _walkDirection);

		// Finally, here's where we actually move.
		moveViaVelocity(attemptToMoveDir, moveSpeed, moveAcceleration);


		// Now check if we're on top of an item we can pick up, if so, display the hand symbol.
		bool onItem = false;
		int numObjectsFound = _body.Cast(Vector2.zero, _maybeRaycastResults);
		for (int i = 0; i < numObjectsFound && i < _maybeRaycastResults.Length; i++) {
			RaycastHit2D result = _maybeRaycastResults[i];
			Tile tileHit = result.transform.GetComponent<Tile>();
			if (tileHit != null && tileHit.hasTag(TileTags.CanBeHeld)) {
				onItem = true;
				if (tileWereHolding != null) {
					break;
				}
			}
		}
		handSymbol.SetActive(onItem);
	}

	void Update() {

		if (GameManager.instance.gameIsOver) {
			return;
		}
			
		// Update our aim direction
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 toMouse = (mousePosition - (Vector2)transform.position).normalized;
		aimDirection = toMouse;

		// Update our invincibility frame counter.
		if (_iFrameTimer > 0) {
			_iFrameTimer -= Time.deltaTime;
			_sprite.enabled = !_sprite.enabled;
			if (_iFrameTimer <= 0) {
				_sprite.enabled = true;
			}
		}

		// If we press space, we're attempting to either pickup, drop, or switch items.
		if (Input.GetKeyDown(KeyCode.Space)) {
			bool pickedUpOrDroppedItem = false;

			// First, drop the item we're holding
			if (tileWereHolding != null) {
				// Keep track of the fact that we just dropped this item so we don't pick it up again.
				_lastTileWeHeld = tileWereHolding;
				// Put it at out feet
				tileWereHolding.dropped(this);

				// If we're no longer holding an item, we successfully dropped it.
				if (tileWereHolding == null) {
					pickedUpOrDroppedItem = true;
				}
			}


			// If we successully dropped the item
			if (tileWereHolding == null) {
				// Check to see if we're on top of an item that can be held
				int numObjectsFound = _body.Cast(Vector2.zero, _maybeRaycastResults);
				for (int i = 0; i < numObjectsFound && i < _maybeRaycastResults.Length; i++) {
					RaycastHit2D result = _maybeRaycastResults[i];
					Tile tileHit = result.transform.GetComponent<Tile>();
					// Ignore the tile we just dropped
					if (tileHit == null || tileHit == _lastTileWeHeld) {
						continue;
					}
					if (tileHit.hasTag(TileTags.CanBeHeld)) {
						tileHit.pickUp(this);
						if (tileWereHolding != null) {
							pickedUpOrDroppedItem = true;
							break;
						}
					}
				}
			}

			if (pickedUpOrDroppedItem) {
				AudioManager.playAudio(pickupDropSound);
			}

			// Finally, clear the last tile we held so we can pick it up again next frame if we want to
			_lastTileWeHeld = null;
		}

		// If we click the mouse, we try to use whatever item we're holding.
		if (Input.GetMouseButtonDown(0)) {
			if (tileWereHolding != null) {
				tileWereHolding.useAsItem(this);
			}
		}

		updateSpriteSorting();
	}


	// The only trigger collision the player cares about is the exit.
	void OnTriggerEnter2D(Collider2D other) {
		Tile otherTile = other.transform.GetComponent<Tile>();
		if (otherTile != null) {
			if (otherTile.hasTag(TileTags.Exit)) {
				AudioManager.playAudio(exitSound);
				GameManager.instance.playerJustWon();
			}
		}
	}


}
