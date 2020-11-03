using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AI;

public class EnemyCombat : MonoBehaviour
{
    private int hp = 10;
    public Rigidbody rb;
    public Transform target;
    [SerializeField] float knockDistanceModifier;
    [SerializeField] float knockDuration;
    [SerializeField] float knockPause;

    NavMeshAgent agent;

    [SerializeField] private Image hpBar;
    // Start is called before the first frame update
    void Start()
    {
        //HPText.text = hp.ToString();
        rb = GetComponent<Rigidbody>();
        //sets maxHP to beginning hp in order to get the correct fill amount for hpbar
        int maxHP = hp;
        agent = GetComponent<NavMeshAgent>();
        target = GameObject.Find("Player").transform;

    }

    // Update is called once per frame
    void Update()
    {
        //DEBUG: Tests if knockback works
        if (Input.GetButtonDown("Fire1"))
        {
            takeDamage(1);
        }
        //HPText.transform.rotation = HPText.transform.LookAt(target) + Quaternion.Euler(0, 180, 0);
        //HPText.transform.LookAt(target) += Quaternion.Euler(0, 180, 0);

        //Sets hp text to change based on players perspective
        //So it's not backwards to the player
        Vector3 textDirection = transform.position - target.transform.position;
        transform.rotation = Quaternion.LookRotation(textDirection);
    }
    public void takeDamage(int dmg)
    {
        //Debug.Log(dmg + "Damage Taken");
        agent.isStopped = true;
        hp -= dmg;
        if(hp <= 0)
        {
            Destroy(gameObject);
        }
        hpBar.fillAmount = (float)(hp * 0.1);
        //HPText.text = hp.ToString();
        //KNOCKBACK
        // Gets the difference between enemy and player position
        // To knockback enemy away from player
        Vector3 knockDirection = transform.position - target.transform.position;
        rb.velocity = knockDirection * knockDistanceModifier;
        //Invokes once enemy is no longer being knocked back and pauses
        Invoke("AgentStop", knockDuration);
    }
    private void AgentStop()
    {
        //Enemy briefly pauses after being knocked back
        //Where it's velocity is 0
        rb.velocity = Vector3.zero;
        Invoke("AgentStart", knockPause);
    }
    private void AgentStart()
    {
        //Enemy continues moving
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }
}
