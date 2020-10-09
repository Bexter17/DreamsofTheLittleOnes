using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    private int hp = 10;
    public Rigidbody rb;
    public Transform target;
    public int knockDistance;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
    //DEBUG: Tests if knockback works
        //if (Input.GetButtonDown("Fire1"))
        //{
        //    takeDamage(1);
        //}
    }
    private void takeDamage(int dmg)
    {
        hp -= dmg;
        //Knockback
        // Get the difference between enemy and player position
        // To knockback enemy away from player
        Vector3 direction = transform.position - target.transform.position;
        // direction.normalized so that knockback distance is the same regardless of how far the enemy is from the player
        // ForceMode.Impulse so that knockback happense instantly
        rb.AddForce(direction.normalized * knockDistance, ForceMode.Impulse);
    }
}
