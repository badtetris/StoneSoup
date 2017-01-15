using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Tile {

	public float moveSpeed = 10f;
	public float moveAcceleration = 10f;

	protected int _walkDirection = 2;

	protected Tile _lastTileWeHeld = null;


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

		if (attemptToMoveDir.x > 0) {
			_sprite.flipX = false;
		}
		else if (attemptToMoveDir.x < 0) {
			_sprite.flipX = true;
		}


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
		moveViaVelocity(attemptToMoveDir, moveSpeed, moveAcceleration);
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			// First, drop the item we're holding
			if (tileWereHolding != null) {
				// Keep track of the fact that we just dropped this item so we don't pick it up again.
				_lastTileWeHeld = tileWereHolding;
				// Put it at out feet
				tileWereHolding.transform.localPosition = new Vector3(0.2f, -0.4f, -0.1f);
				tileWereHolding.dropped(this);
			}

			// Check to see if we're on top of an item that can be held
			RaycastHit2D[] maybeResults = new RaycastHit2D[10];
			int numObjectsFound = _body.Cast(Vector2.zero, maybeResults);
			for (int i = 0; i < numObjectsFound && i < maybeResults.Length; i++) {
				RaycastHit2D result = maybeResults[i];
				if (result.transform.gameObject.tag != "Tile") {
					continue;
				}
				Tile tileHit = result.transform.GetComponent<Tile>();
				// Ignore the tile we just dropped
				if (tileHit == _lastTileWeHeld) {
					continue;
				}

				if (tileHit.hasTag(TileTags.CanBeHeld)) {
					tileHit.pickUp(this);
				}

			}

			// Finally, clear the last tile we held so we can pick it up again next frame if we want to
			_lastTileWeHeld = null;
		}
		if (Input.GetMouseButtonDown(0)) {
			if (tileWereHolding != null) {
				tileWereHolding.useAsItem(this);
			}
		}
	}


}
