using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class on364_FireTile : Tile {
    public float explodeWhen = .1f;
    public bool triggered = false;
	public SpriteRenderer thisSprRen;

    public GameObject Explosion;

    void Update()
    {
        if (triggered)
        {
            explodeWhen -= Time.deltaTime;
        }
        if (explodeWhen <= 0f)
        {
            GameObject boom = Instantiate(Explosion, transform.position, Quaternion.identity);
			triggered = !triggered;
        }
    }

    void OnTriggerEnter2D(Collider2D otherCollider)
    {
        Tile tileCheck = otherCollider.GetComponent<Tile>();
        if (tileCheck != null && tileCheck.hasTag(TileTags.Creature))
        {
            if (!triggered)
            {
				thisSprRen.enabled = true;
                triggered = true;
            }
        }
    }

}
