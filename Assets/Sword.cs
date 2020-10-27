using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    private bool Attacking = false;

    private GameObject Player;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            if (Attacking)
            {
                collision.gameObject.SendMessage("TakeDamage", 1);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");    
    }

    // Update is called once per frame
    void Update()
    {

        Attacking = Player.GetComponent("isAttacking");
    }
}
