using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleFlame : Tile {

    private float _timeBeforeWeCanMakeNewFlame = 0.5f;


    void Update() {

        if (_sprite != null) {
            _sprite.flipX = Mathf.Sin(Time.time * 4f) >= 0;
        }

        if (_timeBeforeWeCanMakeNewFlame > 0) {
            _timeBeforeWeCanMakeNewFlame -= Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D otherCollider) {
        Tile maybeTile = otherCollider.GetComponent<Tile>();
        if (maybeTile != null && _timeBeforeWeCanMakeNewFlame <= 0) {
            maybeTile.takeDamage(this, 1);
            Vector2 awayFromUs = (maybeTile.transform.position - transform.position).normalized;
            maybeTile.addForce(awayFromUs * 2500);

            if (maybeTile.hasTag(TileTags.Flammable)) {
                GameObject newFlame = Instantiate(gameObject, maybeTile.transform.position, Quaternion.identity);
                newFlame.GetComponent<Tile>().init();
            }
        }
    }


}
