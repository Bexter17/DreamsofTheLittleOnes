using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnemyCombat : MonoBehaviour
{
    private int hp = 10;
    public Rigidbody rb;
    public Transform target;
    public int knockDistance;

    [SerializeField] private Image hpBar;
    // Start is called before the first frame update
    void Start()
    {
        //HPText.text = hp.ToString();
        rb = GetComponent<Rigidbody>();
        //sets maxHP to beginning hp in order to get the correct fill amount for hpbar
        int maxHP = hp;
    }

    // Update is called once per frame
    void Update()
    {
        //DEBUG: Tests if knockback works
        //if (Input.GetButtonDown("Fire1"))
        //{
        //    takeDamage(1);
        //}
        //HPText.transform.rotation = HPText.transform.LookAt(target) + Quaternion.Euler(0, 180, 0);
        //HPText.transform.LookAt(target) += Quaternion.Euler(0, 180, 0);

        //Sets hp text to change based on players perspective
        //So it's not backwards to the player
        Vector3 textDirection = transform.position - target.transform.position;
        transform.rotation = Quaternion.LookRotation(textDirection);
    }
    private void takeDamage(int dmg)
    {
        hp -= dmg;
        if(hp <= 0)
        {
            Destroy(gameObject);
        }
        hpBar.fillAmount = (float)(hp * 0.1);
        //HPText.text = hp.ToString();
        //Knockback
        // Get the difference between enemy and player position
        // To knockback enemy away from player
        Vector3 knockDirection = transform.position - target.transform.position;
        // direction.normalized so that knockback distance is the same regardless of how far the enemy is from the player
        // ForceMode.Impulse so that knockback happense instantly
        rb.AddForce(knockDirection.normalized * knockDistance, ForceMode.Impulse);
    }
}
