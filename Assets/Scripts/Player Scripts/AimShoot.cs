using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;

public class AimShoot : MonoBehaviour
{
    public float hitForce = 1000f;

    public Camera FreeAim;

    public GameObject rangePrefab;

    //Cooldown variables
    GameObject abilityObject1;
    public Image abilityImage1;
    public KeyCode ability1;
    public float cooldown1;
    public bool isCooldown1 = false;

    // Start is called before the first frame update
    void Start()
    {
        abilityObject1 = GameObject.FindGameObjectWithTag("Ability 4");
        abilityImage1 = abilityObject1.GetComponent<Image>();
        abilityImage1.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Ability1();
        //Ray ray = FreeAim.ScreenPointToRay(Input.mousePosition);
        //100 shoots fairly far, if we wanted to do a sniper like shot double this value

            
                //if (Input.GetMouseButton(1) && Input.GetButtonDown("Fire5") &! isCooldown1)
                //{
                 //   RaycastHit hit;
                 //   if (Physics.Raycast(transform.position, Camera.main.transform.forward, out hit))   //Shoots directly forward from camera wherever it is looking
                  //  {
                  //      Debug.Log(hit.collider.gameObject.name);
                  //  }

                 //   Vector3 lookdirection = hit.point - transform.position;
                 //   GameObject bullet = Instantiate(rangePrefab, transform.position, Quaternion.LookRotation(lookdirection)) as GameObject;  //Instantiate projectile and then delete after 5 seconds
                //    bullet.GetComponent<Rigidbody>().AddForce(lookdirection * hitForce);
                //    Destroy(bullet, 5);
              //  }

            
    }

    void Ability1()
    {
        if (Input.GetKey(ability1) && isCooldown1 == false)
        {
            isCooldown1 = true;
            abilityImage1.fillAmount = 1;

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

        if (isCooldown1)
        {
            abilityImage1.fillAmount -= 1 / cooldown1 * Time.deltaTime;

            if (abilityImage1.fillAmount <= 0)
            {
                abilityImage1.fillAmount = 0;
                isCooldown1 = false;
            }
        }
    }
}

