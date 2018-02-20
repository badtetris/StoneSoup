using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class on364_BulletShooter : Wall
{
    public GameObject bullet;
    public float timeSinceLastShot;
    public Vector3 shotDir;

    // Use this for initialization
    void Start()
    {

    }

    protected override void updateSpriteSorting()
    {
        // DON'T. Keep us sorted on the floor!
    }

    // Update is called once per frame
    void Update()
    {
        if (timeSinceLastShot > .25f)
        {
            GameObject shot = Instantiate(bullet, transform.position+shotDir.normalized*1.5f, Quaternion.identity);
            shot.transform.eulerAngles = shotDir;
            shotDir += new Vector3(0, 0, 15f);
            timeSinceLastShot = 0;
            Debug.Log(shot.transform.rotation.eulerAngles);
        }
        timeSinceLastShot += Time.deltaTime;
    }

}
