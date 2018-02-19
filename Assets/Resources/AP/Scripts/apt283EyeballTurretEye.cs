using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283EyeballTurretEye : MonoBehaviour {

	protected Vector3 _startPos;

	protected apt283BasicTurret _parentTurret;

	public Sprite normalTurretSprite, blinkingTurretSprite;

	protected SpriteRenderer _sprite;

	// Use this for initialization
	void Start () {
		_startPos = transform.localPosition;
		_parentTurret = GetComponentInParent<apt283BasicTurret>();
		_sprite = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		// Move TOWARDS our aim direction.
		Vector3 aimPosition = _startPos + (Vector3)_parentTurret.aimDirection*0.1f;
		// SNAP the aim position. 
		aimPosition.x = Mathf.Round(10f*aimPosition.x) / 10f;
		aimPosition.y = Mathf.Round(10f*aimPosition.y) / 10f;
		transform.localPosition = aimPosition;

		if (_parentTurret.timeSinceLastFire < 0.2f) {
			_parentTurret.sprite.sprite = blinkingTurretSprite;
			_sprite.enabled = false;
		}
		else {
			_parentTurret.sprite.sprite = normalTurretSprite;
			_sprite.enabled = true;
		}

	}
}
