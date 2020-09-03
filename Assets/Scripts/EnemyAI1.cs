using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI1 : MonoBehaviour
{
    // The player that the enemy will chase
    public Transform target;
    //public Vector3 initialPos;
    //bool isInitPos = true;

    // How fast enemy moves
    public float enemyMovement;
    public Rigidbody rb;

    // The distance the enemy will begin to chase player
    public float chaseRange;
    public float attackRange;

    //bool isMoving = true;
    bool isPatrolling = false;

    NavMeshAgent agent;

    public Transform waypoint1;
    public Transform waypoint2;

    // Amount of damage done by enemy to player
    public int dmgDealt;
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player").transform;
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        if (enemyMovement <= 0)
        {
            enemyMovement = 10f;
        }
        if (chaseRange <= 0)
        {
            chaseRange = 5f;
        }
        if (attackRange <= 0)
        {
            attackRange = 2f;
        }
        if (dmgDealt <= 0)
        {
            dmgDealt = 1f;
        }
        Patrol();
        //MoveContinuouslyForward();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Vector3.Distance(target.position, gameObject.transform.position) < attackRange)
        //{
        //    agent.ResetPath();
        //}
        // Checks if the distance between enemy and player
        // is less then chaseRange
        if (Vector3.Distance(target.position, gameObject.transform.position) < chaseRange)
        {
            Chase();
        }

        //else if (!isInitPos)
        //{
        //    GoHome();
        //}
        else if (!isPatrolling /*&& isInitPos*/)
        {
            Patrol();
        }
        //MoveForward();


    }
    private void Chase()
    {
        //Debug.Log("CHASE");
        isPatrolling = false;
        //isInitPos = false;
        // Rotates to always face the player
        //transform.LookAt(target, Vector3.up);
        // Sets player as destination
        agent.SetDestination(target.transform.position);
        // Move forward
    }

    // Turns around and continues
    private void Patrol()
    {
        //Debug.Log("PATROL");
        //At the begin of patrolling sets first patrol destination
        if (!isPatrolling)
        {
            agent.SetDestination(waypoint1.position);
            isPatrolling = true;
        }
    }
    // During patrol alternate going between Waypoint1 and Waypoint2
    private void OnTriggerEnter(Collider other)
    {
        if (isPatrolling)
        {
            if (other.CompareTag("WayPoint1"))
            {
                //Debug.Log("Waypoint");
                agent.SetDestination(waypoint2.position);
            }
            else if (other.CompareTag("WayPoint2"))
            {
                agent.SetDestination(waypoint1.position);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!isPatrolling)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                //Debug.Log("Player Hit");
            }
        }
    }
}

//Go in circles
//transform.Rotate(0, 1, 0);
//transform.position += transform.forward* enemyMovement * Time.deltaTime;