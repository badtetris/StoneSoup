using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class on364_BulletShooter : Wall
{
    public GameObject bullet;
    public float timeSinceLastShot, bulletFdDir;
	public BoxCollider2D thisb2d;

    // Use this for initialization
    void Start()
    {
		thisb2d = GetComponent<BoxCollider2D> ();
    }

    protected override void updateSpriteSorting()
    {
        // DON'T. Keep us sorted on the floor!
    }

    // Update is called once per frame
    void Update()
    {
        if (timeSinceLastShot > .5f)
        {
            GameObject shot = Instantiate(bullet, transform.position, Quaternion.identity);
			shot.transform.Rotate(0, 0, bulletFdDir);
			bulletFdDir += 45f;
			shot.GetComponent<Tile> ().GetComponent<Rigidbody2D>().AddForce(shot.transform.up*2.5f, ForceMode2D.Impulse);
            timeSinceLastShot = 0;
			Debug.Log(shot.transform.up);
            Debug.Log(shot.transform.rotation.eulerAngles);
			Physics2D.IgnoreCollision(thisb2d, shot.GetComponent<Tile>().GetComponent<BoxCollider2D>());
        }
        timeSinceLastShot += Time.deltaTime;
    }

}
