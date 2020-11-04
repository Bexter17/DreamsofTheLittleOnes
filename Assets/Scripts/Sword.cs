using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    private bool Attacking = false;

    private GameObject Player;

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            Debug.Log("Enemy collision detected");

            Attacking = Player.GetComponent<CharacterMechanics>().isAttacking;

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

}
