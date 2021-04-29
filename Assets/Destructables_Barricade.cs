using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructables_Barricade : MonoBehaviour
{
    public GameObject destroyedVersion;
    public float waitTime = 2f;

    public Animator hammerAnimations;
    public GameObject Hammer;

    private Rigidbody brokenBody;

    CharacterMechanics character;

    bool onlyHappensOnce = false;

    private void Start()
    {
        GameObject Player = GameObject.FindGameObjectWithTag("Player");

        character = Player.GetComponent<CharacterMechanics>();
    }

    private void Update()
    {

    }
    //private void OnMouseDown()
    //{


    //    GameObject randomPickup = Instantiate(pickUps[index], transform.position, transform.rotation);
    //    GameObject brokenVersion = Instantiate(destroyedVersion, transform.position, transform.rotation, destroyedVersion.transform.parent);
    //    brokenVersion.transform.localScale = new Vector3(2, 2, 2);
    //    Object.Destroy(brokenVersion, 5f);

    //    Destroy(gameObject);

    //    randomPickup.transform.localScale = new Vector3(1, 1, 1);

    //    Instantiate(destroyedVersion, transform.position, transform.rotation, destroyedVersion.transform.parent);

    //}

    public void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.tag == "Hammer2")
        //{
        //    if (character.isAttacking)
        //    {
        //        onlyHappensOnce = true;
        //        Debug.Log("Working Hammer");
        //        GameObject brokenVersion = Instantiate(destroyedVersion, transform.position, transform.rotation, destroyedVersion.transform.parent);
        //        brokenVersion.transform.localScale = new Vector3(3, 3, 3);
        //        Object.Destroy(brokenVersion, 5f);

        //        Destroy(gameObject);

        //        //randomPickup.transform.localScale = new Vector3(1, 1, 1);

        //        // Instantiate(destroyedVersion, transform.position, transform.rotation, destroyedVersion.transform.parent);
        //    }
        //}




    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision Detected: Tag = " + collision.gameObject.tag);

        if (!onlyHappensOnce)
        {
            //if (collision.gameObject.tag == "HammerSmashAOE")
            //{
            //    onlyHappensOnce = true;
            //    GameObject brokenVersion = Instantiate(destroyedVersion, transform.position, transform.rotation, destroyedVersion.transform.parent);
            //    brokenVersion.transform.localScale = new Vector3(3, 3, 3);

            //    Object.Destroy(brokenVersion, 5f);
            //    Destroy(gameObject);
            //}
            //if (collision.gameObject.tag == "WhirlwindAOE")
            //{
            //    onlyHappensOnce = true;
            //    GameObject brokenVersion = Instantiate(destroyedVersion, transform.position, transform.rotation, destroyedVersion.transform.parent);
            //    brokenVersion.transform.localScale = new Vector3(3, 3, 3);
            //    Object.Destroy(brokenVersion, 5f);

            //    Destroy(gameObject);
            //}


            if (collision.gameObject.tag == "Dash Collider")
            {

                onlyHappensOnce = true;
                Debug.Log("Working Hammer");
                GameObject brokenVersion = Instantiate(destroyedVersion, transform.position, transform.rotation, destroyedVersion.transform.parent);
                brokenVersion.transform.localScale = new Vector3(4, 4, 4);
                Object.Destroy(brokenVersion, 5f);

                Destroy(gameObject);

                //randomPickup.transform.localScale = new Vector3(1, 1, 1);

                // Instantiate(destroyedVersion, transform.position, transform.rotation, destroyedVersion.transform.parent);
            }
            //if (collision.gameObject.tag == "Hammer")
            //{
            //    if (character.isAttacking)
            //    {
            //        onlyHappensOnce = true;
            //        Debug.Log("Working Hammer");
            //        GameObject randomPickup = Instantiate(pickUps[index], transform.position, transform.rotation);
            //        GameObject brokenVersion = Instantiate(destroyedVersion, transform.position, transform.rotation, destroyedVersion.transform.parent);
            //        brokenVersion.transform.localScale = new Vector3(2, 2, 2);
            //        Object.Destroy(brokenVersion, 5f);

            //        Destroy(gameObject);

            //        //randomPickup.transform.localScale = new Vector3(1, 1, 1);

            //        // Instantiate(destroyedVersion, transform.position, transform.rotation, destroyedVersion.transform.parent);
            //    }
            //}
        }
    }

}