using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimShoot : MonoBehaviour
{
    public float hitForce = 1000f;

    public Camera FreeAim;

    public GameObject rangePrefab;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        //Ray ray = FreeAim.ScreenPointToRay(Input.mousePosition);
        //100 shoots fairly far, if we wanted to do a sniper like shot double this value


        if (Input.GetMouseButton(1) && Input.GetButtonDown("Fire5"))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Camera.main.transform.forward, out hit))   //Shoots directly forward from camera wherever it is looking
            {
                Debug.Log(hit.collider.gameObject.name);
            }

            Vector3 lookdirection = hit.point - transform.position;
            GameObject bullet = Instantiate(rangePrefab, transform.position, Quaternion.LookRotation(lookdirection)) as GameObject;  //Instantiate projectile and then delete after 5 seconds
            bullet.GetComponent<Rigidbody>().AddForce(lookdirection * hitForce);
            Destroy(bullet, 5);
        }
    }
}

