using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple script for just shaking almost any kind of object. 
// WARNING: should only be placed on objects that effectively stay still (otherwise, it needs to be placed on a parent that stays still).
public class ObjShake : MonoBehaviour {

	public float shakeMagnitude = 0.2f;
	public float shakeTime = 0.25f;

	protected float _shakeCounter = 0;

	protected Vector3 _startPos;

	// Update is called once per frame
	void Update () {
		if (_shakeCounter > 0) {
			_shakeCounter -= Time.deltaTime;
			if (_shakeCounter <= 0) {
				transform.localPosition = _startPos;
			}
			else {
				transform.localPosition = _startPos + Vector3.right*Random.Range(-shakeMagnitude, shakeMagnitude) + Vector3.up*Random.Range(-shakeMagnitude, shakeMagnitude);
			}
		}	
	}

	public void shake() {
		if (_shakeCounter <= 0) {
			_startPos = transform.localPosition;
		}
		_shakeCounter = shakeTime;
	}

}
