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
        if (Input.GetButtonDown("Fire1"))
        {
            takeDamage(1);
        }
    }
    private void takeDamage(int dmg)
    {
        hp -= dmg;
        //Knockback
        rb.AddForce(Vector3.back * knockDistance);
    }
}
