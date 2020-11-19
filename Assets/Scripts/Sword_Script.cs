using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword_Script : MonoBehaviour
{
    private bool Attacking = false;

    private GameObject Player;

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            Debug.Log("Enemy collision detected");

            if (Attacking)
            {
                collision.gameObject.SendMessage("takeDamage", 1);

                Debug.Log("Attacking == true");
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");    
    }

    public void activateAttack()
    {
        Debug.Log("Sword Activated");
        Attacking = true;
    }

    public void deactivateAttack()
    {
        Debug.Log("Sword Deactivated");
        Attacking = false;
    }

}
