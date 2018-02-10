using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Simple script that tints the color of a tile based on how much health it has left.
public class HealthMeterTint : MonoBehaviour {

	protected Tile _ourTile;

	protected int _maxHealth;

	public Color damageTint;

	// Use this for initialization
	void Start () {
		_ourTile = GetComponent<Tile>();
		_maxHealth = _ourTile.health;
	}
	
	// Update is called once per frame
	void Update () {
		int currentHealth = _ourTile.health;
		float progressToDeath = 1f - (currentHealth / (float)_maxHealth);
		progressToDeath = Mathf.Clamp(progressToDeath, 0f, 1f);
		Color tintColor = Color.Lerp(Color.white, damageTint, progressToDeath);
		_ourTile.sprite.color = tintColor;
	}
}
