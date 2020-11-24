using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;


//Animation Script
public class EnemyAI1 : MonoBehaviour
{
    //Connect EnemyAI1 script and EnemyCombat script
    EnemyCombat EnemyCombatScript;

    // The player that the enemy will chase
    public Transform target;
    //public Vector3 initialPos;
    //bool isInitPos = true;

    //Animations
    Animator eAnim;

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
        EnemyCombatScript = gameObject.GetComponent<EnemyCombat>();

        eAnim = gameObject.GetComponent<Animator>();

        target = GameObject.Find("Player").transform;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
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
            attackRange = 3f;
        }
        if (dmgDealt <= 0)
        {
            dmgDealt = 1;
        }
        Patrol();
        //MoveContinuouslyForward();
    }

    // Update is called once per frame
    void Update()
    {
        // Look at destination on x and z axis
        Vector3 targetPosition = new Vector3(agent.destination.x, transform.position.y, agent.destination.z);
        transform.LookAt(targetPosition);
        //if (Vector3.Distance(target.position, gameObject.transform.position) < attackRange)
        //{
        //    agent.ResetPath();
        //}

        if (Vector3.Distance(target.position, gameObject.transform.position) < attackRange)
        {
            //Debug.Log("STOP");
            agent.isStopped = true;
        }

        // Checks if the distance between enemy and player
        // is less then chaseRange
        else if (Vector3.Distance(target.position, gameObject.transform.position) < chaseRange) /*&& Vector3.Distance(target.position, gameObject.transform.position) > attackRange)*/
            {
                Chase();
                //Honk();
                //Debug.Log("CHASE");
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
        

        if (EnemyCombatScript.death == true)
        {
            eAnim.SetTrigger("IsPunching");
            eAnim.SetBool("IsDying", true);
            eAnim.SetTrigger("IsDead");
            Destroy(gameObject, 5);
        }
    }
    public void Chase()
    {
        agent.isStopped = false;
        //Debug.Log("CHASE");
        isPatrolling = false;
        //isInitPos = false;
        // Rotates to always face the player
        //transform.LookAt(target, Vector3.up);
        // Sets player as destination
        agent.SetDestination(target.transform.position);
        // Move forward
    }
    // Calls Chase() for all enemies
    private void Honk()
    {
        GameObject[] enemies;
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for(int i = 0; i < enemies.Length; i++)
        {
            enemies[i].GetComponent<EnemyAI1>().Chase();
        }
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
    
    // Used for enemy animations and patrolling between waypoints
    private void OnTriggerEnter(Collider other)
    {
        // Plays punching animation when player enters collision
        if (other.CompareTag("Player"))
            eAnim.SetTrigger("isPunching");

        // During patrol alternate going between Waypoint1 and Waypoint2
        // On colliding with waypoint
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

    //Sets enemy to walking animation if player leaves collision
    private void OnTriggerExit(Collider other)
    {
       // eAnim.ResetTrigger("isPunching");
    }
    

    /*private void OnCollisionEnter(Collision collision)
    {
        if (!isPatrolling)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                //Debug.Log("Player Hit");
            }
        }
    }*/
}

//Go in circles
//transform.Rotate(0, 1, 0);
//transform.position += transform.forward* enemyMovement * Time.deltaTime;