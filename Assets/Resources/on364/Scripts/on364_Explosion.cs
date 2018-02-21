using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class on364_Explosion : Tile {
	public int size;
	public float timePassed = .1f;

	// Use this for initialization
	void Start () {
		 
	}
	
	// Update is called once per frame
	void Update () {
		if (size == 5) {
			Collider2D[] dmgObjs = Physics2D.OverlapCircleAll(transform.position, 15);
			foreach (Collider2D hitObj in dmgObjs) {
				Tile tile = hitObj.GetComponent<Tile>();
				if (tile == this) {
					continue;
				}
				if (tile != null) {
					tile.takeDamage(this, 2, DamageType.Explosive);
				}
			}
		}
		if (timePassed > 0) 
		{
			timePassed -= Time.deltaTime;
		} 
		else 
		{
			transform.localScale += new Vector3 (5, 5, 0);
			timePassed = .1f;
			size++;
		}
	}
}
