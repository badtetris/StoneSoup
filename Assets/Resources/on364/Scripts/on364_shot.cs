using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class on364_shot : Tile {


    public float deathTimer = 2f;

    public float damageThreshold = 4;

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        if (deathTimer > 0f)
        {
            deathTimer = deathTimer - Time.deltaTime;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    protected override void updateSpriteSorting()
    {
        // DON'T. Keep us sorted on the floor!
    }

    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Tile>() != null)
        {
            float impact = collisionImpactLevel(collision);
            if (impact < damageThreshold)
            {
                return;
            }
            Tile otherTile = collision.gameObject.GetComponent<Tile>();
            otherTile.takeDamage(this, 1);
        }
    }
}
