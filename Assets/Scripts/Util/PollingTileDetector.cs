using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A version of the tile detector that occasionally
// polls the physics engine for whether tiles are nearby.
// Probably much more efficient than the normal tile detector. 
public class PollingTileDetector : MonoBehaviour {

	protected static Collider2D[] _castResults = new Collider2D[10];

	[SerializeField] 
	[EnumFlagsAttribute]
	public TileTags tagsToDetect = 0;

	public float detectionRadius = 12;
	// We randomize how much time we wait until the next poll, mainly to ensure that polling for a lot of the 
	// tile detectors is spread out across multiple frames (i.e. everyone trying to poll on the same frame would
    // probably introduce lag on that frame)
	public float minTimeBetweenPolls = 0.5f;
	public float maxTimeBetweenPolls = 1f;

	protected float _timeTillNextPoll;

	protected Tile _parentTile;

	public LayerMask layerMask = 0x1 + 0x200;


	void Start() {
		_parentTile = GetComponentInParent<Tile>();
		_timeTillNextPoll = Random.Range(minTimeBetweenPolls, maxTimeBetweenPolls);
		gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
	}

	void Update() {
		if (_parentTile == null) {
			return;
		}
		_timeTillNextPoll -= Time.deltaTime;
		if (_timeTillNextPoll <= 0) {
			performPoll();
			_timeTillNextPoll = Random.Range(minTimeBetweenPolls, maxTimeBetweenPolls);
		}
	}

	// Can be called by something else to force a poll at key moments (i.e. if an enemy always wants to do a poll before taking a step)
	public void performPoll() {
		int numResults = Physics2D.OverlapCircleNonAlloc(transform.position, detectionRadius, _castResults, layerMask);

		for (int i = 0; i < numResults && i < _castResults.Length; i++) {
			
			Collider2D result = _castResults[i];
			Tile otherTile = result.GetComponent<Tile>();
			if (otherTile != null && otherTile.hasTag(tagsToDetect)) {
				_parentTile.tileDetected(otherTile);
			}
		}
	}

}
