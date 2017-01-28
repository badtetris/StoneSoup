﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Tile {

	public float moveSpeed = 10f;
	public float moveAcceleration = 10f;

	protected int _walkDirection = 2;

	protected Tile _lastTileWeHeld = null;

	// How long we're invincible after receiving a hit.
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

	public override void takeDamage(Tile tileDamagingUs, int amount, DamageType damageType) {
		if (_iFrameTimer <= 0) {
			// If this is enough damage to kill us, we start the death sequence.
			if (amount >= health && health > 0) {
				health = 0;
				GameManager.instance.playerJustDefeated(tileDamagingUs);
			}
			else {
				base.takeDamage(tileDamagingUs, amount, damageType);
				_iFrameTimer = totalIFrameTime;
			}
			
		}
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

		// Update our aim direction
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 toMouse = (mousePosition - (Vector2)transform.position).normalized;
		aimDirection = toMouse;


		if (_iFrameTimer > 0) {
			_iFrameTimer -= Time.deltaTime;
			_sprite.enabled = !_sprite.enabled;
			if (_iFrameTimer <= 0) {
				_sprite.enabled = true;
			}
		}

		if (Input.GetKeyDown(KeyCode.Space)) {
			// First, drop the item we're holding
			if (tileWereHolding != null) {
				// Keep track of the fact that we just dropped this item so we don't pick it up again.
				_lastTileWeHeld = tileWereHolding;
				// Put it at out feet
				tileWereHolding.dropped(this);
			}


			// If we successully dropped the item
			if (tileWereHolding == null) {
				// Check to see if we're on top of an item that can be held
				if (_maybeRaycastResults == null) {
					_maybeRaycastResults = new RaycastHit2D[10];
				}
				int numObjectsFound = _body.Cast(Vector2.zero, _maybeRaycastResults);
				for (int i = 0; i < numObjectsFound && i < _maybeRaycastResults.Length; i++) {
					RaycastHit2D result = _maybeRaycastResults[i];
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
						if (tileWereHolding != null) {
							break;
						}
					}

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

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag == "Tile") {
			Tile otherTile = other.transform.GetComponent<Tile>();
			if (otherTile.hasTag(TileTags.Exit)) {
				SceneManager.LoadScene("LevelCompleteScene");
			}
		}
	}


}
