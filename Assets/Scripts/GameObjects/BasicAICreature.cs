using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A class with some common stuff I ended up using whenever I made AI controlled objects (like enemies)
// It mainly defines a standard for moving towards a specific grid location (_targetGridPos) and 
// Having a tile type you'll chase (and attack).
public class BasicAICreature : Tile {

	// How close we need to be to our target before we're basically there
	public const float GRID_SNAP_THRESHOLD = 0.1f;

	// When we move, we try to move to grid snapped locations, so our current target
	// is stored in grid coordinates.
	protected Vector2 _targetGridPos;
	public Vector2 targetGridPos {
		get { return _targetGridPos; }
	}

	protected bool _takingCorrectingStep = false;
	public bool takingCorrectingStep {
		get { return _takingCorrectingStep; }
	}


	// We move similar to how the player moves, so we keep similar tunable values.
	public float moveSpeed = 5;
	public float moveAcceleration = 100;

	// This doesn't NEED to imply you'll ATTACK these tags. You'll maybe just follow them around.
	// But this can be useful in case you want to have an enemy switch sides and become a friend.
	public TileTags tagsWeChase = TileTags.Friendly;


	// A lot of the AIs will use a list of neighboring positions at some point. Here's a shared list for that so you don't have to allocate too many of them.
	protected static List<Vector2> _neighborPositions = new List<Vector2>(8);


	public virtual void Start () {
		_targetGridPos = Tile.toGridCoord(globalX, globalY);
	}
	
	public virtual void FixedUpdate() {
		Vector2 targetGlobalPos = Tile.toWorldCoord(_targetGridPos.x, _targetGridPos.y);
		if (Vector2.Distance(transform.position, targetGlobalPos) >= 0.1f) {
			// If we're away from our target position, move towards it.
			Vector2 toTargetPos = (targetGlobalPos - (Vector2)transform.position).normalized;
			moveViaVelocity(toTargetPos, moveSpeed, moveAcceleration);
			// Figure out which direction we're going to face. 
			// Prioritize side and down.
			if (_anim != null) {
				if (toTargetPos.x >= 0) {
					_sprite.flipX = false;
				}
				else {
					_sprite.flipX = true;
				}
				// Make sure we're marked as walking.
				_anim.SetBool("Walking", true);
				if (Mathf.Abs(toTargetPos.x) > 0 && Mathf.Abs(toTargetPos.x) > Mathf.Abs(toTargetPos.y)) {
					_anim.SetInteger("Direction", 1);
				}
				else if (toTargetPos.y > 0 && toTargetPos.y > Mathf.Abs(toTargetPos.x)) {
					_anim.SetInteger("Direction", 0);
				}
				else if (toTargetPos.y < 0 && Mathf.Abs(toTargetPos.y) > Mathf.Abs(toTargetPos.x)) {
					_anim.SetInteger("Direction", 2);
				}
			}
		}
		else {
			moveViaVelocity(Vector2.zero, 0, moveAcceleration);
			if (_anim != null) {
				_anim.SetBool("Walking", false);
			}
		}
	}


	protected virtual void takeStep() {
		// Here's the function you can override to figure out how your AI object moves.
		// takeStep will USUALLY set _targetGridPos to do this. 
		_targetGridPos = Tile.toGridCoord(globalX, globalY);
	}


}
