using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class on364_FireTile : Tile {
    public float explodeWhen = 2f;
    public bool triggered = false;

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
        }
    }

    void OnTriggerEnter2D(Collider2D otherCollider)
    {
        Tile tileCheck = otherCollider.GetComponent<Tile>();
        if (tileCheck != null && tileCheck.hasTag(TileTags.Creature))
        {
            if (!triggered)
            {
                triggered = true;
            }
        }
    }

}
